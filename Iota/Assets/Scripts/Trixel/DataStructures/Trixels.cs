using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;


public class Trixel {
    
}


public enum FaceType {X, Y, Z}

public class TrixelFace {
    private FaceType  Type;
    private Vector3[] Vertices;
    private Vector3[] Normals;

    private float T, B, L, R;
    public TrixelFace(FaceType t, Vector3 tl, Vector3 tr, Vector3 br, Vector3 bl) {
        Vertices = new []{tl, tr, br, bl};
        
        T = Vertices[0].y;
        B = Vertices[3].y;
        L = Vertices[0].x;
        R = Vertices[1].x;
        
        var normal = CalculateNormal();
        Normals = new[] { normal, normal, normal, normal};

        Type = t;
    }

    public bool Contains(Vector3 v) {
        return v.x < L && v.x > R &&
               v.y > B && v.y < T;
    }
    
    public TrixelFace[] OpenSailWalk(Vector3 v) {
        Debug.Log($"making the sail!");


        var newFaces = new List<TrixelFace>();

        float pixelStep = 0.5f;

        // is worth, or is?
        var isLeft   = (L - (v.x - pixelStep)) - 1 >= 1;
        var isRight  = (R - (v.x + pixelStep)) - 1 >= 1;
        var isTop    = (T - (v.y + pixelStep)) - 1 >= 1;
        var isBottom = (B - (v.y - pixelStep)) - 1 >= 1;

        if (isLeft) {
            Debug.Log($"left face added");
            // left face
            newFaces.Add(
                new TrixelFace(
                Type,
                new Vector3(L, v.y + pixelStep, v.z),
                new Vector3(v.x + pixelStep, v.y + pixelStep, v.z), 
                new Vector3(v.x + pixelStep, v.y - pixelStep, v.z), 
                new Vector3(L, v.y - pixelStep, v.z)
            ));
        }
        if (isRight) {
            Debug.Log($"right face added");
            // right face
            newFaces.Add(new TrixelFace(
                Type,
                new Vector3(v.x - pixelStep, v.y + pixelStep, v.z),
                new Vector3(R, v.y + pixelStep, v.z),
                new Vector3(R, v.y - pixelStep, v.z),
                new Vector3(v.x - pixelStep, v.y - pixelStep, v.z)
            ));
        }
        if (isTop) {
            Debug.Log($"top face added");
            // top face
            newFaces.Add(new TrixelFace(
                Type,
                new Vector3(v.x + pixelStep, T, v.z),
                new Vector3(v.x - pixelStep, T, v.z),
                new Vector3(v.x - pixelStep, v.y + pixelStep, v.z),
                new Vector3(v.x + pixelStep, v.y + pixelStep, v.z)
            ));
        }
        if (isBottom) {
            Debug.Log($"bottom face added");
            // bottom face
            newFaces.Add( 
                new TrixelFace(
                    Type,
                    new Vector3(v.x + pixelStep, v.y - pixelStep, v.z), 
                    new Vector3(v.x - pixelStep, v.y - pixelStep, v.z), 
                    new Vector3(v.x - pixelStep, B, v.z), 
                    new Vector3(v.x + pixelStep, B, v.z)
                ));
        }
        // Top right face
        if (isTop && isRight) {
            Debug.Log($"top right face added");
            newFaces.Add(
                new TrixelFace(
                    Type,
                    new Vector3(v.x - pixelStep, T, v.z), 
                    Vertices[1],
                    new Vector3(R, v.y + pixelStep, v.z),
                    new Vector3(v.x - pixelStep, v.y + pixelStep, v.z)
                ));
        }
        // top left face
        if (isTop && isLeft) {
            Debug.Log($"top left face added");
            newFaces.Add(
                new TrixelFace(
                    Type,
                    Vertices[0],
                    new Vector3(v.x + pixelStep, T, v.z),
                    new Vector3(v.x + pixelStep, v.y + pixelStep, v.z), 
                    new Vector3(L, v.y + pixelStep, v.z)
                ));
        }
        // bottom left face
        if (isBottom && isLeft) {
            Debug.Log($"bottom left face added");
            newFaces.Add(
                new TrixelFace(
                    Type,
                    new Vector3(L, v.y - pixelStep, v.z),
                    new Vector3(v.x + pixelStep, v.y - pixelStep, v.z), 
                    new Vector3(v.x + pixelStep, B, v.z),
                    Vertices[3]
                ));
        }
        if (isBottom && isRight) {
            Debug.Log($"bottom right face added");
            newFaces.Add(
                new TrixelFace(
                    Type,
                    new Vector3(v.x - pixelStep, v.y - pixelStep, v.z),
                    new Vector3(R, v.y - pixelStep, v.z), 
                    Vertices[2],
                    new Vector3(v.x - pixelStep, B, v.z)
                ));
        }
        return newFaces.ToArray();
    }
    
