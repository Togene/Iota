using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

// https://www.theappguruz.com/blog/simple-cube-mesh-generation-unity3d

public class Point {
    public bool        Active   = false;
    public Vector3     Position = new();
    public BoxCollider Collider = new ();

    public Point(){}
    public Point(Vector3 p, bool a) {
        Active   = a;
        Position = p;
    }
}

public class Points {
    private Dictionary<string, Point> list = new Dictionary<string, Point>();

    public ref Dictionary<string, Point> GetList() {
        return ref list;
    }
    
    public Point this[string a] {
        get { return list[a]; }
        set { list[a] = value; }
    }
    
    public void Add(Vector3 v) {
        list.Add(Helpers.VectorKey(v), new Point(v, true));
    }
    
    public Point GetPointByVector(Vector3  v)               { return list[Helpers.VectorKey(v)];}
    public Point GetPointByIndex(int       x, int y, int z) { return GetPointByVector(new Vector3(x, y, z)); }
    public bool  Contains(Vector3          v)         { return list.ContainsKey(Helpers.VectorKey(v)); }
    public bool  IsActive(Vector3          v)         { return list[Helpers.VectorKey(v)].Active; }
    public void  SetPointsActive(Vector3   v, bool b) { list[Helpers.VectorKey(v)].Active = b; }
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

// Hashed Array! Dictionary of Indices, point to Array of Vertices
public class NullableInt {
    private int I { get; set; }
    
    // public object this[int i]
    // {
    //     get { return InnerList[i]; }
    //     set { InnerList[i] = value; }
    // }
    
    NullableInt() { }
    
    NullableInt(int i) {
        I = i;
    }
    public int Value() {
        return I;
    }
    public static implicit operator NullableInt(int d) => new (d);
    public override                 string ToString()  => $"{I}";
}

public class Trixel : MonoBehaviour {
    [Range(2, 16)] public int                       Resolution;
    [Range(0.0f, 16.0f)] public float               RenderSpeed;
    
    private Points                     _points;
    private Dictionary<string,Vector3> Vertices  = new();
    // private Vector3[]                  Vertices       = new[] { };
    private Dictionary<string,Vector3> OffsetVertices = new();
    private List<int> TestIndices = new ();
    
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
        Vertices         = new Dictionary<string, Vector3>();
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

