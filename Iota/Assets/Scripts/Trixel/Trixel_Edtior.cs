using UnityEngine;


public class TrixelBlock {
    private int ID;
    
    private TrixelData Data; 
    private Trixels    _Trixels;

    private Texture      t;
    private MeshRenderer _mr;
    private Mesh         _mesh;
    private Vector3      Position;
    
    public TrixelBlock(Vector3 p, Texture _t) {
        ID       = GetHashCode();
        Position = p;
        t        = _t;
        _mesh    = new Mesh();
        _Trixels = new Trixels(16);
        _Trixels.Init();
    }

    public bool Contains(Vector3 v) {
        return _Trixels.Contains(v);
    }
    
    public void SetActive(Vector3 v, bool b) {
         _Trixels.SetActive(v, b);
    }
    
    public void RemoveNullPoint(Vector3 v) {
        _Trixels.RemoveNullPoint(v.Key());
    }
    
    public void AddNullPoint(Vector3 v) {
        _Trixels.AddNullPoint(v.Key());
    }
    
    public Mesh Renderer() {
        Debug.Log($"id: {ID}");
        _mesh = new Mesh();
        Data  = _Trixels.LittleBabysMarchingCubes();
        
        _mesh.vertices  = Data.v.Transform(Position);
        _mesh.triangles = Data.i;
        _mesh.normals   = Data.n;
        _mesh.uv        = Data.uv;
        
        return _mesh;
    }

    public void OnDrawGizmos() {
        _Trixels.OnDrawGizmos();
    }
}

public class Trixel_Edtior : MonoBehaviour {
    public                      Texture2D SpriteXYZ;
    private                     Vector3   _direction, _hitPoint, _head;
    [Range(0.0f, 16.0f)] public float     RenderSpeed;
    
    private TrixelBlock _selectedBlock;

    private MeshRenderer mr;
    private MeshFilter   mf;
    private MeshCollider mc;
    
    private void Awake() {
        mr          = GetComponent<MeshRenderer>();
        mf          = GetComponent<MeshFilter>();
        mc          = GetComponent<MeshCollider>();
        mr.material = new Material((Shader.Find("Unlit/Trixel")));
        mr.material.SetTexture("_MainTexX", SpriteXYZ);
    }
    
    // Mesh Junk
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
    void Start() {
        _selectedBlock = new TrixelBlock(Vector3.zero, SpriteXYZ);
        mf.mesh        = _selectedBlock.Renderer();
        mc.sharedMesh  = mf.mesh;
    }

    // Update is called once per frame
    void Update() {
        if (MouseSelect() && _selectedBlock != null) {
            // creating null point
            if (Input.GetMouseButtonDown(0) && _selectedBlock.Contains(_hitPoint - _direction/2)) {
                _selectedBlock.SetActive(_hitPoint - _direction/2, false);
                _selectedBlock.AddNullPoint((_hitPoint - _direction/2));
                mf.mesh       = _selectedBlock.Renderer();
                mc.sharedMesh = mf.mesh;
            }
        
            // removing null point
            if (Input.GetMouseButtonDown(1) && _selectedBlock.Contains(_hitPoint + _direction/2)) {
                _selectedBlock.SetActive(_hitPoint + _direction/2, true);
                _selectedBlock.RemoveNullPoint((_hitPoint + _direction/2));
                mf.mesh       = _selectedBlock.Renderer();
                mc.sharedMesh = mf.mesh;
            }
        }
        
        if (Input.GetKeyDown(KeyCode.P)) {
            Helpers.ClearConsole();
            // Init();
        }
    }

    public void SetActiveTrixel(Trixels t) {
        // _selectedTrixels = t;
    }
    
    private void OnDrawGizmos() {
        if (_selectedBlock != null)
            _selectedBlock.OnDrawGizmos();
    }
}
