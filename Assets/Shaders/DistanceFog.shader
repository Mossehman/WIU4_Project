Shader "Unlit/DistanceFog"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "DistanceFog"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // The Blit.hlsl file provides the vertex shader (Vert),
            // input structure (Attributes) and output strucutre (Varyings)

            #pragma vertex vert
            #pragma fragment frag



            float4 _fogColor;
            TEXTURE2D(_CameraTexture);
            SAMPLER(sampler_CameraTexture);

            TEXTURE2D(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

            float _fogMinDepth;
            float _fogMaxDepth;
            float _skyboxBlend;

            struct VertexData{
                float4 position : POSITION;
                half2 uv : TEXCOORD0;
            };

            struct VertToFrag {

                float4 position : SV_POSITION;
                half2 uv : TEXCOORD0;
            };

            VertToFrag vert(VertexData vd)
            {
                VertToFrag v2f;
                v2f.position = TransformObjectToHClip(vd.position);
                v2f.uv = vd.uv;
                return v2f;
            }

            half4 frag (VertToFrag input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float4 camColor = SAMPLE_TEXTURE2D(_CameraTexture, sampler_CameraTexture, input.uv);
                float depth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, input.uv);
                float isNotSky = -step(depth, 0) + 1;
                float skyboxFactor = (1 - isNotSky) * _skyboxBlend;

                if (_fogMaxDepth >= _fogMinDepth || depth > _fogMinDepth)
                {
                    return camColor;
                }

                float t = (_fogMinDepth - depth) / (_fogMinDepth - _fogMaxDepth);

                if (isNotSky > 0)
                {
                    return lerp(camColor, _fogColor, t);
                }
                else
                {
                    return lerp(camColor, _fogColor, t * skyboxFactor);
                }

            }
            ENDHLSL
        }
    }
}
