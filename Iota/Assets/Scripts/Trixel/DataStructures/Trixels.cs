using System.Collections.Generic;
using System.Linq;
using UnityEngine;


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
    
    public void NewSurface(string pKey, bool addPoints) {
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
            return new Vector2(lastVert.z, (lastVert.y/3) + (1/3f)) + new Vector2(0.5f, 0.5f/3);
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
        
            var c = new Vector3(Resolution, Resolution, Resolution) * 0.5f;
            NullPoints.Sort((a, b) => 
                Vector3.Distance(_points[a].Position + resolutionOffset, c).
                    CompareTo(Vector3.Distance(_points[b].Position + resolutionOffset, c)));
        
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

        // faces.Sort((a, b) => a.size.CompareTo(b.size));
        // var vertices = _points.VerticesIndexMap.Values.ToArray();
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
        
        // Gizmos.color = new Color(1,1,1,.1f);````
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
