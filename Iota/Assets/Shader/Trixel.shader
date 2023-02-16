// thank https://roystan.net/articles/toon-shader/
Shader "Unlit/Trixel" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _RampTexture ("Texture", 2D) = "white" {}
    }
    SubShader {
        Pass {
            Tags {"LightMode" = "ForwardBase" "PassFlags" = "OnlyDirectional"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            // make fog work
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

                       
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                half3 normal : NORMAL;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                half3 viewDir : TEXCOORD1;
                float4 pos : SV_POSITION;
                half3 worldNormal : NORMAL;
                SHADOW_COORDS(2)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _RampTexture;
            float4 _RampTexture_ST;
            
            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = WorldSpaceViewDir(v.vertex);
                
                // UNITY_TRANSFER_FOG(o,o.vertex);
                TRANSFER_SHADOW(o) 
                return o;
            }

            float4 TexSliceStep(in v2f i, int atlasSize) {
                float4 color;
                float uvY = i.uv.y*atlasSize;
                if (uvY < 1) {
                    color = tex2D(_MainTex, float2(i.uv.x/atlasSize, uvY));
                } else if (uvY > 1 && uvY < 2) {
                    color = tex2D(_MainTex, float2(1.0/atlasSize, -1) + float2(i.uv.x/atlasSize, uvY));
                } else {
                    color = tex2D(_MainTex, float2((1.0/atlasSize)* 2, -2) + float2(i.uv.x/atlasSize, uvY));;
                }
                if (color.a <= 0){
                    discard;
                }
                return color;
            }
            
            float4 frag (v2f i) : SV_Target {
                // smoothstep(0, 0.01, ndotl);
                // UNITY_APPLY_FOG(i.fogCoord, col);
                float3 viewDir = normalize(i.viewDir);
                float3 normal = normalize(i.worldNormal);
                float spec = smoothstep(
                    0.005, 0.01, pow(dot(normalize(i.worldNormal), normalize(_WorldSpaceLightPos0 + viewDir)) * .99, 445));

             
                float nDotL = tex2D(_RampTexture, float2(
                    1 - (saturate(dot(normal,
                         normalize(_WorldSpaceLightPos0.xyz))) * 0.49 + 0.49), 0.5));
                float shadow = SHADOW_ATTENUATION(i);
                float light = smoothstep(0, 1, nDotL * shadow);
                // 
                
                float rimAmount = .9;
                float rim =smoothstep(
                    rimAmount- 0.01, rimAmount +.01 , 1 - dot(viewDir, normalize(i.worldNormal))) * pow(nDotL, 100);
                
                return TexSliceStep(i, 3) * light + spec + rim;
            }
            ENDCG
        }
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
