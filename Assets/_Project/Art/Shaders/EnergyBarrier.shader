// Hangar energy barrier (P23): translucent blue field with rising scanlines, shimmer, glowing edges and a pulse.
// Unlit, transparent, double-sided. URP, works on WebGL 2. Driven by GameOff.Core.EnergyBarrier (_Pulse).
Shader "GameOff/EnergyBarrier"
{
    Properties
    {
        _Color ("Field Color", Color) = (0.15, 0.45, 1.0, 0.25)
        _LineColor ("Scanline Color", Color) = (0.45, 0.8, 1.0, 0.6)
        _EdgeColor ("Edge Glow Color", Color) = (0.6, 0.9, 1.0, 0.9)
        _LineDensity ("Scanlines per Meter", Float) = 1.5
        _LineSpeed ("Scanline Speed (m/s)", Float) = 0.6
        _LineSharpness ("Scanline Sharpness", Range(1, 64)) = 18
        _ShimmerScale ("Shimmer Cells per Meter", Float) = 1.2
        _ShimmerSpeed ("Shimmer Speed", Float) = 1.3
        _EdgeWidth ("Edge Width (m)", Float) = 0.6
        _Size ("Field Size (m)", Vector) = (28, 12, 0, 0)
        _Pulse ("Pulse 0..1 (set by script)", Range(0, 1)) = 0
        _Intensity ("Intensity", Float) = 3.5
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True" }
        Blend SrcAlpha One   // additive-ish glow, keeps the stars behind visible
        ZWrite Off
        Cull Off

        Pass
        {
            Name "Unlit"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color, _LineColor, _EdgeColor;
                float _LineDensity, _LineSpeed, _LineSharpness, _ShimmerScale, _ShimmerSpeed, _EdgeWidth, _Pulse, _Intensity;
                float4 _Size;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float fog : TEXCOORD1; };

            Varyings vert (Attributes v)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = p.positionCS;
                o.uv = v.uv;
                o.fog = ComputeFogFactor(p.positionCS.z);
                return o;
            }

            float Hash(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }

            float ValueNoise(float2 p)
            {
                float2 i = floor(p), f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(Hash(i), Hash(i + float2(1, 0)), u.x), lerp(Hash(i + float2(0, 1)), Hash(i + float2(1, 1)), u.x), u.y);
            }

            half4 frag (Varyings i) : SV_Target
            {
                float2 m = i.uv * _Size.xy;                       // position on the field in meters
                float t = _Time.y;

                // Rising scanlines
                float lines = pow(0.5 + 0.5 * sin((m.y - t * _LineSpeed) * _LineDensity * 6.2831853), _LineSharpness);
                // Slow shimmer cells
                float shimmer = ValueNoise(m * _ShimmerScale + float2(t * 0.3, t * _ShimmerSpeed));
                // Glow toward the four edges (emitters)
                float2 d = min(m, _Size.xy - m);
                float edge = saturate(1.0 - min(d.x, d.y) / max(_EdgeWidth, 0.001));
                edge *= edge;

                half4 col = _Color * (0.6 + 0.8 * shimmer);
                col += _LineColor * lines * (0.5 + 0.5 * shimmer);
                col += _EdgeColor * edge;
                // Pulse: brighter, denser flicker when something passes through
                float flicker = 0.5 + 0.5 * sin(t * 60.0 + m.x * 3.0);
                col += _LineColor * _Pulse * (0.6 + 0.4 * flicker);
                col.rgb *= _Intensity;
                col.a = saturate(col.a);
                col.rgb = MixFog(col.rgb, i.fog);
                return col;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
