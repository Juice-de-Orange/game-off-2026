// Passing ships outside the station (P55): simple lit, opaque, NO fog – like the sky shaders. Main's indoor fog (0.012)
// would otherwise swallow ships at 100–300 m. Light = ambient + sun (_GameOffSunDir, set by SkyRig) on the albedo,
// plus the emission map (windows, engines). No shadows.
Shader "GameOff/ShipNoFog"
{
    Properties
    {
        _BaseMap ("Albedo", 2D) = "white" {}
        _BaseColor ("Tint", Color) = (1, 1, 1, 1)
        _EmissionMap ("Emission", 2D) = "black" {}
        _EmissionColor ("Emission Color", Color) = (1, 1, 1, 1)
        _Ambient ("Ambient", Range(0, 1)) = 0.3
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }
        Pass
        {
            Name "Unlit"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_EmissionMap); SAMPLER(sampler_EmissionMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST; half4 _BaseColor; half4 _EmissionColor; float _Ambient;
            CBUFFER_END
            float4 _GameOffSunDir;

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float3 normalWS : TEXCOORD1; };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv).rgb * _BaseColor.rgb;
                float3 sun = normalize(_GameOffSunDir.xyz + float3(0, 1e-4, 0));
                float lit = _Ambient + (1 - _Ambient) * saturate(dot(normalize(i.normalWS), sun));
                half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, i.uv).rgb * _EmissionColor.rgb;
                return half4(albedo * lit + emission, 1);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
