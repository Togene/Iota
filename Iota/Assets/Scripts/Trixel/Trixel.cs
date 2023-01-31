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
    public Square[]     Faces        = new Square[6];
    
    public Point(){}
    public Point(Vector3 p, bool a) {
        Active   = a;
        Position = p;
    }

    public void SetFaces(Vertex[] v, 
        int TTL, int TTR, int TBR, int TBL, 
        int BTL, int BTR, int BBR, int BBL) {
        
        // top face
        Faces[0] = new Square(this, v, TTL, TTR, TBR, TBL);
        
        // bottom face
        Faces[1] = new Square(this, v, BBL, BBR, BTR, BTL);
        
        // front face
        Faces[2] = new Square(this, v, TTR, TTL, BTL, BTR); 
        
        // back face
        Faces[3] = new Square(this, v, TBL, TBR, BBR, BBL);
        
        // left face
        Faces[4] = new Square(this, v, TTL, TBL, BBL, BTL);
        
        // right face
        Faces[5] = new Square(this, v, TBR, TTR, BTR, BBR);
    }
    
    public void SetFace(Square s) {
        Faces[0] = s;
    }
}

public class Vertex {
     public  bool      Virtual = false;
     public  Vector3   Vertice;
     public  string    Key;
     public  int       Type;
     
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

public class Square {
    public Point                    P;
    private Dictionary <string, int> i = new();
    public  float                      size;
    private Vertex[]                 vertices;
    public  int[]                    indices;
    
    public Square(){}
    
    public Square(Point p, Vertex[] v, int TL, int TR, int BR, int BL) {
        P       = p;
        indices = new[] { TL, TR, BR, BL };
        i.Add(Helpers.VectorKey(v[TL]), TL);
        i.Add(Helpers.VectorKey(v[TR]), TR);
        i.Add(Helpers.VectorKey(v[BR]), BR);
        i.Add(Helpers.VectorKey(v[BL]), BL);
        vertices = new []{v[TL], v[TR], v[BR], v[BL]};
    }
    
    public void CalculateSize() {
        size = (vertices[0].Vertice - vertices[2].Vertice).magnitude;
    }

    public int[] Dump() {
        return new[] {
            indices[0], indices[1], indices[2],
            indices[0], indices[2], indices[3],
        };
    }
}

public class Points {
    private Dictionary<string, Point> list = new();

    public ref Dictionary<string, Point> GetList() {
        return ref list;
    }
    
    public  List<Point> GetPointList() {
        return  list.Values.ToList();
    }
        
    public Dictionary<string, Vertex> Vertices = new();
    
    public Point this[string a] {
        get { return list[a]; }
        set { list[a] = value; }
    }

    public void Add(Vector3 v) {
        list.Add(v.Key(), new Point(v, true));
    }

    public void ClearCheck() {
        foreach (var p in list) {
            p.Value.FacesChecked[0] = false;
            p.Value.FacesChecked[1] = false;
            p.Value.FacesChecked[2] = false;
            p.Value.FacesChecked[3] = false;
            p.Value.FacesChecked[4] = false;
            p.Value.FacesChecked[5] = false;
        }
    }
    
    bool VerticeCondition(bool a, bool b, bool c) {
        return a && b && c || !a && b && c || !a && !b && c || !a && b && !c || a && !b && !c;
    }
    
    int AddVertice(Vertex v, bool real, int type) {
        if (!Vertices.ContainsKey(v.Key)) {
            Vertices.Add(v.Key, v);
            v.Virtual = !real;
            v.Type    = type;
            return Vertices.Count - 1;
        }
        return Vertices.Keys.ToList().IndexOf(v.Key);
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
                return Top(list[v.Key()]);
            case 1: // bottom
                return Down(list[v.Key()]);
            case 2: // front
                return Forward(list[v.Key()]);
            case 3: // front
                return Back(list[v.Key()]);
            case 4: // left
                return Left(list[v.Key()]);
            case 5: // back
                return Right(list[v.Key()]);
            default:
                return false;
        }
    }

    public void FaceState(ref List<Square> ps, Vector3 v, int checkType) {
        if (!Checked(v, checkType) && !ActiveByCheckType(checkType, v)) {
            var p = list[(v).Key()];
            
            var indices = CellState(p);
            p.SetFaces(Vertices.Values.ToArray(), 
                indices[0], indices[1], indices[2], indices[3], 
                indices[4], indices[5], indices[6], indices[7]);
            ps.Add(p.Faces[checkType]);
            list[(v).Key()].FacesChecked[checkType] = true;
        }
    }
    
