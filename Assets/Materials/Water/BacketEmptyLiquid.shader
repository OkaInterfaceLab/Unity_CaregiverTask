Shader "Unlit/BacketEmptyLiquid"
{
    Properties
    {
        _WaveCenter("Wave Center", Vector) = (0.0, 0.0, 0.0, 0.0)
        _WaveParams("Wave Params", Vector) = (0.0, 0.0, 0.0, 0.0)
        _LiquidColorForward("Liquid Color Forward", Color) = (0.5, 0.5, 0.5, 0.5) // ìßâﬂó¶Çí≤êÆ
        _LiquidColorBack("Liquid Color Back", Color) = (0.8, 0.8, 0.8, 0.5) // ìßâﬂó¶Çí≤êÆ
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha // ìßâﬂçáê¨ê›íË
        ZWrite On
        ZTest LEqual

        // óºñ ï`âÊ
        Cull Off

        Pass
        {
            Name "BacketEmptyLiquid"

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 _WaveCenter;
            float4 _WaveParams;
            float4 _LiquidColorForward;
            float4 _LiquidColorBack;

            #define WaveSize (_WaveParams.x)
            #define WaveCycleCoef (_WaveParams.y)
            #define WaveOffsetCycleCoef (_WaveParams.z)

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldPos));
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // îgÇÃçÇÇ≥ÇÃåvéZ
                float waveBaseY = _WaveCenter.y;
                float2 localXZ = _WaveCenter.xz - i.worldPos.xz;
                float2 waveInput = localXZ * WaveCycleCoef;
                float waveInputOffset = _Time.y * WaveOffsetCycleCoef;
                waveInput += waveInputOffset;
                float clipPosY = waveBaseY + (sin(waveInput.x) + sin(waveInput.y)) * WaveSize;

                // ÉNÉäÉbÉsÉìÉO
                clip(clipPosY - i.worldPos.y);

                // ï\ó†ñ ÇÃêFê›íË
                half NdotV = dot(i.worldNormal, i.viewDir);
                half4 color = lerp(_LiquidColorBack, _LiquidColorForward, step(0.0h, NdotV));

                // ìßâﬂê´ÇîΩâf
                color.a = lerp(_LiquidColorBack.a, _LiquidColorForward.a, step(0.0h, NdotV));
                return color;
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}