    public Vector3[] DumpVertices() {
        return Vertices;
    }
    public Vector3[] DumpNormals() {
        return Normals;
    }
    public int[] DumpIndces(int step) {
        return new [] {
            0 + step, 2 + step, 1 + step,
            0 + step, 3 + step, 2 + step
        };
    }
    public Vector3 CalculateNormal() {
        return Helpers.GetNormal(Vertices[0], Vertices[2], Vertices[1]);
    }
}

public class TrixelBlock {
    private List<Vector3> Vertices = new();
    private List<int>     Indices  = new();
    private List<Vector3> Normals  = new();
    private List<Vector2> UVS      = new();
    
    private Dictionary<string, List<TrixelFace>> Faces = new();
    private TrixelData                     Data;
    
    // private int zCount, xCount, yCount = 0;
    public TrixelBlock(int r) {
        float hRes = (float)r / 2;
        // tl - tr
        // |     |
        // bl - br
        var v = new List<Vector3> {
            // top
            new (-hRes, +hRes, -hRes), // tl | 0
            new (+hRes, +hRes, -hRes), // tr | 1
            new (+hRes, +hRes, +hRes), // br | 2
            new (-hRes, +hRes, +hRes), // bl | 3
            // bottom
            new (-hRes, -hRes, -hRes), // tl | 4
            new (+hRes, -hRes, -hRes), // tr | 5
            new (+hRes, -hRes, +hRes), // br | 6
            new (-hRes, -hRes, +hRes), // bl | 7
        };
        // top
        // Faces.Add(new Vector3(0, +hRes, 0).ExtractY(0), 
            // new(v[0], v[1], v[2], v[3]));
        // bottom
        // Faces.Add(new Vector3(0, -hRes, 0).ExtractY(0), 
            // new(v[7], v[6], v[5], v[4]));
        // front
        // Faces.Add(new Vector3(0, 0, +hRes).ExtractZ(zCount), 
        //     new(v[3], v[2], v[6], v[7]));
        Faces.Add(
            new Vector3(0, 0, -hRes).ExtractZ(0), 
             new List<TrixelFace>() {
                 // back
                 new (FaceType.Z, v[1], v[0], v[4], v[5])
             });
        // left
        // Faces.Add(new Vector3(-hRes, 0, 0).ExtractX(0), 
            // new(v[0], v[3], v[7], v[4]));
        // right
        // Faces.Add(new Vector3(+hRes, 0, 0).ExtractX(0),
            // new(v[2], v[1], v[5], v[6]));

        ParseFaces();
    }

    Tuple<string, TrixelFace> GetFace(Vector3 v) {
        // if (Faces.ContainsKey(v.ExtractX(0))) {
        //     return new(v.ExtractX(0), Faces[v.ExtractX(0)]);
        // }
        if (Faces.ContainsKey(v.ExtractZ(0))){
            // Faces[v.ExtractZ(0)].Contains(v)
            foreach (var face in Faces[v.ExtractZ(0)]) {
                if (face.Contains(v)) {
                    return new (v.ExtractZ(0), face); 
                }
            }
        }
        // if (Faces.ContainsKey(v.ExtractY(0))) {
        //      return new (v.ExtractY(0), Faces[v.ExtractY(0)]);
        // }
        return null;
    }
    
    
    public void Edit(Vector3 v) {
        var maybeFace = GetFace(v);
        if (maybeFace != null) {
            foreach (var newFaces in maybeFace.Item2.OpenSailWalk(v)) {
                Faces[maybeFace.Item1].Add(newFaces);
            }
            Faces[maybeFace.Item1].Remove(maybeFace.Item2);
        }
        ParseFaces();
    }