    public void Hop(ref List<Square> ps, Vector3 start, Vector3 step, int checkType, bool skipStart = true) {
        Vector3 next = start;
        if (skipStart) {
             next = start + step;
        }
        if (ContainsAndActive(next) && !Checked(next, checkType) && !ActiveByCheckType(checkType, next)) {
            var p       = list[(next).Key()];
            
            var indices = CellState(p);
            p.SetFaces(Vertices.Values.ToArray(), 
                indices[0], indices[1], indices[2], indices[3], 
                indices[4], indices[5], indices[6], indices[7]);
            ps.Add(p.Faces[checkType]);
            Hop(ref ps, p.Position, step, checkType);
            
            list[(next).Key()].FacesChecked[checkType] = true;
        }
    }
    
    public void SingleHop(ref List<Square> ps, Vector3 v, int checkType) {
        if (ContainsAndActive(v) && !Checked(v, checkType) && !ActiveByCheckType(checkType, v)) {
            var p = list[v.Key()];
            
            var indices = CellState(p);
            p.SetFaces(Vertices.Values.ToArray(), 
                indices[0], indices[1], indices[2], indices[3], 
                indices[4], indices[5], indices[6], indices[7]);
            ps.Add(p.Faces[checkType]);
            list[v.Key()].FacesChecked[checkType] = true;
        }
    }
    
    public Point GetPointByVector(Vector3 v)               { return list[Helpers.VectorKey(v)];}
    public Point GetPointByIndex(int x, int y, int z) { return GetPointByVector(new Vector3(x, y, z)); }
    public bool  Contains(Vector3 v)         { return list.ContainsKey(Helpers.VectorKey(v)); }
    public bool  IsActive(Vector3 v)         { return list[Helpers.VectorKey(v)].Active; }

