using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Tile_Manager : MonoBehaviour {
    public string     currentIndex;
    public Sprite     testSprite;
    public GameObject selector;
    public Color      selectColor;
    
    [Range(10, 1000)]
    public int worldSize = 10;
    
    [Range(0.01f, 10f)]
    public float tileSelectFade = 3f;
    
    private bool                           running;
    private  Dictionary<string, GameObject> tiles;
    
    void Awake() {
        running = true;
        tiles   = new Dictionary<string, GameObject>();

        StartCoroutine(createWorld());
        StartCoroutine(trackMouseIndex());
    }

    void Start() {
    }

    // Update is called once per frame
    void Update() {
        
    }

    void createTile(int x, int y) {
        string tileKey = getCoordinate(x,y);
        var    newTile = new GameObject(tileKey);
        newTile.transform.position = new Vector3(x, -0.1f, y);
               
        SpriteRenderer sr = newTile.AddComponent<SpriteRenderer>();
        sr.sprite                  = testSprite;
        newTile.transform.rotation = Quaternion.Euler(90, 0, 0);
        newTile.transform.parent   = transform;
                
        tiles.Add(tileKey, newTile);
    }
        
    string getCoordinate(int x, int y) {
        return string.Format("{0},{1}", x, y);
    } 
    
    IEnumerator createWorld() {
        for (int x = 0; x < worldSize; x++) {
            for (int y = 0; y < worldSize; y++) {
                createTile(x - worldSize / 2, y - worldSize / 2);
            }
        }
        yield return null;
    }

    IEnumerator trackMouseIndex() {
        while (running) {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z    = Camera.main.transform.position.y;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
            
            // need to get coord based on width + height of square
            string coord = getCoordinate(
                Mathf.RoundToInt(worldPosition.x), 
                Mathf.RoundToInt(worldPosition.z));
            
            selector.transform.position = 
                new Vector3(Mathf.RoundToInt(worldPosition.x), 0, Mathf.RoundToInt(worldPosition.z));
            
            if (tiles.ContainsKey(coord)) {
                if (coord != currentIndex) {
                    if(currentIndex != "") StartCoroutine(pingTile(currentIndex));
                    currentIndex                                      = coord;
                }
                else {
                    tiles[coord].GetComponent<SpriteRenderer>().color = selectColor;
                } 
            }
            else {
                //print("can create?");
                if (Input.GetMouseButton(0)) {
                    createTile(Mathf.RoundToInt(worldPosition.x), Mathf.RoundToInt(worldPosition.z));
                }
            }
            yield return null;
        }
        yield return null;
    }

    IEnumerator pingTile(string coord) {
        print("pinging");
        float time                                              = 0;
        while (time < tileSelectFade) {
            tiles[coord].GetComponent<SpriteRenderer>().color = Color.Lerp(
                tiles[coord].GetComponent<SpriteRenderer>().color, Color.white, time/tileSelectFade);
            time += Time.deltaTime;
            yield return null;
        }
        tiles[coord].GetComponent<SpriteRenderer>().color = Color.white;
        yield return null;
    }
    
}
