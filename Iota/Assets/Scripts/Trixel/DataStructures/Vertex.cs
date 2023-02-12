using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex {
    public bool    Virtual = false;
    public Vector3 Vertice { get; set; }
    public string  Key;
    public int     Index;
    public int     Type;
    // public Vector3 Normal = Vector3.up;
     
    public Vertex(Vector3 v, bool isVirtual)  {
        Vertice = v;
        Virtual = isVirtual;
    }
     
    public Vertex(Vector3 v) : this(v, false) {
        Key     = v.Key();
        Vertice = v;
    }
    // Custom cast from "Vector3":
    public static implicit operator Vertex( Vector3 x ) { return new Vertex( x ); }
    // Custom cast to "Vector3":
    public static implicit operator Vector3( Vertex x ) { return x.Vertice; }
}
