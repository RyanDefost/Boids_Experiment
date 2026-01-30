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
                float2 gridPosition;
                float4 teamColor;
                float maxSpeed;
            };
            
            StructuredBuffer<CrowdAgent> agents;
            
            struct Attributes {
                float3 positionOS : POSITION;
                uint instanceID : SV_InstanceID;
            };
            
            struct Varyings {
                float4 positionCS : SV_POSITION;
                float4 teamColor : TEXCOORD0;
            };
            
            Varyings vert(Attributes v) {
                CrowdAgent agent = agents[v.instanceID];

                float3 positionOS = v.positionOS;
                positionOS.x += agent.position.x;
                positionOS.y += agent.position.y;
                
                Varyings o;
                o.positionCS = TransformObjectToHClip(positionOS);
                o.teamColor = agent.teamColor;
                return o;
            }
            
            half4 frag(Varyings i) : SV_Target { 
                
                half3 color = half3(i.teamColor.x, i.teamColor.y, i.teamColor.z);
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}