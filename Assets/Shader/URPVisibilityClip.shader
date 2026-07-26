Shader "Custom/URPVisibilityClip"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _HoleCenterViewport ("Hole Center Viewport", Vector) = (0.5,0.5,0,1)
        _HoleRadiusPixels ("Hole Radius Pixels", Float) = 40
        _HoleSoftnessPixels ("Hole Softness Pixels", Float) = 8
        _HoleEnabled ("Enable Hole", Float) = 1
        _PlaneEnabled ("Enable Plane Clip", Float) = 0
        _CutoutRadius ("Hole Radius", Float) = 1.4
        _CutoutSoftness ("Hole Softness", Float) = 0.3
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

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
                float4 _HoleCenterViewport;
                float _HoleRadiusPixels;
                float _HoleSoftnessPixels;
                float _HoleEnabled;
                float _PlaneEnabled;
                float _CutoutRadius;
                float _CutoutSoftness;
            CBUFFER_END

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
                    float2 fragPixel = IN.positionCS.xy;
                    float2 holePixel = saturate(_HoleCenterViewport.xy) * _ScaledScreenParams.xy;

                    float distanceToHoleCenterPx = distance(fragPixel, holePixel);
                    float safeRadius = max(_HoleRadiusPixels, 0.5);
                    float safeSoftness = max(_HoleSoftnessPixels, 0.5);
                    float holeMask = smoothstep(safeRadius, safeRadius + safeSoftness, distanceToHoleCenterPx);
                    clip(holeMask - 0.001);
                }

                if (_PlaneEnabled > 0.5)
                {
                    // Plane clipping is intentionally disabled in the simplified occluder-only setup.
                }

                half4 baseSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                Light mainLight = GetMainLight();
                half3 normalWS = normalize(IN.normalWS);
                half ndotl = saturate(dot(normalWS, mainLight.direction));
                half3 litColor = baseSample.rgb * (0.25h + ndotl * mainLight.color);
                return half4(litColor, baseSample.a);
            }
            ENDHLSL
        }
    }
}