    int AddVertice(Vector3 v) {
        if (!Vertices.ContainsKey(Helpers.VectorKey(v))) {
            Vertices.Add(Helpers.VectorKey(v), v);
            return Vertices.Count - 1;
        }
        return Vertices.Values.ToList().IndexOf(v);
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
    Dictionary<int, int> GenerateVerticesByRule(Point p) {
        Dictionary<int, int> indiceMap = new();

        // if completely surrounded, just ignore 🙄
        if (_points.Surrounded(p)) {
            p.Active = false;
            return indiceMap;
        }

        // top left
        if (VerticeCondition(!_points.ForwardLeft(p), !_points.Left(p), !_points.Forward(p))) {
            indiceMap.Add(0, AddVertice(p.Position + (Vector3.forward + Vector3.left + Vector3.up) / 2));
        }
        // top right
        if (VerticeCondition(!_points.ForwardRight(p), !_points.Right(p), !_points.Forward(p))) {
            indiceMap.Add(1, AddVertice(p.Position + (Vector3.forward + Vector3.right + Vector3.up) / 2));
        }
        // bottom right
        if (VerticeCondition(!_points.BackRight(p), !_points.Right(p), !_points.Back(p))) {
            indiceMap.Add(2, AddVertice(p.Position + (Vector3.back + Vector3.right + Vector3.up) / 2));
        }
        // bottom left
        if (VerticeCondition(!_points.BackLeft(p), !_points.Left(p), !_points.Back(p))) {
            indiceMap.Add(3,  AddVertice(p.Position + (Vector3.back + Vector3.left + Vector3.up) / 2));
        }
        return indiceMap;
    }

    void AddBasicTriangle(int[] i) {
        Indices.AddRange(new [] {
            i[0], i[1], i[2],
        });
    }
    
    // "1111" - full : TL, TR, BR, BL
    //  TL ---- TR
    //  |        |
    //  |        |
    //  BL ---- BR
    // "0000" - empty
    private Dictionary<int, NullableInt[]> heldCases = new();
    string Case(string caseKey, int[] i) {
        switch (caseKey) { 
             //  o --
             //  |
             case "1000": // case 1
                return($"{1}");
            /*
             *  -- o
             *     |
             */
            case "0100":
                if (heldCases.ContainsKey(13)) { // case 2 has a case 13
                    var deloadedIndices = heldCases[13];
                    deloadedIndices[1] = i[0];

                    foreach (var nullInt in deloadedIndices) {
                        Indices.Add(nullInt.Value());
                    }
                    //Indices.AddRange(new []{deloadedIndices[2].Value(), i[0], i[1]});
                    heldCases.Remove(13);
                }
                return($"{2}");
            /*
            *  o -- o
            *  |    |
            */
            case "1100":
                return($"{3}");
            /*
           *      |
           *   -- o
           */
            case "0010":
                return($"{4}");
            case "1010":
                return($"{5}");
            case "0110":
                return($"{6}");
            case "1110": // case 7
                if (heldCases.ContainsKey(11)) {
                    var deloadedIndices = heldCases[11];
                    deloadedIndices[2] = i[0];

                    foreach (var nullInt in deloadedIndices) {
                        Indices.Add(nullInt.Value());
                    }
                    
                    Indices.AddRange(new []{ i[0], i[2], deloadedIndices[0].Value()});
                    heldCases.Remove(11);
                }
                
                AddBasicTriangle(i);
                return($"{7}");
            case "0001":
                return($"{8}");
            case "1001":
                return($"{9}");
            case "0101":
                return($"{10}");
            case "1101":
                // hold the case for another triangle
                if (!heldCases.ContainsKey(11)) {
                    heldCases.Add(11, new NullableInt[]{i[2], i[1], null});
                }
                
                AddBasicTriangle(i);
                return($"{11}");
            case "0011":
                return($"{12}");
            case "1011":
                AddBasicTriangle(i);
                
                // hold the case for another triangle
                if (!heldCases.ContainsKey(13)) {
                    heldCases.Add(13, new NullableInt[]{i[0], null, i[1]});
                }
                
                return($"{13}");
            case "0111":
                if (heldCases.ContainsKey(13)) { // case 7 has a case 13
                    var deloadedIndices = heldCases[13];
                    deloadedIndices[1] = i[0];

                    foreach (var nullInt in deloadedIndices) {
                        Indices.Add(nullInt.Value());
                    }
                    
                    Indices.AddRange(new []{deloadedIndices[2].Value(), i[0], i[1]});
                    heldCases.Remove(13);
                }
                
                AddBasicTriangle(i);
                return($"{14}");
            case "1111":
                Indices.AddRange(new [] {
                    i[0], i[1], i[2],
                    i[0], i[2], i[3],
                });
                return($"{15}");
            default:
                return($"{0}");
        }
    }
    
    // List<Tuple<int, int>> Walk(ref Point head, bool inverse, bool duplicate) {
    //  
    // }

    IEnumerator LittleBabysMarchingCubes() {
        Helpers.ClearConsole();
        
        _mf.mesh       = new Mesh();
        
        // stores position key and indice
        Vertices       = new Dictionary<string, Vector3>();
        
        OffsetVertices = new Dictionary<string, Vector3>();
        Indices        = new ();
        _mesh          = new Mesh();
        TestIndices    = new List<int>();
        heldCases      = new();
        // stepping top x-y
       
       
        Point head         = new Point();
        Vector3 walkVector = new Vector3(0, Resolution - 1, Resolution - 1) - resolutionOffset;
        int     flipFlop   = 1;
        bool    done       = false;
        while (!done) {
            if (walkVector.x > Resolution - 1 || walkVector.x < - resolutionOffset.x) {
                walkVector.x =  - resolutionOffset.x;
                walkVector.z -= 1;
            }
            if (walkVector.z < -resolutionOffset.z) {
                done = true;
            }
        
            if (_points.Contains(walkVector) && _points.IsActive(walkVector)) {
                head  = _points[Helpers.VectorKey(walkVector)];
                _head = head.Position;
                
                var newIndices = GenerateVerticesByRule(head);
                string caseKey = $"" +
                                 $"{(newIndices.ContainsKey(0) ? 1 : 0)}" +
                                 $"{(newIndices.ContainsKey(1) ? 1 : 0)}" +
                                 $"{(newIndices.ContainsKey(2) ? 1 : 0)}" +
                                 $"{(newIndices.ContainsKey(3) ? 1 : 0)}" +
                                 $"";
                print($"case: {Case(caseKey, newIndices.Values.ToArray())} # count: {newIndices.Count}");
            }
            walkVector.x += 1 * flipFlop;
            yield return new WaitForSeconds(RenderSpeed);
        }
        
        _mesh.vertices  = Vertices.Values.ToArray();
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
        Gizmos.color = Color.magenta;
        foreach (var p in Vertices) {
            Gizmos.DrawCube(p.Value, new Vector3(0.1f, 0.1f, 0.1f));
        }
       
        Gizmos.color = Color.blue;
        foreach (var p in OffsetVertices) {
            Gizmos.DrawCube(p.Value, new Vector3(0.1f, 0.1f, 0.1f));
        }

        if (TestIndices == null || TestIndices.Count == 0) {
            return;
        }
        
        Gizmos.color = Color.magenta;
        for (int i = 0; i < TestIndices.Count; i+=2) {
            var a = Vertices.Values.ToArray()[TestIndices[i]];
            var b = Vertices.Values.ToArray()[TestIndices[(i + 1)]];
            Gizmos.DrawWireSphere(a, 0.125f);
            Gizmos.DrawWireSphere(b, 0.125f);
            Gizmos.DrawLine(a, b);  
        }
        Gizmos.DrawGUITexture(new Rect(1,1,1,1), Texture2D.normalTexture);
    }
}
