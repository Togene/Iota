using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Trixel_Edtior : MonoBehaviour {
    [Range(2, 16)]       public int          Resolution;
    [Range(0.0f, 16.0f)] public float        RenderSpeed;
    [Range(0, 16)]       public int          InitTrixelNo;
    private                     List<Trixel> Trixels = new List<Trixel>();
    
    public                      Texture2D    SpriteXYZ;
    public                      Material     mat;
    public                      Trixel       ActiveTrixel;
    private                     MeshCollider Collider;
    private                     Vector3      _direction, _hitPoint, _head;
    
    // Mesh Junk
    private MeshFilter   _mf;
    private MeshRenderer _mr;
    private Mesh         _mesh;

    void Awake() {
        Collider                 = this.AddComponent<MeshCollider>();
        _mf                      = this.AddComponent<MeshFilter>();
        _mr                      = this.AddComponent<MeshRenderer>();
        _mr.material             = mat;
        _mr.material.mainTexture = SpriteXYZ;

        for (int i = 0; i < InitTrixelNo; i++) {
            Trixels.Add(new Trixel(Vector3.right*i, Resolution));
        }
        
        Init();
    }
    
    void Init() {
        ActiveTrixel = new Trixel(Vector3.zero, Resolution);
        _mf.mesh     = new Mesh();
        ActiveTrixel.Init();
        
        foreach (var t in Trixels) {
            t.Init();
        }
        
        
        StartCoroutine(LittleBabysMarchingCubes());
    }

    IEnumerator LittleBabysMarchingCubes() {
        _mf.mesh = new Mesh();
        _mesh    = new Mesh();
        
        foreach (var t in Trixels) {
            t.Init();
        }

        
        ActiveTrixel.LittleBabysMarchingCubes(ref _mesh);
        
        _mf.mesh       = _mesh;
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
        if (MouseSelect()) {
            // creating null point
            if (Input.GetMouseButtonDown(0) && ActiveTrixel._points.Contains(_hitPoint - _direction/2)) {
                ActiveTrixel._points.SetPointsActive(_hitPoint - _direction/2, false);
                ActiveTrixel.AddNullPoint((_hitPoint - _direction/2).Key());
                StartCoroutine(LittleBabysMarchingCubes());
            }

            // removing null point
            if (Input.GetMouseButtonDown(1) && ActiveTrixel._points.Contains(_hitPoint + _direction/2)) {
                ActiveTrixel._points.SetPointsActive(_hitPoint + _direction/2, true);
                ActiveTrixel.RemoveNullPoint((_hitPoint + _direction/2).Key());
                StartCoroutine(LittleBabysMarchingCubes());
            }
        }
        
        if (Input.GetKeyDown(KeyCode.P)) {
            Helpers.ClearConsole();
            Init();
        }
    }

    private void OnDrawGizmos() {
        if (ActiveTrixel != null)
            ActiveTrixel.OnDrawGizmos();
    }
}
