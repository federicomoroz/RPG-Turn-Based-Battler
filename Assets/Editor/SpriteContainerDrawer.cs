using UnityEditor;
using UnityEngine;
using Sprites;

[CustomPropertyDrawer(typeof(SpriteContainer))]
public class SpriteContainerDrawer : PropertyDrawer
{
    #region Fields
    private float columnWidth => 0.7f; // 70% width for the first column
    private float previewSize => 50f; // Fixed size for the sprite preview
    private float padding => 20f; // Padding between columns
    #endregion
    #region UI Drawing
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        SerializedProperty 
            spriteProp, 
            playSfxProp, 
            sfxProp, 
            volumeProp;

        FindProperties(property, out spriteProp, out playSfxProp, out sfxProp, out volumeProp);

        DrawColumns(position, spriteProp, playSfxProp, sfxProp, volumeProp);
        DrawSpritePreview(position, spriteProp);
        EditorGUI.EndProperty();
    }
    private void DrawColumns(
        Rect position, 
        SerializedProperty spriteProp, 
        SerializedProperty playSfxProp, 
        SerializedProperty sfxProp, 
        SerializedProperty volumeProp
        )
    {
        float yPosition = position.y;
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float columnWidthValue = position.width * columnWidth; // Calculate column width based on position

        // Draw Sprite Field
        DrawSpriteField(new Rect(position.x, yPosition, columnWidthValue, lineHeight), spriteProp);
        yPosition += lineHeight + 2;

        // Draw PlaySfx Toggle
        DrawPlaySfxToggle(new Rect(position.x, yPosition, columnWidthValue, lineHeight), playSfxProp);
        yPosition += lineHeight + 2;

        // Draw SFX and Volume fields if PlaySfx is true
        if (playSfxProp.boolValue)
        {
            DrawSfxField(new Rect(position.x, yPosition, columnWidthValue, lineHeight), sfxProp);
            yPosition += lineHeight + 2;
            DrawVolumeField(new Rect(position.x, yPosition, columnWidthValue, lineHeight), volumeProp);
        }
    }

    private void DrawSpriteField(Rect rect, SerializedProperty spriteProp) =>
        EditorGUI.PropertyField(rect, spriteProp, new GUIContent("Sprite"));
    private void DrawPlaySfxToggle(Rect rect, SerializedProperty playSfxProp) =>
        EditorGUI.PropertyField(rect, playSfxProp, new GUIContent("Play Audio"));
    private void DrawSfxField(Rect rect, SerializedProperty sfxProp) =>
        EditorGUI.PropertyField(rect, sfxProp, new GUIContent("SFX"));
    private void DrawVolumeField(Rect rect, SerializedProperty volumeProp) =>
        EditorGUI.PropertyField(rect, volumeProp, new GUIContent("Volume"));
    private void DrawSpritePreview(Rect position, SerializedProperty spriteProp)
    {
        if (spriteProp.objectReferenceValue != null)
        {
            Sprite sprite = spriteProp.objectReferenceValue as Sprite;
            if (sprite != null)
            {
                Rect previewRect = GetPreviewRect(position, sprite);
                Rect spriteRectUV = new Rect(sprite.textureRect.x / sprite.texture.width, sprite.textureRect.y / sprite.texture.height,
                    sprite.textureRect.width / sprite.texture.width, sprite.textureRect.height / sprite.texture.height);
                GUI.DrawTextureWithTexCoords(previewRect, sprite.texture, spriteRectUV);
            }
        }
    }
    #endregion
    #region Helpers / Utils
    private Rect GetPreviewRect(Rect position, Sprite sprite)
    {
        float previewX = position.x + position.width * columnWidth + padding; // Align to the right
        float previewY = position.y + (position.height - previewSize) / 2; // Vertically centered
        float spriteAspect = sprite.rect.width / sprite.rect.height;

        // Adjust the preview rectangle based on the sprite aspect ratio
        Rect previewRect = new Rect(previewX, previewY, previewSize, previewSize);
        if (spriteAspect > 1)
            previewRect.height = previewSize / spriteAspect; // Adjust height for wide sprites
        else
            previewRect.width = previewSize * spriteAspect; // Adjust width for tall sprites

        return previewRect;
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty playSfxProp = property.FindPropertyRelative("playSfx");
        float height = EditorGUIUtility.singleLineHeight * 2 + 4;

        if (playSfxProp.boolValue)
            height += EditorGUIUtility.singleLineHeight * 2 + 4;

        return Mathf.Max(height, 52); // Ensure enough space for the sprite preview (52px for padding)
    }
    #endregion
    #region Assets Loading
    private static void FindProperties(
        SerializedProperty property, 
        out SerializedProperty spriteProp, 
        out SerializedProperty playSfxProp, 
        out SerializedProperty sfxProp, 
        out SerializedProperty volumeProp
        )
    {
        spriteProp = property.FindPropertyRelative("sprite");
        playSfxProp = property.FindPropertyRelative("playSfx");
        sfxProp = property.FindPropertyRelative("sfx");
        volumeProp = property.FindPropertyRelative("volume");
    }
    #endregion
}
