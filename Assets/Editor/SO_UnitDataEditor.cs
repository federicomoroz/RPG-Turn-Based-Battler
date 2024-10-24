using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Sprites;

[CustomEditor(typeof(SO_UnitData))]
public class SO_UnitDataEditor : Editor
{
    #region Fields
    private int 
        colorSchemeIndex = 0,
        totalSprites = 0,
        loadedSprites = 0,
        currentVariantIndex = 0;

    private List<UnitSpritesSet> spriteVariants;
    private SO_UnitData currentUnitData;
    private string[] colorVariantFolders;
    #endregion
    #region UI Drawing
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SO_UnitData unitData = (SO_UnitData)target;

        if (unitData.SpritesVariants != null && unitData.SpritesVariants.Length > 0)
            DrawSpritesPreviewer(unitData);

        if (GUILayout.Button("Load Sprites"))
        {
            currentUnitData = unitData;
            StartLoadingSprites();
        }

        if (GUILayout.Button("Build Unit"))
            Build(unitData);
    }
    private void DrawSpritesPreviewer(SO_UnitData unitData)
    {
        string[] colorSchemeOptions = new string[unitData.SpritesVariants.Length];

        for (int i = 0; i < unitData.SpritesVariants.Length; i++)
            colorSchemeOptions[i] = $"Scheme {i + 1}";

        colorSchemeIndex = EditorGUILayout.Popup("Color Scheme", colorSchemeIndex, colorSchemeOptions);

        if (unitData.SpritesVariants.Length > 0)
        {
            Sprite selectedPortrait = unitData.SpritesVariants[colorSchemeIndex].portrait;
            Sprite selectedSprite = unitData.SpritesVariants[colorSchemeIndex].spritesheet.Length > 0
                ? unitData.SpritesVariants[colorSchemeIndex].spritesheet[0]
                : null;

            GUIStyle centeredStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter
            };

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            if (selectedPortrait != null)
            {
                GUILayout.Label("Portrait:", centeredStyle, GUILayout.Width(94));
                GUILayout.Label(selectedPortrait.texture, GUILayout.Width(94), GUILayout.Height(94));
            }
            else
                GUILayout.Label("Portrait: N/A", centeredStyle, GUILayout.Width(94));

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            if (selectedSprite != null)
            {
                GUILayout.Label("Sprite:", centeredStyle, GUILayout.Width(94));
                Texture2D spriteTexture = GetSpriteTexture(selectedSprite);
                if (spriteTexture != null)
                    GUILayout.Label(spriteTexture, GUILayout.Width(94), GUILayout.Height(94));
                else
                    GUILayout.Label("Failed to load sprite texture", centeredStyle, GUILayout.Width(94));

            }
            else
                GUILayout.Label("Sprite: N/A", centeredStyle, GUILayout.Width(94));

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }
    #endregion    
    #region Assets Loading
    private Texture2D GetSpriteTexture(Sprite sprite)
    {
        if (sprite.texture.isReadable)
        {
            Rect spriteRect = sprite.textureRect;
            Texture2D texture = new Texture2D((int)spriteRect.width, (int)spriteRect.height);

            texture.SetPixels(sprite.texture.GetPixels((int)spriteRect.x, (int)spriteRect.y, (int)spriteRect.width, (int)spriteRect.height));
            texture.Apply();

            return texture;
        }
        else
        {
            Debug.LogError("Texture is not readable!");
            return null;
        }
    }
    private void StartLoadingSprites()
    {
        string unitName = currentUnitData.Name;
        string basePath = $"Resources/Sprites/Characters/{unitName}";
        string fullPath = Path.Combine(Application.dataPath, basePath);

        if (!Directory.Exists(fullPath))
        {
            Debug.LogError($"Folder not found: {fullPath}");
            return;
        }

        colorVariantFolders = Directory.GetDirectories(fullPath);
        totalSprites = 0;
        loadedSprites = 0;
        spriteVariants = new List<UnitSpritesSet>();
        currentVariantIndex = 0;

        // Primero, calcular el número total de sprites para la barra de progreso
        foreach (var variantFolder in colorVariantFolders)
        {
            string variantName = Path.GetFileName(variantFolder);
            string spritesheetPath = $"Sprites/Characters/{unitName}/{variantName}/sprites_{unitName.ToLower()}_{variantName}";
            totalSprites += Resources.LoadAll<Sprite>(spritesheetPath)?.Length ?? 0;
        }

        if (totalSprites == 0)
        {
            Debug.LogError($"No sprites found in any of the variant folders.");
            return;
        }

        // Iniciar la carga de sprites en el siguiente ciclo de actualización
        EditorApplication.update += LoadSprite;
        EditorUtility.DisplayProgressBar("Sprites Loading", "Starting sprite loading...", 0f);
    }
    private void LoadSprite()
    {       
        if (currentVariantIndex >= colorVariantFolders.Length)
        {
            FinishLoadingSprites();
            return;
        }

        string unitName = currentUnitData.Name;
        string variantFolder = colorVariantFolders[currentVariantIndex];
        string variantName = Path.GetFileName(variantFolder);

        string portraitPath = $"Sprites/Characters/{unitName}/{variantName}/portrait_{unitName.ToLower()}_{variantName}";
        Sprite portrait = Resources.Load<Sprite>(portraitPath);

        if (portrait == null)        
            Debug.LogError($"Portrait not found at: {portraitPath}");        
        else
        {
            string spritesheetPath = $"Sprites/Characters/{unitName}/{variantName}/sprites_{unitName.ToLower()}_{variantName}";
            Sprite[] sprites = Resources.LoadAll<Sprite>(spritesheetPath);

            if (sprites != null && sprites.Length > 0)
            {
                loadedSprites += sprites.Length;

                UnitSpritesSet newSet = new UnitSpritesSet
                {
                    portrait = portrait,
                    spritesheet = sprites
                };

                spriteVariants.Add(newSet);
            }
            else            
                Debug.LogError($"No sprites found at: {spritesheetPath}");            

            // Actualizar barra de progreso
            float progress = (float)loadedSprites / totalSprites;
            EditorUtility.DisplayProgressBar("Sprites Loading", $"Loading {loadedSprites} / {totalSprites} sprites...", progress);

            // Forzar actualización de la interfaz y darle tiempo
            System.Threading.Thread.Sleep(256); // Añadir un retraso de 100ms para ver la barra de progreso
            EditorApplication.QueuePlayerLoopUpdate(); // Forzar actualización del editor
        }

        currentVariantIndex++;
    }
    private void FinishLoadingSprites()
    {
        EditorApplication.update -= LoadSprite;

        currentUnitData.SetSpriteVariants(spriteVariants.ToArray());

        string message = $"Successfully loaded {spriteVariants.Count} variants and {loadedSprites} sprites.";
        EditorUtility.DisplayDialog("Sprites Loading", message, "Ok");

        EditorUtility.ClearProgressBar();

        EditorUtility.SetDirty(currentUnitData);
        AssetDatabase.SaveAssets();
    }
    #endregion
    #region Helpers / Utils
    private void Build(SO_UnitData unitData) { }
    #endregion
}
