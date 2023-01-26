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
}
    
public class Point {
    public bool    Active   = false;
    public bool    Checked  = false;
    public Vector3 Position = new();
    public Square  Face;
    
    public Point(){}
    public Point(Vector3 p, bool a) {
        Active   = a;
        Position = p;
    }

    public void SetFace(Vertex[] v, int TL, int TR, int BR, int BL) {
        Face = new Square(v, TL, TR, BR, BL);
    }
    
    public void SetFace(Square s) {
        Face = s;
    }
}

public class Vertex {
     public  bool      Virtual = false;
     public  Vector3   Vertice;
     private Vector3[] Connected;
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
    private Point                    p;
    private Dictionary <string, int> i = new();
    public  float                      size;
    // private Vertex[]                 vertices;
    public  int[]                    indices;
    
    public Square(){}
    
    public Square(Vertex[] v, int TL, int TR, int BR, int BL) {
        indices  = new[] { TL, TR, BR, BL };
        // vertices = new[] { v[TL], v[TR], v[BR], v[BL] };
        p        = this.p;
        i.Add(Helpers.VectorKey(v[TL]), TL);
        i.Add(Helpers.VectorKey(v[TR]), TR);
        i.Add(Helpers.VectorKey(v[BR]), BR);
        i.Add(Helpers.VectorKey(v[BL]), BL);
    }
    
    public bool Contains(string key) {
        return i.ContainsKey(key);
    }
    
    public Square MergeTop() {
        var newSquare = new Square();
        return newSquare;
    }

    public void CalculateSize(Vertex a, Vertex b) {
        size = (a.Vertice - b.Vertice).magnitude;
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
            p.Value.Checked = false;
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

        return caseIndex; 
    }
    
    public void Hop(ref List<Point> ps, Vector3 start, Vector3 step) {
         Vector3 next = start + step;
        if (ContainsAndActive(next) && !Checked(next)) {
            var p       = list[(next).Key()];
            ps.Add(p);
            var indices = CellState(p);
            p.SetFace(Vertices.Values.ToArray(), 
                indices[0], indices[1], indices[2], indices[3]);
            
            Hop(ref ps, p.Position, step);
            list[(next).Key()].Checked = true;
        }
    }
    
    public Point GetPointByVector(Vector3 v)               { return list[Helpers.VectorKey(v)];}
    public Point GetPointByIndex(int x, int y, int z) { return GetPointByVector(new Vector3(x, y, z)); }
    public bool  Contains(Vector3 v)         { return list.ContainsKey(Helpers.VectorKey(v)); }
    public bool  IsActive(Vector3 v)         { return list[Helpers.VectorKey(v)].Active; }
    public bool  Checked(Vector3 v)         { return list[Helpers.VectorKey(v)].Checked; }
    public void  MarkChecked(Vector3 v, bool b)         {  list[Helpers.VectorKey(v)].Checked = b; }
    public void  SetPointsActive(Vector3 v, bool b) { list[Helpers.VectorKey(v)].Active = b; }
    public bool  ContainsAndActive(Vector3 v) { return Contains(v) && IsActive(v); }
    public bool  ContainsAndNotActive(Vector3 v) { return Contains(v) && !IsActive(v); }
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
    private List<int>                 TestIndices    = new ();
    
    private List<int>   Indices = new();
    public  BoxCollider Collider;
    private Vector3     _direction, _hitPoint, _head;
    
    // private new _indiceTracker = 
    
    private Vector3 resolutionOffset;

    private MeshFilter   _mf;
    private MeshRenderer _mr;
    private Mesh         _mesh;
    
    void Awake() {
        Collider      = this.AddComponent<BoxCollider>();
        Collider.size = new Vector3(Resolution, Resolution, Resolution);
        
        _mf          = this.AddComponent<MeshFilter>();
        _mr          = this.AddComponent<MeshRenderer>();
        _mr.material = new Material(Shader.Find("Diffuse"));
        
        Init();
    }

