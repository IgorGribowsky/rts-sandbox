// The fog of war (M-027). Three passes:
//
// 0 Screen, over the finished picture (FogOfWarEffect). Every pixel finds its
//   point in the world from the depth texture and looks the fog up there:
//   the alpha of _FogSmooth is the darkness, 1 black (never seen), 0.5 grey
//   (seen before), 0 clear. Grey is darker, paler, colder ground with mist
//   drifting over it, thicker at the edge of sight; black is
//   _UnexploredColor. The edge wavers a little, like a cloud.
//
// 1 Compose, into the smooth texture (FogOfWar): the grid stretched with a
//   cubic B-spline, previous and newest pass mixed by _FogBlend.
// 2 Blur, run twice: across, then along.
//
// Clouds and wobble read a small tiling noise texture instead of working
// noise out per pixel: cheap on a phone.
Shader "Hidden/RTS/FogOfWar"
{
    Properties
    {
        _MainTex ("Picture", 2D) = "white" {}
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    sampler2D _MainTex;
    float4 _MainTex_TexelSize;

    // A Gaussian of nine taps read as five, thanks to bilinear filtering.
    static const float BlurOffset1 = 1.3846153846;
    static const float BlurOffset2 = 3.2307692308;
    static const float BlurWeight0 = 0.2270270270;
    static const float BlurWeight1 = 0.3162162162;
    static const float BlurWeight2 = 0.0702702703;
    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        // 0 Screen
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

            sampler2D _FogSmooth;
            sampler2D _NoiseTex;
            float4 _FogArea;        // xMin, zMin, 1 / width, 1 / height
            float4 _UnexploredColor;
            float4 _Explored;       // darkening, desaturation
            float4 _ExploredTint;
            float4 _CloudColor;
            float4 _Clouds;         // strength, 1 / size, drift in metres
            float4 _Wobble;         // metres, crawl
            float3 _FogCameraPos;
            float3 _RayBL;
            float3 _RayBR;
            float3 _RayTL;
            float3 _RayTR;

            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, i.uv);

                float2 screenUv = i.uv;
                #if UNITY_UV_STARTS_AT_TOP
                if (_MainTex_TexelSize.y < 0)
                {
                    screenUv.y = 1 - screenUv.y;
                }
                #endif

                float depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUv));
                float3 ray = lerp(lerp(_RayBL, _RayBR, screenUv.x), lerp(_RayTL, _RayTR, screenUv.x), screenUv.y);
                float2 ground = (_FogCameraPos + ray * depth).xz;

                // The edge wavers: the fog is looked up a little to the side.
                float2 wobble = tex2D(_NoiseTex, ground / 40.0 + _Wobble.y).gb - 0.5;
                float2 at = ground + wobble * 2.0 * _Wobble.x;
                float darkness = tex2D(_FogSmooth, (at - _FogArea.xy) * _FogArea.zw).a;

                // 0 clear .. 1 grey, then 0 grey .. 1 black.
                float grey = saturate(darkness * 2.0);
                float black = saturate(darkness * 2.0 - 1.0);

                // Two layers of mist drifting different ways, with some contrast.
                float2 cloudAt = ground * _Clouds.y;
                float drift = _Clouds.z * _Clouds.y;
                float cloudA = tex2D(_NoiseTex, cloudAt + float2(drift, drift * 0.4)).r;
                float cloudB = tex2D(_NoiseTex, cloudAt * 2.1 + float2(-drift * 0.9, drift * 1.2)).r;
                float cloud = smoothstep(0.35, 0.75, cloudA * 0.65 + cloudB * 0.35);

                // Mist lies over the grey, and thickest where sight fades out.
                float edge = grey * (1.0 - grey) * 4.0;
                float mist = cloud * _Clouds.x * saturate(grey + edge * 0.6);

                float luminance = dot(color.rgb, float3(0.299, 0.587, 0.114));
                float3 explored = lerp(color.rgb, luminance.xxx, _Explored.y);
                explored *= (1.0 - _Explored.x) * _ExploredTint.rgb;

                color.rgb = lerp(color.rgb, explored, grey);
                color.rgb = lerp(color.rgb, _CloudColor.rgb, mist);
                color.rgb = lerp(color.rgb, _UnexploredColor.rgb, black);

                // Half a step of noise per pixel hides what is left of the
                // eight-bit steps in the soft gradients.
                float dither = frac(52.9829189 * frac(dot(i.pos.xy, float2(0.06711056, 0.00583715))));
                color.rgb += (dither - 0.5) / 255.0;
                return color;
            }
            ENDCG
        }

        // 1 Compose: the grid stretched to the smooth texture with a cubic
        //   B-spline — bilinear left a crease along every cell, and the
        //   cells showed as squares — previous and newest pass mixed.
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            sampler2D _FogTex;
            sampler2D _FogPrevTex;
            float4 _FogTex_TexelSize;
            float _FogBlend;

            float4 CubicWeights(float v)
            {
                float4 n = float4(1.0, 2.0, 3.0, 4.0) - v;
                float4 s = n * n * n;
                float x = s.x;
                float y = s.y - 4.0 * s.x;
                float z = s.z - 4.0 * s.y + 6.0 * s.x;
                float w = 6.0 - x - y - z;
                return float4(x, y, z, w) / 6.0;
            }

            // Sixteen texels read as four bilinear taps.
            float Bicubic(sampler2D tex, float2 uv)
            {
                float2 texel = uv * _FogTex_TexelSize.zw - 0.5;
                float2 f = frac(texel);
                texel -= f;

                float4 xc = CubicWeights(f.x);
                float4 yc = CubicWeights(f.y);

                float4 c = texel.xxyy + float2(-0.5, 1.5).xyxy;
                float4 s = float4(xc.xz + xc.yw, yc.xz + yc.yw);
                float4 offset = (c + float4(xc.yw, yc.yw) / s) * _FogTex_TexelSize.xxyy;

                float s0 = tex2D(tex, offset.xz).a;
                float s1 = tex2D(tex, offset.yz).a;
                float s2 = tex2D(tex, offset.xw).a;
                float s3 = tex2D(tex, offset.yw).a;

                float sx = s.x / (s.x + s.y);
                float sy = s.z / (s.z + s.w);
                return lerp(lerp(s3, s2, sx), lerp(s1, s0, sx), sy);
            }

            float4 frag(v2f_img i) : SV_Target
            {
                float d = lerp(Bicubic(_FogPrevTex, i.uv), Bicubic(_FogTex, i.uv), _FogBlend);
                return float4(1, 1, 1, d);
            }
            ENDCG
        }

        // 2 Blur, across or along by _BlurStep.
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            float4 _BlurStep;

            float4 frag(v2f_img i) : SV_Target
            {
                float2 step1 = _BlurStep.xy * BlurOffset1;
                float2 step2 = _BlurStep.xy * BlurOffset2;
                float d = tex2D(_MainTex, i.uv).a * BlurWeight0
                    + (tex2D(_MainTex, i.uv + step1).a + tex2D(_MainTex, i.uv - step1).a) * BlurWeight1
                    + (tex2D(_MainTex, i.uv + step2).a + tex2D(_MainTex, i.uv - step2).a) * BlurWeight2;
                return float4(1, 1, 1, d);
            }
            ENDCG
        }
    }

    Fallback Off
}
