#define DIVIDE 7
#define TAU 6.28318530718

StructuredBuffer<float3> _PositionBuffer;

float _Fade;
float _Width;

int _TentacleCount;
int _SegmentCount;

float3x3 rot3D(float3 axis, float angle)
{
    float c, s;
    sincos(angle, s, c);

    float t = 1 - c;
    float x = axis.x;
    float y = axis.y;
    float z = axis.z;

    return float3x3(
        t * x * x + c,      t * x * y - s * z,  t * x * z + s * y,
        t * x * y + s * z,  t * y * y + c,      t * y * z - s * x,
        t * x * z - s * y,  t * y * z + s * x,  t * z * z + c
    );
}

struct MeshAttributes
{
    float3 position;
#if SHADERPASS == SHADERPASS_MOTION_VECTORS
    float3 prevPos;
#endif
    float3 normal;
    float4 tangent;
    float2 uv;
    float4 color;
};

AttributesMesh ConvertToAttributesMesh(MeshAttributes mesh)
{
    AttributesMesh am;
    am.positionOS = mesh.position;
#ifdef ATTRIBUTES_NEED_NORMAL
    am.normalOS = mesh.normal;
#endif
#ifdef ATTRIBUTES_NEED_TANGENT
    am.tangentOS = mesh.tangent;
#endif
#ifdef ATTRIBUTES_NEED_TEXCOORD0
    am.uv0 = mesh.uv;
#endif
#ifdef ATTRIBUTES_NEED_TEXCOORD1
    am.uv1 = 0;
#endif
#ifdef ATTRIBUTES_NEED_TEXCOORD2
    am.uv2 = 0;
#endif
#ifdef ATTRIBUTES_NEED_TEXCOORD3
    am.uv3 = 0;
#endif
#ifdef ATTRIBUTES_NEED_COLOR
    am.color = mesh.color;
#endif
    UNITY_TRANSFER_INSTANCE_ID(input, am);
    return am;
}

PackedVaryingsType VertexOutout(MeshAttributes mesh)
{
    AttributesMesh am = ConvertToAttributesMesh(mesh);

    VaryingsType vt;
    vt.vmesh = VertMesh(am);
#if SHADERPASS == SHADERPASS_MOTION_VECTORS
    AttributesPass ap;
    ap.previousPositionOS = mesh.prevPos;
    #if defined (_ADD_PRECOMPUTED_VELOCITY)
        ap.precomputedVelocity = float3(-150,0,0);
    #endif
    return MotionVectorVS(vt, am, ap);
#endif

    return PackVaryingsType(vt);
}

// Geometry shader function body
[maxvertexcount(DIVIDE*4)]
void TentacleGeom( point AttributesToGS input[1],
                   inout TriangleStream<PackedVaryingsType> outStream)
{
    // ID attr
    uint vid = input[0].vertexID;
	bool isBreak = vid % _SegmentCount == (_SegmentCount - 1);
	if(isBreak) return;

    uint size = _TentacleCount * _SegmentCount;

    float uvRate = 1 / (float)(_SegmentCount - 1);
	float norm_tid = (vid / _SegmentCount) / (float)_TentacleCount;

    uint previd = max(vid - 1, 0);
	uint nextid = vid + 1;
	uint nnid = min(nextid + 1, size - 1);

    MeshAttributes output0, output1, output2, output3;

    // Compute some attributes
    float3 pos = _PositionBuffer[vid];
	float3 posNext = _PositionBuffer[nextid];
	
	float3 posPrev = _PositionBuffer[previd % _SegmentCount == _SegmentCount - 1 ? vid : previd];
	float3 posNext2 = _PositionBuffer[nextid % _SegmentCount == _SegmentCount - 1 ? nextid : nnid];
	
	float3 dir = normalize(posNext - posPrev);
	float3 dirNext = normalize(posNext2 - pos);
	float3 up = float3(dir.x > dir.y && dir.x > dir.z,
                       dir.y > dir.x && dir.y > dir.z,
                       dir.z > dir.x && dir.z > dir.y);
	
	float3 xDir = normalize(cross(up, dir));
	float3 yDir = normalize(cross(xDir, dir));
	float3 xDirNext = normalize(cross(up, dirNext));
	float3 yDirNext = normalize(cross(xDirNext, dirNext));
	
	float divRate = 1 / (float)DIVIDE;
	float3x3 mat = rot3D(dir, TAU * divRate);
	float3x3 matNext = rot3D(dirNext, TAU * divRate);
	
	float3 rotyDir = normalize(mul(yDir, mat));
	float3 rotyDirNext = normalize(mul(yDirNext, matNext));

    for(uint i = 0; i < DIVIDE; i++)
	{
		float cu = uvRate * (vid % _SegmentCount);
		float cn = uvRate * ((vid+1) % _SegmentCount);
		
		float ru = 1 - smoothstep(0, 1, saturate(cu - _Fade)/(1 - _Fade));
		float rn = 1 - smoothstep(0, 1, saturate(cn - _Fade)/(1 - _Fade));
		
		output0.position = pos + (yDir * _Width * .1)*ru;
		output1.position = pos + (rotyDir * _Width * .1)*ru;
		output2.position = posNext + (yDirNext * _Width * .1)*rn;
		output3.position = posNext + (rotyDirNext * _Width * .1)*rn;

    #if SHADERPASS == SHADERPASS_MOTION_VECTORS
        output0.prevPos = (float3)0;
        output1.prevPos = (float3)0;
        output2.prevPos = (float3)0;
        output3.prevPos = (float3)0;
    #endif

		output0.normal = yDir;
		output1.normal = rotyDir;
		output2.normal = yDirNext;
		output3.normal = rotyDirNext;

        output0.tangent = float4(dir, 1);
		output1.tangent = float4(dir, 1);
		output2.tangent = float4(dirNext, 1);
		output3.tangent = float4(dirNext, 1);
		
		output0.color = float4(norm_tid.xxx, 1);
		output1.color = float4(norm_tid.xxx, 1);
		output2.color = float4(norm_tid.xxx, 1);
		output3.color = float4(norm_tid.xxx, 1);

		output0.uv = float2(cu, i     * divRate) * float2(50, 1);
		output1.uv = float2(cu, (i+1) * divRate) * float2(50, 1);
		output2.uv = float2(cn, i     * divRate) * float2(50, 1);
		output3.uv = float2(cn, (i+1) * divRate) * float2(50, 1);
	
        // Vertex output
		outStream.Append(VertexOutout(output0));
        outStream.Append(VertexOutout(output2));
        outStream.Append(VertexOutout(output1));
        outStream.Append(VertexOutout(output3));
        outStream.RestartStrip();

		yDir = rotyDir;
		yDirNext = rotyDirNext;
		rotyDir = normalize(mul(rotyDir, mat));
		rotyDirNext = normalize(mul(rotyDirNext, matNext));
	}
}