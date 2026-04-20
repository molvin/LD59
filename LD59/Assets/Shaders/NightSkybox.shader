Shader "Custom/NightSkybox"
{
    Properties
    {
        _SkyColor ("Sky Color", Color) = (0.3, 0.6, 1.0, 1)
        _HorizonOffset ("Horizon Offset", Float) = 0.0
        _StarDensity ("Star Density", Range(0,1)) = 0.3
        _StarMinSize ("Star Min Size", Range(0,1)) = 0.1
        _StarMaxSize ("Star Max Size", Range(0,1)) = 0.5
        _StarColor ("Star Color", Color) = (1,1,1,1)
        _StarPixelation ("Star Pixelation", Float) = 100
        _RandomSign0Tex ("Random Star Sign Texture 0", 2D) = "black"{}
        _RandomSign1Tex ("Random Star Sign Texture 1", 2D) = "black"{}
        _RandomSign2Tex ("Random Star Sign Texture 2", 2D) = "black"{}
        _RandomSignSize ("Random Star Sign Size", Float) = 1.0
        _RandomSignDensity ("Random Star Sign Density", Range(0, 1)) = 0.1
        _RandomSignMinHeight ("Random Star Sign Min Height", Range(0, 1)) = 0.1
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
            float _HorizonOffset;
            float _StarDensity;
            float _StarMinSize;
            float _StarMaxSize;
            float4 _StarColor;
            float _StarPixelation;

            sampler2D _StarSign0Tex;
            sampler2D _StarSign1Tex;
            sampler2D _StarSign2Tex;
            sampler2D _MoonTex;
            sampler2D _RandomSign0Tex;
            sampler2D _RandomSign1Tex;
            sampler2D _RandomSign2Tex;

            float3 _StarSign0Dir;
            float3 _StarSign1Dir;
            float3 _StarSign2Dir;
            float3 _MoonDir;
            float _StarSign0Size;
            float _StarSign1Size;
            float _StarSign2Size;
            float _MoonSize;
            float _RandomSignSize;
            float _RandomSignDensity;
            float _RandomSignMinHeight;

            float hash(float2 p)
            {
                p = frac(p * float2(127.1, 311.7));
                p += dot(p, p + 45.23);
                return frac(p.x * p.y);
            }

            float3 BlendStars(float3 col, float3 viewDir)
            {
                float theta = acos(clamp(viewDir.y, -1.0, 1.0));
                float phi = atan2(viewDir.z, viewDir.x);
                float2 sphereUV = float2(phi / (2.0 * UNITY_PI) + 0.5, theta / UNITY_PI);
                float2 cellUV = sphereUV * _StarPixelation;
                float2 cellID = floor(cellUV);
                float2 cellFrac = frac(cellUV);
                float rng1 = hash(cellID);
                float rng2 = hash(cellID + float2(13.7, 57.3));
                if (rng1 < _StarDensity)
                {
                    float starSize = lerp(_StarMinSize, _StarMaxSize, rng2);
                    float2 d = abs(cellFrac - 0.5);
                    if (max(d.x, d.y) < starSize * 0.5)
                        col = _StarColor.rgb;
                }
                return col;
            }

            float3 BlendStarSign(float3 col, float3 viewDir, float3 signDir, sampler2D signTex, float size)
            {
                float halfSize = sin(size * UNITY_PI / 180.0);

                float3 ref = abs(signDir.y) < 0.999 ? float3(0, 1, 0) : float3(0, 0, 1);
                float3 right = normalize(cross(ref, signDir));
                float3 up = cross(signDir, right);

                float2 uv = float2(dot(viewDir, right), dot(viewDir, up)) / (2.0 * halfSize) + 0.5;

                float inBounds = step(0.0, uv.x) * step(uv.x, 1.0)
                               * step(0.0, uv.y) * step(uv.y, 1.0)
                               * step(0.0, dot(viewDir, signDir));

                float4 s = tex2D(signTex, uv) * inBounds;
                return lerp(col, s.rgb, s.a);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.dir = v.vertex.xyz;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 viewDir = normalize(i.dir);
                float3 adjViewDir = normalize(float3(viewDir.x, viewDir.y + _HorizonOffset, viewDir.z));
                float3 col = _SkyColor.rgb;
                col = BlendStars(col, viewDir);
                col = BlendStarSign(col, adjViewDir, _StarSign0Dir, _StarSign0Tex, _StarSign0Size);
                col = BlendStarSign(col, adjViewDir, _StarSign1Dir, _StarSign1Tex, _StarSign1Size);
                col = BlendStarSign(col, adjViewDir, _StarSign2Dir, _StarSign2Tex, _StarSign2Size);
                col = BlendStarSign(col, adjViewDir, _MoonDir, _MoonTex, _MoonSize);

                static const int GRID_W = 24;
                static const int GRID_H = 12;
                for (int gy = 0; gy < GRID_H; gy++)
                {
                    for (int gx = 0; gx < GRID_W; gx++)
                    {
                        float2 cellID = float2(gx, gy);
                        if (hash(cellID) >= _RandomSignDensity)
                            continue;

                        float phi = (float(gx) + hash(cellID + float2(7.3, 2.1))) / float(GRID_W) * 2.0 * UNITY_PI;
                        float y = lerp(_RandomSignMinHeight, 1.0, (float(gy) + hash(cellID + float2(3.7, 9.1))) / float(GRID_H));
                        float r = sqrt(max(0.0, 1.0 - y * y));
                        float3 rdir = float3(r * cos(phi), y, r * sin(phi));

                        float texSel = hash(cellID + float2(17.3, 5.7));
                        if (texSel < 0.333)
                            col = BlendStarSign(col, adjViewDir, rdir, _RandomSign0Tex, _RandomSignSize);
                        else if (texSel < 0.667)
                            col = BlendStarSign(col, adjViewDir, rdir, _RandomSign1Tex, _RandomSignSize);
                        else
                            col = BlendStarSign(col, adjViewDir, rdir, _RandomSign2Tex, _RandomSignSize);
                    }
                }

                return float4(col, 1);
            }
            ENDHLSL
        }
    }
}
