Shader "Custom/SineWave"
{
    Properties
    {
        _Amplitude ("Amplitude", Float) = 0.1
        _Frequency ("Frequency", Float) = 5.0
        _Krangle ("Krangle", Float) = 0.1
        _LineColor ("Line Color", Color) = (1, 1, 1, 1)
        _BgTex ("Background Texture", 2D) = "black" {}
        _Thickness ("Line Thickness", Float) = 0.01
        _ScrollSpeed ("Scroll Speed", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };


            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BgTex);
            SAMPLER(sampler_BgTex);

            CBUFFER_START(UnityPerMaterial)
                float _Amplitude;
                float _Frequency;
                float _Krangle;
                float4 _LineColor;
                float4 _BgTex_ST;
                float _Thickness;
                float _ScrollSpeed;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float y = uv.y - 0.5;
                float tau = 3.141592653579893238 * 2;
                float krangleFreq = 3.5; 

                float phase = _Frequency * tau * uv.x - _Time.y * _ScrollSpeed;
                float waveY = (1.0 - _Krangle) * _Amplitude * sin(phase);
                float krangle = _Krangle * _Amplitude * sin(phase * krangleFreq);

                float dist = abs(y - (waveY + krangle));
                float mask = step(dist, _Thickness);
                float4 bg = SAMPLE_TEXTURE2D(_BgTex, sampler_BgTex, TRANSFORM_TEX(uv, _BgTex));
                return lerp(bg, _LineColor, mask);
            }

            ENDHLSL
        }
    }
}
