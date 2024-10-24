using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Skills;

[CustomEditor(typeof(Element))]
public class ElementEditor : Editor
{
    #region Properties
    SerializedProperty iconProperty;
    SerializedProperty effectivenessProperty;
    #endregion
    #region Inspector Methods
    public override void OnInspectorGUI()
    {
        // Actualizamos los valores serializados del objeto
        serializedObject.Update();

        Element element = (Element)target;

        // Dibujar el campo de icono
        DrawIconField();

        // Mostrar la previsualización del icono si existe
        DrawIconPreview(element);

        GUILayout.Space(10); // Espacio antes del array de resistencias

        // Dibujar el array de resistencias
        DrawEffectivenessArray();

        GUILayout.Space(10); // Espacio antes del botón

        // Botón para autocompletar resistencias
        DrawAutoFillButton(element);

        // Aplicar los cambios en el objeto serializado
        serializedObject.ApplyModifiedProperties();
    }
    #endregion
    #region UI Drawing Methods
    private void DrawIconField() => EditorGUILayout.PropertyField(iconProperty);
    private void DrawIconPreview(Element element)
    {
        if (element.icon != null)
        {
            GUILayout.Space(10); // Espacio para separar la previsualización del icono
            GUILayout.Label("Icon Preview", EditorStyles.boldLabel);

            // Obtener la textura y las coordenadas del sprite
            Texture2D texture = element.icon.texture;
            Rect spriteRect = element.icon.rect;
            Rect coords = GetTextureCoordinates(texture, spriteRect);

            // Dibujar la porción de la textura del sprite
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.DrawTextureWithTexCoords(GUILayoutUtility.GetRect(64, 64), texture, coords);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        else
            GUILayout.Label("No Icon Assigned");
    }
    private Rect GetTextureCoordinates(Texture2D texture, Rect spriteRect)
    {
        Vector2 textureSize = new Vector2(texture.width, texture.height);

        return new Rect(
            spriteRect.x / textureSize.x,
            spriteRect.y / textureSize.y,
            spriteRect.width / textureSize.x,
            spriteRect.height / textureSize.y
        );
    }
    private void DrawEffectivenessArray() => EditorGUILayout.PropertyField(effectivenessProperty, true);
    private void DrawAutoFillButton(Element element)
    {
        if (GUILayout.Button("Auto-Fill"))
            AutoFillEffectivenesses(element);
    }
    #endregion
    #region Autofill Methods
    private void AutoFillEffectivenesses(Element element)
    {
        List<Effectiveness> newEffectivenesses = GetExistingEffectivenesses(element);

        // Encontrar todos los ScriptableObject de tipo Element
        string[] guids = AssetDatabase.FindAssets("t:Element");

        if (guids.Length == 0)
        {
            Debug.LogWarning("No se encontraron ScriptableObjects de tipo Element.");
            return;
        }

        AddMissingElementsToEffectivenesses(guids, newEffectivenesses);
        SortEffectivenessesAlphabetically(newEffectivenesses);

        // Actualizar la lista de efectividades en el ScriptableObject
        UpdateElementEffectivenesses(element, newEffectivenesses);

        Debug.Log("Efectividades auto-completadas y ordenadas alfabéticamente.");
    }
    private List<Effectiveness> GetExistingEffectivenesses(Element element) =>
        new List<Effectiveness>(element.effectivenesses);
    private void AddMissingElementsToEffectivenesses(string[] guids, List<Effectiveness> effectivenesses)
    {
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Element foundElement = AssetDatabase.LoadAssetAtPath<Element>(path);

            if (!EffectivenessExists(effectivenesses, foundElement))
                effectivenesses.Add(CreateEffectivenessForElement(foundElement));
        }
    }
    private bool EffectivenessExists(List<Effectiveness> effectivenesses, Element foundElement) =>
        effectivenesses.Exists(e => e.element == foundElement);
    private Effectiveness CreateEffectivenessForElement(Element foundElement) =>
        new Effectiveness
        {
            name = foundElement.name,
            element = foundElement,
            type = Effectiveness.Type.Neutral // Valor por defecto
        };
    private void SortEffectivenessesAlphabetically(List<Effectiveness> effectivenesses) =>
        effectivenesses.Sort((x, y) => string.Compare(x.name, y.name));
    private void UpdateElementEffectivenesses(Element element, List<Effectiveness> newEffectivenesses)
    {
        element.effectivenesses = newEffectivenesses.ToArray();

        // Marcar el objeto como modificado para que se guarden los cambios
        EditorUtility.SetDirty(element);
        AssetDatabase.SaveAssets();
    }
    #endregion
    #region Lifecycle
    private void OnEnable()
    {
        iconProperty = serializedObject.FindProperty("icon");
        effectivenessProperty = serializedObject.FindProperty("effectivenesses");
    }
    #endregion
}
