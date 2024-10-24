using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Sprites;

[CustomEditor(typeof(SO_ObjectView))]
public class SO_ObjectViewEditor : Editor
{
    #region Fields
    private int currentFrame = 0;
    private float lastFrameTime;
    private float frameDuration = 0.05f;
    private bool showSfxFoldout;

    private SO_ObjectView _objectViewData;

    SerializedProperty _spritesProp;
    SerializedProperty _sfxProp;
    SerializedProperty _sfxCastProp;
    SerializedProperty _sfxLoopProp;
    SerializedProperty _sfxImpactProp;
    #endregion
    #region Unity Methods
    private void OnEnable()
    {
        _objectViewData = (SO_ObjectView)target;
        FindSerializedProperties();
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawNameField();
        DrawSpriteField();
        DrawSpriteDragAndDropArea(_objectViewData);
        DrawAnimationControls();
        DrawSpritePreview();
        DrawSfxFoldout();

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
            EditorUtility.SetDirty(_objectViewData);
    }
    #endregion
    #region Property Drawing Methods
    private void DrawNameField()
    {
        EditorGUI.BeginChangeCheck();
        _objectViewData.Name = EditorGUILayout.TextField("Name", _objectViewData.Name);
        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(_objectViewData);
    }

    private void DrawSpriteField() => EditorGUILayout.PropertyField(_spritesProp, new GUIContent("Sprites"), true);

    private void DrawAnimationControls()
    {
        _objectViewData.isAnimated = EditorGUILayout.Toggle("Is Animated", _objectViewData.isAnimated);
        if (_objectViewData.isAnimated)
            _objectViewData.animationSpeed = (int)EditorGUILayout.Slider("Animation Speed", _objectViewData.animationSpeed, 0, 10);
    }

    private void DrawSpritePreview()
    {
        if (_objectViewData.sprites != null && _objectViewData.sprites.Length > 0)
        {
            if (_objectViewData.sprites[0].sprite != null)
            {
                GUILayout.Label("Projectile Preview");
                frameDuration = CalculateFrameChangeInterval(_objectViewData.animationSpeed);

                EditorApplication.update -= UpdateAnimation;
                EditorApplication.update += UpdateAnimation;

                DisplaySpritePreview();
            }
            else
                GUILayout.Label("First sprite is null.");
        }
        else
            EditorGUILayout.HelpBox("No sprites assigned.", MessageType.Warning);
    }

    private void DrawSfxFoldout()
    {
        showSfxFoldout = EditorGUILayout.Foldout(showSfxFoldout, "SFX");
        if (showSfxFoldout)
            DrawSfxFields();
    }

    private void DrawSfxFields()
    {
        EditorGUILayout.PropertyField(_sfxCastProp, new GUIContent("Cast"));
        EditorGUILayout.PropertyField(_sfxLoopProp, new GUIContent("Loop"));
        EditorGUILayout.PropertyField(_sfxImpactProp, new GUIContent("Impact"));
    }
    #endregion
    #region Sprite Drag & Drop Methods
    private void DrawSpriteDragAndDropArea(SO_ObjectView objectViewData)
    {
        Rect dropArea = GUILayoutUtility.GetRect(0, 100, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "");

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 16
        };

