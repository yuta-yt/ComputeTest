using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent (typeof(TentacleCompute))]
public class RenderTentacles : MonoBehaviour
{
    int tentacleCount;
    int segmentCount;

    TentacleCompute tc;

    [SerializeField] Material material = null;

    [SerializeField, Range(0f, 1.0f)] float fadeStart = .6f;
    [SerializeField, Range(0f, 2.0f)] float Width = .5f;

    void Start()
    {
        tc = GetComponent<TentacleCompute>();
        tentacleCount = tc.TentacleCount;
        segmentCount = tc.SegmentCount;
    }

    MaterialPropertyBlock prop;

    void Update()
    {
        if (prop == null) prop = new MaterialPropertyBlock();

        prop.SetBuffer("_PositionBuffer", tc.PositionBuffer);
        prop.SetFloat("_Fade", fadeStart);
        prop.SetFloat("_Width", Width);
        prop.SetInt("_TentacleCount", tentacleCount);
        prop.SetInt("_SegmentCount", segmentCount);
        prop.SetFloat("_t", Time.time);

        Graphics.DrawProcedural(
            material,
            new Bounds(transform.localPosition, transform.lossyScale * 5),
            MeshTopology.Points,
            tentacleCount * segmentCount, 1, null,
            prop, ShadowCastingMode.TwoSided, true,
            gameObject.layer
        );
    }
}
