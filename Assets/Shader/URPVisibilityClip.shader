Shader "Custom/URPVisibilityClip"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _HoleEnabled ("Enable Hole", Float) = 1
        _PlaneEnabled ("Enable Plane Clip", Float) = 0
        _CutoutRadius ("Hole Radius", Float) = 1.4
        _CutoutSoftness ("Hole Softness", Float) = 0.3
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalRenderPipeline"
            "Queue" = "Geometry"
            "RenderType" = "Opaque"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _HoleEnabled;
                float _PlaneEnabled;
                float _CutoutRadius;
                float _CutoutSoftness;
            CBUFFER_END

            float3 _Vis_TargetPos;
            float3 _Vis_PlanePoint;
            float3 _Vis_PlaneNormal;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS = positionInputs.positionCS;
                OUT.positionWS = positionInputs.positionWS;
                OUT.normalWS = normalInputs.normalWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                if (_HoleEnabled > 0.5)
                {
                    float distanceToTarget = distance(IN.positionWS, _Vis_TargetPos);
                    float holeMask = smoothstep(_CutoutRadius, _CutoutRadius + max(_CutoutSoftness, 0.0001), distanceToTarget);
                    clip(holeMask - 0.001);
                }

                if (_PlaneEnabled > 0.5)
                {
                    float3 planeNormal = normalize(_Vis_PlaneNormal);
                    float signedDistance = dot(IN.positionWS - _Vis_PlanePoint, planeNormal);
                    clip(signedDistance);
                }

                half4 baseSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                return baseSample;
            }
            ENDHLSL
        }
    }
}