    public void ParseFaces() {
        Vertices = new();
        Indices  = new();
        Normals = new();
        
        // break face 
        for (int x = 0; x < Faces.Values.ToArray().Length; x++) {
            var faces = Faces.Values.ToArray()[x];
            Debug.Log(faces.Count);
            for (int i = 0; i < faces.ToArray().Length; i++) {
                var f = faces.ToArray()[i];
                
                var verts = f.DumpVertices();
                Vertices.AddRange(verts);
                Normals.AddRange(f.DumpNormals());
                Indices.AddRange(f.DumpIndces(i * verts.Length));
            }
        }
        Data = new TrixelData(
            Vertices.ToArray(),
            Indices.ToArray(),
            Normals.ToArray(),
            null
        );
    }
    
    public  Mesh Render() {
        Mesh mesh = new ();
        mesh.vertices  = Data.v;
        mesh.normals   = Data.n;
        mesh.triangles = Data.i;
        return mesh;
    }

    public void OnDrawGizmos() {
        if (Vertices == null || Vertices.Count == 0)
            return;
        
        foreach (var v in Vertices) {
            Gizmos.DrawSphere(v, 0.125f);
        }
    }
}

public class Trixels {
    int                         Resolution;
    Points                      _points;
    Vector3                     resolutionOffset;
    List<int>                   Indices       = new();
    List<string>                surfacePoints = new();
    List<string>                NullPoints    = new();
    Dictionary<string, Surface> Surfaces      = new();
    
    public Trixels(int r) {
        Resolution = r;
        Init();
    }
    
    public void Init() {
        _points    = new Points();
        NullPoints = new List<string>();
        
        resolutionOffset = new Vector3(Resolution, Resolution, Resolution) * 0.5f;
        for (int x = 0; x < Resolution; x++) {
            for (int y = 0; y < Resolution; y++) {
                for (int z = 0; z < Resolution; z++) {
                    _points.Add((new Vector3(x, y, z) - resolutionOffset));
                }
            }
        }
    }
    
    public void SetActive(Vector3 v, bool b) {
        _points.SetPointsActive(v, b);
    }
    
    public bool Contains(Vector3 v) {
        return _points.Contains(v);
    }
    
    public void AddNullPoint(string s) {
        NullPoints.Add(s);
    }
    
    public void RemoveNullPoint(string s) {
        NullPoints.Remove(s);
    }
    
    void NewSurface(string pKey, bool addPoints) {
        var p = _points[pKey];
        
        if (!_points.Top(p)) {
            if (!Surfaces.ContainsKey(p.Position.ExtractY(0))) {
                Surfaces.Add(p.Position.ExtractY(0), new Surface(0));
            }
            if (addPoints) surfacePoints.Add(pKey);;
        }
        if (!_points.Down(p)) {
            if (!Surfaces.ContainsKey(p.Position.ExtractY(1))) {
                Surfaces.Add(p.Position.ExtractY(1), new Surface(1));
            }
            if (addPoints) surfacePoints.Add(pKey);;
        }
        // // front and back
        if (!_points.Forward(p)) {
            if (!Surfaces.ContainsKey(p.Position.ExtractZ(2))) {
                Surfaces.Add(p.Position.ExtractZ(2), new Surface(2));
            }
            if (addPoints) surfacePoints.Add(pKey);;
        }
        if (!_points.Back(p)) {
            if (!Surfaces.ContainsKey(p.Position.ExtractZ(3))) {
                Surfaces.Add(p.Position.ExtractZ(3), new Surface(3));
            }
            if (addPoints) surfacePoints.Add(pKey);;
        }
        // left and right
        if (!_points.Left(p)) {
            if (!Surfaces.ContainsKey(p.Position.ExtractX(4))) {
                Surfaces.Add(p.Position.ExtractX(4), new Surface(4));
            }
            if (addPoints) surfacePoints.Add(pKey);;
        } 
        if (!_points.Right(p)) {
            if (!Surfaces.ContainsKey(p.Position.ExtractX(5))) {
                Surfaces.Add(p.Position.ExtractX(5), new Surface(5));
            }
            if (addPoints) surfacePoints.Add(pKey);;
        }
    }
    
