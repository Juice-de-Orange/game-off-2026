// Planet ring (P24): flat annulus mesh, u = 0 at the inner edge and 1 at the outer edge.
// Alpha-blended, double-sided, brighter when the sun shines on it. No fog.
Shader "GameOff/PlanetRing"
{
    Properties
    {
        _MainTex ("Ring (RGBA, u = radius)", 2D) = "white" {}
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _Ambient ("Ambient", Range(0, 1)) = 0.15
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent-90" "IgnoreProjector" = "True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            Name "Unlit"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST; half4 _Tint; float _Ambient;
            CBUFFER_END
            float4 _GameOffSunDir;

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float3 normalWS : TEXCOORD1; };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                half4 t = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(i.uv.x, 0.5));
                float lit = _Ambient + (1 - _Ambient) * abs(dot(normalize(i.normalWS), normalize(_GameOffSunDir.xyz + float3(0, 1e-4, 0))));
                return half4(t.rgb * _Tint.rgb * lit, t.a * _Tint.a);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
