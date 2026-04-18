// Hidden blit shader used by WaterFoamBaker.
// Each pass is a 3x3 max-filter that dilates the foam proximity mask by 1 texel.
// Running N passes spreads the foam zone N texels outward from the shoreline.
Shader "Hidden/WaterFoamSpread"
{
    Properties { _MainTex ("", 2D) = "black" {} }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4    _MainTex_TexelSize;

            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert(appdata_img v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.texcoord;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float maxVal = 0;
                for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    float2 uv = i.uv + float2(dx, dy) * _MainTex_TexelSize.xy;
                    maxVal = max(maxVal, tex2D(_MainTex, uv).r);
                }
                return float4(maxVal, maxVal, maxVal, 1);
            }
            ENDCG
        }
    }
}
