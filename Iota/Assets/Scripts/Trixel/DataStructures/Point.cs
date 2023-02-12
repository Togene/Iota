using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point {
    public bool    Active       = false;
    public bool[]  FacesChecked = new bool[6];
    public Vector3 Position     = new();
    // public List<Vertex> vertices     = new List<Vertex>();
    public Face[] Faces = new Face[6];
    
    // public Point(){}
    public Point(Vector3 p, bool a) {
        Active   = a;
        Position = p;
    }

    public void SetFaces(int faceType, Vertex[] v) {
        // TTL 0, TTR 1, TBR 2, TBL 3, BTL 4, BTR 5, BBR 6, BBL 7
        Faces[faceType] = new Face(Position.Key(), new []{v[0], v[1], v[2], v[3]});
    }
}
