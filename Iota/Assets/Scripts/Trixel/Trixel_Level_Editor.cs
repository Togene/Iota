using System;
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

public class Trixel_Level_Editor : MonoBehaviour {
    [Range(2, 16)] public int Resolution;
    [Range(0, 16)] public int InitTrixelNo;

    
    private Dictionary<string, TrixelData> blockList       = new ();
    private Dictionary<string, Trixels>     TrixelsIndexMap = new();
    private string[]                       TrixelKeys;

    private MeshFilter   _mf;
    private MeshRenderer _mr;
    private Mesh         _mesh;
    public  Material     mat;
    private MeshCollider Collider;
    
    // public  Trixel        SelectedTrixel;
    private Trixel_Edtior te;
    void Awake() {
        _mf          = this.AddComponent<MeshFilter>();
        _mr          = this.AddComponent<MeshRenderer>();
        _mr.material = mat;
        Collider     = this.AddComponent<MeshCollider>();
        te           = GetComponent<Trixel_Edtior>();
        Init();
    }

    void Init() {
        // for (int i = 0; i < InitTrixelNo; i++) {
        //     var pos = (Vector3.right * i) *Resolution;
        //     var t   = new Trixel(pos, Resolution);
        //     t.Init();
        //     TrixelsIndexMap.Add(pos.Key(), t);
        //     TrixelKeys = TrixelsIndexMap.Keys.ToArray();
        // }
        //SelectedTrixel = Trixels[1];
        // StartCoroutine(LittleBabysMarchingCubes());
    }

    void AddTrixel(Vector3 v) {
        TrixelsIndexMap.Add(v.Key(), new Trixels(Resolution));
        TrixelKeys = TrixelsIndexMap.Keys.ToArray();
    }
    
    IEnumerator LittleBabysMarchingCubes() {
        _mesh = new Mesh();
        
        List<Vector3> v  = new List<Vector3>();
        List<int>     i  = new List<int>();
        List<Vector3> n  = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        
        for (int t = 0; t < TrixelKeys.Length; t++) {
            var data = new TrixelData();
            
            data = TrixelsIndexMap[TrixelKeys[t]].LittleBabysMarchingCubes();
            
            
            var indices  = data.i.Clone() as int[];
            for (int x = 0; x < indices.Length; x++) {
                indices[x] += v.Count;
            }
            i.AddRange(indices);
            v.AddRange(data.v);
            n.AddRange(data.n);
            uv.AddRange(data.uv);
        }
        
        _mesh.vertices  = v.ToArray(); 
        _mesh.triangles = i.ToArray();
        _mesh.normals   = n.ToArray();
        
        // todo: cry, then merge uv's
        _mesh.uv = uv.ToArray();
        
        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();
        _mesh.RecalculateTangents();
      
        _mesh.name          = "new face"; 
        _mf.mesh            = _mesh;
        Collider.sharedMesh = _mesh;
         
        yield return null;
    }
    
    bool MouseSelect() {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)) {
            var trixelGrid = new Vector3
            (
                Resolution * Mathf.Round(hit.point.x / Resolution),
                Resolution * Mathf.Round(hit.point.y / Resolution),
                Resolution * Mathf.Round(hit.point.z / Resolution)
            );
            
            // print($"trixel hitting {trixelGrid}");
            
            if (Input.GetMouseButtonDown(0)){
                if (!TrixelsIndexMap.ContainsKey(trixelGrid.Key())) {
                    AddTrixel(trixelGrid);
                    StartCoroutine(LittleBabysMarchingCubes());
                }
                else {
                    te.SetActiveTrixel(TrixelsIndexMap[trixelGrid.Key()]);
                }
            }
            return true;
        }
        return false;
    }
    
    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    private void LateUpdate() {
        MouseSelect();
    }
    
    private void OnDrawGizmos() {
        if (TrixelKeys != null && TrixelKeys.Length != 0) {
            foreach (var keys in TrixelKeys) {
                TrixelsIndexMap[keys].OnDrawGizmos();
            }
        }
    }
}
