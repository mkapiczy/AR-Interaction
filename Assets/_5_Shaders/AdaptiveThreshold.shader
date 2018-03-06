Shader "Custom/AdaptiveThreshold" {
Properties{
        
    }

    SubShader {
    	Pass {
    	CGPROGRAM
    		#pragma vertex vertexShader
    		#pragma fragment fragmentShader

    		#include "UnityCG.cginc"

    		struct appData {
    			float3 position: POSITION;
    			float3 normal : NORMAL;
    		};

    		struct vertexToFragment {
    			float4 position: SV_POSITION;
    			float3 normal : NORMAL;
    			float3 worldPosition : TEXCOORD0;
    		};

    		vertexToFragment vertexShader(appData IN) {
    			vertexToFragment OUT;
    			OUT.position = UnityObjectToClipPos(IN.position);
    			OUT.normal = UnityObjectToWorldNormal(IN.normal);
    			OUT.worldPosition = mul(UNITY_MATRIX_M, IN.position); // from object coordinates to world coordinates
    			return OUT;
    		}

    		float4 fragmentShader(vertexToFragment IN) : SV_TARGET {
    			float3 cameraPos = _WorldSpaceCameraPos;
    			float3 direction = normalize(cameraPos - IN.worldPosition);
    			float dotProduct = dot(direction, IN.normal);
    			float result;
    			if(dotProduct < 0.4) result = 0;
    			else result = 1;
    			return float4(result, result, result, 1);
    		}

		ENDCG
    	}
	}
} 


