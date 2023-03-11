using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;



public class Trixel {
    
}


public enum FaceType {Xp, Xn, Yp, Yn, Zp, Zn}

public class TrixelFace {
    public  FaceType  Type;
    public  Vector3[] Vertices;
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
        return v.x < L && v.x > R && v.y > B && v.y < T;
    }

    public TrixelFace[] OpenSailWalk(Vector3 v) {
        Debug.Log($"making the sail!");
        var   newFaces  = new List<TrixelFace>();
        float pixelStep = 0.5f;
        
        // is worth, or is?
        var isLeft   = Mathf.Abs((L) - (v.x)) >= 1;
        var isRight  = Mathf.Abs((R) - (v.x)) >= 1;
        var isTop    = Mathf.Abs((T) - (v.y)) >= 1;
        var isBottom = Mathf.Abs((B) - (v.y)) >= 1;
        
        if (isLeft) {
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
            // branches
            var branchNo = Mathf.Abs(Mathf.Ceil(T) - Mathf.Ceil(v.y)) % 16;

            for (int i = 0; i < branchNo; i++) {
                var newVec = new Vector3(0, i+1, 0);
                
                // left branches
                newFaces.Add(
                    new TrixelFace(
                        Type,
                        new Vector3(L, v.y + pixelStep, v.z) + newVec,
                        new Vector3(v.x + pixelStep, v.y + pixelStep, v.z) + newVec, 
                        new Vector3(v.x + pixelStep, v.y - pixelStep, v.z) + newVec, 
                        new Vector3(L, v.y - pixelStep, v.z) + newVec
                    ));
                // right branches
                newFaces.Add(new TrixelFace(
                    Type,
                    new Vector3(v.x - pixelStep, v.y + pixelStep, v.z) + newVec,
                    new Vector3(R, v.y + pixelStep, v.z) + newVec,
                    new Vector3(R, v.y - pixelStep, v.z) + newVec,
                    new Vector3(v.x - pixelStep, v.y - pixelStep, v.z) + newVec
                ));
            }
            
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
            // bottom face
            
            var branchNo = (Mathf.Abs(Mathf.Ceil(B) - Mathf.Ceil(v.y)) % 16) - 1;

            for (int i = 0; i < branchNo; i++) {
                var newVec = new Vector3(0, i+1, 0);
                
                // left branches
                newFaces.Add(
                    new TrixelFace(
                        Type,
                        new Vector3(L, v.y + pixelStep, v.z) - newVec,
                        new Vector3(v.x + pixelStep, v.y + pixelStep, v.z) - newVec, 
                        new Vector3(v.x + pixelStep, v.y - pixelStep, v.z) - newVec, 
                        new Vector3(L, v.y - pixelStep, v.z) - newVec
                    ));
                // right branches
                newFaces.Add(new TrixelFace(
                    Type,
                    new Vector3(v.x - pixelStep, v.y + pixelStep, v.z) - newVec,
                    new Vector3(R, v.y + pixelStep, v.z) - newVec,
                    new Vector3(R, v.y - pixelStep, v.z) - newVec,
                    new Vector3(v.x - pixelStep, v.y - pixelStep, v.z) - newVec
                ));
            }
            
            newFaces.Add( 
                new TrixelFace(
                    Type,
                    new Vector3(v.x + pixelStep, v.y - pixelStep, v.z), 
                    new Vector3(v.x - pixelStep, v.y - pixelStep, v.z), 
                    new Vector3(v.x - pixelStep, B, v.z), 
                    new Vector3(v.x + pixelStep, B, v.z)
                ));
        }
        if (isTop && isRight) {
            // top right face
            // newFaces.Add(
            //     new TrixelFace(
            //         Type,
            //         new Vector3(v.x - pixelStep, T, v.z), 
            //         Vertices[1],
            //         new Vector3(R, v.y + pixelStep, v.z),
            //         new Vector3(v.x - pixelStep, v.y + pixelStep, v.z)
            //     ));
        }
        if (isTop && isLeft) {
            // top left face
            // newFaces.Add(
            //     new TrixelFace(
            //         Type,
            //         Vertices[0],
            //         new Vector3(v.x + pixelStep, T, v.z),
            //         new Vector3(v.x + pixelStep, v.y + pixelStep, v.z), 
            //         new Vector3(L, v.y + pixelStep, v.z)
            //     ));
        }
        if (isBottom && isLeft) {
            // bottom left face
            // newFaces.Add(
            //     new TrixelFace(
            //         Type,
            //         new Vector3(L, v.y - pixelStep, v.z),
            //         new Vector3(v.x + pixelStep, v.y - pixelStep, v.z), 
            //         new Vector3(v.x + pixelStep, B, v.z),
            //         Vertices[3]
            //     ));
        }
        if (isBottom && isRight) {
            // bottom right face
            // newFaces.Add(
            //     new TrixelFace(
            //         Type,
            //         new Vector3(v.x - pixelStep, v.y - pixelStep, v.z),
            //         new Vector3(R, v.y - pixelStep, v.z), 
            //         Vertices[2],
            //         new Vector3(v.x - pixelStep, B, v.z)
            //     ));
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
                 new (FaceType.Zn, v[1], v[0], v[4], v[5])
             });
     
        Faces.Add(
            new Vector3(-hRes, 0, 0).ExtractX(0),
            // left
            new List<TrixelFace>() {
                new(FaceType.Xn, v[0], v[3], v[7], v[4])
            });
        // right
        // Faces.Add(new Vector3(+hRes, 0, 0).ExtractX(0),
            // new(v[2], v[1], v[5], v[6]));

        ParseFaces();
    }

    Tuple<string, Vector3, TrixelFace> GetFaces(Vector3 v) {
        var backKey   = (v+Vector3.back/2).ExtractZ(0);
        var isBack = Faces.ContainsKey(backKey);
        
        var leftKey = (v+Vector3.left/2).ExtractX(0);
        var isLeft  = Faces.ContainsKey(leftKey);
        
        var topKey = (v+Vector3.up/2).ExtractY(0);
        var isTop  = Faces.ContainsKey(topKey);
        
        if (isBack)
            foreach (var face in Faces[backKey]) 
                if (face.Contains(v)) { return new (
                    backKey, v+Vector3.back/2, face); }
        
        if (isLeft)
            foreach (var face in Faces[leftKey]) 
                if (face.Contains(v)) { return new (
                    leftKey, v+Vector3.left/2, face); }
        
        if (isTop)
            foreach (var face in Faces[topKey]) 
                if (face.Contains(v)) { return new (
                    topKey, v+Vector3.up/2, face); }
     
        return null;
    }
    
    
    public TrixelFace Connect(ref TrixelFace i, ref TrixelFace o) {
        var TL = i.Vertices[0];
        var TR = i.Vertices[1];
        var BR = i.Vertices[2];
        var BL = i.Vertices[3];
        
        var oTL = o.Vertices[0];
        var oTR = o.Vertices[1];
        var oBR = o.Vertices[2];
        var oBL = o.Vertices[3];

        var code    = "";
        var biggest = float.MinValue;
    
        // ^ on top
        if (TL == oBL && TR == oBR) {
            var newSize = Vector3.Distance(oTL, i.Vertices[3]);
            if (newSize > biggest) {
                biggest = newSize;
                code    = "t";
            }
        }
        // <- on left 
        if (TL == oTR && BL == oBR) {
            var newSize = Vector3.Distance(oTL, i.Vertices[1]);
            if (newSize > biggest) {
                biggest = newSize;
                code    = "l";
            }
        }
        // v on bottom
        if (BL == oTL && BR == oTR) {
            var newSize = Vector3.Distance(oBL, i.Vertices[0]);
            if (newSize > biggest) {
                biggest = newSize;
                code    = "b";
            }
        }
        // -> on right
        if (TR == oTL && BR == oBL) {
            var newSize = Vector3.Distance(i.Vertices[0], oTR);
            if (newSize > biggest) {
                biggest = newSize;
                code    = "r";
            }
        }
        Debug.Log($"{biggest}{code}");
        switch (code) {
            case "t" :
                return new TrixelFace(
                    i.Type, oTL, oTR, i.Vertices[2], i.Vertices[3]);
            case "l":
                return new TrixelFace(
                    i.Type, oTL, i.Vertices[1], i.Vertices[2], oBL);
            case "b":
                return new TrixelFace(
                    i.Type, i.Vertices[0], i.Vertices[1], oBR, oBL);
            case "r":
                return new TrixelFace(
                    i.Type, i.Vertices[0], oTR, oBR, i.Vertices[3]);
            default:
                return null;
        }
    }

     bool MergeFaces(ref TrixelFace newFace, List<TrixelFace> list) {
        var connected = false;
        for (int i = 0; i < list.Count; i++) {
            // connection test, if new face, add new face and remove current
            var currentFace   = list[i];
            var connectedFace = Connect(ref currentFace, ref newFace);
            
            if (connectedFace != null) {
                list.Remove(currentFace);
                newFace = connectedFace;
                connected = true;
            }
        }
        return connected;
    }

     void CleanFaces(string key, int iterations = 4) {
        for (int u = 0; u < iterations; u++) {
            // per face, after initial merging, do another round and check
            for (int k = 0; k < Faces[key].ToList().Count; k++) {
                var newFace = Faces[key][k];
                var faceRef = newFace;
                if (MergeFaces(ref newFace, Faces[key])) {
                    Faces[key].Remove(faceRef);
                    Faces[key].Add(newFace);
                }
            }
        }
    }
    
    public void Edit(Vector3 v, bool invert) {
        // v comes in as centre of cuboid
        var maybeFace = GetFaces(v);
        
        // split face if found
        if (maybeFace != null) {
            if (!invert) {
                 // newFaces    = new TrixelFace[]{};
                 var newFaces = maybeFace.Item3.OpenSailWalk(maybeFace.Item2);
                
                // per new face attempt to merge with eixisting faces
                for (int k = 0; k < newFaces.Length; k++) {
                    var newFace = newFaces[k];
                    MergeFaces(ref newFace, Faces[maybeFace.Item1]);
                    Faces[maybeFace.Item1].Add(newFace);
                }
                // CleanFaces(maybeFace.Item1, 12);
                
                // remove split face, probs should do a check
                var key = maybeFace.Item1;
                Faces[key].Remove(maybeFace.Item3);
                ParseFaces();
            }
        }
        else {
            if (invert) {
                var   vBack     = v + Vector3.back / 2;
                var   key       = vBack.ExtractZ(0);
                if (Faces.ContainsKey(key)) {
                    Faces[key].AddRange(new [] {
                        Helpers.NewFace(FaceType.Zn, vBack, Vector3.back)
                    });
                }
                else {
                    Faces.Add(key, new List<TrixelFace>() {
                        Helpers.NewFace(FaceType.Zn, vBack, Vector3.back)
                    });
                }
                CleanFaces(key, 8);
                ParseFaces();
            }
        }
    }

    public void ParseFaces(string faceKey = "") {
        Vertices = new();
        Indices  = new();
        Normals  = new();
        
        var faces = new List<TrixelFace>();
       
        if (faceKey == "") {
            foreach (var f in Faces.Values) {
                faces.AddRange(f);
            }
        }
        else {
            faces = Faces[faceKey];
        }
        
        for (int i = 0; i < faces.Count; i++) {
            var verts = faces[i].DumpVertices();
            Vertices.AddRange(verts);
            Normals.AddRange(faces[i].DumpNormals());
            Indices.AddRange(faces[i].DumpIndces(i * verts.Length));
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
