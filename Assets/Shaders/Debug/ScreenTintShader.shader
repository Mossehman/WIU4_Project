Shader "Unlit/ScreenTintShader"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "ColorBlitPass"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex vert
            #pragma fragment frag



            float4 _Color;
            TEXTURE2D_X(_CameraTexture);
            SAMPLER(sampler_CameraTexture);

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
                float4 color = SAMPLE_TEXTURE2D_X(_CameraTexture, sampler_CameraTexture, input.uv);
                return color * _Color;
            }
            ENDHLSL
        }
    }
}