    void HandleSurfaces(ref Dictionary<string, Surface> surfaces, string pKey, bool skipStart = true) {
        var np = _points[pKey];
        
        if (_points.ContainsAndActive(np.Position + Vector3.down)) {
            var yKey = _points.GetPointByVector(np.Position + Vector3.down).Position.ExtractY(0);
            if (surfaces.ContainsKey(yKey)) {
                surfaces[yKey].SurfaceHop((np.Position + Vector3.down).Key(), ref _points, false);
            }
        }
        if (_points.ContainsAndActive(np.Position + Vector3.up)) {
            var yKey = _points.GetPointByVector(np.Position + Vector3.up).Position.ExtractY(1);
            if (surfaces.ContainsKey(yKey)) {
                surfaces[yKey].SurfaceHop((np.Position + Vector3.up).Key(), ref _points, false);
            }
        }
        if (_points.ContainsAndActive(np.Position + Vector3.back)) {
            var zKey = _points.GetPointByVector(np.Position + Vector3.back).Position.ExtractZ(2);
            if (surfaces.ContainsKey(zKey)) {
                surfaces[zKey].SurfaceHop((np.Position + Vector3.back).Key(), ref _points, false);
            }
        }
        if (_points.ContainsAndActive(np.Position + Vector3.forward)) {
            var zKey = _points.GetPointByVector(np.Position + Vector3.forward).Position.ExtractZ(3);
            if (surfaces.ContainsKey(zKey)) {
                surfaces[zKey].SurfaceHop((np.Position + Vector3.forward).Key(), ref _points, false);
            }
        }
        if (_points.ContainsAndActive(np.Position + Vector3.right)) {
            var xKey = _points.GetPointByVector(np.Position + Vector3.right).Position.ExtractX(4);
            if (surfaces.ContainsKey(xKey)) {
                surfaces[xKey].SurfaceHop((np.Position + Vector3.right).Key(), ref _points, false);
            }
        }
        
        if (_points.ContainsAndActive(np.Position + Vector3.left)) {
            var xKey = _points.GetPointByVector(np.Position + Vector3.left).Position.ExtractX(5);
            if (surfaces.ContainsKey(xKey)) {
                surfaces[xKey].SurfaceHop((np.Position + Vector3.left).Key(), ref _points, false);
            }
        }
    
        if (surfaces.ContainsKey(np.Position.ExtractY(0))) {
            surfaces[np.Position.ExtractY(0)].SurfaceHop(pKey, ref _points, skipStart);
        }
        if (surfaces.ContainsKey(np.Position.ExtractY(1))) {
            surfaces[np.Position.ExtractY(1)].SurfaceHop(pKey, ref _points, skipStart);
        }
        
        if (surfaces.ContainsKey(np.Position.ExtractZ(2))) {
            surfaces[np.Position.ExtractZ(2)].SurfaceHop(pKey, ref _points, skipStart);
        }
        if (surfaces.ContainsKey(np.Position.ExtractZ(3))) {
            surfaces[np.Position.ExtractZ(3)].SurfaceHop(pKey, ref _points, skipStart);
        }
        
        if (surfaces.ContainsKey(np.Position.ExtractX(4))) {
            surfaces[np.Position.ExtractX(4)].SurfaceHop(pKey, ref _points, skipStart);
        }
        if (surfaces.ContainsKey(np.Position.ExtractX(5))) {
            surfaces[np.Position.ExtractX(5)].SurfaceHop(pKey, ref _points, skipStart);
        }
    }
    
    // spiral walk

    Vector2 GenerateUV(Vertex v, Vector3 dir) {
        var lastVert = (((v.Vertice)+resolutionOffset/Resolution) / Resolution);
      
        if (dir == Vector3.up || dir == Vector3.down) {
            return new Vector2(lastVert.x, lastVert.z/3 + 0) + new Vector2(0.5f, 0.5f/3);
        } 
        if (dir == Vector3.left || dir == Vector3.right) {
            return new Vector2(lastVert.z, (lastVert.y/3) + (1/3f)) + 
                   new Vector2(0.5f, 0.5f/3);
        } 
        if (dir == Vector3.forward || dir == Vector3.back) {
            return  new Vector2(lastVert.x, (lastVert.y / 3) + (2/3f)) + new Vector2(0.5f, 0.5f/3);
        }
        return Vector2.one;
    }
    
