using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(MeshFilter), typeof(MeshRenderer))]
[ExecuteInEditMode]
public abstract class PCBase : MonoBehaviour
{

    MeshFilter filter;
    MeshRenderer renderere;

    public MeshFilter Filter{
        get{
            if(filter == null){
                filter = GetComponent<MeshFilter>();
            }
            return filter;
        }
    }

    
    public MeshRenderer Renderere{
        get{
            if(renderere == null){
                renderere = GetComponent<MeshRenderer>();
            }
            return renderere;
        }
    }

    private void Rebuild(){
        if(Filter.sharedMesh = null) Destroy(Filter.sharedMesh);
        Filter.sharedMesh = Build();
    }

    void Start()
    {
        Rebuild();
    }

    // Update is called once per frame
    void Update()
    {
        Rebuild();
    }

    protected abstract Mesh Build();
}
