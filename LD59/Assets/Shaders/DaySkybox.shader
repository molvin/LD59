Shader "Custom/DaySkybox"
{
    Properties
    {
        _SkyColor ("Sky Color", Color) = (0.3, 0.6, 1.0, 1)
        _SunColor ("Sun Color", Color) = (1.0, 0.95, 0.8, 1)
        _SunSize ("Sun Size", Float) = 0.05
        _SunBloom ("Sun Bloom", Float) = 0.3
        _SunPixels ("Sun Pixel Size", Float) = 16

        [NoScaleOffset]
        _CloudNoise ("Cloud Noise", 2D) = "white" {}
        _CloudScale ("Cloud Scale", Float) = 1.0
        _CloudColor ("Cloud Color", Color) = (1.0, 1.0, 1.0, 1)
        _CloudPixels ("Cloud Pixel Size", Float) = 32
        _CloudThreshold ("Cloud Threshold", Float) = 0.5
        _CloudContrast ("Cloud Contrast", Float) = 2.0
        _CloudScrollSpeed ("Cloud Scroll Speed", Vector) = (0.5, 0.2, 0, 0)
        _CloudScrollSpeedFactor ("Cloud Scroll Speed Factor", Float) = 0.001

        _HazeColor ("Haze Color", Color) = (0.8, 0.85, 0.9, 1)
        _HazeStrength ("Haze Strength", Float) = 1.0
        _HazeFalloff ("Haze Falloff", Float) = 4.0
        _HazePixels ("Haze Pixel Size", Float) = 64
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float3 dir : TEXCOORD0; };

            float4 _SkyColor;
            float4 _SunColor;
            float _SunSize;
            float _SunBloom;
            float _SunPixels;
            sampler2D _CloudNoise;
            float _CloudScale;
            float4 _CloudColor;
            float _CloudPixels;
            float _CloudThreshold;
            float _CloudContrast;
            float4 _CloudScrollSpeed;
            float _CloudScrollSpeedFactor;
            float4 _HazeColor;
            float _HazeStrength;
            float _HazeFalloff;
            float _HazePixels;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.dir = v.vertex.xyz;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.dir);
                float3 sunDir = normalize(_WorldSpaceLightPos0.xyz);

                // SUN
                float3 pixDir = normalize(floor(dir * _SunPixels) / _SunPixels);
                float sunDirDot = dot(pixDir, sunDir);
                float sunDisk = smoothstep(1.0 - _SunSize, 1.0 - _SunSize * 0.1, sunDirDot);
                float bloom = pow(max(sunDirDot, 0.0), 8.0) * _SunBloom;

                // CLOUDS
                float2 cloudUV    = dir.xz / max(dir.y, 0.0000001) * _CloudScale + _CloudScrollSpeed.xy * _Time.y * _CloudScrollSpeedFactor;
                float2 pixCloudUV = floor(cloudUV * _CloudPixels) / _CloudPixels;
                float  cloud      = tex2Dlod(_CloudNoise, float4(pixCloudUV, 0, 0)).r;
                cloud = saturate((cloud - _CloudThreshold) / max(1.0 - _CloudThreshold, 0.001));
                cloud = pow(cloud, _CloudContrast);
                cloud *= smoothstep(0.0, 0.2, dir.y);

                // HAZE
                float pixDirY = floor(dir.y * _HazePixels) / _HazePixels;
                float haze = pow(1.0 - saturate(abs(pixDirY)), _HazeFalloff) * _HazeStrength;

                // COMPOSE
                float3 col = lerp(_SkyColor.rgb, _CloudColor.rgb, cloud);
                col = lerp(col, _SunColor.rgb, saturate(bloom));
                col = lerp(col, _SunColor.rgb, sunDisk);
                col = lerp(col, _HazeColor.rgb, saturate(haze));

                return float4(col, 1);
            }
            ENDHLSL
        }
    }
}
