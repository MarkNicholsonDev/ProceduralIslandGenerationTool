/// <summary>
/// This code below is for a custom water shader which creates a realistic looking water surface,
/// this was created through several tutorials merged into one shader from Catlike Coding including:
/// - Texture distortion for the initial reflective surface
/// - Directional flow for adding movement to the surface (An improvement in the future is for synchronised directional flow)
/// - Looking through water to make the water transparent rather than opaque
/// Link to Catlike coding: https://catlikecoding.com/unity/tutorials/flow/
/// 
/// To help understand the custom shader syntax and language I started by looking into this introduction:
/// Link to Ray Wenderlich: https://www.raywenderlich.com/5671826-introduction-to-shaders-in-unity
/// This was used for understanding the structure and syntax of custom shaders such as the Cg language
/// used for CGPROGRAM but also what all the different functions were actually for.
/// (This was used to help explain the shader code in these comments)
/// </summary>
Shader "Custom/Water" {
	// The properties section defines all the maps and textures to be passed into the shader
	Properties{
		//Basic texture settings for the water plane
		_Color("Color", Color) = (1,1,1,1)
		[NoScaleOffset] _MainTex("Deriv (AG) Height (B)", 2D) = "black" {}
		[NoScaleOffset] _FlowMap("Flow (RG)", 2D) = "black" {}
		
		//Used for changing appearance of water by adding additional copies of itself across the plane
		// - can result in better looking water and reduces stretching of the texture
		[Toggle(_DUAL_GRID)] _DualGrid("Dual Grid", Int) = 0
		_Tiling("Tiling, Constant", Float) = 1
		_TilingModulated("Tiling, Modulated", Float) = 1
		_GridResolution("Grid Resolution", Float) = 10
		
		//Used for adding simulated flow to the plane of water
		_Speed("Speed", Float) = 1
		_FlowStrength("Flow Strength", Float) = 1
		_HeightScale("Height Scale, Constant", Float) = 0.25
		_HeightScaleModulated("Height Scale, Modulated", Float) = 0.75

		//Used for adding a fog to simulate light going into the water and dissapating out
		_WaterFogColor("Water Fog Color", Color) = (0, 0, 0, 0)
		_WaterFogDensity("Water Fog Density", Range(0, 2)) = 0.1

		//Used for basic reflective water property
		_RefractionStrength("Refraction Strength", Range(0, 1)) = 0.25
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
		//Start of the custom shader
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			GrabPass { "_WaterBackground" }
			
			//CGPROGRAM declares that the following code is using the Cg language, this is somewhat
			//similar to using in-line CSS code in a HTML file and it defines basic shader configurations
			CGPROGRAM
			#pragma surface surf Standard alpha finalcolor:ResetAlpha
			#pragma target 3.0

			#pragma shader_feature _DUAL_GRID

			#include "flow.cginc"
			#include "TransparentWater.cginc"
			// Definition of basic opaque water plane properties
			sampler2D _MainTex, _FlowMap;
			float _Tiling, _TilingModulated, _GridResolution, _Speed, _FlowStrength;
			float _HeightScale, _HeightScaleModulated;

			struct Input {
				float2 uv_MainTex;
				//ScreenPos required for determining the depth at which the camera can see
				float4 screenPos;
			};

			//Definition of standard plane properties
			half _Glossiness;
			half _Metallic;
			fixed4 _Color;

			//Unpacking of the height map
			float3 UnpackDerivativeHeight(float4 textureData) {
				float3 dh = textureData.agb;
				dh.xy = dh.xy * 2 - 1;
				return dh;
			}

			//This method is for dividing up of the plane into grid sections which can be manipulated
			//individually by the shader adding differerent directions etc
			float3 FlowCell(float2 uv, float2 offset, float time, float gridB) {
				float2 shift = 1 - offset;
				shift *= 0;
				offset *= 0.5;
				if (gridB) {
					offset += 0.25;
					shift -= 0.25;
				}
				//Below is applying the standard data of the water shader onto the cells within the flow grid
				float2x2 derivRotation;
				float2 uvTiled =
					(floor(uv * _GridResolution + offset) + shift) / _GridResolution;
				float3 flow = tex2D(_FlowMap, uvTiled).rgb;
				flow.xy = flow.xy * 2 - 1;
				flow.z *= _FlowStrength;
				float tiling = flow.z * _TilingModulated + _Tiling;
				float2 uvFlow = DirectionalFlowUV(
					uv + float2(0, 1), flow, tiling, time,
					derivRotation
				);
				float3 dh = UnpackDerivativeHeight(tex2D(_MainTex, uvFlow));
				dh.xy = mul(derivRotation, dh.xy);
				dh *= flow.z * _HeightScaleModulated + _HeightScale;
				return dh;
			}

			//This combines the flow cells to create a grid which can then manipulate the
			//cells to flow in specific directions - Although the flow in mine is currently not
			//synchronised across the whole grid/tileset.
			float3 FlowGrid(float2 uv, float time, bool gridB) {
				float3 dhA = FlowCell(uv, float2(0, 0), time, gridB);
				float3 dhB = FlowCell(uv, float2(1, 0), time, gridB);
				float3 dhC = FlowCell(uv, float2(0, 1), time, gridB);
				float3 dhD = FlowCell(uv, float2(1, 1), time, gridB);

				float2 t = uv * _GridResolution;
				if (gridB) {
					t += 0.25;
				}
				t = abs(2 * frac(t) - 1);
				float wA = (1 - t.x) * (1 - t.y);
				float wB = t.x * (1 - t.y);
				float wC = (1 - t.x) * t.y;
				float wD = t.x * t.y;

				return dhA * wA + dhB * wB + dhC * wC + dhD * wD;
			}

			//surf is the main shader function which all the other functions above are ran through,
			//here the shader properties are set but also takes in all of the input so far.
			void surf(Input IN, inout SurfaceOutputStandard o) {
				float time = _Time.y * _Speed;
				float2 uv = IN.uv_MainTex;
				float3 dh = FlowGrid(uv, time, false);
				#if defined(_DUAL_GRID)
					dh = (dh + FlowGrid(uv, time, true)) * 0.5;
				#endif
				fixed4 c = dh.z * dh.z * _Color;
				c.a = _Color.a;
				o.Albedo = c.rgb;
				o.Normal = normalize(float3(-dh.xy, 1));
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = c.a;

				o.Emission = ColorBelowWater(IN.screenPos, o.Normal) * (1 - c.a);
			}

			//This resets the final alpha value so that the water becomes transparent and the fog below is visible
			void ResetAlpha(Input IN, SurfaceOutputStandard o, inout fixed4 color) {
				color.a = 1;
			}

			ENDCG
		}
}
