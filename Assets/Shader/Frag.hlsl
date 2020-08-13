float _t;

float3 Emission(FragInputs input)
{
    float3 a = float3(1.46,.75,.95);
	float3 b = float3(.55,.51,.48);
	float3 c = float3(.5,.87,.33);
	float3 d = float3(.69,.72,.85);

    float paletteOffset = 1.08;
	float paletteScale = 15;

    float3 palette = a + b * cos(3.14159 * 2 * (c * (input.color.x+paletteOffset)*paletteScale + d));

    float3 emissive = 1 - saturate(abs(frac(_t*.3+input.color.x*.15)-(input.texCoord0.x/50)*.8));
    return pow(emissive, 70) * 8000 * pow(palette, 2.2);
}

void CustomFrag(  PackedVaryingsToPS packedInput,
            OUTPUT_GBUFFER(outGBuffer)
            #ifdef _DEPTHOFFSET_ON
            , out float outputDepth : SV_Depth
            #endif
            )
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(packedInput);
    FragInputs input = UnpackVaryingsMeshToFragInputs(packedInput.vmesh);

    // input.positionSS is SV_Position
    PositionInputs posInput = GetPositionInput(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS);

#ifdef VARYINGS_NEED_POSITION_WS
    float3 V = GetWorldSpaceNormalizeViewDir(input.positionRWS);
#else
    // Unused
    float3 V = float3(1.0, 1.0, 1.0); // Avoid the division by 0
#endif

    SurfaceData surfaceData;
    BuiltinData builtinData;
    GetSurfaceAndBuiltinData(input, V, posInput, surfaceData, builtinData);

    // Custom : Set Emission From Color & UV
    builtinData.emissiveColor += Emission(input);

    ENCODE_INTO_GBUFFER(surfaceData, builtinData, posInput.positionSS, outGBuffer);

#ifdef _DEPTHOFFSET_ON
    outputDepth = posInput.deviceDepth;
#endif
}