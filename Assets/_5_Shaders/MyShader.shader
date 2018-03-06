Shader "Custom/MyShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
    }
    SubShader
    {
       // Render the mask after regular geometry, but before masked geometry and
        // transparent things.
        Tags {"Queue" = "Geometry-10" }
       
        // Turn off lighting, because it's expensive and the thing is supposed to be
        // invisible anyway.
        Lighting Off

        // Draw into the depth buffer in the usual way.  This is probably the default,
        // but it doesn't hurt to be explicit.
        ZTest LEqual
        ZWrite On

        // Don't draw anything into the RGBA channels. This is an undocumented
        // argument to ColorMask which lets us avoid writing to anything except
        // the depth buffer.

        ColorMask 0

        // Do nothing specific in the pass:

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // For TRANSFORM_TEXT	
            #include "UnityCG.cginc"

            float4 _Color;
            // Main texture
            sampler2D _MainTex;
            float4 _MainTex_ST;

            // VertexInput
            struct VertIn {
                float4 position : POSITION;
                float4 color : COLOR;
            };

            // Vertex Output
            struct VertOut {
                float4 position : POSITION;
                float4 color : COLOR;
            };

            VertOut vert (VertIn i) {
                VertOut o;
                // Transforms a point from object space to the camera’s clip space in homogeneous coordinates. This is 
                // the equivalent of mul(UNITY_MATRIX_MVP, float4(pos, 1.0)), and should be used in its place.
                o.position = UnityObjectToClipPos(i.position);
                o.color = i.color;
                return o;
            }

            struct FragOut {
            	float4 color : COLOR;
            };

            float4 frag (VertOut i) : SV_Target {
            	float4 c = tex2D(_MainTex, i.color);
                if ((c.r * c.g * c.b) == 0) discard;            //Most IMPORTANT working Code
                c.rgb *= c.a;
	            return c;
            }
            ENDCG
        }
    }
}
// https://docs.unity3d.com/Manual/ShaderTut2.html
//The vertex and fragment programs here don’t do anything fancy; vertex program uses the TRANSFORM_TEX macro from UnityCG.cginc
// to make sure texture scale and offset is applied correctly, and fragment program just samples the texture and multiplies by the color property.