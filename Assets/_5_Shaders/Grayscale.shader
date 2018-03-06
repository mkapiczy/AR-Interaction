
 Shader "Custom/GreyScale" {
	Properties {
		_MainTex ("Texture", 2D) = "white" { } // The texture for the noise
		_Factor1 ("Factor 1", float) = 1 // Factors for noise, used in pseudorandom noise generation
        _Factor2 ("Factor 2", float) = 1
        _Factor3 ("Factor 3", float) = 1
	}
	SubShader {
		Pass {
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			// main texture for the noise
			sampler2D _MainTex;

			struct v2f {
				float4  pos : SV_POSITION; // Vertex position
				float2  uv : TEXCOORD0; // The first uv coordinate
			};

			struct Input {
				float3 worldPos;
			};

			float _Factor1;
            float _Factor2;
            float _Factor3;
 
            float noise(half2 uv)
            {
				// Pseudorandom noise
                return frac(sin(dot(uv, float2(_Factor1, _Factor2))) * _Factor3);
            }
			
			float4 _MainTex_ST;
      		v2f vert (appdata_base v) {
        		v2f o;
				// Convert to camera position
        		o.pos = UnityObjectToClipPos (v.vertex);
        		o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
				return o;
      		}

			half4 frag (v2f i) : COLOR {
        		half4 texcol = tex2D (_MainTex, i.uv);
				fixed4 col = noise(i.uv);
				// Multiply the noise with the texture
        		texcol.rgb = dot(dot(texcol.rgb,col.rgb), float3(0.3, 0.59, 0.11));
        		return texcol;
      		}

			ENDCG  
		}
	}
	Fallback "VertexLit"
} 


