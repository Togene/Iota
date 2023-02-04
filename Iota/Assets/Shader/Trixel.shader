Shader "Unlit/Trixel"
{
    Properties
    {
        _MainTexX ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

           #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                half3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                half3 worldNormal : NORMAL;
            };

            sampler2D _MainTexX;
            float4 _MainTexX_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                o.uv = TRANSFORM_TEX(v.uv, _MainTexX);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i, fixed facing : VFACE) : SV_Target {
                // sample the texture

                float uvY = i.uv.y*3;

                float4 color = float4(1,1,1,0);
                if (uvY < 1) {
                    color = tex2D(_MainTexX, float2(i.uv.x/3, uvY));
                } else if (uvY > 1 && uvY < 2) {
                    color = tex2D(_MainTexX, float2(0, -1) + float2(i.uv.x/3, uvY));
                } else {
                    color =  tex2D(_MainTexX, float2(0, -2) + float2(i.uv.x/3, uvY));;
                }

                half3 worldNormal = normalize(i.worldNormal * facing);
                
                fixed ndotl = saturate(dot(worldNormal, normalize(_WorldSpaceLightPos0.xyz)));
                fixed3 lighting = ndotl * _LightColor0;
                lighting += ShadeSH9(half4(worldNormal, 1.0));
                
                UNITY_APPLY_FOG(i.fogCoord, col);
                return color * float4(lighting.rgb, 1.0);
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}
