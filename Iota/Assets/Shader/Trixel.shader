Shader "Unlit/Trixel"
{
    Properties
    {
        _MainTexX ("Texture", 2D) = "white" {}
        _MainTexY ("Texture", 2D) = "white" {}
        _MainTexZ ("Texture", 2D) = "white" {}
         
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTexX;
            float4 _MainTexX_ST;

            sampler2D _MainTexY;
            float4 _MainTexY_ST;

            sampler2D _MainTexZ;
            float4 _MainTexZ_ST;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                o.uv = TRANSFORM_TEX(v.uv, _MainTexX);
                o.uv1 = TRANSFORM_TEX(v.uv, _MainTexY);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // sample the texture

                float uvY = i.uv.y*3;

                float4 color = float4(1,1,1,0);

                if (uvY < 1) {
                    color = tex2D(_MainTexX, float2(i.uv.x, uvY));
                } else if (uvY > 1 && uvY < 2) {
                    color = tex2D(_MainTexY, float2(0, -1) + float2(i.uv.x, uvY));
                } else {
                    color =  tex2D(_MainTexZ, float2(0, -2) + float2(i.uv.x, uvY));;
                }
                UNITY_APPLY_FOG(i.fogCoord, col);
                return color;
            }
            ENDCG
        }
    }
}
