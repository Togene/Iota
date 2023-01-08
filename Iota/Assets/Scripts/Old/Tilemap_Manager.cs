
using UnityEngine;
using UnityEngine.Tilemaps;

public class Tilemap_Manager : MonoBehaviour {
    private Tilemap tilemap;
    // Start is called before the first frame update
    void Start() {
         tilemap = GetComponentInChildren<Tilemap>();
         string  col = tilemap.GetColor(Vector3Int.one).ToString();
         // GameObject pos = tilemap.GetTile(Vector3Int.one);
         
         print(tilemap.size.ToString());
         print(col);
         // print(pos);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
