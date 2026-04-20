Shader "Custom/Water"
{
    Properties
    {
        _ShallowColor       ("Shallow Color",        Color)      = (0.15, 0.6, 0.7, 1)
        _DeepColor          ("Deep Color",           Color)      = (0.02, 0.1, 0.3, 1)
        _DepthFactor        ("Depth Factor",         Float)      = 1.0
        _FresnelPower       ("Fresnel Power",        Float)      = 3.0
        _ShoreFadeStrength  ("Shore Fade Strength",  Float)      = 1.0
        _AlphaMin           ("Alpha Min (top-down)", Range(0,1)) = 0.6

        [Header(Waves)]
        _WaveAmplitude      ("Wave Amplitude",       Float)      = 0.1
        _WaveFrequency      ("Wave Frequency",       Float)      = 0.5
        _WaveSpeed          ("Wave Speed",           Float)      = 1.0
        _WaveNoise          ("Wave Noise Texture",   2D)         = "gray" {}
        _WaveNoiseScale     ("Wave Noise Scale (m)", Float)      = 10.0
        _WaveNoiseStrength  ("Wave Noise Strength",  Float)      = 0.3

        [Header(Dropoff)]
        _CurveFactor        ("How Much Curve per Meter Distance", Float)      = 100
        _CurvePower         ("Falloff Power of the Curve", Float)      = 200

        [Header(Normal Maps)]
        _NormalMapA         ("Normal Map A",         2D)         = "bump" {}
        _NormalMapB         ("Normal Map B",         2D)         = "bump" {}
        _NormalScale        ("Normal Scale (m)",     Float)      = 5.0
        _NormalStrength     ("Normal Strength",      Float)      = 0.5
        _ScrollDirectionA   ("Scroll Direction A (XZ)", Vector)    = (1, 0.5, 0, 0)
        _ScrollSpeedA       ("Scroll Speed A",         Float)     = 0.05
        _ScrollDirectionB   ("Scroll Direction B (XZ)", Vector)   = (-0.5, 1, 0, 0)
        _ScrollSpeedB       ("Scroll Speed B",         Float)     = 0.03

        [Header(Shore Foam)]
        _FoamMap            ("Shore Foam Map",       2D)         = "black" {}
        _FoamNoise          ("Foam Noise Texture",  2D)         = "white" {}
        _FoamNoiseScale     ("Foam Noise Scale (m)",Float)      = 3.0
        _FoamNoiseSpeed     ("Foam Noise Speed",    Vector)     = (0.02, 0.01, 0, 0)
        _FoamThreshold      ("Foam Threshold",      Range(0,1)) = 0.5
        _FoamColor          ("Foam Color",           Color)      = (1, 1, 1, 1)

        [Header(Specular)]
        _SpecularColor      ("Specular Color",       Color)      = (1, 1, 1, 1)
        _SpecularSmoothness ("Specular Smoothness",  Float)      = 0.08
        _SpecularThreshold  ("Specular Threshold",   Float)      = 0.5
        _SpecularStrength   ("Specular Strength",    Float)      = 1.0
        _SpecularDistFade   ("Specular Distance Fade", Float)   = 0.02
        _SpecularPixelSize  ("Specular Pixel Size (m)", Float)  = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue"          = "Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D(_NormalMapA); SAMPLER(sampler_NormalMapA);
            TEXTURE2D(_NormalMapB); SAMPLER(sampler_NormalMapB);
            TEXTURE2D(_WaveNoise);  SAMPLER(sampler_WaveNoise);
            TEXTURE2D(_FoamMap);    SAMPLER(sampler_FoamMap);
            TEXTURE2D(_FoamNoise);  SAMPLER(sampler_FoamNoise);

            CBUFFER_START(UnityPerMaterial)
                float4 _ShallowColor;
                float4 _DeepColor;
                float  _DepthFactor;
                float  _FresnelPower;
                float  _ShoreFadeStrength;
                float  _AlphaMin;
                float  _NormalScale;
                float  _NormalStrength;
                float4 _ScrollDirectionA;
                float  _ScrollSpeedA;
                float4 _ScrollDirectionB;
                float  _ScrollSpeedB;
                float  _WaveAmplitude;
                float  _WaveFrequency;
                float  _WaveSpeed;
                float  _WaveNoiseScale;
                float  _WaveNoiseStrength;
                float  _CurveFactor;
                float  _CurvePower;
                float4 _FoamMapParams;
                float4 _FoamColor;
                float  _FoamNoiseScale;
                float4 _FoamNoiseSpeed;
                float  _FoamThreshold;
                float4 _SpecularColor;
                float  _SpecularSmoothness;
                float  _SpecularThreshold;
                float  _SpecularStrength;
                float  _SpecularDistFade;
                float  _SpecularPixelSize;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 screenPos  : TEXCOORD0;
                float3 worldPos   : TEXCOORD1;
            };

            float WaveHeight(float2 xz)
            {
                float t = _Time.y * _WaveSpeed;
                float f = _WaveFrequency;

                float2 noiseUV = xz / _WaveNoiseScale + float2(0.02, 0.01) * _Time.y;
                float  noise   = SAMPLE_TEXTURE2D_LOD(_WaveNoise, sampler_WaveNoise, noiseUV, 0).r;
                float2 warp    = (noise - 0.5) * 2.0 * _WaveNoiseStrength;
                float2 p       = xz + warp;

                float  h  = sin(p.x * f + t);
                       h += sin((p.x * 0.8 + p.y * 0.6) * f * 1.3 + t * 0.9) * 0.6;
                       h += sin(p.y * f * 0.7 - t * 1.1) * 0.4;
                return h * _WaveAmplitude;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                worldPos.y += WaveHeight(worldPos.xz);
                float dist = distance(_WorldSpaceCameraPos.xz, worldPos.xz);
                float dropoff = pow(abs(dist * _CurveFactor), _CurvePower);
                worldPos.y -= dropoff;
                OUT.positionCS = TransformWorldToHClip(worldPos);
                OUT.screenPos  = ComputeScreenPos(OUT.positionCS);
                OUT.worldPos   = worldPos;
                return OUT;
            }

            float CalculateSpecular(float3 normal, float3 viewDir, float3 lightDir, float smoothness)
            {
                float angle    = acos(clamp(dot(normalize(lightDir + viewDir), normal), -1.0, 1.0));
                float exponent = angle / smoothness;
                return exp(-exponent * exponent);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv      = IN.screenPos.xy / IN.screenPos.w;
                float3 viewDir = normalize(_WorldSpaceCameraPos - IN.worldPos);

                // Depth-based water color
                float sceneDepth   = LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams);
                float surfaceDepth = IN.screenPos.w;
                float depth        = max(0.0, sceneDepth - surfaceDepth);
                float3 waterColor  = lerp(_ShallowColor.rgb, _DeepColor.rgb, 1 - exp(-depth * _DepthFactor));

                // Sample two scrolling normal maps in world-space XZ
                float2 uvA = IN.worldPos.xz / _NormalScale + _Time.y * normalize(_ScrollDirectionA.xy) * _ScrollSpeedA;
                float2 uvB = IN.worldPos.xz / _NormalScale + _Time.y * normalize(_ScrollDirectionB.xy) * _ScrollSpeedB;
                float3 nA  = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMapA, sampler_NormalMapA, uvA));
                float3 nB  = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMapB, sampler_NormalMapB, uvB));

                // Blend normals and convert to world space
                float3 blended     = normalize(nA + nB);
                float3 worldNormal = normalize(float3(
                    blended.x * _NormalStrength,
                    1.0,
                    blended.y * _NormalStrength
                ));

                // Specular: Gaussian shaped by the animated normal, hard-stepped for glitter
                float  camDist   = length(_WorldSpaceCameraPos.xz - IN.worldPos.xz);
                float  distFade  = exp(-camDist * _SpecularDistFade);

                // Snap world XZ to a grid so the specular normal is constant per cell → pixelated blocks
                float2 snappedXZ = floor(IN.worldPos.xz / _SpecularPixelSize) * _SpecularPixelSize;
                float2 suvA = snappedXZ / _NormalScale + _Time.y * normalize(_ScrollDirectionA.xy) * _ScrollSpeedA;
                float2 suvB = snappedXZ / _NormalScale + _Time.y * normalize(_ScrollDirectionB.xy) * _ScrollSpeedB;
                float3 snA  = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMapA, sampler_NormalMapA, suvA));
                float3 snB  = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMapB, sampler_NormalMapB, suvB));
                float3 specBlended = normalize(snA + snB);
                float3 specNormal  = normalize(float3(specBlended.x * _NormalStrength, 1.0, specBlended.y * _NormalStrength));

                Light  mainLight = GetMainLight();
                float  spec      = CalculateSpecular(specNormal, viewDir, mainLight.direction, _SpecularSmoothness);
                spec = step(_SpecularThreshold, spec);
                waterColor += _SpecularColor.rgb * mainLight.color * spec * _SpecularStrength * distFade;

                // Shore foam 
                // float2 foamUV   = (IN.worldPos.xz - _FoamMapParams.xy) / _FoamMapParams.z;
                // float  foamVal  = SAMPLE_TEXTURE2D(_FoamMap, sampler_FoamMap, foamUV).r;
                // float2 foamNoiseUV = IN.worldPos.xz / _FoamNoiseScale + _Time.y * _FoamNoiseSpeed.xy;
                // float  foamNoise   = SAMPLE_TEXTURE2D(_FoamNoise, sampler_FoamNoise, foamNoiseUV).r;
                // float  foam        = step(_FoamThreshold, foamVal * foamNoise);
                // waterColor         = lerp(waterColor, _FoamColor, foam);

                // Alpha: fresnel (view angle) × shore fade (depth)
                float fresnelFactor = pow(saturate(dot(viewDir, float3(0, 1, 0))), _FresnelPower);
                float fresnel       = lerp(1.0, _AlphaMin, fresnelFactor);
                float shoreFade     = 1 - exp(-depth * _ShoreFadeStrength);

                return float4(saturate(waterColor), fresnel * shoreFade);
            }
            ENDHLSL
        }
    }
}

