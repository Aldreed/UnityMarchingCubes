Shader "RG2" {
    Properties {
	_AmbientStrength ("Ambient" , Float) = 0.5
	_SpecularStrength ("Specular", Float) = 0.5
    }
    SubShader {
        Tags {
            "LightMode" = "ForwardBase"
        }
        LOD 100

        Pass {
            HLSLPROGRAM
            #pragma vertex vertexShader
            #pragma fragment fragmentShader

            #include "UnityCG.cginc"
	    #include "UnityLightingCommon.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
		fixed4 colr : COLOR;
            };

            struct vertexToFragment {
                float3 normal : NORMAL;
                float4 vertex : SV_POSITION;
                float3 lightDirection : TEXCOORD1;
                float3 viewDirection : TEXCOORD2;
		fixed4 colr : COLOR;
            };

	    float _AmbientStrength;
	    float _SpecularStrength;            

            vertexToFragment vertexShader ( appdata data ) {
                vertexToFragment output;
                
                output.vertex = UnityObjectToClipPos ( data.vertex );
                output.normal = UnityObjectToWorldNormal ( data.normal );

                float3 worldPosition = mul ( unity_ObjectToWorld, data.vertex ).xyz;
                
                output.lightDirection = normalize ( UnityWorldSpaceLightDir ( worldPosition ) );
                output.viewDirection  = normalize ( _WorldSpaceCameraPos.xyz - worldPosition.xyz );
                
		output.colr = data.colr;
                
                return output;
            }

            fixed4 fragmentShader ( vertexToFragment data ) : SV_Target {
		fixed4 color = data.colr;
		float3 ambient = _AmbientStrength * _LightColor0; 

                float3 normal = normalize ( data.normal );
                float3 lightDirection = normalize ( data.lightDirection );
                float  diff = max ( 0, dot ( normal, lightDirection ) );
		float3 diffuse = diff * _LightColor0;

                float3 reflection = reflect ( -lightDirection, normal );
                float  spec = pow ( max ( 0, dot ( normalize ( data.viewDirection ), reflection ) ), 25 );
		float3 specular = _SpecularStrength * spec * _LightColor0;

                // return fixed4 ( color.rgb * diff + fixed3 ( specular, specular, specular ), 1.0 );
		float3 temp = ambient + diffuse + specular;
		fixed4 res = fixed4(fixed3(temp.x,temp.y,temp.z),1.0) * color;
		return res;
            }
            ENDHLSL
        }
    }
}