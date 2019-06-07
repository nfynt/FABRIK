﻿// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
// Credits To - Allan Zucconi for comprehensive tutorial at: http://www.alanzucconi.com/2016/01/27/arrays-shaders-heatmaps-in-unity3d/#more-2003

Shader "NFYNT/Heatmap" {
	Properties {
		_HeatTex ("Texture", 2D) = "white" {}
	}
	SubShader {
		Tags {"Queue"="Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha // Alpha blend

		Pass {
			CGPROGRAM
			#pragma vertex vert             
			#pragma fragment frag

			struct vertInput {
				float4 pos : POSITION;
			};  

			struct vertOutput {
				float4 pos : POSITION;
				fixed3 worldPos : TEXCOORD1;
			};

			vertOutput vert(vertInput input) {
				vertOutput o;
				o.pos = UnityObjectToClipPos(input.pos);
				o.worldPos = mul(unity_ObjectToWorld, input.pos).xyz;
				return o;
			}

			uniform int _Points_Length = 0;
			uniform float4  _Points [1000];		// (x, y, z) = position
			uniform float4  _Properties [1000];	// x = radius, y = intensity
			
			sampler2D _HeatTex;

			half4 frag(vertOutput output) : COLOR {
				// Loops over all the points
				float h = 0;
				for (int i = 0; i < _Points_Length; i++)
				{
					// Calculates the contribution of each point
					float di = distance(output.worldPos, _Points[i].xyz);

					float ri = _Properties[i].x;
					float hi = 1 - saturate(di / ri);

					h += hi * _Properties[i].y;
				}

				// Converts (0-1) according to the heat texture
				h = saturate(h);
				half4 color = tex2D(_HeatTex, fixed2(h, 0.5));
				return color;
			}
			ENDCG
		}
	} 
	Fallback "Diffuse"
}