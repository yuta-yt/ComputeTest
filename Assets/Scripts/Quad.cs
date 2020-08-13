using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quad : PCBase
{
    [SerializeField, Range(.1f, 10f)]
    protected float size = 1.0f;

    protected override Mesh Build(){
        var mesh = new Mesh();

        var hsize = size * .5f;

        var Vertices = new Vector3[] {
            new Vector3(-hsize, hsize, 0f),
            new Vector3(hsize, hsize, 0f),
            new Vector3(hsize, -hsize, 0f),
            new Vector3(-hsize, -hsize, 0f)
        };

        var uv = new Vector2[] {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0f, 1f)
        };

        var normals = new Vector3[] {
            new Vector3(0f, 0f, -1f),
            new Vector3(0f, 0f, -1f),
            new Vector3(0f, 0f, -1f),
            new Vector3(0f, 0f, -1f)
        };

        var triangles = new int[] {
            0,1,2,
            2,3,0
        };

        mesh.SetVertices(Vertices);
        mesh.SetUVs(0, uv);
        mesh.SetNormals(normals);
        mesh.SetTriangles(triangles, 0, true);

        return mesh;
    }
}
