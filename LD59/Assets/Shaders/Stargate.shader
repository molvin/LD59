Shader "Custom/Stargate"
{
    Properties
    {
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _Speed ("Scroll Speed", Float) = 0.5
        _Brightness ("Brightness", Float) = 2.0
        _Contrast ("Contrast", Float) = 2.0
        _PixelSize ("Pixel Size", Float) = 64.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

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

            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            float _Speed;
            float _Brightness;
            float _Contrast;
            float _PixelSize;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float t = _Time.y;

                float2 uv = floor(i.uv * _PixelSize) / _PixelSize - 0.5;
                float angle = atan2(uv.y, uv.x) / (2.0 * UNITY_PI); // -0.5..0.5
                float radius = length(uv);

                float logDepth = -log(radius + 0.001);
                float2 tunnelUV = float2(angle, logDepth - t * _Speed);

                float n1 = tex2D(_NoiseTex, tunnelUV * float2(1.0, 0.5)).r;
                float n2 = tex2D(_NoiseTex, tunnelUV * float2(2.0, 1.0) + float2(0.5, 0.3)).r;
                float noise = n1 * 0.7 + n2 * 0.3;

                noise = saturate((noise - 0.5) * _Contrast + 0.5);
                float fade = smoothstep(0.0, 0.08, radius) * smoothstep(0.5, 0.35, radius);
                float intensity = noise * fade * _Brightness;

                return fixed4(intensity, intensity, intensity, 1.0);
            }
            ENDCG
        }
    }
}