    void Init() {
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
    }

    Point EvalualatePoints(List<Point> list, bool first = true) {
        var vertices = _points.Vertices.ToArray();
        for (int i = 0; i < list.Count; i++) {
            var vp = list[i];
            for (int k = 0; k < list.Count; k++) {
                var vp1 = list[k];
                if (i == k) {continue;}

                var TL1 = vp.Face.indices[0];
                var TR1 = vp.Face.indices[1];
                var BR1 = vp.Face.indices[2];
                var BL1 = vp.Face.indices[3];
                     
                var TL2 = vp1.Face.indices[0];
                var TR2 = vp1.Face.indices[1];
                var BR2 = vp1.Face.indices[2];
                var BL2 = vp1.Face.indices[3];
                     
                if (BL2 == TL1) {
                    vp.Face.indices[0] = TL2;
                }
                if (BR2 == TR1) {
                    vp.Face.indices[1] = TR2;
                }
                if (TR2 == BR1) {
                    vp.Face.indices[2] = BR2;
                }
                if (TL1 == TR2) {
                    vp.Face.indices[0] = TL2;
                }
                if (BL1 == TL2) {
                    vp.Face.indices[3] = BL2;
                }
                if (BL1 == BR2) {
                    vp.Face.indices[3] = BL2;
                }
                vp.Face.CalculateSize(vertices[vp.Face.indices[0]].Value, vertices[vp.Face.indices[2]].Value);
            }
        }
        list.Sort((a, b) => b.Face.size.CompareTo(a.Face.size));
        if (first) {
            return list[0];
        }
        return list.Last();
    }

    void AdjacentHope(Point p, ref List<Point> faces, bool f, bool b, bool l, bool r) {
        if (f) {
            List<Point> frontHopList = new List<Point>();
            _points.Hop(ref frontHopList, p.Position, Vector3.forward);
            
            if (frontHopList.Count != 0) {
                print($"front hop {frontHopList.Count}");
                foreach (var fp in frontHopList) {
                    AdjacentHope(fp, ref faces, false, false, true, true);
                }
                faces.Add(EvalualatePoints(frontHopList));
            }
        }

        if (l) {
            List<Point> leftHopList = new List<Point>();
            _points.Hop(ref leftHopList, p.Position, Vector3.left);
            
            if (leftHopList.Count != 0) {
                foreach (var lp in leftHopList) {
                    AdjacentHope(lp, ref faces, false, false, false, false);
                }
                faces.Add(EvalualatePoints(leftHopList));
            }
        }

        if (r) {
            List<Point> rightHopList = new List<Point>();
            _points.Hop(ref rightHopList, p.Position, Vector3.right);
            
            if (rightHopList.Count != 0 ) {
                foreach (var rp in rightHopList) {
                    AdjacentHope(rp, ref faces, false, false, false, false);
                }
                faces.Add(EvalualatePoints(rightHopList));
            }
        }

        if (b) {
            List<Point> backHopList = new List<Point>();
            _points.Hop(ref backHopList, p.Position, Vector3.back);
            
            if (backHopList.Count != 0 && b) {
                foreach (var bp in backHopList) {
                    AdjacentHope(bp, ref faces, false, false, true, true);
                }
                faces.Add(EvalualatePoints(backHopList));
            }
        }
    }
    
    // spiral walk
    void Walk2(ref List<Point> faces) {
        List<Point> nullPoints = new();
        foreach (var npoint in _points.GetPointList()) {
            if (!npoint.Checked && !npoint.Active) {
                nullPoints.Add(npoint);
            }
        }

        var c = new Vector3(Resolution, Resolution, Resolution) - resolutionOffset;
        nullPoints.Sort((a, b) => Vector3.Distance(a.Position + resolutionOffset, c).CompareTo(Vector3.Distance(b.Position + resolutionOffset, c)));
        
        foreach (var p in nullPoints) {
            _head = p.Position;
            AdjacentHope(p, ref faces, true, true, true, true);
        }
    }
    
