using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {
    private Vector3 _v;
    
    [Range(0.1f, 100.0f)]
    public float speed = 70f;

    void Awake() {
        Debug.Log("huh?");
    }
    
    void Start() {
        
    }
    
    void Update() {
        float speedOvertime = speed * Time.deltaTime;
            if (Input.GetKey("w")) {
                _v.z += speedOvertime;
            }
            if (Input.GetKey("s")) {
                _v.z -= speedOvertime;
            }
            if (Input.GetKey("d")) {
                _v.x += speedOvertime;
            }
            if (Input.GetKey("a")) {
                _v.x -= speedOvertime;
            }
        _v                  *= 0.99f;
        transform.position +=  _v;
    }
}
