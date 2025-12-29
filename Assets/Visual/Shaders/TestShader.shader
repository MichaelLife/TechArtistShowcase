Shader "Custom/TestShader"
{
Properties
{
    _SandColor("Sand Color", Color) = (1, 1, 1, 1)
    _SandTexture("Sand Texture", 2D) = "white" {}

    _GrassColor("Grass Color", Color) = (1, 1, 1, 1)
    _GrassTexture("Grass Texture", 2D) = "white"{}
}

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                half3 normal : NORMAL;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float3 positionWS : TEXCOORD2;
                half3 normal : TEXCOORD0;
                half4 color : COLOR;
            };

            TEXTURE2D(_SandTexture);
            TEXTURE2D(_GrassTexture);
            half4 _SandColor;
            half4 _GrassColor;
            SAMPLER(sampler_SandTexture);
            SAMPLER(sampler_GrassTexture);
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);

                // Get the position of the vertex in different spaces
                VertexPositionInputs positions = GetVertexPositionInputs(IN.positionOS);

                // Set positionWS to the world space position of the vertex
                OUT.positionWS = positions.positionWS.xyz;
                OUT.normal = TransformObjectToWorldNormal(IN.normal);  
                OUT.color = IN.color;
                return OUT;
            }

            half4 TriplanarProjection(in Texture2D _Texture, sampler _Sampler, in half4 _Color, in float3 _Blends, in float3 _Pos)
            {   
                float3 projX = SAMPLE_TEXTURE2D(_Texture, _Sampler, _Pos.yz) * _Blends.x;
                float3 projY = SAMPLE_TEXTURE2D(_Texture, _Sampler, _Pos.xz) * _Blends.y;
                float3 projZ = SAMPLE_TEXTURE2D(_Texture, _Sampler, _Pos.xy) * _Blends.z;

                half4 color = 0;
                color.rgb = projX + projY + projZ;
                color *= _Color;

                return color;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 worldPos = IN.positionWS;
                float3 normal = IN.normal;

                float3 blends = abs(normal);
                blends /= blends.x + blends.y + blends.z;

                half4 sandColor = 0;
                sandColor = TriplanarProjection(_SandTexture, sampler_SandTexture, _SandColor, blends, worldPos);
                half4 grassColor = 0;
                grassColor = TriplanarProjection(_GrassTexture, sampler_GrassTexture, _GrassColor, blends, worldPos);

                half4 color = -IN.color;
                
                return color;
            }
            ENDHLSL
        }
    }
}
