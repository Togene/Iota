using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class KeyCuboid {
    public string key;
    public Cuboid cubiod;

    public KeyCuboid(string k, Cuboid c) {
        key    = k;
        cubiod = c;
    }
}

[Serializable]
public class Cuboid {
    public  int                 size;
    private List<string>        keys = new ();
    public  string              id;
    Dictionary<string, Vector3> AllVertices   = new ();
    private int[]               faces       =  new int[]{};
    public  Vector3[]           cachedVerts = { };
    
    Dictionary<float, List<Vector3>> axisChains = new Dictionary<float, List<Vector3>>();
    
    public  Color[] DebugColors =  { 
        new (0, 0, 1), new (0, 1, 0), new (1, 0, 0), new (1, 1, 0), 
    };

    private List<int> frontIndices, backIndices, topIndices, bottomIndices, leftIndices, rightIndices = new (); 
    private List<Vector3> frontVertices, backVertices, topVertices, bottomVertices, leftVertices, rightVertices = new (); 
    
    
    public Cuboid() {
    }

    public Cuboid(Vector3[] v) {
        foreach (Vector3 vert in v) {
            AddVertice(vert);
        }
    }

    public Cuboid(Vector3 c, Vector3 n) : this(c, n, 1.0f, new List<Cuboid>()) {
    }
    
    public Cuboid(Vector3 c, Vector3 n, float s) : this(c, n, s, new List<Cuboid>()) {
    }
    
    public Cuboid(Vector3 c, Vector3 n, List<Cuboid> m) : this(c, n, 1.0f, m) {
    }
    
    public Cuboid(Vector3 c, Vector3 n, float s, List<Cuboid> m) {
        id = Time.realtimeSinceStartup.ToString().Replace(".", "");

        if (m.Count != 0) {
            Debug.Log($"<color=#00FF00> need to check breaks");
            foreach (var cube in m) {
                size++;
                HandOverKeys(cube.keys);
                AllVertices = cube.AllVertices;
            }
        }
        
        // WORLD dictionary key
        AddKey(Helpers.VectorKey(c, n));
        
        Vector3 left  = new Vector3(0.5f, 0, 0) * s;
        Vector3 right = left * -1f;
            
        Vector3 back  = new Vector3(0.0f, 0, 0.5f) * s;
        Vector3 front = back * -1f;
        
        Vector3 top    = new Vector3(0, 0.5f, 0) * s;
        Vector3 bottom =  top * -1.0f;

        Vector3 halfDirection = Helpers.RoundVector(n / 2);
        Vector3 frontLeft     = Helpers.RoundVector(c + left + front - halfDirection);
        Vector3 frontRight    = Helpers.RoundVector(c + right + front - halfDirection);
        Vector3 backLeft      = Helpers.RoundVector(c + left + back - halfDirection);
        Vector3 backRight     = Helpers.RoundVector(c + right + back - halfDirection);
        
        AddVertice(top + frontLeft);  // 0
        AddVertice(top + frontRight); // 1
        AddVertice(top + backLeft);  // 2
        AddVertice(top + backRight); // 3
        AddVertice(bottom + frontLeft);  // 4
        AddVertice(bottom + frontRight); // 5
        AddVertice(bottom + backLeft);  // 6
        AddVertice(bottom + backRight); // 7
        size++;
        InitAllFaces();
        GenerateFaces();
    }
    
    void HandOverKeys(List<string> k) {
        foreach (var key in k) {
            keys.Add(key);
        }
    }
    
    public List<string> GetKeys() {
        return keys;
    }
    
    void AddKey(string k) {
        keys.Add(k);
    }
    
    // seed - vertex indices to start from, this ensures concave/convex structures are keep intact
    // by starting with vertices that were originally entrance/start vertices to a concave/indent/carve point
    // z+
    // 0, 1
    // 4, 5
    int[] FrontFace() {
        return FrontFace(new int[] { });
    }
    int[] FrontFace(int[] seed) {
        if (seed.Length != 0) {
            Debug.Log($"front algorithm time for {id}!");
        }
        frontIndices = new List<int>{
            2, 3, 7,
            2, 7, 6
        };
        return frontIndices.ToArray();
    }
    Vector3[] FrontVertices() {
        frontVertices = new List<Vector3> {
            cachedVerts[2],
            cachedVerts[3],
            cachedVerts[6],
            cachedVerts[7],
        };
        return frontVertices.ToArray();
    }
    