    public TrixelData LittleBabysMarchingCubes() {
        Helpers.ClearConsole();
        // var mesh                = new Mesh();
        _points.VerticesKeyMap = new Dictionary<string, Vertex>();
        Indices                = new ();
        _points.ClearCheck();
        
        var faces = new List<Face>();
        {   // surface walk
            surfacePoints = new List<string>();
            Surfaces      = new Dictionary<string, Surface>();
        
            foreach (var pair in _points.GetList()) {
                if (pair.Value.Active) {
                    NewSurface(pair.Key, true);
                }
            }
        
            NullPoints.Sort((a, b) => 
                Vector3.Distance(_points[a].Position + resolutionOffset, resolutionOffset).
                    CompareTo(Vector3.Distance(_points[b].Position + resolutionOffset, resolutionOffset)));
        
            foreach (var np in NullPoints) {
                HandleSurfaces(ref Surfaces, np);
            }
            foreach (var sp in surfacePoints) {
                HandleSurfaces(ref Surfaces, sp);
            }
            foreach (var surface in Surfaces.ToList()) {
                if (surface.Value.Buffered()) {
                    faces.AddRange(surface.Value.GetSurfaceFaces());
                    Surfaces.Remove(surface.Key);
                }
            }
        }
        Debug.Log($"# of faces {faces.Count}");

        var cleanedVertices = new List<Vertex>();
        var cleanedUVs      = new List<Vector2>();
        var normals         = new List<Vector3>();

        if (faces.Count == 0) {
            return new TrixelData();
        }

        foreach (var face in faces) {
            face.CalculateNormal();
            
            normals.Add(face.Normal);
            normals.Add(face.Normal);
            normals.Add(face.Normal);
            normals.Add(face.Normal);
            
            var cleanedIndices = new List<int>();
            cleanedVertices.Add(_points.VerticesKeyMap[Helpers.VectorKey(_points.Vertices[face.indices[0]].Vertice)]);
            cleanedIndices.Add(cleanedVertices.Count - 1);
            cleanedUVs.Add(GenerateUV(cleanedVertices.Last(), face.Normal));
            
            cleanedVertices.Add(_points.VerticesKeyMap[Helpers.VectorKey(_points.Vertices[face.indices[1]].Vertice)]);
            cleanedIndices.Add(cleanedVertices.Count - 1);
            cleanedUVs.Add(GenerateUV(cleanedVertices.Last(), face.Normal));
            
            cleanedVertices.Add(_points.VerticesKeyMap[Helpers.VectorKey(_points.Vertices[face.indices[2]].Vertice)]);
            cleanedIndices.Add(cleanedVertices.Count - 1);
            cleanedUVs.Add(GenerateUV(cleanedVertices.Last(), face.Normal));
            
            cleanedVertices.Add(_points.VerticesKeyMap[Helpers.VectorKey(_points.Vertices[face.indices[3]].Vertice)]);
            cleanedIndices.Add(cleanedVertices.Count - 1);
            cleanedUVs.Add(GenerateUV(cleanedVertices.Last(), face.Normal));
            
            face.SetIndices(cleanedIndices.ToArray());
            
            // if (!OffsetVertices.ContainsKey(Helpers.VectorKey(vertices[indices[0]].Vertice))) {
            //     if (vertices[indices[0]].Virtual) {
            //         OffsetVertices.Add(
            //             Helpers.VectorKey(vertices[indices[0]].Vertice),
            //             vertices[indices[0]].Vertice);
            //     }
            // }
            //
            // if (!OffsetVertices.ContainsKey(Helpers.VectorKey(vertices[indices[1]].Vertice))) {
            //     if (vertices[indices[1]].Virtual) {
            //         OffsetVertices.Add(
            //             Helpers.VectorKey(vertices[indices[1]].Vertice), 
            //             vertices[indices[1]].Vertice);
            //     }
            // }
            //
            // if (!OffsetVertices.ContainsKey(Helpers.VectorKey(vertices[indices[2]].Vertice))) {
            //     if (vertices[indices[2]].Virtual) {
            //         OffsetVertices.Add(
            //             Helpers.VectorKey(vertices[indices[2]].Vertice), 
            //             vertices[indices[2]].Vertice);
            //     }
            // }
            //
            // if (!OffsetVertices.ContainsKey(Helpers.VectorKey(vertices[indices[3]].Vertice))) {
            //     if (vertices[indices[3]].Virtual) {
            //         OffsetVertices.Add(
            //             Helpers.VectorKey(vertices[indices[3]].Vertice), 
            //             vertices[indices[3]].Vertice);
            //     }
            // }
            Indices.AddRange(face.Dump());
        }
        Debug.Log($"vertices: {cleanedVertices.Count}");

        return new TrixelData(
            cleanedVertices.ToVector3Array(),
            Indices.ToArray(),
            normals.ToArray(),
            cleanedUVs.ToArray()
        );
    }

