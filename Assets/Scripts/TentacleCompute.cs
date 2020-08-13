using System.Runtime.InteropServices;
using UnityEngine;

public class TentacleCompute : MonoBehaviour
{
    public ComputeShader CS;

    #region attribute

    [SerializeField, Range(0f, 500f)] int tentacleCount = 100;

    public int TentacleCount
    {
        get{ return tentacleCount; }
    }

    [SerializeField, Range(0f, 5f)] float Length = 1.0f;
    [SerializeField, Range(0f, 500f)] float Spring = 70f;
    [SerializeField, Range(0f, 100f)] float Damping = 10f;
    [SerializeField] Vector3 Gravity = new Vector3(0f,-2f,0f);
    [SerializeField, Range(0f, 5f)] float noiseScale = 1.0f;
    [SerializeField, Range(0f, 8.0f)] float noiseAmplitude = 2.5f;
    [SerializeField] float noiseSpeed = .1f;
    [SerializeField, Range(0f, 1.0f)] float lengthRandomness = .5f;
    [SerializeField, Range(2f, 50f)] float rotSpeed = 10f;
    [SerializeField] float Seed = 0f;

    #endregion

    #region ComputeBuffer

    ComputeBuffer _PositionBuffer;
    public ComputeBuffer PositionBuffer
    {
        get{ return _PositionBuffer; }
    }

    ComputeBuffer _VelocityBuffer;
    ComputeBuffer _RootBuffer;

    #endregion

    int kernelIndex;
    int segmentCount;

    public int SegmentCount
    {
        get{ return segmentCount; }
    }

    #region MonoBehaviour

    void Start()
    {
        kernelIndex = CS.FindKernel("CSSpring");

        tentacleCount = tentacleCount + (3 - (tentacleCount % 3));

        uint x, y, z;
        CS.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
        segmentCount = (int)x;

        _PositionBuffer = new ComputeBuffer(tentacleCount * segmentCount,
                                           Marshal.SizeOf(typeof(Vector3)));
        _VelocityBuffer = new ComputeBuffer(tentacleCount * segmentCount,
                                           Marshal.SizeOf(typeof(Vector3)));

        _RootBuffer = new ComputeBuffer(tentacleCount, Marshal.SizeOf(typeof(Vector3)));
        Vector3[] root = new Vector3[tentacleCount];
        Vector2 rrange = new Vector2(-.03f, .03f);
        for (int i = 0; i < tentacleCount; i+=3)
        {
            Vector3 p = Vector3.Normalize(new Vector3(Random.Range(-1f, 1f), 
                                                    Random.Range(-1f, 1f), 
                                                    Random.Range(-1f, 1f)));

            root[i] = p;
            root[i+1] = p + new Vector3(Random.Range(rrange.x, rrange.y), 
                                        Random.Range(rrange.x, rrange.y), 
                                        Random.Range(rrange.x, rrange.y));
            root[i+2] = p + new Vector3(Random.Range(rrange.x, rrange.y), 
                                        Random.Range(rrange.x, rrange.y), 
                                        Random.Range(rrange.x, rrange.y));
        }
        _RootBuffer.SetData(root);

        CS.SetBuffer(kernelIndex, "_PositionBuffer", _PositionBuffer);
        CS.SetBuffer(kernelIndex, "_VelocityBuffer", _VelocityBuffer);
        CS.SetBuffer(kernelIndex, "_RootBuffer", _RootBuffer);
    }

    void Update()
    {
        CS.SetFloat("_Length", Length);
        CS.SetFloat("_Spring", Spring);
        CS.SetFloat("_Damping", Damping);
        CS.SetVector("_Gravity", Gravity);
        CS.SetFloat("_noiseScale", noiseScale);
        CS.SetFloat("_noiseAmplitude", noiseAmplitude);
        CS.SetFloat("_noiseSpeed", noiseSpeed);
        CS.SetFloat("_LengthRandomness", lengthRandomness);

        CS.SetFloat("_t", Time.time);
        CS.SetFloat("_dt", Time.deltaTime);
        CS.SetFloat("_Seed", Seed);

        float wave = Mathf.Sin(Mathf.PI * 2 * ((Time.time/rotSpeed)%1) );
        var rotM = Matrix4x4.Rotate(Quaternion.Euler(360*wave,360*wave,0));
        CS.SetMatrix("_RotationMat", rotM);

        CS.Dispatch(kernelIndex, tentacleCount, 1, 1);
    }

    void onDisable(){
        if(_PositionBuffer != null) _PositionBuffer.Release();
        _PositionBuffer = null;

        if(_VelocityBuffer != null) _VelocityBuffer.Release();
        _VelocityBuffer = null;

        if(_RootBuffer     != null) _RootBuffer.Release();
        _RootBuffer = null;
    }

    #endregion
}
