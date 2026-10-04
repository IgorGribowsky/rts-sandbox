// A see-through copy of a building: the one under the cursor while placing
// it, and the ones queued with Shift (M-010). Unlit colour, brighter and more
// solid at the silhouette, like a hologram, so the shape reads on any ground.
//
// A depth-only pass first, so only the front faces show and the inside of the
// model does not shine through.
Shader "RTS/Ghost"
{
    Properties
    {
        _Color ("Colour", Color) = (1, 1, 1, 0.4)
        _RimPower ("Rim sharpness", Range(0.5, 8)) = 2.5
        _RimBoost ("Rim strength", Range(0, 2)) = 0.8
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }

        Pass
        {
            ZWrite On
            ColorMask 0
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _RimPower;
            float _RimBoost;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD0;
                float3 view : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.view = WorldSpaceViewDir(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float facing = saturate(dot(normalize(i.normal), normalize(i.view)));
                float rim = pow(1 - facing, _RimPower) * _RimBoost;
                // Faces turned up catch a little more light: the shape keeps its volume.
                float top = saturate(normalize(i.normal).y) * 0.25;
                // The rim lightens in the ghost's own colour, never washes it to white:
                // a red ghost must stay red.
                fixed4 col = _Color;
                col.rgb = lerp(col.rgb * (0.8 + top), col.rgb + 0.25, saturate(rim * 0.6));
                col.a = saturate(_Color.a * (1 + rim));
                return col;
            }
            ENDCG
        }
    }
}
