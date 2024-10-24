using UnityEditor;
using UnityEngine;
using Sound;
using Sprites;

[CreateAssetMenu(fileName = "newProjectileViewData", menuName = "Data/Projectile Data/View Data")]
public class SO_ObjectView : ScriptableObject
{
    #region Fields
    public string Name;  
    public SpriteContainer[] sprites;
    public bool isAnimated;        
    public ProjectileSfxContainer sfx = new ProjectileSfxContainer();

    [HideInInspector]
    public int animationSpeed;
    #endregion
    #region Helpers / Utils
    private void UpdateNameFromAsset()
    {
        string assetPath = AssetDatabase.GetAssetPath(this);
        string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        Name = FormatName(fileName);
    }    
    private string FormatName(string fileName)
    {
        string[] words = fileName.Split('_');
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
        }
        return string.Join(" ", words);
    }
    #endregion
    #region Lifecycle
    private void OnEnable()
    {
        if (string.IsNullOrEmpty(Name))
            UpdateNameFromAsset();      
    }
    #endregion
}




