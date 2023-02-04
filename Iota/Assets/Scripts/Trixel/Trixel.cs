using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Input = UnityEngine.Input;

// https://www.theappguruz.com/blog/simple-cube-mesh-generation-unity3d

public static class ExtensionMethods {
    public static Vector3[] ToVector3Array(this Vertex[] v) {
        return Array.ConvertAll(v, item => (Vector3)item);;
    }

    public static string ExtractY(this Vector3 v, int surfaceID) {
        var y = v.y;

        if (y == 0) {
            y = -99;
        }
        
        return $"{new Vector3(0, y, 0).ToString()}{surfaceID}";
    }
    public static string ExtractX(this Vector3 v, int surfaceID) {
        var x = v.x;

        if (x == 0) {
            x = -99;
        }
        
        return $"{new Vector3(x, 0, 0).ToString()}{surfaceID}";
    }
    public static string ExtractZ(this Vector3 v, int surfaceID) {
        var z = v.z;

        if (z == 0) {
            z = -99;
        }
        
        return $"{new Vector3(0, 0, z).ToString()}{surfaceID}";
    }
}
    
public class Point {
    public bool         Active       = false;
    public bool[]       FacesChecked = new bool[6];
    public Vector3      Position     = new();
    public List<Vertex> vertices     = new List<Vertex>();
    public Face[]     Faces        = new Face[6];
    
    // public Point(){}
    public Point(Vector3 p, bool a) {
        Active   = a;
        Position = p;
    }

    public void SetFaces(Vertex[] v, 
        int TTL, int TTR, int TBR, int TBL, 
        int BTL, int BTR, int BBR, int BBL) {
        
        
        // TTL 0, TTR 1, TBR 2, TBL 3, BTL 4, BTR 5, BBR 6, BBL 7
        // top face
        Faces[0] = new Face(this, new []{v[0], v[1], v[2], v[3]});
        // bottom face
        Faces[1] = new Face(this, new []{v[4], v[5], v[6], v[7]});
        // front face
        Faces[2] = new Face(this, new []{v[1], v[0], v[4], v[5]});
        // back face
        Faces[3] = new Face(this, new []{v[3], v[2], v[6], v[7]});
        // left face
        Faces[4] = new Face(this, new []{v[0], v[3], v[7], v[4]});
        // right face
        Faces[5] = new Face(this, new []{v[2], v[1], v[5], v[6]});
    }
    //
    // public void SetFace(Face s) {
    //     Faces[0] = s;
    // }
}


public class Vertex {
     public bool    Virtual = false;
     public Vector3 Vertice;
     public string  Key;
     public int     Index;
     public int     Type;
     // public Vector3 Normal = Vector3.up;
     
     public Vertex(Vector3 v, bool isVirtual)  {
         Vertice = v;
         Virtual = isVirtual;
     }
     
     public Vertex(Vector3 v) : this(v, false) {
         Key     = v.Key();
         Vertice = v;
     }
     // Custom cast from "Vector3":
     public static implicit operator Vertex( Vector3 x ) { return new Vertex( x ); }
     // Custom cast to "Vector3":
     public static implicit operator Vector3( Vertex x ) { return x.Vertice; }
}

public class Face {
    public  Point                    P;
    private Dictionary <string, int> i = new();
    // public  float                    Size;
    private Vertex[]                 vertices;
    public Vector3                   Normal;
    public  int[]                    indices;
    
    public Face(){}
    
    public Face(Point p, Vertex[] v) {
        P       = p;
        indices = new[] { v[0].Index, v[1].Index, v[2].Index, v[3].Index};
        
        i.Add(Helpers.VectorKey(v[0]), v[0].Index);
        i.Add(Helpers.VectorKey(v[1]), v[1].Index);
        i.Add(Helpers.VectorKey(v[2]), v[2].Index);
        i.Add(Helpers.VectorKey(v[3]), v[3].Index);
        vertices = v; 
        // new []{v[TL], v[TR], v[BR], v[BL]};
        CalculateNormal();
    }
    
