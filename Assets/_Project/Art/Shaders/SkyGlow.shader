// Camera-facing additive glow (sun disc and halo) for P24. Billboarding happens in the vertex shader,
// so no script is needed; the quad's X scale sets the size. No fog.
Shader "GameOff/SkyGlow"
{
    Properties
    {
        _MainTex ("Glow (alpha)", 2D) = "white" {}
        _Color ("Color", Color) = (1, 0.93, 0.8, 1)
        _Intensity ("Intensity", Float) = 2
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent-100" "IgnoreProjector" = "True" }
        Blend SrcAlpha One
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
                float4 _MainTex_ST; half4 _Color; float _Intensity;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings vert (Attributes v)
            {
                Varyings o;
                float3 centerWS = TransformObjectToWorld(float3(0, 0, 0));
                float size = length(TransformObjectToWorldDir(float3(1, 0, 0), false));
                float3 centerVS = TransformWorldToView(centerWS);
                float3 posVS = centerVS + float3(v.positionOS.xy * size, 0);
                o.positionCS = TransformWViewToHClip(posVS);
                o.uv = v.uv;
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                half4 t = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                // Fade to zero toward the quad border so the halo never shows a square edge.
                float edge = smoothstep(1.0, 0.75, length(i.uv * 2.0 - 1.0));
                return half4(t.rgb * _Color.rgb * _Intensity, t.a * _Color.a * edge);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
