#if !defined(FLOW_INCLUDED)
#define FLOW_INCLUDED

//This function takes the UV map provided and then allows for a scrolling effect to be applied
//onto it which gives the illusion of flow on the water surface
float3 FlowUVW(
	float2 uv, float2 flowVector, float2 jump,
	float flowOffset, float tiling, float time, bool flowB
) {
	float phaseOffset = flowB ? 0.5 : 0;
	float progress = frac(time + phaseOffset);
	float3 uvw;
	uvw.xy = uv - flowVector * (progress + flowOffset);
	uvw.xy *= tiling;
	uvw.xy += phaseOffset;
	uvw.xy += (time - progress) * jump;
	uvw.z = 1 - abs(1 - 2 * progress);
	return uvw;
}

//This function is responisble for creating flow across flow cells within the flow grid used to generate a water surface
float2 DirectionalFlowUV(
	float2 uv, float3 flowVectorAndSpeed, float tiling, float time, out float2x2 rotation
) {
	//This works by taking in x and y coordinates and then the tiling coordinates
	//and then the speeed into a float2 uv which then is multiplied by the time to sync it up
	//with the simulated time within Unity
	float2 dir = normalize(flowVectorAndSpeed.xy);
	rotation = float2x2(dir.y, dir.x, -dir.x, dir.y);
	uv = mul(float2x2(dir.y, -dir.x, dir.x, dir.y), uv);
	uv.y -= time * flowVectorAndSpeed.z;
	return uv * tiling;
}

#endif