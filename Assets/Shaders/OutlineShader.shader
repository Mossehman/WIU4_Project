Shader "Unlit/OutlineShader"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "OutlinePass"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // The Blit.hlsl file provides the vertex shader (Vert),
            // input structure (Attributes) and output strucutre (Varyings)

            #pragma vertex vert
            #pragma fragment frag

            float _scale;
            float _depthThreshold;
            float _normalThreshold;

            float _depthNormalThreshold;
            float _depthNormalThresholdScale;
            
            float4 _outlineColor;

            TEXTURE2D(_CameraTexture);
            TEXTURE2D(_CameraDepthTexture);
            TEXTURE2D(_normalBuffer);
            
            SAMPLER(sampler_CameraDepthTexture);
            SAMPLER(sampler_normalBuffer);
            SAMPLER(sampler_CameraTexture);

            float4 _CameraDepthTexture_TexelSize;

            float4x4 _clipToView;

            struct VertexData{
                float4 position : POSITION;
                half2 uv : TEXCOORD0;
            };

            struct VertToFrag {

                float4 position : SV_POSITION;
                half2 texcoord : TEXCOORD0;
                float3 viewSpaceDir : TEXCOORD1;
            };

            VertToFrag vert(VertexData vd)
            {
                VertToFrag v2f;
                v2f.position = TransformObjectToHClip(vd.position);
                v2f.viewSpaceDir = mul(_clipToView, v2f.position).xyz;
                v2f.texcoord = vd.uv;
                return v2f;
            }


            half4 frag (VertToFrag input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float4 cameraColor = SAMPLE_TEXTURE2D(_CameraTexture, sampler_CameraTexture, input.texcoord);

                float halfScaleFloor = floor(_scale * 0.5f);
                float halfScaleCeil = ceil(_scale * 0.5f);

                float2 bottomLeftUV     = input.texcoord - float2(_CameraDepthTexture_TexelSize.x, _CameraDepthTexture_TexelSize.y) * halfScaleFloor;
                float2 topRightUV       = input.texcoord + float2(_CameraDepthTexture_TexelSize.x, _CameraDepthTexture_TexelSize.y) * halfScaleCeil;
                float2 bottomRightUV    = input.texcoord + float2(_CameraDepthTexture_TexelSize.x * halfScaleCeil, -_CameraDepthTexture_TexelSize.y * halfScaleFloor);
                float2 topLeftUV        = input.texcoord + float2(-_CameraDepthTexture_TexelSize.x * halfScaleFloor, _CameraDepthTexture_TexelSize.y * halfScaleCeil);
                
                float depth0 = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, bottomLeftUV);
                float depth1 = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, topRightUV);
                float depth2 = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, bottomRightUV);
                float depth3 = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, topLeftUV);

                float depth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, input.texcoord);
                float isNotSky = -step(depth, 0) + 1;

                if (isNotSky <= 0) {
                    return cameraColor;
                }

                float depthFiniteDifference0 = depth1 - depth0;
                float depthFiniteDifference1 = depth3 - depth2;
                
                float edgeDepth = sqrt(pow(depthFiniteDifference0, 2) + pow(depthFiniteDifference1, 2)) * 100;

                float3 normal0 = SAMPLE_TEXTURE2D(_normalBuffer, sampler_normalBuffer, bottomLeftUV);
                float3 normal1 = SAMPLE_TEXTURE2D(_normalBuffer, sampler_normalBuffer, topRightUV);
                float3 normal2 = SAMPLE_TEXTURE2D(_normalBuffer, sampler_normalBuffer, bottomRightUV);
                float3 normal3 = SAMPLE_TEXTURE2D(_normalBuffer, sampler_normalBuffer, topLeftUV);

                float3 viewNormal = normal0 * 2 - 1;
                float NdotV = 1 - dot(viewNormal, -input.viewSpaceDir);

                float normalThreshold01 = saturate((NdotV - _depthNormalThreshold)) / (1 - _depthNormalThreshold); 

                float normalThreshold = normalThreshold01 * _depthNormalThresholdScale + 1;
                float depthThreshold = _depthThreshold * depth0 * normalThreshold;

                edgeDepth = edgeDepth > depthThreshold ? 1 : 0;


                float3 normalFiniteDifference0 = normal1 - normal0;
                float3 normalFiniteDifference1 = normal3 - normal2;
                
                float edgeNormal = sqrt(dot(normalFiniteDifference0, normalFiniteDifference0) + dot(normalFiniteDifference1, normalFiniteDifference1));
                edgeNormal = edgeNormal > _normalThreshold ? 1 : 0;
                
 
                
                float edge = max(edgeDepth, edgeNormal);

                if (edge <= 0)
                {
                    return cameraColor;
                }
                else
                {
                    return _outlineColor;
                }   

                //float isNotSky = -step(depth, 0) + 1;
                //return float4(depth, depth, depth, 1.0);
            }
            ENDHLSL
        }
    }
}
