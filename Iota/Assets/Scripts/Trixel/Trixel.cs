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

public class Rectangle {
    private NullableInt TL {get; set;}
    private NullableInt TR {get; set;}
    private NullableInt BL {get; set;}
    private NullableInt BR {get; set;}

    public Rectangle() { }
    
    public Rectangle(NullableInt tl, NullableInt tr, NullableInt bl, NullableInt br) {
        TL = tl;
        TR = tr;
        BL = bl;
        BR = br;
    }
    
    // ----------- TopLeft ----------- \\
    public bool Has0() {
        return TL != null;
    }
    public void Set0(int i) {
        TL = i;
    }
    // ----------- TopLeft ----------- \\
    
    // ----------- TopRight ----------- \\
    public bool Has1() {
        return TR != null;
    }
    public void Set1(int i) {
        TR = i;
    }
    // ----------- TopRight ----------- \\
    
    // ----------- BottomRight ----------- \\
    public bool Has2() {
        return BR != null;
    }
    public void Set2(int i) {
        BR = i;
    }
    // ----------- BottomRight ----------- \\
    
    // ----------- BottomRight ----------- \\
    public bool Has3() {
        return BL != null;
    }
    public void Set3(int i) {
        BL = i;
    }
    // ----------- BottomRight ----------- \\

    public bool Contains(Vector3[] v, Vector3 a) {
        return TL != null && v[TL.Value()].Equals(a) ||
               TR != null && v[TR.Value()].Equals(a) ||
               BR != null && v[BR.Value()].Equals(a) ||
               BL != null && v[BL.Value()].Equals(a);
    }
    
    public bool TopIsStraight(Vector3[] v) {
        return Helpers.IsStraight(v[TL.Value()], v[TR.Value()], Vector3.right);
    }
    public bool BottomIsStraight(Vector3[] v) {
        return Helpers.IsStraight(v[BL.Value()], v[BR.Value()], Vector3.right);
    }
    public bool RightIsStraight(Vector3[] v) {
        return Helpers.IsStraight(v[TR.Value()], v[BR.Value()], Vector3.right);
    }
    public bool LeftIsStraight(Vector3[] v) {
        return Helpers.IsStraight(v[BL.Value()], v[TL.Value()], Vector3.right);
    }

    public Rectangle[] GetOpenFaces(Vector3[] v) {
        List<Rectangle> newRects = new List<Rectangle>();
        
        if (!TopIsStraight(v)) { // make a left
            newRects.Add(new Rectangle(null, null, TL, TR));
        }
        if (!BottomIsStraight(v)) { // make a right
            newRects.Add(new Rectangle(BL, BR, null, null));
        }
        if (!LeftIsStraight(v)) { // make a top
            newRects.Add(new Rectangle(TL, BL, null, null));
        }
        if (!RightIsStraight(v)) { // make a top
            newRects.Add(new Rectangle(BR, TR, null, null));
        }
        return newRects.ToArray();
    }
    
    public bool Complete(Vector3[] v) {
        return TopIsStraight(v) && BottomIsStraight(v) && LeftIsStraight(v) && RightIsStraight(v);
    }
    
    public bool Connected() {
        return TL != null && TR != null && BL != null && BR != null;
    }

    public bool CompleteAndConnected(Vector3[] v) {
        return Connected() && Complete(v);
    }

    public int[] Indices() {
        if (Connected()){
            return new[] {TL.Value(), TR.Value(), BR.Value(), TL.Value(), BR.Value(), BL.Value()};
        }
        return new int[] {};
    }
    
    public override string ToString() => $"{TL}-{TR}-{BR}-{BL}";
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

    public Rectangle ConnectedEdge(List<Rectangle> edges, NullableInt i) {
        var verts = Vertices.Values.ToArray();
        
        foreach (var e in edges) {
            if (e.Contains(verts, verts[i.Value()])) {
                return e;
            }
        }
        return null;
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
        
        List<Rectangle> rectangles = new ();
        
        // stepping top x-y
        bool  done = false;
        bool  shadowDone = false;
        Point head = new Point();
        
        Vector3 walkVector = new Vector3(0, Resolution - 1, Resolution - 1) - resolutionOffset;
        int     flipFlop   = 1;
    
        List<Rectangle> top = new (), bottom = new (), left = new (), right = new ();
            
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

                if (indiceMap.Count != 0) {
                    NullableInt tl = null, tr = null, bl = null, br = null;
                    
                    if (indiceMap.ContainsKey(0)) {
                        tl = indiceMap[0];
                    }
                    if (indiceMap.ContainsKey(1)) {
                        tr = indiceMap[1];
                    }
                    if (indiceMap.ContainsKey(2)) {
                        br = indiceMap[2];
                    }
                    if (indiceMap.ContainsKey(3)) {
                        bl = indiceMap[3];
                    }
                    
                    if (tl != null && tr != null) {
                        // print($"top edge created");
                        // top
                        var newTop = new Rectangle(null, null, tl, tr);
                        top.Add(newTop);
                        rectangles.Add(newTop); 
                    }
                    if (bl != null && br != null) {
                        // print($"bottom edge created");
                        // bottom

                        var newBottom = new Rectangle(bl, br, null, null);
                        bottom.Add(newBottom);
                        rectangles.Add(newBottom);
                    }
                    if (tl != null && bl != null) {
                        // print($"left edge created");
                        // left
                        
                        // left, right or top edge is connected
                        var bottomEdge = ConnectedEdge(bottom, tl);

                        if (bottomEdge != null) {
                            print($"there's a bottom edge connected broooother");
                        }
                        else {
                            var newLeft = new Rectangle(null, tl, null, bl);
                            left.Add(newLeft);
                            rectangles.Add(newLeft);
                        }
                       
                    }
                    if (tr != null && br != null) {
                        // print($"right edge created");
                        // right
                        var newRight = new Rectangle(tr, null, br, null);
                        right.Add(newRight);
                        rectangles.Add(newRight); 
                    }
                }
            }
        
            walkVector.x += 1 * flipFlop;
            yield return new WaitForSeconds(RenderSpeed);
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
            
                var indiceMap = GenerateVerticesByRule(head, false);
                
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
