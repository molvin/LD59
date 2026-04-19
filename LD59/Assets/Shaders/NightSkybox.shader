Shader "Custom/NightSkybox"
{
    Properties
    {
        _SkyColor ("Sky Color", Color) = (0.3, 0.6, 1.0, 1)
        _HorizonOffset ("Horizon Offset", Float) = 0.0
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

            sampler2D _StarSign0Tex;
            sampler2D _StarSign1Tex;
            sampler2D _StarSign2Tex;
            sampler2D _MoonTex;

            float3 _StarSign0Dir;
            float3 _StarSign1Dir;
            float3 _StarSign2Dir;
            float3 _MoonDir;
            float _StarSign0Size;
            float _StarSign1Size;
            float _StarSign2Size;
            float _MoonSize;

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
                col = BlendStarSign(col, adjViewDir, _StarSign0Dir, _StarSign0Tex, _StarSign0Size);
                col = BlendStarSign(col, adjViewDir, _StarSign1Dir, _StarSign1Tex, _StarSign1Size);
                col = BlendStarSign(col, adjViewDir, _StarSign2Dir, _StarSign2Tex, _StarSign2Size);
                col = BlendStarSign(col, adjViewDir, _MoonDir, _MoonTex, _MoonSize);
                return float4(col, 1);
            }
            ENDHLSL
        }
    }
}
