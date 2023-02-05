using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(Trixel))]
public class Trixel_Inspector : Editor {
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Trixel TrixelCraveButton = (Trixel)target;
        if (GUILayout.Button("Carve")) {
            TrixelCraveButton.Carve();
        }
    }
}
