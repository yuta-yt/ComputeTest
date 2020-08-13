using UnityEngine;

public class CSTexTest : MonoBehaviour
{
    RenderTexture renderTexA;

    public ComputeShader computeShader;
    int kernelIndexA;
    uint X, Y, Z;

    void Start()
    {
        renderTexA = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
        renderTexA.enableRandomWrite = true;
        renderTexA.Create();  

        kernelIndexA = computeShader.FindKernel("KernelFunctionA");      

        computeShader.GetKernelThreadGroupSizes(kernelIndexA, out X, out Y, out Z);

        computeShader.SetTexture(kernelIndexA, "texBuffer", renderTexA);
    }

    void Update()
    {
        computeShader.Dispatch(kernelIndexA,
                               renderTexA.width / (int)X,
                               renderTexA.height / (int)Y,
                               (int)Z);

        this.GetComponent<MeshRenderer>().material.SetTexture("_BaseColorMap", renderTexA);
    }
}
