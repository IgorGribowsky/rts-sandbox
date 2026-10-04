// A mark lying on the ground: build grid cells (M-010), aim hints (M-020).
// Unlit, alpha blended, never writes depth and is pulled towards the camera a
// little, so it never flickers against the ground it lies on.
//
// The picture is white and takes its colour from _Color. Optional motion:
// _Pulse breathes the alpha, _Scroll slides the picture along V (a pattern
// running along an arrow), _Spin turns it around its centre (a ring).
Shader "RTS/GroundMark"
{
    Properties
    {
        _MainTex ("Picture (white, alpha)", 2D) = "white" {}
        _Color ("Colour", Color) = (1, 1, 1, 1)
        _Pulse ("Pulse depth (0..1)", Range(0, 1)) = 0
        _PulseSpeed ("Pulse speed", Float) = 3
        _Scroll ("Scroll along V, per second", Float) = 0
        _Spin ("Spin, turns per second", Float) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Offset -1, -1

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Pulse;
            float _PulseSpeed;
            float _Scroll;
            float _Spin;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                float2 uv = TRANSFORM_TEX(v.uv, _MainTex);

                float angle = _Time.y * _Spin * 6.2831853;
                float s = sin(angle);
                float c = cos(angle);
                float2 centred = uv - 0.5;
                uv = float2(centred.x * c - centred.y * s, centred.x * s + centred.y * c) + 0.5;

                uv.y -= _Time.y * _Scroll;
                o.uv = uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                float breath = 1 - _Pulse * (0.5 + 0.5 * sin(_Time.y * _PulseSpeed));
                col.a *= breath;
                return col;
            }
            ENDCG
        }
    }
}
