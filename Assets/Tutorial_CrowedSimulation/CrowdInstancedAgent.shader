Shader "Crowd/InstancedAgent" {
    SubShader {
        Tags { "RenderPipeline"="UniversalRenderPipeline" "RenderType"="Opaque" }
        
        Pass {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct CrowdAgent {
                float2 position;
                float2 velocity;
                float2 target;
                float maxSpeed;
            };
            
            StructuredBuffer<CrowdAgent> agents;
            
            struct Attributes {
                float3 positionOS : POSITION;
                uint instanceID : SV_InstanceID;
            };
            
            struct Varyings {
                float4 positionCS : SV_POSITION;
            };
            
            Varyings vert(Attributes v) {
                CrowdAgent agent = agents[v.instanceID];
                
                float3 positionOS = v.positionOS;
                positionOS.x += agent.position.x;
                positionOS.y += agent.position.y;
                
                Varyings o;
                o.positionCS = TransformObjectToHClip(positionOS);
                return o;
            }
            
            half4 frag() : SV_Target { 
                return half4(1,1,1,1);
            }
            ENDHLSL
        }
    }
}