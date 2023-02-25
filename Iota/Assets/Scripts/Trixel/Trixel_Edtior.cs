using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Block {
    private int ID;
    
    private TrixelData Data; 
    private Trixels    _Trixels;

    private Texture      t;
    private MeshRenderer _mr;
    private Mesh         _mesh;
    private Vector3      Position;
    
    public Block(Vector3 p, Texture _t) {
        ID       = GetHashCode();
        Position = p;
        t        = _t;
        _mesh    = new Mesh();
        _Trixels = new Trixels(16);
        _Trixels.Init();
    }

    public void Init() {
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
        _mesh.RecalculateTangents();
        _mesh.RecalculateNormals();
        return _mesh;
    }

    public void OnDrawGizmos() {
        _Trixels.OnDrawGizmos();
    }
}

public enum EditorModes {LOOK, PAINT, CARVE}

public class Trixel_Edtior : MonoBehaviour {
    public Texture2D SpriteXYZ;
    
    // [Range(0.0f, 16.0f)] public float     RenderSpeed;
    // private Block _selectedBlock;

    private MeshRenderer mr;
    private MeshFilter   mf;
    private MeshCollider mc;
    private Vector3      _direction, _hitPoint, _head;

    private Image    image;
    private TMP_Text modeText;
    private Canvas   ui;

    public EditorModes mode = EditorModes.LOOK;

    [SerializeField] private RectTransform _ColorPickerUI;
    [SerializeField] private Texture2D     colorPicker;

    [SerializeField] GameObject testObject;

    private TrixelBlock EditBlock;
    
    private void Awake() {
        mr = this.AddComponent<MeshRenderer>();
        mf = this.AddComponent<MeshFilter>();
        mc = this.AddComponent<MeshCollider>();

        mr.material  = new Material((Shader.Find("Unlit/Trixel")));
        mr.material.SetTexture("_MainTex", SpriteXYZ);
        mr.material.SetTexture("_RampTexture", 
            Resources.Load<Texture2D>("Shader/Textures/Ramp"));
        
        ui             = GetComponentInChildren<Canvas>();
        
        // preview sprite UI
        // image          = ui.GetComponentInChildren<Image>();
        // image.material = new Material((Shader.Find("Unlit/Trixel")));
        // image.material.SetTexture("_MainTex", SpriteXYZ);
        // image.material.SetInt("_NoLight", 1);

        // modeText  = ui.GetComponentInChildren<TMP_Text>();
        EditBlock = new TrixelBlock(16);
        //_ColorPickerUI.gameObject.SetActive(false);
    }
    
    // Mesh Junk
    bool MouseSelect() {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)) {
            _direction = hit.normal;
            // snap to plane WxH and distance based on normal/facing direction
            // if we snap on all axis's the hit point hovers above the hit point
            // var step = 16/16;
            _hitPoint = new Vector3(
                (_direction.x != 0) ? 
                    hit.point.x : (Mathf.Round((hit.point.x - 0.5f))) + 0.5f,
                (_direction.y != 0) ? 
                    hit.point.y : (Mathf.Round((hit.point.y - 0.5f))) + 0.5f,
                (_direction.z != 0) ? 
                    hit.point.z : (Mathf.Round((hit.point.z - 0.5f))) + 0.5f
            );
            return true;
        }
        return false;
    }

    // Start is called before the first frame update
    void Start() {
        New();
    }

    void New() {
        // _selectedBlock = new Block(Vector3.zero, SpriteXYZ);
        RenderOut();
    }

    void LateUpdate() { }
    
    // Update is called once per frame
    void Update() {
        switch (mode) {
            case EditorModes.LOOK:
                if (_ColorPickerUI.gameObject.activeSelf) {
                   // _ColorPickerUI.gameObject.SetActive(false);
                }
                // ez
                // SetModeText("LOOK");
                break;
            case EditorModes.CARVE:
                if (_ColorPickerUI.gameObject.activeSelf) {
                    //_ColorPickerUI.gameObject.SetActive(false);
                }
                // hard part done-ish
                CarveMode();
                // SetModeText("CARVE");
                break;
            case EditorModes.PAINT:
                // fun times
                if (!_ColorPickerUI.gameObject.activeSelf) {
                    //_ColorPickerUI.gameObject.SetActive(true);
                }
                
                // SetModeText("PAINT");
                break;
        }
        
        if (Input.GetKeyDown(KeyCode.P)) {
            Clear();
        }
    }
    
    public void SetModeText(string t) {
        modeText.text = t;
    }
    
    public void Clear() {
        Helpers.ClearConsole();
        // _selectedBlock.Init();
        EditBlock = new TrixelBlock(16);
        RenderOut();
    }

    public void CarveMode() {
        if (MouseSelect()) {
            // creating null point
            if (Input.GetMouseButtonDown(0)) {
                EditBlock.Edit(_hitPoint); // - _direction/2
                RenderOut();
            }
        
            // removing null point
            if (Input.GetMouseButtonDown(1)) {
                EditBlock.Edit(_hitPoint); // - _direction/2
                RenderOut();
            }
        }
    }
    
    // public void CarveMode() {
    //     if (MouseSelect() && _selectedBlock != null) {
    //         // creating null point
    //         if (Input.GetMouseButtonDown(0) && _selectedBlock.Contains(_hitPoint - _direction/2)) {
    //             _selectedBlock.SetActive(_hitPoint - _direction/2, false);
    //             _selectedBlock.AddNullPoint((_hitPoint - _direction/2));
    //             RenderOut();
    //         }
    //     
    //         // removing null point
    //         if (Input.GetMouseButtonDown(1) && _selectedBlock.Contains(_hitPoint + _direction/2)) {
    //             _selectedBlock.SetActive(_hitPoint + _direction/2, true);
    //             _selectedBlock.RemoveNullPoint((_hitPoint + _direction/2));
    //             RenderOut();
    //         }
    //     }
    // }
    
    public void SetActiveTrixel(Trixels t) {
        // _selectedTrixels = t;
    }

    public void RenderOut() {
        mf.mesh       = EditBlock.Render();
        mc.sharedMesh = mf.mesh;
    }

    public void OnClickColor() {
        SetColor();
    }
    
     void SetColor() {
        Vector3 imagePos   = _ColorPickerUI.position;
        float   globalPosX = Input.mousePosition.x - imagePos.x;
        float   globalPosY = Input.mousePosition.y - imagePos.y;

        int localPosX = (int)(globalPosX * (colorPicker.width / _ColorPickerUI.rect.width));
        int localPosY = (int)(globalPosY * (colorPicker.height / _ColorPickerUI.rect.height));

        Color c = colorPicker.GetPixel(localPosX, localPosY);
        SetActualColor(c);
    }

    void SetActualColor(Color c) {
        testObject.GetComponent<MeshRenderer>().material.color = c;
    }
    
    private void OnDrawGizmos() {
        if (EditBlock != null)
            EditBlock.OnDrawGizmos();
        // if (_selectedBlock != null)
        //    _selectedBlock.OnDrawGizmos();
        
        Gizmos.color = Color.green;
        Gizmos.DrawRay(_hitPoint, _direction);
    }
}