    // z- <-
    // 2, 3
    // 6, 7
    int[] BackFace() {
        return BackFace(new int[] { });
    }
    int[] BackFace(int[] seed) {
        if (seed.Length != 0) {
            Debug.Log($"back algorithm time for {id}!");
        }
        backIndices = new List<int>{
            0, 4, 1,
            1, 4, 5
        };
        return backIndices.ToArray();
    }
    
   Vector3[] BackVertices() {
       backVertices = new List<Vector3> {
           cachedVerts[0],
           cachedVerts[1],
           cachedVerts[4],
           cachedVerts[5],
       };
       return backVertices.ToArray();
   }
        
    // y+ ->
    // 0, 1
    // 2, 3
    int[] TopFace() {
        topIndices = new List<int>{
            0, 1, 3,
            0, 3, 2
        };
        return topIndices.ToArray();
    }
    
    Vector3[] TopVertices() {
        topVertices = new List<Vector3> {
            cachedVerts[0],
            cachedVerts[1],
            cachedVerts[2],
            cachedVerts[3],
        };
        return topVertices.ToArray();
    }
    
    // y- <-
    // 4, 5
    // 6, 7
    int[] BottomFace() {
        bottomIndices = new List<int>{
            4, 7, 5,
            4, 6, 7
        };
        return bottomIndices.ToArray();
    }
    
    Vector3[] BottomVertices() {
        bottomVertices = new List<Vector3> {
            cachedVerts[4],
            cachedVerts[5],
            cachedVerts[6],
            cachedVerts[7],
        };
        return bottomVertices.ToArray();
    }
    
    // x- <-
    // 1, 3
    // 5, 7
    int[] LeftFace() {
        leftIndices = new List<int>{
            1, 7, 3,
            1, 5, 7
        };
        return leftIndices.ToArray();
    }
    
    Vector3[] LeftVertices() {
        leftVertices = new List<Vector3> {
            cachedVerts[1],
            cachedVerts[3],
            cachedVerts[5],
            cachedVerts[7],
        };
        return leftVertices.ToArray();
    }
    
    // x+ ->
    // 0, 2
    // 4, 6
    int[] RightFace() {
        rightIndices = new List<int>{
            0, 2, 6,
            0, 6, 4
        };
        return rightIndices.ToArray();
    }
    
    Vector3[] RightVertices() {
        rightVertices = new List<Vector3> {
            cachedVerts[0],
            cachedVerts[2],
            cachedVerts[4],
            cachedVerts[6],
        };
        return rightVertices.ToArray();
    }

    int MaybeIndexOfVertice(Vector3 v) {
        if (AllVertices.ContainsKey(Helpers.VectorKey(v))) {
            return AllVertices.Values.ToList().IndexOf(v);
        }
        return -1;
    }
    
    void AddVertice(Vector3 v) {
        string key = Helpers.VectorKey(v);
        if (!AllVertices.ContainsKey(key)) {
            AllVertices.Add(key, v);
        } else {
            AllVertices.Remove(key);
        }
    }
    
    // add to cuboid vertices
    public void AddVertices(Vector3[] verts, Vector3 n) {
        Debug.Log($"adding vertices");
        foreach (Vector3 v in verts) {
            AddVertice(v);
        }
    }

    void CacheVertices() {
        if (cachedVerts == null || cachedVerts.Length == 0) {
            cachedVerts = AllVertices.Values.ToArray();
        }
    }
        
    public Vector3[] GetVertices() {
        CacheVertices();
        return cachedVerts;
    }
    
    public Color[] GetDebugColors() {
        return DebugColors.ToArray();
    }

    public int[] GetFaces() {
        if (faces.Length == 0) {
            GenerateFaces();
        }
        return faces;
    }

    public void InitAllFaces() {
        CacheVertices();
        
        FrontFace();
        BackFace();
        LeftFace();
        RightFace();
        TopFace();
        BottomFace();
        
        FrontVertices();
        BackVertices();
        LeftVertices();
        RightVertices();
        TopVertices();
        BackVertices();
    }
    
    public void GenerateFaces() {
        faces = new int[]{};
        faces = faces.Concat(frontIndices).ToArray();
        faces = faces.Concat(backIndices).ToArray();
        faces = faces.Concat(leftIndices).ToArray();
        faces = faces.Concat(rightIndices).ToArray();
        faces = faces.Concat(topIndices).ToArray();
        faces = faces.Concat(bottomIndices).ToArray();
    }