    // public void CalculateSize() {
    //     Size = (vertices[0].Vertice - vertices[2].Vertice).magnitude;
    // }
    
    public void CalculateNormal() {
        Normal = Helpers.GetNormal(
            vertices[0], 
            vertices[2], 
            vertices[1]);
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

public class Points {
    public  Dictionary<string, Vertex> VerticesKeyMap = new();
    // public  List<string> VerticeKeys = new();
    public  Vertex[]                   Vertices         = new Vertex[]{};
    private Dictionary<string, Point>  PointsList       = new();
  
    
    public ref Dictionary<string, Point> GetList() {
        return ref PointsList;
    }
    
    public  List<Point> GetPointList() {
        return  PointsList.Values.ToList();
    }

    public Point this[string a] {
        get { return PointsList[a]; }
        set { PointsList[a] = value; }
    }

    public void Add(Vector3 v) {
        PointsList.Add(v.Key(), new Point(v, true));
    }

    public void ClearCheck() {
        foreach (var p in PointsList) {
            p.Value.FacesChecked[0] = false;
            p.Value.FacesChecked[1] = false;
            p.Value.FacesChecked[2] = false;
            p.Value.FacesChecked[3] = false;
            p.Value.FacesChecked[4] = false;
            p.Value.FacesChecked[5] = false;
        }
    }
    // 😭 
    bool VerticeCondition(bool a, bool b, bool c) {
        return a && b && c || !a && b && c || !a && !b && c || !a && b && !c || a && !b && !c;
    }
    
    int AddVertice(Vertex v, bool real, int type) {
        if (!VerticesKeyMap.ContainsKey(v.Key)) {
            
            v.Virtual = !real;
            v.Type    = type;
            
            VerticesKeyMap.Add(v.Key, v);
            Vertices = VerticesKeyMap.Values.ToArray();
            v.Index  = VerticesKeyMap.Count - 1;

            return VerticesKeyMap.Count - 1;
        }
        
        // TODO: replace
        return VerticesKeyMap[v.Key].Index;
    }
    
    Dictionary<int, int> CellState(Point p) {
        Dictionary<int, int> caseIndex = new();
            // TL
            caseIndex.Add(0, AddVertice(p.Position + (Vector3.forward + Vector3.left + Vector3.up) / 2, 
                VerticeCondition(!ForwardLeft(p), !Left(p), !Forward(p)), 0));
            // TR
            caseIndex.Add(1, AddVertice(p.Position + (Vector3.forward + Vector3.right + Vector3.up) / 2,
                VerticeCondition(!ForwardRight(p), !Right(p), !Forward(p)), 1));
            // BR
            caseIndex.Add(2, AddVertice(p.Position + (Vector3.back + Vector3.right + Vector3.up) / 2, 
                VerticeCondition(!BackRight(p), !Right(p), !Back(p)), 2));
            // BL
            caseIndex.Add(3,AddVertice(p.Position + (Vector3.back + Vector3.left + Vector3.up) / 2,
                VerticeCondition(!BackLeft(p), !Left(p), !Back(p)), 3));
            
            // TL
            caseIndex.Add(4, AddVertice(p.Position + (Vector3.forward + Vector3.left + Vector3.down) / 2, 
                VerticeCondition(!ForwardLeft(p), !Left(p), !Forward(p)), 4));
            // TR
            caseIndex.Add(5, AddVertice(p.Position + (Vector3.forward + Vector3.right + Vector3.down) / 2,
                VerticeCondition(!ForwardRight(p), !Right(p), !Forward(p)), 5));
            // BR
            caseIndex.Add(6, AddVertice(p.Position + (Vector3.back + Vector3.right + Vector3.down) / 2, 
                VerticeCondition(!BackRight(p), !Right(p), !Back(p)), 6));
            // BL
            caseIndex.Add(7,AddVertice(p.Position + (Vector3.back + Vector3.left + Vector3.down) / 2,
                VerticeCondition(!BackLeft(p), !Left(p), !Back(p)), 7));
            
            return caseIndex; 
    }

    
    public bool ActiveByCheckType(int i, Vector3 v) {
        switch (i) {
            case 0: // top
                return Top(PointsList[v.Key()]);
            case 1: // bottom
                return Down(PointsList[v.Key()]);
            case 2: // front
                return Forward(PointsList[v.Key()]);
            case 3: // front
                return Back(PointsList[v.Key()]);
            case 4: // left
                return Left(PointsList[v.Key()]);
            case 5: // back
                return Right(PointsList[v.Key()]);
            default:
                return false;
        }
    }

    public void Hop(ref List<Face> ps, Vector3 start, Vector3 step, int checkType, bool skipStart = true) {
        Vector3 next = start;
        if (skipStart) {
             next = start + step;
        }
        if (ContainsAndActive(next) && !Checked(next, checkType) && !ActiveByCheckType(checkType, next)) {
            var p       = PointsList[(next).Key()];
            
            var indices = CellState(p);
            p.SetFaces(
                new [] {
                    Vertices[indices[0]], Vertices[indices[1]], Vertices[indices[2]], Vertices[indices[3]],
                    Vertices[indices[4]], Vertices[indices[5]], Vertices[indices[6]], Vertices[indices[7]]
                }, 
                indices[0], indices[1], indices[2], indices[3], 
                indices[4], indices[5], indices[6], indices[7]);
            
            ps.Add(p.Faces[checkType]);
            
            Hop(ref ps, p.Position, step, checkType);
            
            PointsList[(next).Key()].FacesChecked[checkType] = true;
        }
    }

    public Point GetPointByVector(Vector3 v) {
        return PointsList[v.Key()];
    }
    public bool Contains(Vector3 v) {
        return PointsList.ContainsKey(v.Key());
    }
    public bool IsActive(Vector3 v) {
        return PointsList[v.Key()].Active;
    }
    public bool Checked(Vector3 v, int checkType) {
        return PointsList[v.Key()].FacesChecked[checkType];
    }
    public void SetPointsActive(Vector3 v, bool b) {
        PointsList[v.Key()].Active = b;
    }
    public bool ContainsAndActive(Vector3 v) {
        return Contains(v) && IsActive(v);
    }
    public bool Top(Point p) {
         return ContainsAndActive(p.Position + Vector3.up); 
    }
    public bool Down(Point p) {
        return ContainsAndActive(p.Position + Vector3.down);
    }
    public bool Right(Point p) {
        return ContainsAndActive(p.Position + Vector3.right);
    }
    public bool Left(Point p) {
        return ContainsAndActive(p.Position + Vector3.left);
    }
    public bool Forward(Point p) {
        return ContainsAndActive(p.Position + Vector3.forward);
    }
    public bool Back(Point p) {
        return ContainsAndActive(p.Position + Vector3.back);
    }
    bool ForwardRight(Point p) {
        return ContainsAndActive(p.Position + Vector3.forward + Vector3.right);
    }
    bool ForwardLeft(Point p) {
        return ContainsAndActive(p.Position + Vector3.forward + Vector3.left);
    }
    bool BackRight(Point p) {
        return ContainsAndActive(p.Position + Vector3.back + Vector3.right);
    }
    bool BackLeft(Point p) {
        return ContainsAndActive(p.Position + Vector3.back + Vector3.left);
    }
} 

//     0 + step, -- Top Left        // 0
//     1 + step, -- Top Right       // 1
//     2 + step, -- Bottom Right    // 2

//     0 + step, -- Top Left        // 3
//     2 + step, -- Bottom Right    // 4
//     3 + step  -- Bottom Left     // 5
public class Trixel : MonoBehaviour {
    [Range(2, 16)] public int                       Resolution;
    [Range(0.0f, 16.0f)] public float               RenderSpeed;
    
    List<Point>          NullPoints = new();
    private Points       _points;
    private List<int>    Indices = new();
    private MeshCollider Collider;
    private Vector3      _direction, _hitPoint, _head;
    public  Material     mat;
    private Vector3      resolutionOffset;
    
    private MeshFilter   _mf;
    private MeshRenderer _mr;
    private Mesh         _mesh;
    
    void Awake() {
        Collider     = this.AddComponent<MeshCollider>();
        _mf          = this.AddComponent<MeshFilter>();
        _mr          = this.AddComponent<MeshRenderer>();
        _mr.material = mat;
        Init();
    }

    void Init() {
        _direction       = new Vector3();
        _points          = new Points();
        resolutionOffset = new Vector3(Resolution / 2, Resolution / 2, Resolution / 2);
        _mf.mesh         = new Mesh();
        NullPoints       = new List<Point>();
        
        for (int x = 0; x < Resolution; x++) {
            for (int y = 0; y < Resolution; y++) {
                for (int z = 0; z < Resolution; z++) {
                    Vector3 newPosition = new Vector3(x, y, z) - resolutionOffset;
                    _points.Add(newPosition);
                }
            }
        }
        StartCoroutine(LittleBabysMarchingCubes());
    }

    public class Surface {
        private List<Face> Faces = new ();
        public int SurfaceType;
        
        List<Face> frontList = new ();
        List<Face> backList  = new ();
        List<Face> leftList  = new ();
        List<Face> rightList = new ();
        
        public Surface() {
        }
        
        public Surface(int type) {
            SurfaceType = type;
        }

        Vector3 TypeToStepVector(string s) {
            switch (s) {
                case "front":
                    switch (SurfaceType) {
                        case 0: case 1: // top and bottom
                            return Vector3.forward;
                        case 2: case 3: // front and back
                            return Vector3.up;
                        case 4: case 5: // left and right
                            return Vector3.up;
                    }
                    Debug.LogError("surface type not recognized");
                    return Vector3.zero;
                case "back":
                    switch (SurfaceType) {
                        case 0: case 1: // top and bottom
                            return Vector3.back;
                        case 2: case 3: // front and back
                            return Vector3.down;
                        case 4: case 5: // left and right
                            return Vector3.down;
                    }
                    Debug.LogError("surface type not recognized");
                    return Vector3.zero;
                case "left":
                    switch (SurfaceType) {
                        case 0: case 1: // top and bottom
                            return Vector3.left;
                        case 2: case 3: // front and back
                            return Vector3.left;
                        case 4: case 5: // left and right
                            return Vector3.forward;
                    }
                    Debug.LogError("surface type not recognized");
                    return Vector3.zero;
                case "right":
                    switch (SurfaceType) {
                        case 0: case 1: // top and bottom
                            return Vector3.right;
                        case 2: case 3: // front and back
                            return Vector3.right;
                        case 4: case 5: // left and right
                            return Vector3.back;
                    }
                    Debug.LogError("surface type not recognized");
                    return Vector3.zero;
                default:
                    Debug.LogError("step key not recognized");
                    return Vector3.zero;
            }
        }
        
        public void SurfaceHop(Point p, ref Points points, bool skipStart = true) {
            Vector3 frontStep = TypeToStepVector("front");
            Vector3 backStep  = TypeToStepVector("back");
            Vector3 leftStep  = TypeToStepVector("left");
            Vector3 rigtStep  = TypeToStepVector("right");
            
            List<Face> localFront = new List<Face>();
            List<Face> localBack  = new List<Face>();
            List<Face> localLeft  = new List<Face>();
            List<Face> localRight = new List<Face>();
            
            points.Hop(ref localFront, p.Position, frontStep, SurfaceType, skipStart);
            points.Hop(ref localBack, p.Position, backStep, SurfaceType, skipStart);
            points.Hop(ref localLeft, p.Position, leftStep, SurfaceType, skipStart);
            points.Hop(ref localRight, p.Position, rigtStep, SurfaceType, skipStart);
            
            if (localFront.Count != 0) {
                List<Face> tangentLeft  = new List<Face>();
                List<Face> tangentRight = new List<Face>();
                foreach (var fp in localFront) {
                    points.Hop(ref tangentLeft, fp.P.Position, leftStep, SurfaceType, skipStart);
                    points.Hop(ref tangentRight, fp.P.Position, rigtStep, SurfaceType, skipStart);
                }
                
                if (tangentLeft.Count != 0) {
                    localLeft.AddRange(EvalualatePoints(tangentLeft));
                }
                if (tangentRight.Count != 0) {
                    localRight.AddRange(EvalualatePoints(tangentRight));
                }
                AddFront(EvalualatePoints(localFront));
            }
            
            if (localBack.Count != 0) {
                List<Face> tangentLeft  = new List<Face>();
                List<Face> tangentRight = new List<Face>();
                foreach (var fp in localBack) {
                    points.Hop(ref tangentLeft, fp.P.Position, leftStep, SurfaceType, skipStart);
                    points.Hop(ref tangentRight, fp.P.Position, rigtStep, SurfaceType, skipStart);
                }
                if (tangentLeft.Count != 0) {
                    localLeft.AddRange(EvalualatePoints(tangentLeft));
                }
                if (tangentRight.Count != 0) {
                    localRight.AddRange(EvalualatePoints(tangentRight));
                }
                AddBack(EvalualatePoints(localBack));
            }
            
            if (localLeft.Count != 0) {
                AddLeft(EvalualatePoints(localLeft));
            }
            if (localRight.Count != 0) {
                AddRight(EvalualatePoints(localRight));
            }
        }
        
        public void AddFront(List<Face> p) {
            frontList.AddRange(p);
        }
        public void AddBack(List<Face> p) {
            backList.AddRange(p);
        }
        public void AddLeft(List<Face> p) {
            leftList.AddRange(p);
        }
        public void AddRight(List<Face> p) {
            rightList.AddRange(p);
        }
        
        bool MergePoints(ref Face vp, ref Face vp1) {
            var TL1 = vp.indices[0];
            var TR1 = vp.indices[1];
            var BR1 = vp.indices[2];
            var BL1 = vp.indices[3];
                     
            var TL2 = vp1.indices[0];
            var TR2 = vp1.indices[1];
            var BR2 = vp1.indices[2];
            var BL2 = vp1.indices[3];
            
            bool connected = false;

            // vp1 on bottom        
            if (BL1 == TL2 && BR1 == TR2) {
                vp.indices[2] = BR2;        
                vp.indices[3] = BL2;
                connected                        = true;
            }
            // vp1 on top
            if (TL1 == BL2 && TR1 == BR2) {
                vp.indices[0] = TL2;        
                vp.indices[1] = TR2;
                connected                        = true;
            }
            // vp1 on right
            if (TR1 == TL2 && BR1 == BL2) {
                vp.indices[1] = TR2;        
                vp.indices[2] = BR2;
                connected                        = true;
            }
            // vp1 on left 
            if (TL1 == TR2 && BL1 == BR2) {
                vp.indices[0] = TL2;        
                vp.indices[3] = BL2;
                connected                        = true;
            }
            
            // vp.CalculateSize();
            // vp1.CalculateSize();
            return connected;
        }
        
        List<Face> EvalualatePoints(List<Face> list) {
            if (list.Count == 1) {
                return list;
            }
            for (int x = 0; x < 4; x++) {
                for (int i = 0; i < list.ToList().Count; i++) {
                    var vp = list[i];
                    for (int k = 0; k < list.ToList().Count; k++) {
                        var vp1 = list[k];
                        if (i == k) {continue;}
                        if (MergePoints(ref vp, ref vp1)) {
                            list.Remove(vp1);
                        }
                    }
                }
            }
            return list;
        }

        public bool Buffered() {
            return frontList.Count != 0 || backList.Count != 0 || leftList.Count != 0 || rightList.Count != 0;
        }

        public List<Face> GetSurfaceFaces() {
            var faces = new List<Face>();
            
            if (frontList.Count != 0) {
                Faces.AddRange(EvalualatePoints(frontList));
            }
            if (backList.Count != 0) {
                Faces.AddRange(EvalualatePoints(backList));
            }
            if (leftList.Count != 0) {
                Faces.AddRange(EvalualatePoints(leftList));
            }
            if (rightList.Count != 0) {
                Faces.AddRange(EvalualatePoints(rightList));
            }
            
            var final = new List<Face>();
            final.AddRange(EvalualatePoints(Faces));
            foreach (var face in final) {
                faces.Add(face);
            }
            return faces;
        }
    }
    
    void HandleSurfaces(ref Dictionary<string, Surface> surfaces, Point np, bool skipStart = true) {

        // if (!skipStart) {
            var down = np.Position + Vector3.down;
            if (_points.ContainsAndActive(down)) {
                var yKey = _points.GetPointByVector(down).Position.ExtractY(0);
                if (surfaces.ContainsKey(yKey)) {
                    surfaces[yKey].SurfaceHop(_points.GetPointByVector(down), ref _points, false);
                }
            }
            
            var up = np.Position + Vector3.up;
            if (_points.ContainsAndActive(up)) {
                var yKey = _points.GetPointByVector(up).Position.ExtractY(1);
                if (surfaces.ContainsKey(yKey)) {
                    surfaces[yKey].SurfaceHop(_points.GetPointByVector(up), ref _points, false);
                }
            }
            
            var back = np.Position + Vector3.back;
            if (_points.ContainsAndActive(back)) {
                var zKey = _points.GetPointByVector(back).Position.ExtractZ(2);
                if (surfaces.ContainsKey(zKey)) {
                    surfaces[zKey].SurfaceHop(_points.GetPointByVector(back), ref _points, false);
                }
            }
            
            var forward = np.Position + Vector3.forward;
            if (_points.ContainsAndActive(forward)) {
                var zKey = _points.GetPointByVector(forward).Position.ExtractZ(3);
                if (surfaces.ContainsKey(zKey)) {
                    surfaces[zKey].SurfaceHop(_points.GetPointByVector(forward), ref _points, false);
                }
            }
            
            var right = np.Position + Vector3.right;
            if (_points.ContainsAndActive(right)) {
                var xKey = _points.GetPointByVector(right).Position.ExtractX(4);
                if (surfaces.ContainsKey(xKey)) {
                    surfaces[xKey].SurfaceHop(_points.GetPointByVector(right), ref _points, false);
                }
            }
            
            var left = np.Position + Vector3.left;
            if (_points.ContainsAndActive(left)) {
                var xKey = _points.GetPointByVector(left).Position.ExtractX(5);
                if (surfaces.ContainsKey(xKey)) {
                    surfaces[xKey].SurfaceHop(_points.GetPointByVector(left), ref _points, false);
                }
            }
        // }
    

        if (surfaces.ContainsKey(np.Position.ExtractY(0))) {
            surfaces[np.Position.ExtractY(0)].SurfaceHop(np, ref _points, skipStart);
        }
        if (surfaces.ContainsKey(np.Position.ExtractY(1))) {
            surfaces[np.Position.ExtractY(1)].SurfaceHop(np, ref _points, skipStart);
        }
        
        if (surfaces.ContainsKey(np.Position.ExtractZ(2))) {
            surfaces[np.Position.ExtractZ(2)].SurfaceHop(np, ref _points, skipStart);
        }
        if (surfaces.ContainsKey(np.Position.ExtractZ(3))) {
            surfaces[np.Position.ExtractZ(3)].SurfaceHop(np, ref _points, skipStart);
        }
        
        if (surfaces.ContainsKey(np.Position.ExtractX(4))) {
            surfaces[np.Position.ExtractX(4)].SurfaceHop(np, ref _points, skipStart);
        }
        if (surfaces.ContainsKey(np.Position.ExtractX(5))) {
            surfaces[np.Position.ExtractX(5)].SurfaceHop(np, ref _points, skipStart);
        }
    }
    
    // spiral walk
    void Walk2(ref List<Face> faces) {
        List<Point> surfacePoints = new();
        var         surfaces      = new Dictionary<string, Surface>();
        
        foreach (var p in _points.GetPointList()) {
            // top and bottom
            if (p.Active) {
                if (!_points.Top(p)) {
                    surfacePoints.Add(p);
                    if (!surfaces.ContainsKey(p.Position.ExtractY(0))) {
                        surfaces.Add(p.Position.ExtractY(0), new Surface(0));
                        surfacePoints.Add(p);
                    }
                    // else {
                       
                    // }
                }
                if (!_points.Down(p)) {
                    surfacePoints.Add(p);
                    if (!surfaces.ContainsKey(p.Position.ExtractY(1))) {
                        surfaces.Add(p.Position.ExtractY(1), new Surface(1));
                        surfacePoints.Add(p);
                    }
                    // else {
                     
                    // }
                }
                // // front and back
                if (!_points.Forward(p)) {
                    surfacePoints.Add(p);
                    if (!surfaces.ContainsKey(p.Position.ExtractZ(2))) {
                        surfaces.Add(p.Position.ExtractZ(2), new Surface(2));
                        surfacePoints.Add(p);
                    }
                    // else {
                    //     
                    // }
                }
                if (!_points.Back(p)) {
                    surfacePoints.Add(p);
                    if (!surfaces.ContainsKey(p.Position.ExtractZ(3))) {
                        surfaces.Add(p.Position.ExtractZ(3), new Surface(3));
                        surfacePoints.Add(p);
                    }
                    // else {
                    // }
                }
                // left and right
                if (!_points.Left(p)) {
                    surfacePoints.Add(p);
                    if (!surfaces.ContainsKey(p.Position.ExtractX(4))) {
                        surfaces.Add(p.Position.ExtractX(4), new Surface(4));
                    }
                    // else {
                        // surfacePoints.Add(p);
                    // }
                } 
                if (!_points.Right(p)) {
                    surfacePoints.Add(p);
                    if (!surfaces.ContainsKey(p.Position.ExtractX(5))) {
                        surfaces.Add(p.Position.ExtractX(5), new Surface(5));
                    }
                    // else {
                        // surfacePoints.Add(p);
                    // }
                }
            }
            else {
                // nullPoints.Add(p);
            }
        }
        
        var c = new Vector3(Resolution, Resolution, Resolution) * 0.5f;
        
        NullPoints.Sort((a, b) => 
            Vector3.Distance(a.Position + resolutionOffset, c).
                CompareTo(Vector3.Distance(b.Position + resolutionOffset, c)));
        
        foreach (var np in NullPoints) {
            HandleSurfaces(ref surfaces, np, true);
        }

        foreach (var surface in surfaces.ToList()) {
            if (surface.Value.Buffered()) {
                faces.AddRange(surface.Value.GetSurfaceFaces());
                 surfaces.Remove(surface.Key);
            }
        }

        if (surfaces.Count != 0) {
            foreach (var np in surfacePoints) {
                HandleSurfaces(ref surfaces, np);
            }
            
            foreach (var surface in surfaces.ToList()) {
                if (surface.Value.Buffered()) {
                    faces.AddRange(surface.Value.GetSurfaceFaces());
                    surfaces.Remove(surface.Key);
                }
            }
        }
    }

    Vector2 GenerateUV(Vertex v, Vector3 dir) {
        var lastVert = (v.Vertice+resolutionOffset/Resolution) / Resolution;
        // print($"{lastVert}");
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
    
    IEnumerator LittleBabysMarchingCubes() {
        Helpers.ClearConsole();
        
        _mf.mesh       = new Mesh();
        
        _points.VerticesKeyMap = new Dictionary<string, Vertex>();
        Indices          = new ();
        _mesh            = new Mesh();

        _points.ClearCheck();
        
        var   faces = new List<Face>();
        Walk2(ref faces);

        // faces.Sort((a, b) => a.size.CompareTo(b.size));
        // var vertices = _points.VerticesIndexMap.Values.ToArray();
        print($"# of faces {faces.Count}");

        var cleanedVertices = new List<Vertex>();
        var cleanedUVs      = new List<Vector2>();
        var normals         = new List<Vector3>();
        
        if (faces.Count != 0) {
            for (int i = 0; i < faces.Count; i++) {
                var indices = faces[i].indices;
                
                faces[i].CalculateNormal();
                
                normals.Add(faces[i].Normal);
                normals.Add(faces[i].Normal);
                normals.Add(faces[i].Normal);
                normals.Add(faces[i].Normal);
                
                var cleanedIndices = new List<int>();
                cleanedVertices.Add(_points.VerticesKeyMap[Helpers.VectorKey(_points.Vertices[indices[0]].Vertice)]);
                cleanedIndices.Add(cleanedVertices.Count - 1);

                cleanedUVs.Add(GenerateUV(cleanedVertices.Last(), faces[i].Normal));
                
                cleanedVertices.Add(_points.VerticesKeyMap[Helpers.VectorKey(_points.Vertices[indices[1]].Vertice)]);
                cleanedIndices.Add(cleanedVertices.Count - 1);
                
                cleanedUVs.Add(GenerateUV(cleanedVertices.Last(), faces[i].Normal));
                
                cleanedVertices.Add(_points.VerticesKeyMap[Helpers.VectorKey(_points.Vertices[indices[2]].Vertice)]);
                cleanedIndices.Add(cleanedVertices.Count - 1);
               
                cleanedUVs.Add(GenerateUV(cleanedVertices.Last(), faces[i].Normal));
                
                cleanedVertices.Add(_points.VerticesKeyMap[Helpers.VectorKey(_points.Vertices[indices[3]].Vertice)]);
                cleanedIndices.Add(cleanedVertices.Count - 1);
                
                cleanedUVs.Add(GenerateUV(cleanedVertices.Last(), faces[i].Normal));
                
                faces[i].SetIndices(cleanedIndices.ToArray());
                
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
                Indices.AddRange(faces[i].Dump());
            }
        }
        
        print($"vertices: {cleanedVertices.Count}");
        _mesh.vertices  = cleanedVertices.ToArray().ToVector3Array(); 
        _mesh.triangles = Indices.ToArray();
        _mesh.normals   = normals.ToArray();
        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();
        _mesh.uv = cleanedUVs.ToArray();
        Collider.sharedMesh = _mesh;
        
        _mesh.name = "new face";
        _mf.mesh   = _mesh;
        
        yield return null;
    }

    bool MouseSelect() {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)) {
            _direction = hit.normal;
            // snap to plane WxH and distance based on normal/facing direction
            // if we snap on all axis's the hit point hovers above the hit point
            _hitPoint = new Vector3(
                (_direction.x != 0) ? hit.point.x : Mathf.Round(hit.point.x),
                (_direction.y != 0) ? hit.point.y : Mathf.Round(hit.point.y),
                (_direction.z != 0) ? hit.point.z : Mathf.Round(hit.point.z));
            return true;
        }
        return false;
    }
    
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        if (MouseSelect()) {
            // creating null point
            if (Input.GetMouseButtonDown(0) && _points.Contains(_hitPoint - _direction/2)) {
                _points.SetPointsActive(_hitPoint - _direction/2, false);
                NullPoints.Add(_points[(_hitPoint - _direction/2).Key()]);
                
                StartCoroutine(LittleBabysMarchingCubes());
            }
            
            // removing null point
            if (Input.GetMouseButtonDown(1) && _points.Contains(_hitPoint + _direction/2)) {
                _points.SetPointsActive(_hitPoint + _direction/2, true);
                NullPoints.Remove(_points[(_hitPoint + _direction/2).Key()]);
                
                StartCoroutine(LittleBabysMarchingCubes());
            }
        }
        
        if (Input.GetKeyDown(KeyCode.P)) {
            Helpers.ClearConsole();
            Init();
        }
    }

    private void OnDrawGizmos() {
        // if (_points == null || _points.GetList() == null || _points.GetList().Count == 0) {
        //     return;
        // }
        //
        // Gizmos.color = new Color(1,1,1,1f);
        // Gizmos.DrawWireCube(
        //     Vector3.zero, 
        //     Vector3.one * Resolution);
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
