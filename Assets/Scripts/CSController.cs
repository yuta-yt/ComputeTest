using UnityEngine;

public class CSController : MonoBehaviour
{
    public ComputeShader computeShader;
    int KernelIndex_A;
    int KernelIndex_B;
    ComputeBuffer intComputeBuffer;

    void Start()
    {
        Debug.Log("start");
        KernelIndex_A = computeShader.FindKernel("KernelFunctionA");
        KernelIndex_B = computeShader.FindKernel("KernelFunctionB");

        int[] result = new int[4];
        intComputeBuffer = new ComputeBuffer(4, sizeof(int));

        computeShader.SetBuffer(KernelIndex_A, "intBuffer", intComputeBuffer);
        computeShader.SetFloat("value", 3f);
        computeShader.Dispatch(KernelIndex_A, 1, 1, 1);

        intComputeBuffer.GetData(result);
        for (int i = 0; i < 4; i++) Debug.Log(result[i]);

        computeShader.SetBuffer(KernelIndex_B, "intBuffer", intComputeBuffer);
        computeShader.Dispatch(KernelIndex_B, 1,1,1);

        intComputeBuffer.GetData(result);
        for (int i = 0; i < 4; i++) Debug.Log(result[i]);

        intComputeBuffer.Release();
    }
}
