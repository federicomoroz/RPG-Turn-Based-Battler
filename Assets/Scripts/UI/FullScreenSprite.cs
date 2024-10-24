using UnityEditor;
using UnityEngine;

public class FullScreenSprite : MonoBehaviour
{
    private SpriteRenderer _sr;    
    private void Start()
    {
        if (_sr == null)
            _sr = gameObject.GetComponent<SpriteRenderer>();      

        FitSpriteToScreen();
    }

    public FullScreenSprite SetSprite(Sprite sprite)
    {
        if(_sr == null)
            _sr = gameObject.AddComponent<SpriteRenderer>();

        _sr.sprite = sprite;
        FitSpriteToScreen();
        return this;
    }


    private void FitSpriteToScreen()
    {
        _sr.sortingLayerName = "Background";

        // Obtén el tamaño del sprite
        float spriteWidth = _sr.sprite.bounds.size.x;
        float spriteHeight = _sr.sprite.bounds.size.y;

        // Verifica si estamos en el Editor o en modo Runtime
        if (Application.isPlaying)
        {
            // Modo Runtime: obtén el tamaño de la cámara en unidades del mundo
            float worldScreenHeight = Camera.main.orthographicSize * 2;
            float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

            // Calcula la escala necesaria para Runtime
            Vector3 newScale = transform.localScale;
            newScale.x = worldScreenWidth / spriteWidth;
            newScale.y = worldScreenHeight / spriteHeight;
            transform.localScale = newScale;
        }
        else
        {
            //// Modo Editor: Calcula el tamaño basado en una aproximación del tamaño de la ventana en el Editor
            //Camera sceneCamera = SceneView.lastActiveSceneView.camera;
            //float editorWorldScreenHeight = sceneCamera.orthographicSize * 2;
            //float editorWorldScreenWidth = editorWorldScreenHeight / Screen.height * Screen.width;

            //// Calcula la escala necesaria para el Editor
            Vector3 newScale = transform.localScale;            
            newScale.x = 2.1f;
            newScale.y = 2.1f;
            transform.localScale = newScale;
        }
    }
}
