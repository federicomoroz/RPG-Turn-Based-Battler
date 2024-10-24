using UnityEditor;
using UnityEngine;
using Skills;

[CustomEditor(typeof(SO_Skill))]
public class SkillEditor : Editor
{
    #region Fields
    private Element[] elements;
    private SerializedProperty 
        iconProperty,
        animationProperty,
        affectProperty;
    #endregion
    #region Unity Methods
    private void OnEnable()
    {
        FindSerializedProperties();
        LoadElements();
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawBasicProperties();
        DrawIconPreview();
        DrawDescriptionProperties();
        DrawAnimationField();
        DrawAffectFoldout();

        serializedObject.ApplyModifiedProperties();
    }
    #endregion
    #region UI Drawing Methods
    private void DrawBasicProperties()
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Name"), new GUIContent("Name"));
        EditorGUILayout.PropertyField(iconProperty);
    }
    private void DrawIconPreview()
    {
        if (iconProperty.objectReferenceValue != null)
        {
            GUILayout.Space(10);
            GUILayout.Label("Icon Preview", EditorStyles.boldLabel);

            Sprite sprite = (Sprite)iconProperty.objectReferenceValue;
            Texture2D texture = sprite.texture;
            Rect spriteRect = sprite.rect;
            Vector2 textureSize = new Vector2(texture.width, texture.height);

            Rect coords = new Rect(
                spriteRect.x / textureSize.x,
                spriteRect.y / textureSize.y,
                spriteRect.width / textureSize.x,
                spriteRect.height / textureSize.y
            );

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.DrawTextureWithTexCoords(GUILayoutUtility.GetRect(64, 64), texture, coords);
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.Label("No Icon Assigned");
        }
    }
    private void DrawDescriptionProperties()
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("description"), new GUIContent("Description"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("priority"), new GUIContent("Priority"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("costMP"), new GUIContent("Cost MP"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("invocation"), new GUIContent("Invocation"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("message"), new GUIContent("Message"));
    }
    private void DrawAnimationField()
    {
        GUILayout.Label("Animation", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(animationProperty, new GUIContent("Animation Clip"));

        // Si no se ha asignado ningún AnimationClip
        if (animationProperty.objectReferenceValue == null)
        {
            GUILayout.Label("No Animation Clip assigned.", EditorStyles.helpBox);
        }
    }
    private void DrawAffectFoldout()
    {
        affectProperty.isExpanded = EditorGUILayout.Foldout(affectProperty.isExpanded, "Affect");

        if (affectProperty.isExpanded)
        {
            EditorGUILayout.PropertyField(affectProperty.FindPropertyRelative("type"), new GUIContent("Type"));
            EditorGUILayout.PropertyField(affectProperty.FindPropertyRelative("basePower"), new GUIContent("Base Power"));
            EditorGUILayout.PropertyField(affectProperty.FindPropertyRelative("variance"), new GUIContent("Variance"));

            DrawElementPopup();
        }
    }
    private void DrawElementPopup()
    {
        SerializedProperty elementProperty = affectProperty.FindPropertyRelative("element");

        if (elements.Length > 0)
        {
            int selectedIndex = 0;
            for (int i = 0; i < elements.Length; i++)
            {
                if (elementProperty.objectReferenceValue == elements[i])
                {
                    selectedIndex = i + 1;
                    break;
                }
            }

            EditorGUI.BeginChangeCheck();
            selectedIndex = EditorGUILayout.Popup("Element", selectedIndex, GetElementNames());
            if (EditorGUI.EndChangeCheck())
            {
                elementProperty.objectReferenceValue = selectedIndex == 0 ? null : elements[selectedIndex - 1];
                EditorUtility.SetDirty(target);
            }
        }
        else
        {
            EditorGUILayout.LabelField("No Elements found.");
        }
    }
    #endregion
    #region Helpers / Utils
    private void FindSerializedProperties()
    {
        iconProperty = serializedObject.FindProperty("icon");
        animationProperty = serializedObject.FindProperty("animation");
        affectProperty = serializedObject.FindProperty("affect");
    }
    private void LoadElements()
    {
        string[] guids = AssetDatabase.FindAssets("t:Element");
        elements = new Element[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            elements[i] = AssetDatabase.LoadAssetAtPath<Element>(path);
        }
    }
    private string[] GetElementNames()
    {
        string[] names = new string[elements.Length + 1];
        names[0] = "None";
        for (int i = 0; i < elements.Length; i++)
        {
            names[i + 1] = elements[i].name;
        }
        return names;
    }
    #endregion
}
