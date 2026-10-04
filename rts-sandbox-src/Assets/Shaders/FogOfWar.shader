// The fog of war over the finished picture (M-027, FogOfWarEffect).
// Every pixel finds its point in the world from the depth texture and looks
// the fog up there: the alpha of _FogTex is the darkness, 1 black (never
// seen), 0.5 grey (seen before), 0 clear. Grey is darker and paler ground
// with a slow haze drifting over it; black is _UnexploredColor.
//
// The fog texture is blurred a little for a soft edge and slides from the
// previous pass to the newest by _FogBlend, so the edge does not jump.
Shader "Hidden/RTS/FogOfWar"
{
    Properties
    {
        _MainTex ("Picture", 2D) = "white" {}
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

            sampler2D _FogTex;
            sampler2D _FogPrevTex;
            float _FogBlend;
            float4 _FogArea;    // xMin, zMin, 1 / width, 1 / height
            float4 _FogTexel;   // blur step in uv
            float4 _UnexploredColor;
            float4 _Explored;   // darkening, desaturation
            float4 _Haze;       // strength, 1 / scale, offset in metres
            float3 _FogCameraPos;
            float3 _RayBL;
            float3 _RayBR;
            float3 _RayTL;
            float3 _RayTR;

            float Darkness(float2 uv)
            {
                float now = tex2D(_FogTex, uv).a;
                float before = tex2D(_FogPrevTex, uv).a;
                return lerp(before, now, _FogBlend);
            }

            // Five taps of the bilinear texture: a soft edge, not a staircase of cells.
            float SoftDarkness(float2 uv)
            {
                float2 d = _FogTexel.xy;
                return (Darkness(uv) * 2.0
                    + Darkness(uv + float2(d.x, d.y))
                    + Darkness(uv + float2(-d.x, d.y))
                    + Darkness(uv + float2(d.x, -d.y))
                    + Darkness(uv + float2(-d.x, -d.y))) / 6.0;
            }

            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float Noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(Hash(i), Hash(i + float2(1, 0)), u.x),
                            lerp(Hash(i + float2(0, 1)), Hash(i + float2(1, 1)), u.x), u.y);
            }

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
                float3 world = _FogCameraPos + ray * depth;

                float2 fogUv = (world.xz - _FogArea.xy) * _FogArea.zw;
                float darkness = SoftDarkness(fogUv);

                // 0 clear .. 1 grey, then 0 grey .. 1 black.
                float grey = saturate(darkness * 2.0);
                float black = saturate(darkness * 2.0 - 1.0);

                float2 hazeAt = (world.xz + float2(_Haze.z, _Haze.z * 0.6)) * _Haze.y;
                float haze = (Noise(hazeAt) * 0.65 + Noise(hazeAt * 2.3 + 17.0) * 0.35 - 0.5) * 2.0;

                float luminance = dot(color.rgb, float3(0.299, 0.587, 0.114));
                float3 explored = lerp(color.rgb, luminance.xxx, _Explored.y);
                explored *= 1.0 - _Explored.x;
                explored *= 1.0 + haze * _Haze.x;

                color.rgb = lerp(color.rgb, explored, grey);
                color.rgb = lerp(color.rgb, _UnexploredColor.rgb, black);
                return color;
            }
            ENDCG
        }
    }

    Fallback Off
}
