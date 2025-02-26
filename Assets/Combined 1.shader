Shader "Custom/Combined 1"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _Blend ("B&W Intensity", Range(0, 1)) = 1.0
        _GrainIntensity ("Grain Intensity", Range(0, 1)) = 0.5
        _GrainSpeed ("Grain Speed", Range(0, 10)) = 1.0
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.5
        _ScanlineSpeed ("Scanline Speed", Range(0, 10)) = 2.0
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Tags { "RenderPipeline" = "UniversalPipeline" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        sampler2D _MainTex;
        float4 _MainTex_TexelSize;
        float _Blend;
        float _GrainIntensity;
        float _GrainSpeed;
        float _ScanlineIntensity;
        float _ScanlineSpeed;

        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
            float2 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
        };

        v2f vert(appdata v)
        {
            v2f o;
            o.vertex = TransformObjectToHClip(v.vertex.xyz);
            o.uv = v.uv;
            return o;
        }

        float randomNoise(float2 uv, float time)
        {
            return frac(sin(dot(uv.xy + time, float2(12.9898, 78.233))) * 43758.5453);
        }

        float scanlineEffect(float2 uv, float time)
        {
            float scanline = sin(uv.y * 600.0 + time * _ScanlineSpeed) * 0.5 + 0.5;
            return lerp(1.0, scanline, _ScanlineIntensity);
        }
        ENDHLSL

        Pass
        {
            Name "Black & White"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag_bw

            float4 frag_bw(v2f i) : SV_Target
            {
                float4 color = tex2D(_MainTex, i.uv);
                float grayscale = dot(color.rgb, float3(0.2126, 0.7152, 0.0722));
                float3 finalColor = lerp(color.rgb, grayscale.xxx, _Blend);

                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Film Grain"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag_grain

            float4 frag_grain(v2f i) : SV_Target
            {
                float4 color = tex2D(_MainTex, i.uv);
                float time = _Time.y * _GrainSpeed;
                float noise = randomNoise(i.uv * _MainTex_TexelSize.xy * 100.0, time);
                float3 grainColor = color.rgb + (noise - 0.5) * _GrainIntensity;

                return float4(grainColor, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "CRT Scan Lines"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag_crt

            float4 frag_crt(v2f i) : SV_Target
            {
                float4 color = tex2D(_MainTex, i.uv);
                float time = _Time.y;
                float scanline = scanlineEffect(i.uv, time);

                return float4(color.rgb * scanline, 1.0);
            }
            ENDHLSL
        }
    }
}