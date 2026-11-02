// Distant planet/moon (P24): lit by the global sun direction from SkyRig, day/night side, city lights on the night side,
// atmosphere rim. No fog (space). Opaque, drawn before the skybox.
Shader "GameOff/SkyPlanet"
{
    Properties
    {
        _MainTex ("Albedo", 2D) = "white" {}
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _EmissionTex ("Night Lights (Emission)", 2D) = "black" {}
        _EmissionStrength ("Night Lights Strength", Float) = 1.5
        _RimColor ("Atmosphere Color", Color) = (0.35, 0.6, 1.0, 1)
        _RimPower ("Atmosphere Falloff", Float) = 3
        _RimStrength ("Atmosphere Strength", Float) = 1.2
        _Ambient ("Night Side Ambient", Range(0, 0.3)) = 0.03
        _SpinSpeed ("Spin (deg/s)", Float) = 0.4
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry+50" }
        Pass
        {
            Name "Unlit"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_EmissionTex); SAMPLER(sampler_EmissionTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST; half4 _Tint; half4 _RimColor;
                float _EmissionStrength, _RimPower, _RimStrength, _Ambient, _SpinSpeed;
            CBUFFER_END
            float4 _GameOffSunDir; // world direction toward the sun, set by GameOff.Core.SkyRig

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float3 normalWS : TEXCOORD1; float3 viewWS : TEXCOORD2; };

            Varyings vert (Attributes v)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = p.positionCS;
                o.uv = v.uv + float2(_Time.y * _SpinSpeed / 360.0, 0);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.viewWS = GetWorldSpaceViewDir(p.positionWS);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float3 n = normalize(i.normalWS);
                float3 sunDir = normalize(_GameOffSunDir.xyz + float3(0, 1e-4, 0));
                float ndl = dot(n, sunDir);
                half3 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).rgb * _Tint.rgb;
                half3 col = albedo * (_Ambient + saturate(ndl));
                // City lights only where it is night
                half3 lights = SAMPLE_TEXTURE2D(_EmissionTex, sampler_EmissionTex, i.uv).rgb;
                col += lights * _EmissionStrength * (1.0 - smoothstep(-0.15, 0.15, ndl));
                // Atmosphere rim, stronger on the sunlit limb
                float rim = pow(1.0 - saturate(dot(n, normalize(i.viewWS))), _RimPower);
                col += _RimColor.rgb * rim * _RimStrength * saturate(ndl + 0.35);
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
