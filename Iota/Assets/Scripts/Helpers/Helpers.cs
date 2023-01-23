using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class Segment {
    private Vector3 A, B;

    public Segment(Vector3 a, Vector3 b) {
        A = a;
        B = b;
    }
    
}

public static class Helpers {

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