    // scan walk
    void Walk(ref List<Point> faces) {
        int     flipFlop   = 1;
        Vector3 walkVector = new Vector3(0, Resolution - 1, Resolution - 1) - resolutionOffset;

        bool streaming = false;
         while (true) { // break added
             if (walkVector.x > Resolution - 1 || walkVector.x < - resolutionOffset.x) {
                 walkVector.x =  - resolutionOffset.x;
                 walkVector.z -= 1;
             }
             if (walkVector.z < -resolutionOffset.z) {
                 break;
             }

             if (_points.Contains(walkVector) && !_points.Checked(walkVector) && !_points.IsActive(walkVector)) {
                     streaming = true;
                     
                     Point p = _points[Helpers.VectorKey(walkVector)];
                     _head = p.Position;
                     AdjacentHope(p, ref faces, true, true, true, true);
                     
             } else {
                 if (streaming) {
                     print($"new face set needed bro"); 
                 }
             }
             walkVector.x += 1 * flipFlop;
         }
    }

    IEnumerator LittleBabysMarchingCubes() {
        Helpers.ClearConsole();
        
        _mf.mesh       = new Mesh();
        
        // stores position key and indice
        _points.Vertices       = new Dictionary<string, Vertex>();
        OffsetVertices = new Dictionary<string, Vertex>();
        Indices        = new ();
        _mesh          = new Mesh();
        TestIndices    = new List<int>();
        // stepping top x-y

        _points.ClearCheck();
        
        var   faces = new List<Point>();
        Walk2(ref faces);
        
        faces.Sort((a, b) => a.Face.size.CompareTo(b.Face.size));
        if (faces.Count != 0) {
            for (int i = 0; i < faces.Count; i++) {
                Indices.AddRange(faces[i].Face.Dump());
            }
        }
 
        _mesh.vertices  = _points.Vertices.Values.ToArray().ToVector3Array(); 
        _mesh.triangles = Indices.ToArray();
        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();
        
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
            if (Input.GetMouseButtonDown(0) && _points.Contains(_hitPoint - _direction/2)) {
                _points.SetPointsActive(_hitPoint - _direction/2, false);
                StartCoroutine(LittleBabysMarchingCubes());
            }
            if (Input.GetMouseButtonDown(1) && _points.Contains(_hitPoint - _direction/2)) {
                _points.SetPointsActive(_hitPoint - _direction/2, true);
                StartCoroutine(LittleBabysMarchingCubes());
            }
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
       
        Gizmos.color = new Color(1,1,1,.4f);
        foreach (var p in _points.GetList()) {
            if (p.Value.Active) Gizmos.DrawWireCube(p.Value.Position, Vector3.one);
        }
        
        Gizmos.color = Color.green;
        Gizmos.DrawRay(_hitPoint, _direction);
        Gizmos.DrawSphere(_head, 0.15f);
        
        if (_points == null || _points.Vertices == null || _points.Vertices.Count == 0) {
            return;
        }
        
        foreach (var p in _points.Vertices) {
            if (!p.Value.Virtual) {
                if (p.Value.Type == 0) {
                    Gizmos.color = Color.blue;
                } else if (p.Value.Type == 1) {
                    Gizmos.color = Color.red;
                } else if (p.Value.Type == 2) {
                    Gizmos.color = Color.yellow;
                } else if (p.Value.Type == 3) {
                    Gizmos.color = Color.green;
                } 
                Gizmos.DrawCube(p.Value, new Vector3(0.1f, 0.1f, 0.1f));
            }
            else {
                Gizmos.color = Color.magenta;
            }
          
        }
        
        Gizmos.color = Color.cyan;
        foreach (var p in OffsetVertices) {
            Gizmos.DrawCube(p.Value + new Vector3(0, 0, 0), new Vector3(0.1f, 0.1f, 0.1f));
        }
    }
}
