#if !defined(LOOKING_THROUGH_WATER_INCLUDED)
#define LOOKING_THROUGH_WATER_INCLUDED
//T his section is based off of the Looking Through Water tutorial from Catlike Coding
//This is used to determine the depth from which the fog should begin to start and then
//fades out as it gets closer to the surface of the water.
sampler2D _CameraDepthTexture, _WaterBackground;
float4 _CameraDepthTexture_TexelSize;

float3 _WaterFogColor;
float _WaterFogDensity;

float _RefractionStrength;

float2 AlignWithGrabTexel(float2 uv) {
#if UNITY_UV_STARTS_AT_TOP
	if (_CameraDepthTexture_TexelSize.y < 0) {
		uv.y = 1 - uv.y;
	}
#endif

	return
		(floor(uv * _CameraDepthTexture_TexelSize.zw) + 0.5) *
		abs(_CameraDepthTexture_TexelSize.xy);
}
//Determines the colour of the fog below the water plane/surface but also how dark the colour
//becomes as the light travels into the water, similarly to an actual body of water.
float3 ColorBelowWater(float4 screenPos, float3 tangentSpaceNormal) {
	float2 uvOffset = tangentSpaceNormal.xy * _RefractionStrength;
	uvOffset.y *= _CameraDepthTexture_TexelSize.z * abs(_CameraDepthTexture_TexelSize.y);
	float2 uv = AlignWithGrabTexel((screenPos.xy + uvOffset) / screenPos.w);

	//This section below is using the camera's position to determine how deep the camera should be able to see
	//and then adjusting the depth of the fog accordingly
	float backgroundDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
	float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(screenPos.z);
	float depthDifference = backgroundDepth - surfaceDepth;

	//Applies the colour changes according to the depth diff calculated above
	uvOffset *= saturate(depthDifference);
	uv = AlignWithGrabTexel((screenPos.xy + uvOffset) / screenPos.w);
	backgroundDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
	depthDifference = backgroundDepth - surfaceDepth;

	float3 backgroundColor = tex2D(_WaterBackground, uv).rgb;
	float fogFactor = exp2(-_WaterFogDensity * depthDifference);
	return lerp(_WaterFogColor, backgroundColor, fogFactor);
}

#endif