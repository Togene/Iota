using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

// https://www.theappguruz.com/blog/simple-cube-mesh-generation-unity3d
public static class Helpers {
    public static TrixelFace NewFace(FaceType t, Vector3 v, Vector3 dir) {
        float pixelStep = 0.5f;
        return new(t,
            new Vector3(v.x + pixelStep, v.y + pixelStep, v.z),
            new Vector3(v.x - pixelStep, v.y + pixelStep, v.z),
            new Vector3(v.x - pixelStep, v.y - pixelStep, v.z),
            new Vector3(v.x + pixelStep, v.y - pixelStep, v.z)
        );
    }
    
    public static Vector3[] Transform(this Vector3[] v, Vector3 p) {
        for(int i = 0; i < v.Length; i++){
            v[i] += p;
        }
        return v;
    }
    public static Vector3[] ToVector3Array(this Vertex[] v) {
        return Array.ConvertAll(v, item => (Vector3)item);;
    }
    public static Vector3[] ToVector3Array(this List<Vertex> v) {
        return v.Select(x=>x.Vertice).ToArray();
    }
    public static string ExtractY(this Vector3 v, int surfaceID) {
        var y = v.y;
        if (y == 0) {
            y = -99;
        }
        return $"{new Vector3(0, y, 0).ToString()}{surfaceID}";
    }
    public static string ExtractX(this Vector3 v, int surfaceID) {
        var x = v.x;
        if (x == 0) {
            x = -99;
        }
        return $"{new Vector3(x, 0, 0).ToString()}{surfaceID}";
    }
    public static string ExtractZ(this Vector3 v, int surfaceID) {
        var z = v.z;
        if (z == 0) {
            z = -99;
        }
        return $"{new Vector3(0, 0, z).ToString()}{surfaceID}";
    }

    public static string ColorToString(this Color p) {
        return $"<color=#{p.ToHexString()}>█</color>";
    }
    
    public static string Key(this Vector3 v) {
        return VectorKey(v, Vector3.zero);
    }

    public static string CaseKey(Dictionary<int, int> d) {
       return $"" +
            $"{(d.ContainsKey(0) ? 1 : 0)}" +
            $"{(d.ContainsKey(1) ? 1 : 0)}" +
            $"{(d.ContainsKey(2) ? 1 : 0)}" +
            $"{(d.ContainsKey(3) ? 1 : 0)}" +
            $"";
    }
        public static bool PointOnSegement(Vector3 a, Vector3 b, Vector3 c) {
           // a - b, segment 1
           // c - d, segment 2
           
           // create vector 
            
           return false;
       }
       
       public static Vector3 FlipNormal(Vector3 v) {
           return new Vector3(
               ((Mathf.Abs(v.x) == 1.0f) ? 0.0f : 1.0f), 
               ((Mathf.Abs(v.y) == 1.0f) ? 0.0f : 1.0f), 
               ((Mathf.Abs(v.z) == 1.0f) ? 0.0f : 1.0f)
           );
       }

       public static bool IsStraight(Vector3 a, Vector3 b, Vector3 along) {
           float product = Math.Abs(Vector3.Dot((a - b).normalized, along.normalized));
           return product == 1 || product == 0 ;
       }
       
       public static Dictionary<Vector3, string> VectorDirection = new() {
           { Vector3.zero, "none"},
           { Vector3.up, "top"},
           { Vector3.down, "bottom"},
           { Vector3.right, "right"},
           { Vector3.left, "left"},
           { Vector3.back, "back"},
           { Vector3.forward, "front"},
       };
   
       public static Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c) {
           return Vector3.Cross((b - a).normalized, (c - a).normalized).normalized;
       }
       
       public static Vector3 RoundVector(Vector3 v) {
           return new Vector3(
               (float)Math.Round(v.x, 1), 
               (float)Math.Round(v.y, 1),
               (float)Math.Round(v.z, 1));
       }
   
       public static string VectorKey(Vector3 c, Vector3 n) {
           var centre = RoundVector(c);
           Vector3 nHalf = RoundVector(n / 2);
           return $"{centre.x - nHalf.x},{centre.y - nHalf.y},{centre.z - nHalf.z}";
       }
       
       public static string VectorKey(Vector3 c) {
           return VectorKey(c, Vector3.zero);
       }
       
       // https://answers.unity.com/questions/707636/clear-console-window.html
       // Thank you Bunny83?
       public static void ClearConsole() {
           var assembly = Assembly.GetAssembly(typeof(SceneView));
           var type     = assembly.GetType("UnityEditor.LogEntries");
           var method   = type.GetMethod("Clear");
           method.Invoke(new object(), null);
       }
}
