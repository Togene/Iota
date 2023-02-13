using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public struct TrixelData {
    public Vector3[] v;
    public int[]     i;
    public Vector3[] n;
    public Vector2[] uv;

    public TrixelData(Vector3[] _v, int[] _i, Vector3[] _n, Vector2[] _uv) {
        v  = _v;
        i  = _i;
        n  = _n;
        uv = _uv;
    }
}

public class Trixel_Edtior : MonoBehaviour {
    [Range(2, 16)]       public int          Resolution;
    [Range(0.0f, 16.0f)] public float        RenderSpeed;
    [Range(0, 16)]       public int          InitTrixelNo;
    
    private Dictionary<string, TrixelData> blockList = new ();
    private Dictionary<string, Trixel>     TrixelsIndexMap   = new();
    private string[]                       TrixelKeys;
    
    public                      Texture2D    SpriteXYZ;
    public                      Material     mat;
    public                      Trixel       SelectedTrixel;
    private                     MeshCollider Collider;
    private                     Vector3      _direction, _hitPoint, _head;
    
    // Mesh Junk
    private MeshFilter   _mf;
    private MeshRenderer _mr;
    private Mesh         _mesh;

    void Awake() {
         _mf          = this.AddComponent<MeshFilter>();
         _mr          = this.AddComponent<MeshRenderer>();
         _mr.material = mat;
         Collider     = this.AddComponent<MeshCollider>();
         
        Init();
    }
    
    void Init() {
        for (int i = 0; i < InitTrixelNo; i++) {
            var pos = (Vector3.right * i) *Resolution;
            var t   = new Trixel(pos, Resolution);
            t.Init();
            TrixelsIndexMap.Add(pos.Key(), t);
            TrixelKeys = TrixelsIndexMap.Keys.ToArray();
        }
        //SelectedTrixel = Trixels[1];
        StartCoroutine(LittleBabysMarchingCubes());
    }

    IEnumerator LittleBabysMarchingCubes() {
        _mesh = new Mesh();
        
        List<Vector3> v = new List<Vector3>();
        List<int>     i = new List<int>();
        List<Vector3> n = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        
        for (int t = 0; t < TrixelKeys.Length; t++) {
            var data = new TrixelData();
            if (!blockList.ContainsKey("0")) {
                data = TrixelsIndexMap[TrixelKeys[t]].LittleBabysMarchingCubes();
                blockList.Add("0", data);
            }
            else {
                data = blockList["0"];
            }
           
            var vertices = data.v.Clone() as Vector3[];
            var indices  = data.i.Clone() as int[];
           
            for (int x = 0; x < indices.Length; x++) {
                indices[x] += v.Count;
            }
            for (int x = 0; x < vertices.Length; x++) {
                vertices[x] += TrixelsIndexMap[TrixelKeys[t]].Position;
            }
            v.AddRange(vertices);
            
            i.AddRange(indices);
            n.AddRange(data.n);
            uv.AddRange(data.uv);
        }
        
        _mesh.vertices  = v.ToArray(); 
        _mesh.triangles = i.ToArray();
        _mesh.normals   = n.ToArray();
        
        // todo: cry, then merge uv's
        _mesh.uv    =    uv.ToArray();
        
        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();
        _mesh.RecalculateTangents();
      
        _mesh.name           = "new face"; 
        _mf.mesh = _mesh;
        Collider.sharedMesh = _mesh;
         
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
            
            var trixelGrid = new Vector3(
                Resolution * Mathf.Round(hit.point.x / Resolution),
                Resolution * Mathf.Round(hit.point.y / Resolution),
                Resolution * Mathf.Round(hit.point.z / Resolution));
                print($"{trixelGrid}");
                
            if (TrixelsIndexMap.ContainsKey(trixelGrid.Key())) {
                SelectedTrixel = TrixelsIndexMap[trixelGrid.Key()];
                print($"eh?");
            }
            else {
                SelectedTrixel = null;
            }

            return true;
        }
        return false;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update() {
        if (MouseSelect() && SelectedTrixel != null) {
            // creating null point
            if (Input.GetMouseButtonDown(0) && SelectedTrixel._points.Contains(_hitPoint - _direction/2)) {
                SelectedTrixel._points.SetPointsActive(_hitPoint - _direction/2, false);
                SelectedTrixel.AddNullPoint((_hitPoint - _direction/2).Key());
                StartCoroutine(LittleBabysMarchingCubes());
            }

            // removing null point
            if (Input.GetMouseButtonDown(1) && SelectedTrixel._points.Contains(_hitPoint + _direction/2)) {
                SelectedTrixel._points.SetPointsActive(_hitPoint + _direction/2, true);
                SelectedTrixel.RemoveNullPoint((_hitPoint + _direction/2).Key());
                StartCoroutine(LittleBabysMarchingCubes());
            }
        }
        
        if (Input.GetKeyDown(KeyCode.P)) {
            Helpers.ClearConsole();
            Init();
        }
    }

    private void OnDrawGizmos() {
        if (SelectedTrixel != null)
            SelectedTrixel.OnDrawGizmos();
    }
}
