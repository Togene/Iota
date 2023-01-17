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

    int AddVertice(Vector3 v, bool useDuplicate = false) {
        if (!Vertices.ContainsKey(Helpers.VectorKey(v))) {
            Vertices.Add(Helpers.VectorKey(v), v);
            return Vertices.Count - 1;
        }
        if (!useDuplicate) {
            return -1;
        }
        return Vertices.Values.ToList().IndexOf(v);
    }

    bool VerticeCondition(bool a, bool b, bool c, bool invert) {
        if (invert) {
            a = !a;
            b = !b;
            c = !c;
        }
        return (a && b && c) || !a && b && c || !a && !b && c || !a && b && !c;
    }
    
    //     0 + step, -- Top Left
    //     1 + step, -- Top Right
    //     2 + step, -- Bottom Right
    
    //     0 + step, -- Top Left
    //     3 + step, -- Bottom Left
    //     2 + step  -- Bottom Right
    Dictionary<int, int> GenerateVerticesByRule(Point p, bool inverse, bool duplicate = false) {
        
        Dictionary<int, int> indiceMap = new();

        // if completely surrounded, just ignore 🙄
        if (_points.Surrounded(p)) {
            p.Active = false;
            return indiceMap;
        }

        // top left
        if (VerticeCondition(!_points.ForwardLeft(p), !_points.Left(p), !_points.Forward(p), inverse)) {
            int index = AddVertice(p.Position + (Vector3.forward + Vector3.left + Vector3.up) / 2, duplicate);
            if (index != -1) {
                indiceMap.Add(0, index);
            }
        }
        // top right
        if (VerticeCondition(!_points.ForwardRight(p), !_points.Right(p), !_points.Forward(p), inverse)) {
            int index = AddVertice(p.Position + (Vector3.forward + Vector3.right + Vector3.up) / 2, duplicate);
            if (index != -1) {
                indiceMap.Add(1, index);
            }
        }
        // bottom right
        if (VerticeCondition(!_points.BackRight(p), !_points.Right(p), !_points.Back(p), inverse)) {
            int index = AddVertice(p.Position + (Vector3.back + Vector3.right + Vector3.up) / 2, duplicate);
            if (index != -1) {
                indiceMap.Add(2, index);
            }
        }
        // bottom left
        if (VerticeCondition(!_points.BackLeft(p), !_points.Left(p), !_points.Back(p), inverse)) {
            int index = AddVertice(p.Position + (Vector3.back + Vector3.left + Vector3.up) / 2, duplicate);
            if (index != -1) {
                indiceMap.Add(3, index);
            }
        }
        return indiceMap;
    }

    IEnumerator LittleBabysMarchingCubes() {
        Helpers.ClearConsole();
        
        _mf.mesh       = new Mesh();
        
        // stores position key and indice
        Vertices       = new Dictionary<string, Vector3>();
        // raw vertices
        // Vertices          = 
        
        OffsetVertices = new Dictionary<string, Vector3>();
        Indices        = new ();
        _mesh          = new Mesh();
        
        // final rectangles
        rectangles = new List<Rectangle>();
        
        // edges
        top        = new (); 
        bottom     = new (); 
        left       = new (); 
        right      = new ();
        
        // stepping top x-y
        bool  done = false;
        bool  shadowDone = false;
        Point head = new Point();
        
        Vector3 walkVector = new Vector3(0, Resolution - 1, Resolution - 1) - resolutionOffset;
        int     flipFlop   = 1;
    
        
        Queue<Tuple<int, int>> indiceQ = new Queue<Tuple<int, int>>();
        while (!shadowDone) {
            if (walkVector.x > Resolution - 1 || walkVector.x < - resolutionOffset.x) {
                walkVector.x   = - resolutionOffset.x;
                walkVector.z -= 1;
            }
        
            if (walkVector.z < -resolutionOffset.z) {
                shadowDone = true;
            }
        
            if (_points.Contains(walkVector) && !_points.IsActive(walkVector)) {
                head  = _points[Helpers.VectorKey(walkVector)];
                _head = head.Position;

                var indiceMap = GenerateVerticesByRule(head, true, true);
                
                if (indiceQ.Count != 0) {
                    // print($"Que - {indiceQ.Count}");
                    var indicePair = indiceQ.Dequeue();
                    
                    if (indiceMap.ContainsKey(indicePair.Item1)){
                        print($"ze-fuck?");
                    }
                    else {
                        indiceMap.Add(indicePair.Item1, indicePair.Item2);
                    }
                }

                if (indiceMap.Count > 1) {
                    CreateEdges(indiceMap);
                }
                else {
                    foreach (var indice in indiceMap) {
                        print($"# of indices {indiceMap.Count}");
                        indiceQ.Enqueue(new Tuple<int, int>(indice.Key, indice.Value));
                        break;
                    }
                }
            }
            walkVector.x += 1 * flipFlop;
            yield return new WaitForSeconds(RenderSpeed);
        }
        
        if (indiceQ.Count != 0) {
            print($"que still buffered - {indiceQ.Count}");
            var         indicePair = indiceQ.Dequeue();
            NullableInt tl = null, tr = null, bl = null, br = null;
            
             if (indicePair.Item1 == 0) {
                 tl = indicePair.Item2;
             }
             if (indicePair.Item1 == 1) {
                 tr = indicePair.Item2;
             }
             if (indicePair.Item1 == 2) {
                 br = indicePair.Item2;
             }
             if (indicePair.Item1 == 3) {
                 bl = indicePair.Item2;
             }
             
            foreach (var rect in rectangles.ToList()) {
                if (rect.Type() == Edge.TOP) {
                    if (br != null) {
                        var newRight = new Rectangle(rect.Get2(), null, br, null);
                        right.Add(newRight);
                        rectangles.Add(newRight);
                    }
                }
                if (rect.Type() == Edge.BOTTOM) {
                }
                if (rect.Type() == Edge.LEFT) {
                }
                if (rect.Type() == Edge.RIGHT) {
                }
            }
        }
        
        // null. null, tl, tr
        foreach (var t in top) {
            var rightEdge = ConnectedEdge(right, t.Get3());
            var leftEdge  = ConnectedEdge(left, t.Get2());
            
            if (rightEdge != null) {
                print($"right edge connection");
                t.Set0(rightEdge.Get0().Value());
                rectangles.Remove(rightEdge);
            }
            if (leftEdge != null) {
                print($"left edge connection");
                t.Set1(leftEdge.Get1().Value());
                rectangles.Remove(leftEdge);
            }
        }
        
        // bl, br, null, null
        foreach (var b in bottom) {
            var rightEdge = ConnectedEdge(right, b.Get0());
            var leftEdge  = ConnectedEdge(left, b.Get1());
            
            if (rightEdge != null) {
                var topEdge = ConnectedEdge(top, rightEdge.Get3());

                if (topEdge != null) {
                    b.Set2(topEdge.Get2().Value());
                    rectangles.Remove(topEdge);
                    top.Remove(topEdge);
                }
                
                b.Set3(rightEdge.Get3().Value());
                rectangles.Remove(rightEdge);
            }
            if (leftEdge != null) {
                var topEdge = ConnectedEdge(top, leftEdge.Get2());

                if (topEdge != null) {
                    b.Set3(topEdge.Get3().Value());
                    rectangles.Remove(topEdge);
                    top.Remove(topEdge);
                }
                
                b.Set2(leftEdge.Get2().Value());
                rectangles.Remove(leftEdge);
            }
            
            if (top.Count != 0) {
                foreach (var t in top) {
                    print($"<color=#FF0000> left edge </color>");
                    var newLeft = new Rectangle(null, b.Get0() , null, t.Get3());
                    // left.Add(newLeft);
                    rectangles.Add(newLeft);
            
                    print($"<color=#F0F000> right edge </color>");
                    var newRight = new Rectangle(b.Get1(), null, t.Get2(), null);
                    // right.Add(newRight);
                    rectangles.Add(newRight);
                }
            }
        }
        
        walkVector = new Vector3(0, Resolution - 1, Resolution - 1) - resolutionOffset;
        flipFlop = 1;
        if (rectangles.Count == 0) {
            rectangles.Add(new Rectangle());
        }
        
        while (!done) {
            if (walkVector.x > Resolution - 1 || walkVector.x < - resolutionOffset.x) {
                walkVector.x   = - resolutionOffset.x;
                walkVector.z -= 1;
            }

            if (walkVector.z < -resolutionOffset.z) {
                done = true;
            }

            if (_points.Contains(walkVector) && _points.IsActive(walkVector)) {
                head  = _points[Helpers.VectorKey(walkVector)];
                _head = head.Position;
            
                var indiceMap = GenerateVerticesByRule(head, false, false);

                // if (indiceMap.Count > 1) {
                //     CreateEdges(indiceMap);
                // }
                // else {
                //     
                // }

                if (indiceMap.Count != 0) {
                    if (rectangles.Count != 0) {
                        foreach (var rect in rectangles.ToList()) {
                            if (rect.Connected()) {
                                continue;
                            }
                            if (indiceMap.ContainsKey(0)) {
                                if (!rect.Has0()) {
                                    rect.Set0(indiceMap[0]);
                                }
                            }
                            if (indiceMap.ContainsKey(1)) {
                                if (!rect.Has1()) {
                                    rect.Set1(indiceMap[1]);
                                } 
                            }
                            if (indiceMap.ContainsKey(2)) {
                                if (!rect.Has2()) {
                                    rect.Set2(indiceMap[2]);
                                }
                            }
                            if (indiceMap.ContainsKey(3)) {
                                if (!rect.Has3()) {
                                    rect.Set3(indiceMap[3]);
                                } 
                            }
                        }
                    }
                }
            }

            walkVector.x += 1 * flipFlop;
            yield return new WaitForSeconds(RenderSpeed);
        }

        if (rectangles.Count != 0) {
            foreach (var rect in rectangles.ToList()) {
                if (rect.CompleteAndConnected(Vertices.Values.ToArray())) {
                    print($"completed rect");
                    Indices.AddRange(rect.Indices());
                }
                else if (rect.Connected()) {
                    print($"trapeziod rect");
                    Indices.AddRange(rect.Indices());
                }
            }
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

    }
}
