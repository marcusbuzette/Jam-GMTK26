Shader "Custom/URP_Outline"
{
    Properties
    {
        _OutlineThickness ("Max Thickness", Float) = 0.1
        _OutlineMinThickness ("Min Thickness", Float) = 0.05 
        _OutlineColor ("Color", Color) = (0, 0, 0, 1)
        _OutlineSpeed ("Speed", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        
        // Outline 
        Pass
        {
            Name "Outline"
            Cull Front
            Tags { "LightMode" = "SRPDefaultUnlit" }
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float2 normalXY : TEXCOORD2;
                float2 normalZ : TEXCOORD3;
                float4 tangent : TANGENT;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float _OutlineMinThickness, _OutlineThickness, _OutlineSpeed; // Adicionado aqui
                half4 _OutlineColor;
            CBUFFER_END

            void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz); // Pegar posição do vertex no mundo
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS, true); // Normal dentro de 2 uvs e normaliza
                
                float currentThickness = 0;
                
                // Remapeia o seno de [-1, 1] direto para [_OutlineMinThickness, _OutlineThickness]
                Unity_Remap_float(sin(_Time.y * _OutlineSpeed), float2(-1, 1), float2(_OutlineMinThickness, _OutlineThickness), currentThickness);
                
                positionWS += normalWS * currentThickness; // Aplica a espessura já calculada

                OUT.positionHCS = TransformWorldToHClip(positionWS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = _OutlineColor;
                return color;
            }
            ENDHLSL
        }
    }
}