    public void UpdateFaces(Vector3[] vertices, Vector3 n) {
        string normalString = Helpers.VectorDirection[n];
        if (normalString == "front") {
            foreach (var v in vertices) {
                if (MaybeIndexOfVertice(v) != -1) {
                    frontVertices.Add(v);
                    frontIndices.Add((MaybeIndexOfVertice(v)));
                }
            }
            
            Debug.Log($"new vertices being added? {frontVertices}");
            Debug.Log($"new indices being added? {frontIndices}");
        }

        if (normalString == "back") {
            List<int> seedIndices = new List<int>();

            foreach (var v in vertices) {
                if (MaybeIndexOfVertice(v) != -1) {
                    backVertices.Add(v);
                    seedIndices.Add((MaybeIndexOfVertice(v)));
                }
            }

            // front face cleanup
            foreach (var vert in frontVertices) {
                if (MaybeIndexOfVertice(vert) == -1) {
                    backVertices.Remove(vert);
                }
            }
            // build new faces
            // foreach (var index in seedIndices) {
            //     Vector3 vert = cachedVerts[index];
            //     
            // }

            // Build axis-Chains || verts horizontal to normal, sorted by vertical position
            // -x ----- vert ---- +x
            axisChains = new Dictionary<float, List<Vector3>>();
            foreach (Vector3 v in backVertices) {

                // vertical axis key
                if (!axisChains.ContainsKey(v.y)) {
                    axisChains.Add(v.y, new List<Vector3> { v });
                }
                else {
                    if (!axisChains[v.y].Contains(v)) {
                        axisChains[v.y].Add(v);
                    }
                }
            }

            
            // build new indices, yay 😭
            
            Debug.Log($"new vertices being added? {backVertices.Count}");
            Debug.Log($"seed being added? {axisChains.Count}");
        }

        if (normalString == "left") {
        }
        
        if (normalString == "right") {
        }
        
        if (normalString == "top") {
        }
        
        if (normalString == "bottom") {
        }
        GenerateFaces();
    }
    
    public Vector3[] GetEffectedFaceVertices(Vector3 n) {
        CacheVertices();
        switch (Helpers.VectorDirection[n]) {
            case "front":
                return FrontVertices();
            case "back":
                return BackVertices();
            case "left":
                return LeftVertices();
            case "right":
                return RightVertices();
            case "top":
                return TopVertices();
            case "bottom":
                return BottomVertices();
            default:
                return  new Vector3[] { };
        }
    }
    
    public int[] GetEffectedFaceIndices(Vector3 n) {
        switch (Helpers.VectorDirection[n]) {
            case "front":
                return FrontFace();
            case "back":
                return BackFace();
            case "left":
                return LeftFace();
            case "right":
                return RightFace();
            case "top":
                return TopFace();
            case "bottom":
                return BottomFace();
            default:
                return new int[] { };
        }
    }
    
    public void Update(Cuboid c, Vector3 n) {
        var newVertices = c.GetEffectedFaceVertices(n);
        Debug.Log($"new vertices {newVertices.Length}");
            // Debug.Log($"vector pointing {Helpers.VectorDirection[n]}");
        AddVertices(newVertices, n);
            // Debug.Log($"{AllVertices.Count}");
        UpdateFaces(newVertices, n);
    }
    
    public void Ping(string p) {
        Debug.Log($"Pinging Cuboid {id} - , {p}");
    }

    public void OnDrawGizmos() {
        Vector3[] verts = GetVertices();
        if (cachedVerts == null || cachedVerts.Length == 0) {
            return;
        }
        for (int i = 0; i < verts.Length; i++) {
            //Gizmos.color = DebugColors[i];
            Gizmos.DrawCube(verts[i], new Vector3(.1f, .1f, .1f));
        }

        if (axisChains == null || axisChains.Count == 0) {
            return;
        }

        foreach (var chain in axisChains) {
            for (int i = 0; i < chain.Value.Count; i++) {
                if ((i + 1) > chain.Value.Count - 1) {
                    break;
                }
                Gizmos.DrawLine(chain.Value[i], chain.Value[i + 1]);
            }
        }
    }
}

public class Trixel_Editor_Old : MonoBehaviour {
    [Range(1, 16)]    public int   _resolution = 1;
    [Range(.01f, 1f)] public float _vertexSize = 1;

    private Vector3      _hitPoint, _direction;
    private MeshCollider _collider;

    public  MeshRenderer _mr;
    public  MeshFilter   _mf;
    public  MeshCollider _mc;
    private Mesh         _mesh;

    private Dictionary<string, Cuboid> Cuboids        = new Dictionary<string, Cuboid>();
    public  List<KeyCuboid>            currentCubiods = new List<KeyCuboid>();
    void Awake() {
    }