        EditorGUI.LabelField(dropArea, "DRAG SPRITES HERE", style);
        DragAndDropSprites(objectViewData);
    }

    private void DragAndDropSprites(SO_ObjectView objectViewData)
    {
        Event currentEvent = Event.current;

        if (currentEvent.type == EventType.DragUpdated)
            HandleSpriteDragUpdated();
        else if (currentEvent.type == EventType.DragPerform)
            HandleSpriteDragPerformed(objectViewData);
    }

    private void HandleSpriteDragUpdated()
    {
        bool canAccept = DragAndDrop.objectReferences.All(obj => obj is Sprite || IsImageFile(obj));
        DragAndDrop.visualMode = canAccept ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
        Event.current.Use();
    }

    private void HandleSpriteDragPerformed(SO_ObjectView objectViewData)
    {
        if (DragAndDrop.objectReferences.Length > 0)
        {
            List<SpriteContainer> allSprites = objectViewData.sprites.ToList();
            List<Object> imagesToAdd = DragAndDrop.objectReferences.Where(IsImageFile).OrderBy(obj => AssetDatabase.GetAssetPath(obj)).ToList();

            foreach (Object obj in imagesToAdd)
                AddImageToSpriteContainer(allSprites, obj);

            UpdateSpriteArray(objectViewData, allSprites);
        }
    }

    private void AddImageToSpriteContainer(List<SpriteContainer> spriteList, Object obj)
    {
        Sprite newSprite = ExtractSpriteFromObject(obj);
        if (newSprite != null)
            spriteList.Add(new SpriteContainer { sprite = newSprite });
    }

    private Sprite ExtractSpriteFromObject(Object obj)
    {
        if (obj is Sprite sprite)
            return sprite;

        if (IsImageFile(obj))
        {
            string path = AssetDatabase.GetAssetPath(obj);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture != null)
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        return null;
    }

    private bool IsImageFile(Object obj)
    {
        string path = AssetDatabase.GetAssetPath(obj);
        return path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase) ||
               path.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase) ||
               path.EndsWith(".jpeg", System.StringComparison.OrdinalIgnoreCase);
    }

    private void UpdateSpriteArray(SO_ObjectView objectViewData, List<SpriteContainer> allSprites)
    {
        objectViewData.sprites = allSprites.ToArray();
        serializedObject.Update();
        SerializedProperty spritesProperty = serializedObject.FindProperty("sprites");
        spritesProperty.arraySize = objectViewData.sprites.Length;

        for (int i = 0; i < objectViewData.sprites.Length; i++)
        {
            SerializedProperty spriteProperty = spritesProperty.GetArrayElementAtIndex(i);
            spriteProperty.FindPropertyRelative("sprite").objectReferenceValue = objectViewData.sprites[i].sprite;
        }

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(objectViewData);
    }
    #endregion
    #region Utility Methods
    private void FindSerializedProperties()
    {
        _spritesProp = serializedObject.FindProperty("sprites");
        _sfxProp = serializedObject.FindProperty("sfx");
        _sfxCastProp = _sfxProp.FindPropertyRelative("cast");
        _sfxLoopProp = _sfxProp.FindPropertyRelative("loop");
        _sfxImpactProp = _sfxProp.FindPropertyRelative("impact");
    }

    private float CalculateFrameChangeInterval(float speed) => Mathf.Lerp(0.2f, 0.05f, speed / 10f);

    private void DisplaySpritePreview()
    {
        float spriteWidth = _objectViewData.sprites[0].sprite.rect.width * 2;
        float spriteHeight = _objectViewData.sprites[0].sprite.rect.height * 2;
        Rect displayRect = GUILayoutUtility.GetRect(spriteWidth, spriteHeight, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));

        Sprite currentSprite = _objectViewData.isAnimated ? _objectViewData.sprites[currentFrame].sprite : _objectViewData.sprites[0].sprite;

        Rect spriteRect = new Rect(
            currentSprite.rect.x / currentSprite.texture.width,
            currentSprite.rect.y / currentSprite.texture.height,
            currentSprite.rect.width / currentSprite.texture.width,
            currentSprite.rect.height / currentSprite.texture.height
        );

        GUI.DrawTextureWithTexCoords(displayRect, currentSprite.texture, spriteRect);
    }

    private void UpdateAnimation()
    {
        if (_objectViewData.sprites == null || _objectViewData.sprites.Length == 0 || _objectViewData.sprites[0].sprite == null)
            return;

        if (EditorApplication.timeSinceStartup - lastFrameTime >= frameDuration)
        {
            lastFrameTime = (float)EditorApplication.timeSinceStartup;

            currentFrame++;
            if (currentFrame >= _objectViewData.sprites.Length)
                currentFrame = 0;

            Repaint();
        }
    }
    #endregion
}