    public bool Checked(Vector3 v, int checkType) { return list[Helpers.VectorKey(v)].FacesChecked[checkType]; }
    public void MarkChecked(Vector3 v, bool b, int checkType) { list[Helpers.VectorKey(v)].FacesChecked[checkType] = b; }
    public void SetPointsActive(Vector3 v, bool b) { list[Helpers.VectorKey(v)].Active = b; }
    public bool ContainsAndActive(Vector3 v) { return Contains(v) && IsActive(v); }
    public bool ContainsAndNotActive(Vector3 v) { return Contains(v) && !IsActive(v); }
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
    public bool ContainsAndActiveAt(Vector3 v) {
        return list.ContainsKey(v.Key()) && list[v.Key()].Active;
    }
    // x-axis, z-y
    public bool ForwardTop(Point p) {
        return ContainsAndActive(p.Position + Vector3.forward + Vector3.up);
    }
    public bool ForwardDown(Point p) {
        return ContainsAndActive(p.Position + Vector3.forward + Vector3.down);
    }
    public bool BackTop(Point p) {
        return ContainsAndActive(p.Position + Vector3.back + Vector3.up);
    }
    public bool BackDown(Point p) {
        return ContainsAndActive(p.Position + Vector3.back + Vector3.down);
    }
    // y-axis, x-z
    public bool ForwardRight(Point p) {
        return ContainsAndActive(p.Position + Vector3.forward + Vector3.right);
    }
    public bool ForwardLeft(Point p) {
        return ContainsAndActive(p.Position + Vector3.forward + Vector3.left);
    }
    public bool BackRight(Point p) {
        return ContainsAndActive(p.Position + Vector3.back + Vector3.right);
    }
    public bool BackLeft(Point p) {
        return ContainsAndActive(p.Position + Vector3.back + Vector3.left);
    }
    // z-axis, x-y
    public bool TopRight(Point p) {
        return ContainsAndActive(p.Position + Vector3.up + Vector3.right);
    }
    private bool topleft(Point p) {
        return ContainsAndActive(p.Position + Vector3.up + Vector3.left);
    }
    public bool bottomRight(Point p) {
        return ContainsAndActive(p.Position + Vector3.down + Vector3.right);
    }
    public bool bottomleft(Point p) {
        return ContainsAndActive(p.Position + Vector3.down + Vector3.left);
    }
    public bool Surrounded(Point p) {
        return Top(p) && Down(p) && Left(p) && Right(p) && Forward(p) && Back(p);
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
    
    private Points                    _points;
    //private Dictionary<string, Vertex> Vertices = new();
    // private Vector3[]                  Vertices       = new[] { };
    private Dictionary<string,Vertex> OffsetVertices = new();
    private List<Vector3>                 SurfaceTests    = new ();
    
    private List<int>   Indices = new();
    public  MeshCollider Collider;
    private Vector3     _direction, _hitPoint, _head;
    public Material mat;
    // private new _indiceTracker = 
    
    private Vector3 resolutionOffset;

    private MeshFilter   _mf;
    private MeshRenderer _mr;
    private Mesh         _mesh;
    
    void Awake() {
        Collider      = this.AddComponent<MeshCollider>();
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
        
        for (int x = 0; x < Resolution; x++) {
            for (int y = 0; y < Resolution; y++) {
                for (int z = 0; z < Resolution; z++) {
                    Vector3 newPosition = new Vector3(x, y, z) - resolutionOffset;
                    _points.Add(newPosition);
                }
            }
        }
        StartCoroutine(LittleBabysMarchingCubes(Vector3.zero));
    }

    public class Surface {
        private List<Square> Faces = new ();
        public  Vector3      Normal;
        public  int          SurfaceType;
        
        List<Square> frontList = new ();
        List<Square> backList  = new ();
        List<Square> leftList  = new ();
        List<Square> rightList = new ();
        
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
            
            List<Square> localFront = new List<Square>();
            List<Square> localBack  = new List<Square>();
            List<Square> localLeft  = new List<Square>();
            List<Square> localRight = new List<Square>();
            
            points.Hop(ref localFront, p.Position, frontStep, SurfaceType, skipStart);
            points.Hop(ref localBack, p.Position, backStep, SurfaceType, skipStart);
            points.Hop(ref localLeft, p.Position, leftStep, SurfaceType, skipStart);
            points.Hop(ref localRight, p.Position, rigtStep, SurfaceType, skipStart);
            
            if (localFront.Count != 0) {
                List<Square> tangentLeft  = new List<Square>();
                List<Square> tangentRight = new List<Square>();
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
                List<Square> tangentLeft  = new List<Square>();
                List<Square> tangentRight = new List<Square>();
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
        
        public void AddFront(List<Square> p) {
            frontList.AddRange(p);
        }
        public void AddBack(List<Square> p) {
            backList.AddRange(p);
        }
        public void AddLeft(List<Square> p) {
            leftList.AddRange(p);
        }
        public void AddRight(List<Square> p) {
            rightList.AddRange(p);
        }
        
        bool MergePoints(ref Square vp, ref Square vp1) {
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
            
            vp.CalculateSize();
            vp1.CalculateSize();
            return connected;
        }
        
        List<Square> EvalualatePoints(List<Square> list) {
            if (list.Count == 1) {
                return list;
            }
            
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
            return list;
        }

        public bool Buffered() {
            return frontList.Count != 0 || backList.Count != 0 || leftList.Count != 0 || rightList.Count != 0;
        }

        public List<Square> GetSurfaceFaces() {
            var faces = new List<Square>();
            
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
            
            var final = new List<Square>();
            final.AddRange(EvalualatePoints(Faces));
            foreach (var face in final) {
                faces.Add(face);
            }
            return faces;
        }
    }

    // // top face
    // Faces[0] = new Square(this, v, TTL, TTR, TBR, TBL);
    //     
    // // bottom face
    // Faces[1] = new Square(this, v, BBL, BBR, BTR, BTL);
    //     
    // // front face
    // Faces[2] = new Square(this, v, TTR, TTL, BTL, BTR); 
    //     
    // // back face
    // Faces[3] = new Square(this, v, TBL, TBR, BBR, BBL);
    //     
    // // left face
    // Faces[4] = new Square(this, v, TTL, TBL, BBL, BTL);
    //     
    // // right face
    // Faces[5] = new Square(this, v, TBR, TTR, BTR, BBR);

     
    void HandleSurfaces(ref Dictionary<string, Surface> surfaces, Point np) {
        // handleAdjacentPoints(np.Position + Vector3.down, ref surfaces);  
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

        if (surfaces.ContainsKey(np.Position.ExtractY(0))) {
            surfaces[np.Position.ExtractY(0)].SurfaceHop(np, ref _points);
        }
        if (surfaces.ContainsKey(np.Position.ExtractY(1))) {
            surfaces[np.Position.ExtractY(1)].SurfaceHop(np, ref _points);
        }
        
        if (surfaces.ContainsKey(np.Position.ExtractZ(2))) {
            surfaces[np.Position.ExtractZ(2)].SurfaceHop(np, ref _points);
        }
        if (surfaces.ContainsKey(np.Position.ExtractZ(3))) {
            surfaces[np.Position.ExtractZ(3)].SurfaceHop(np, ref _points);
        }
        
        if (surfaces.ContainsKey(np.Position.ExtractX(4))) {
            surfaces[np.Position.ExtractX(4)].SurfaceHop(np, ref _points);
        }
        if (surfaces.ContainsKey(np.Position.ExtractX(5))) {
            surfaces[np.Position.ExtractX(5)].SurfaceHop(np, ref _points);
        }
    }
    
    // spiral walk
    void Walk2(ref List<Square> faces) {
        List<Point> nullPoints = new();
        var         surfaces   = new Dictionary<string, Surface>();
        
        foreach (var p in _points.GetPointList()) {
            // top and bottom
            if (!_points.Top(p) && p.Active) {
                if (!surfaces.ContainsKey(p.Position.ExtractY(0))) {
                    surfaces.Add(p.Position.ExtractY(0), new Surface(0));
                }
            }
            if (!_points.Down(p) && p.Active) {
                if (!surfaces.ContainsKey(p.Position.ExtractY(1))) {
                    surfaces.Add(p.Position.ExtractY(1), new Surface(1));
                }
            }
            // // front and back
            if (!_points.Forward(p) && p.Active) {
                if (!surfaces.ContainsKey(p.Position.ExtractZ(2))) {
                    surfaces.Add(p.Position.ExtractZ(2), new Surface(2));
                }
            }
            if (!_points.Back(p) && p.Active) {
                if (!surfaces.ContainsKey(p.Position.ExtractZ(3))) {
                    surfaces.Add(p.Position.ExtractZ(3), new Surface(3));
                }
            }
            // left and right
            if (!_points.Left(p) && p.Active) {
                if (!surfaces.ContainsKey(p.Position.ExtractX(4))) {
                    surfaces.Add(p.Position.ExtractX(4), new Surface(4));
                }
                
            } 
            if (!_points.Right(p) && p.Active) {
                if (!surfaces.ContainsKey(p.Position.ExtractX(5))) {
                    surfaces.Add(p.Position.ExtractX(5), new Surface(5));
                }
            }
            
            // void hoppers
            if (!p.Active) {
                nullPoints.Add(p);
            }
        }

        var c = new Vector3(Resolution, Resolution, Resolution) * 0.5f;
        nullPoints.Sort((a, b) => 
            Vector3.Distance(a.Position + resolutionOffset, c).
                CompareTo(Vector3.Distance(b.Position + resolutionOffset, c)));
        
        foreach (var np in nullPoints.ToList()) {
            HandleSurfaces(ref surfaces, np);
        }

        foreach (var surface in surfaces.ToList()) {
            if (surface.Value.Buffered()) {
                faces.AddRange(surface.Value.GetSurfaceFaces());
                 surfaces.Remove(surface.Key);
            }
        }

        if (surfaces.Count != 0) {
            foreach (var surface in surfaces.ToList()) {
                print($"{surface.Key}");
            }
            
            foreach (var np in _points.GetList().ToList()) {
                HandleSurfaces(ref surfaces, np.Value);
            }
            
            foreach (var surface in surfaces.ToList()) {
                if (surface.Value.Buffered()) {
                    faces.AddRange(surface.Value.GetSurfaceFaces());
                    surfaces.Remove(surface.Key);
                }
            }
        }
    }

    IEnumerator LittleBabysMarchingCubes(Vector3 dir) {
        Helpers.ClearConsole();
        
        _mf.mesh       = new Mesh();
        
        _points.Vertices = new Dictionary<string, Vertex>();
        OffsetVertices   = new Dictionary<string, Vertex>();
        SurfaceTests     = new();
        Indices          = new ();
        _mesh            = new Mesh();

        _points.ClearCheck();
        
        var   faces = new List<Square>();
        Walk2(ref faces);

        if (faces.Count == 0) {
            //Walk2(ref faces, true, dir);
        }
        
        faces.Sort((a, b) => a.size.CompareTo(b.size));
        var vertices = _points.Vertices.Values.ToArray();
        var normals  = new List<Vector3>();
        if (faces.Count != 0) {
            for (int i = 0; i < faces.Count; i++) {
                var indices = faces[i].indices;

                if (!OffsetVertices.ContainsKey(Helpers.VectorKey(vertices[indices[0]].Vertice))) {
                    if (vertices[indices[0]].Virtual) {
                        OffsetVertices.Add(
                            Helpers.VectorKey(vertices[indices[0]].Vertice),
                            vertices[indices[0]].Vertice);
                    }
                }

                if (!OffsetVertices.ContainsKey(Helpers.VectorKey(vertices[indices[1]].Vertice))) {
                    if (vertices[indices[1]].Virtual) {
                        OffsetVertices.Add(
                            Helpers.VectorKey(vertices[indices[1]].Vertice), 
                            vertices[indices[1]].Vertice);
                    }
                }
                
                if (!OffsetVertices.ContainsKey(Helpers.VectorKey(vertices[indices[2]].Vertice))) {
                    if (vertices[indices[2]].Virtual) {
                        OffsetVertices.Add(
                            Helpers.VectorKey(vertices[indices[2]].Vertice), 
                            vertices[indices[2]].Vertice);
                    }
                }
                
                if (!OffsetVertices.ContainsKey(Helpers.VectorKey(vertices[indices[3]].Vertice))) {
                    if (vertices[indices[3]].Virtual) {
                        OffsetVertices.Add(
                            Helpers.VectorKey(vertices[indices[3]].Vertice), 
                            vertices[indices[3]].Vertice);
                    }
                }
                
                Indices.AddRange(faces[i].Dump());
            }
        }
 
        _mesh.vertices  = _points.Vertices.Values.ToArray().ToVector3Array(); 
        _mesh.triangles = Indices.ToArray();
        _mesh.normals   = normals.ToArray();
        
        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();

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
            if (_direction != null) {
                if (Input.GetMouseButtonDown(0) && _points.Contains(_hitPoint - _direction/2)) {
                    _points.SetPointsActive(_hitPoint - _direction/2, false);
                    StartCoroutine(LittleBabysMarchingCubes(_direction));
                }
                if (Input.GetMouseButtonDown(1) && _points.Contains(_hitPoint - _direction/2)) {
                    _points.SetPointsActive(_hitPoint - _direction/2, true);
                    StartCoroutine(LittleBabysMarchingCubes(_direction));
                }
            };
        }
        
        if (Input.GetKeyDown(KeyCode.P)) {
            Helpers.ClearConsole();
            Init();
        }
    }

    private void OnDrawGizmos() {
        if (_points == null || _points.GetList() == null || _points.GetList().Count == 0) {
            return;
        }
       
        Gizmos.color = new Color(1,1,1,1f);
        Gizmos.DrawWireCube(
            Vector3.zero, 
            Vector3.one * Resolution);
        Gizmos.color = new Color(1,1,1,.1f);
        foreach (var p in _points.GetList()) {
            if (p.Value.Active) {
                // Gizmos.DrawWireCube(p.Value.Position, Vector3.one);
            }
            else {
                 // Gizmos.DrawCube(p.Value.Position, Vector3.one);
            }
        }

        Gizmos.color = Color.green;
        Gizmos.DrawRay(_hitPoint, _direction);
        Gizmos.DrawSphere(_head, 0.15f);
        
        if (_points == null || _points.Vertices == null || _points.Vertices.Count == 0) {
            return;
        }
        
        // foreach (var p in _points.Vertices) {
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
        //         Gizmos.DrawCube(p.Value, new Vector3(0.1f, 0.1f, 0.1f));
        //     }
        //     else {
        //         Gizmos.color = Color.magenta;
        //     }
        // }
        
        Gizmos.color = Color.magenta;
        foreach (var p in OffsetVertices) {
            // Gizmos.DrawCube(p.Value + new Vector3(0, 0, 0), new Vector3(0.1f, 0.1f, 0.1f));
        }
        Gizmos.color = Color.yellow;
        foreach (var p in SurfaceTests) {
            // Gizmos.DrawCube(p , new Vector3(0.3f, 0.3f, 0.3f));
        }
    }
}