    void Start() {
        // _mc = AddComponent(MeshCollider);
        // _mf = AddComponent<MeshFilter>();
        // _mr = AddComponent<MeshRenderer>();
        InitCube();
        GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));
    }

    void InitCube() {
        Cuboids = new Dictionary<string, Cuboid>();
        Cuboids.Add($"main", new Cuboid(new Vector3(), new Vector3(), _resolution));
        UpdateCube();
    }

    void UpdateCube() {
        UpdateCube(null);
    }

    void UpdateCube(Vector3[] addedVertices) {
        _mesh           = new Mesh();
        _mesh.vertices  = Cuboids["main"].GetVertices();

        if (addedVertices != null && addedVertices.Length != 0) {
            // Cuboids["main"].AddVertices(addedVertices);
        }
        
        _mesh.triangles = Cuboids["main"].GetFaces();
        _mesh.RecalculateNormals();

        _mf.mesh       = _mesh;
        _mc.sharedMesh = _mesh;
    }

    void Update() {
        if (MouseSelect()) {
            if (Input.GetMouseButtonDown(0)) {
                Dictionary<string, Cuboid> adjacentCuboids = new Dictionary <string, Cuboid>();
                currentCubiods = new List<KeyCuboid>();
                
                string key  = Helpers.VectorKey(_hitPoint, _direction);
                var    step = _resolution / 2;
                CheckAdjacentCubes(
                    new Vector3(_hitPoint.x, _hitPoint.y + step, _hitPoint.z), ref adjacentCuboids);
                CheckAdjacentCubes(
                    new Vector3(_hitPoint.x, _hitPoint.y - step, _hitPoint.z), ref adjacentCuboids);
                CheckAdjacentCubes(
                    new Vector3(_hitPoint.x + step, _hitPoint.y, _hitPoint.z), ref adjacentCuboids);
                CheckAdjacentCubes(
                    new Vector3(_hitPoint.x - step,_hitPoint.y, _hitPoint.z), ref adjacentCuboids);
                CheckAdjacentCubes(
                    new Vector3(_hitPoint.x, _hitPoint.y, _hitPoint.z + step), ref adjacentCuboids);
                CheckAdjacentCubes(
                    new Vector3(_hitPoint.x, _hitPoint.y, _hitPoint.z - step), ref adjacentCuboids);
                
                // if there are no cuboids at this space
                if (!Cuboids.ContainsKey(key)) {
                    // do a merge
                    if (adjacentCuboids.Count != 0) {
                        ManageMerge(key, adjacentCuboids);
                    } else {
                        Cuboid newCuboid = new Cuboid(_hitPoint, _direction);
                        print($"{newCuboid.GetEffectedFaceVertices(_direction).Length}");
                        // Cuboids["main"].AddVertices(newCuboid.GetEffectedFaceVertices(_direction));
                        Cuboids["main"].Update(newCuboid, _direction);
                        Cuboids.Add(key, newCuboid);
                    }
                } else {
                    Cuboids[key].Ping("Hello!");
                }

                foreach (var cube in Cuboids) {
                    currentCubiods.Add(new KeyCuboid(cube.Key, cube.Value));
                }
                UpdateCube();
            }
        }

        if (Input.GetKeyDown(KeyCode.P)) {
            Helpers.ClearConsole();
            InitCube();
        }
    }

    void ManageMerge(string key, Dictionary<string, Cuboid> c) {
        Cuboid mergedCuboid = new Cuboid(_hitPoint, _direction, c.Values.ToList());
        Cuboids.Add(key, mergedCuboid);
        
        foreach (var cube in c) {
            // remove each key/value pair of current adjacent, and point to
            // new merged cuboid (which should have adj cube)
            foreach (var cubeKey in cube.Value.GetKeys()) {
                Cuboids.Remove(cubeKey);
                Cuboids.Add(cubeKey, mergedCuboid);
            }
        }
        Cuboids["main"].Update(mergedCuboid, _direction);
        //Cuboids["main"].AddVertices(mergedCuboid.GetEffectedFaceVertices(_direction));
    }
    
    void CheckAdjacentCubes(Vector3 v, ref Dictionary <string, Cuboid> c) {
        string key = Helpers.VectorKey(v, _direction);
        if (Cuboids.ContainsKey(key)) {
            c.Add(key, Cuboids[key]);
        }
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

    private void OnDrawGizmos() {
        if (Cuboids == null || Cuboids.Count == 0) {
            return;
        }

        foreach (var cube in Cuboids) {
            cube.Value.OnDrawGizmos();
        }
        
        Gizmos.color = Color.green;
        Gizmos.DrawRay(_hitPoint, _direction);
    }
}
