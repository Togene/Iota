using UnityEngine;

public class Face {
    public  string   PKey;
    private Vertex[] Vertices;
    public  Vector3  Normal;
    public  int[]    indices;
    
    public Face(string p, Vertex[] v) {
        PKey     = p;
        indices  = new[] {v[0].Index, v[1].Index, v[2].Index, v[3].Index};
        Vertices = v; 
        CalculateNormal();
    }
    // public void CalculateSize() {
    //     Size = (vertices[0].Vertice - vertices[2].Vertice).magnitude;
    // }
    
    public void CalculateNormal() {
        Normal = Helpers.GetNormal(
            Vertices[0], 
            Vertices[2], 
            Vertices[1]);
    }
    public void SetIndices(int[] _i) {
        indices = _i;
    }
    public int[] Dump() {
        return new[] {
            indices[0], indices[1], indices[2],
            indices[0], indices[2], indices[3],
        };
    }
}
