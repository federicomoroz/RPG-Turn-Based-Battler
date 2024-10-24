using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BattleManager))]
public class BattleManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Dibuja las propiedades predeterminadas del Inspector

        BattleManager battleManager = (BattleManager)target;

        // Añade un botón en el Inspector
        if (GUILayout.Button("Crear y Setear Background"))
        {
            // Llama al método para crear y setear el background
            battleManager.CreateBackgroundInEditor();
        }
    }
}