    void CarveOnAxis(string uPos, int x, int y) {
        for (int u = 0; u < Resolution; u++) {
            Vector3 step = Vector3.zero;

            if (uPos == "X") {
                step = new Vector3(u, x, y);
            } else if (uPos == "Y") {
                step = new Vector3(y, u, x);
            }
            else if (uPos == "Z") {
                step = new Vector3(y, x, u);
            }
            
            Vector3 v = step - resolutionOffset;
            if (_points.ContainsAndActive(v)) {
                _points[v.Key()].Active = false;
            }
        }
    }

    void CarveByTexture(Color[] pixels, int spriteNo, int resolution) {
        for(int i = 0; i < pixels.Length/spriteNo; i++) {
            var x = (i / 16); // row
            var y = (i % 16); // col
            
            // top
            if (pixels[((16*3 * x + y) % pixels.Length)].a == 0) {
                CarveOnAxis("Y", x, y);
            }
            // side
            if (pixels[((16*3 * x + y) % pixels.Length) + resolution].a == 0) {
                CarveOnAxis("X", x, y);
            }
            // front
            if (pixels[((16*3 * x + y) % pixels.Length) + resolution*2].a == 0) {
               CarveOnAxis("Z", x, y);
            }
        }
    }
    
    
    public void Carve() {
        // // var pixels     = ;
        // var currentRow = "";
        // CarveByTexture(SpriteXYZ.GetPixels(), 3, Resolution);
        // StartCoroutine(LittleBabysMarchingCubes());
        Debug.Log("Carving");
    }
    
    public void OnDrawGizmos() {
        Gizmos.color = Color.magenta; //new Color(1,1,1,1f);
        Gizmos.DrawWireCube(  Vector3.one/Resolution - resolutionOffset/Resolution, (Vector3.one*Resolution));
        
        // if (_points == null || _points.GetList() == null || _points.GetList().Count == 0) {
        //     return;
        // }
        //
        
        // Gizmos.color = new Color(1,1,1,.1f);
        // foreach (var p in _points.GetList()) {
        //     if (p.Value.Active) {
        //         // Gizmos.DrawWireCube(p.Value.Position, Vector3.one);
        //     }
        //     else {
        //          // Gizmos.DrawCube(p.Value.Position, Vector3.one);
        //     }
        // }
        //
        // Gizmos.color = Color.green;
        // Gizmos.DrawRay(_hitPoint, _direction);
        // Gizmos.DrawSphere(_head, 0.15f);
        //
        // if (_points == null || _points.VerticesIndexMap == null || _points.VerticesIndexMap.Count == 0) {
        //     return;
        // }
        //
        // foreach (var p in _points.VerticesIndexMap) {
        //     if (!p.Value.Virtual) {
        //         if (p.Value.Type == 0) {
        //             Gizmos.color = Color.blue;
        //         } else if (p.Value.Type == 1) {
        //             Gizmos.color = Color.red;
        //         } else if (p.Value.Type == 2) {
        //             Gizmos.color = Color.yellow;
        //         } else if (p.Value.Type == 3) {
        //             Gizmos.color = Color.green;
        //         } 
        //         // Gizmos.DrawCube(p.Value, new Vector3(0.1f, 0.1f, 0.1f));
        //         Gizmos.DrawRay(p.Value.Vertice, p.Value.Normal);
        //     }
        //     else {
        //         Gizmos.color = Color.magenta;
        //     }
        // }
        //
        // Gizmos.color = Color.cyan;
        // foreach (var p in OffsetVertices) {
        //     Gizmos.DrawCube(p.Value + new Vector3(0, 0, 0), new Vector3(0.1f, 0.1f, 0.1f));
        // }
        // Gizmos.color = Color.yellow;
        // foreach (var p in SurfaceTests) {
        //     // Gizmos.DrawCube(p , new Vector3(0.3f, 0.3f, 0.3f));
        // }
    }
}
