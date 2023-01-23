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
    public bool        Active   = false;
    public bool        Checked  = false;
    public Vector3     Position = new();

    public Point(){}
    public Point(Vector3 p, bool a) {
        Active   = a;
        Position = p;
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
    private Vertex[]                 vertices;
    private int[]                    indices;
    
    public Square(){}
    
    public Square(Point p, Vector3[] v, int TL, int TR, int BR, int BL) {
        indices = new[] { TL, TR, BR, BL };
        p       = this.p;
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

    public void Hop(ref List<Point> ps, Vector3 start, Vector3 step) {
        if (ContainsAndActive(start + step) && !Checked(start + step)) {
            Hop(ref ps, start + step, step);
            ps.Add(list[(start + step).Key()]);
            list[(start + step).Key()].Checked = true;
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
    private Dictionary<string, Vertex> Vertices = new();
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
        Vertices         = new Dictionary<string, Vertex>();
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

    int AddVertice(Vertex v, bool real, int type) {
        if (!Vertices.ContainsKey(v.Key)) {
            Vertices.Add(v.Key, v);
            v.Virtual = !real;
            v.Type    = type;
            return Vertices.Count - 1;
        }
        // print($"duplicate found");
        // if (Vertices[v.Key].Virtual) {
        //OffsetVertices.Add(v.Key, v);
        return Vertices.Keys.ToList().IndexOf(v.Key);
    }

    bool VerticeCondition(bool a, bool b, bool c) {
        return a && b && c || !a && b && c || !a && !b && c || !a && b && !c || a && !b && !c;
    }
    
    
    //     0 + step, -- Top Left
    //     1 + step, -- Top Right
    //     2 + step, -- Bottom Right
    
    //     0 + step, -- Top Left
    //     3 + step, -- Bottom Left
    //     2 + step  -- Bottom Right
    // Dictionary<int, int> GenerateVerticesByRule(Point p) {
    //     Dictionary<int, int> caseIndex = new();
    //
    //     // if completely surrounded, just ignore 🙄
    //     if (_points.Surrounded(p)) {
    //         p.Active = false;
    //         return new ();
    //     }
    //
    //     
    //     if (VerticeCondition(!_points.ForwardLeft(p), !_points.Left(p), !_points.Forward(p))) {
    //         caseIndex.Add(0, -1);
    //     }
    //     if (VerticeCondition(!_points.ForwardRight(p), !_points.Right(p), !_points.Forward(p))) {
    //         caseIndex.Add(1, -1);
    //     }
    //     if (VerticeCondition(!_points.BackRight(p), !_points.Right(p), !_points.Back(p))) {
    //         caseIndex.Add(2, -1);
    //     }
    //     if (VerticeCondition(!_points.BackLeft(p), !_points.Left(p), !_points.Back(p))) {
    //         caseIndex.Add(3, -1);
    //     }
    //     
    //     Dictionary<int, int> indiceMap = new();
    //     int caseState = Case(Helpers.CaseKey(caseIndex));
    //
    //     switch (caseState) {
    //         case 7: case 11: case 13: case 14: case 15: 
    //             // top left
    //             indiceMap.Add(0, AddVertice(p.Position + (Vector3.forward + Vector3.left + Vector3.up) / 2));
    //             // top right
    //             indiceMap.Add(1, AddVertice(p.Position + (Vector3.forward + Vector3.right + Vector3.up) / 2));
    //             // bottom right
    //             indiceMap.Add(2, AddVertice(p.Position + (Vector3.back + Vector3.right + Vector3.up) / 2));
    //             // bottom left
    //             indiceMap.Add(3, AddVertice(p.Position + (Vector3.back + Vector3.left + Vector3.up) / 2));
    //             
    //             var square = new Square(p, Vertices.Values.ToArray(),
    //                 indiceMap[0], indiceMap[1], indiceMap[2], indiceMap[3]);
    //             Indices.AddRange(square.Dump());
    //             
    //             break;
    //         case 1 : // top left
    //             AddVertice(p.Position + (Vector3.forward + Vector3.left + Vector3.up) / 2);
    //             break;
    //         case 2: // top right
    //             AddVertice(p.Position + (Vector3.forward + Vector3.right + Vector3.up) / 2);
    //             break;
    //         case 4: // bottom right
    //             AddVertice(p.Position + (Vector3.back + Vector3.right + Vector3.up) / 2);
    //             break;
    //         case 8: // bottom left
    //             AddVertice(p.Position + (Vector3.back + Vector3.left + Vector3.up) / 2);
    //             break;
    //     }
    //     
    //     return indiceMap;
    // }

    // "1111" - full : TL, TR, BR, BL
    //  TL ---- TR
    //  |        |
    //  |        |
    //  BL ---- BR
    // "0000" - empty
    int Case(string caseKey) {
        switch (caseKey) { 
            //  o -- 
            //  |    |
            //    --
             case "1000": // case 1
                 return 1;
            //    -- o
            //  |    |
            //    --
            case "0100": // case 2
                return 2;
            //  o -- o
            //  |    |
            //    --
            case "1100": // case 3
                return 3;
            //    --  
            //  |    |
            //    -- o
            case "0010": // case 4
                return 4;
            //  o --  
            //  |    |
            //    -- o
            case "1010": // case 5
                return 5;
            //    -- o 
            //  |    |
            //    -- o
            case "0110":
                return 6;
            //  o -- o 
            //  |    |
            //    -- o
            case "1110": // case 7
                return 7;
            //    -- 
            //  |    |
            //  o -- 
            case "0001": // case 8
                return 8;
            //  o -- 
            //  |    |
            //  o --
            case "1001": // case 9
                return 9;
             //    -- o
             //  |    |
             //  o --
            case "0101": // case 10
                return 10;
            //  o -- o
            //  |    |
            //  o --
            case "1101": // case 11
                return 11;
            //    -- 
            //  |    |
            //  o -- o
            case "0011": // case 12
                return 12;
            //  o -- 
            //  |    |
            //  o -- o
            case "1011": // case 13
                return 13;
            //    -- o
            //  |    |
            //  o -- o
            case "0111": // case 14
                return 14;
            //  o -- o
            //  |    |
            //  o -- o
            case "1111":
                return 15;
            default:
                return 0;
        }
    }

    Dictionary<int, int> CellState(Point p) {
        Dictionary<int, int> caseIndex = new();
        
        // TL
        caseIndex.Add(0, AddVertice(p.Position + (Vector3.forward + Vector3.left + Vector3.up) / 2, 
            VerticeCondition(!_points.ForwardLeft(p), !_points.Left(p), !_points.Forward(p)), 0));
        // TR
        caseIndex.Add(1, AddVertice(p.Position + (Vector3.forward + Vector3.right + Vector3.up) / 2,
            VerticeCondition(!_points.ForwardRight(p), !_points.Right(p), !_points.Forward(p)), 1));
        // BR
        caseIndex.Add(2, AddVertice(p.Position + (Vector3.back + Vector3.right + Vector3.up) / 2, 
            VerticeCondition(!_points.BackRight(p), !_points.Right(p), !_points.Back(p)), 2));
        // BL
        caseIndex.Add(3,AddVertice(p.Position + (Vector3.back + Vector3.left + Vector3.up) / 2,
            VerticeCondition(!_points.BackLeft(p), !_points.Left(p), !_points.Back(p)), 3));

        return caseIndex; 
    }
    
    void Walk(bool inverted) {
        int     flipFlop   = 1;
        Vector3 walkVector = new Vector3(0, Resolution - 1, Resolution - 1) - resolutionOffset;
         
         while (true) { // break added
             if (walkVector.x > Resolution - 1 || walkVector.x < - resolutionOffset.x) {
                 walkVector.x =  - resolutionOffset.x;
                 walkVector.z -= 1;
             }
             if (walkVector.z < -resolutionOffset.z) {
                 break;
             }
        
             if (_points.Contains(walkVector) && !_points.IsActive(walkVector) && !_points.Checked(walkVector)) {
                 Point p   = _points[Helpers.VectorKey(walkVector)];
                 _head = p.Position;

                 List<Point> frontHopList = new List<Point>();
                 _points.Hop(ref frontHopList, p.Position, Vector3.forward);
                 
                 List<Point> leftHopList = new List<Point>();
                 _points.Hop(ref leftHopList, p.Position, Vector3.left);
                 
                 List<Point> rightHopList = new List<Point>();
                 _points.Hop(ref rightHopList, p.Position, Vector3.right);
                 
                 List<Point> backHopList = new List<Point>();
                 _points.Hop(ref backHopList, p.Position, Vector3.back);
     
                 for (int i = 0; i < frontHopList.Count; i++) {
                     var vp      = frontHopList[i];
                     var vp1      = frontHopList[(i + 1) % frontHopList.Count];

                     var indices = CellState(vp);
                     var TL1     = indices[0];
                     var TR1     = indices[1];
                     var BR1     = indices[2];
                     var BL1     = indices[3];
                     
                     var indices2 = CellState(vp1);
                     var TL2      = indices2[0];
                     var TR2      = indices2[1];
                     var BR2      = indices2[2];
                     var BL2      = indices2[3];

                     var vertices = Vertices.Values.ToArray().ToVector3Array();
                     if (BL2 == TL1) {
                         TL1 = TL2;
                         //Vertices.Remove(vertices[BL2].Key());
                         // Vertices.Remove(vertices[TL2].Key());
                     }
                     if (BR2 == TR1) {
                         TR1 = TR2;
                         //Vertices.Remove(vertices[BR2].Key());
                     }
                     
                     if (TR2 == BR1) {
                         BR1 = BR2;
                         //Vertices.Remove(vertices[TR2].Key());
                     }
                     if (TL2 == TR1) {
                         BL1 = BL2;
                         //Vertices.Remove(vertices[TL2].Key());
                     }
                     
                     
                     var square = new Square(vp, vertices, TL1, TR1, BR1, BL1);
                     
                     // var square2 = new Square(vp, Vertices.Values.ToArray().ToVector3Array(), 
                     //     indices2[0], indices2[1], indices2[2], indices2[3]);
                     
                     
                     Indices.AddRange(square.Dump());
                     
                 }
                 
                 foreach (var vp in leftHopList) {
                     var indices = CellState(vp);
                     var square = new Square(vp, Vertices.Values.ToArray().ToVector3Array(), 
                         indices[0], indices[1], indices[2], indices[3]);
                     Indices.AddRange(square.Dump());
                 }
                 
                 foreach (var vp in rightHopList) {
                     var indices = CellState(vp);
                     var square = new Square(vp, Vertices.Values.ToArray().ToVector3Array(), 
                         indices[0], indices[1], indices[2], indices[3]);
                     Indices.AddRange(square.Dump());
                 }
                 
                 foreach (var vp in backHopList) {
                     var indices = CellState(vp);
                     var square = new Square(vp, Vertices.Values.ToArray().ToVector3Array(), 
                         indices[0], indices[1], indices[2], indices[3]);
                     Indices.AddRange(square.Dump());
                 }
             }
             walkVector.x += 1 * flipFlop;
         }
    }

    IEnumerator LittleBabysMarchingCubes() {
        Helpers.ClearConsole();
        
        _mf.mesh       = new Mesh();
        
        // stores position key and indice
        Vertices       = new Dictionary<string, Vertex>();
        OffsetVertices = new Dictionary<string, Vertex>();
        Indices        = new ();
        _mesh          = new Mesh();
        TestIndices    = new List<int>();
        // stepping top x-y

        _points.ClearCheck();
        
        Point head = new Point();
        Walk(true);
        
        _mesh.vertices  = Vertices.Values.ToArray().ToVector3Array(); 
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
        
        if (Vertices == null || Vertices.Count == 0) {
            return;
        }
        
        foreach (var p in Vertices) {

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
            }
            else {
                Gizmos.color = Color.magenta;
            }
            Gizmos.DrawCube(p.Value, new Vector3(0.1f, 0.1f, 0.1f));
        }
        
        Gizmos.color = Color.cyan;
        foreach (var p in OffsetVertices) {
            Gizmos.DrawCube(p.Value + new Vector3(0, 0, 0), new Vector3(0.1f, 0.1f, 0.1f));
        }
    }
}
