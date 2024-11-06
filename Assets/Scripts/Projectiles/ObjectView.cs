using Managers;
using UnityEngine;
using Pool;
using Utils;

public class ObjectView
{
    #region Fields
    private int _currentSpriteIndex;
    #endregion
    #region Dependencies
    private SO_ObjectView _data;
    private SpriteRenderer _sr;
    #endregion
    #region Delegates
    private event System.Action 
        OnAnimationComplete,
        CancellationAction;
    #endregion
    #region Constructor
    public ObjectView(SpriteRenderer sr) 
    { 
        _sr = sr;     
        Initialize();
    }
    #endregion
    #region Setup
    public ObjectView SetData(SO_ObjectView data)
    {
        _data = data;
        _sr.sprite = _data.sprites[0].sprite;

        return this;
    }
    public ObjectView SetCompleteCallback(params System.Action[] callbacks) => this.RegisterMultipleCallbacks(ref OnAnimationComplete, callbacks);
    #endregion
    #region Commands
    /// <summary>
    /// El método pide que se le pase un valor de velocidad de animación.
    /// Luego lo traduce a framerate en un rango de entre 0.05s y 0.2s.
    /// </summary>
    /// <param name="speed"></param>
    /// <returns></returns>
    public ObjectView Play(float speed, bool loop = true)
    {
        _currentSpriteIndex = 0;

        var timer = PoolManager.GetObject<Timer>()
            .SetTime(GetFramerate(speed))
            .SetLoops(loop ? 0 : _data.sprites.Length + 1) 
            .RegisterLoopCallback(Animate)
            .RegisterStopAction(ref CancellationAction)
            .RegisterCompleteCallback(Dispose)
            ;

        if(_data.sfx.cast != null) 
            SoundManager.PlaySound(_data.sfx.cast);

        return this;
    }
    public void Stop() => CancellationAction?.Invoke();
    #endregion
    #region Handlers
    private void OnCancellationHandler()
    {
        if (_data.sfx.impact != null)
            SoundManager.PlaySound(_data.sfx.impact);

        Dispose();
    }
    #endregion
    #region Animation
    private void Animate()
    {
        if (_data == null) 
            return;

        if (_currentSpriteIndex >= _data.sprites.Length)      
               _currentSpriteIndex = 0;

        if (_data.sprites[_currentSpriteIndex].playSfx)        
            SoundManager.PlaySound(_data.sprites[_currentSpriteIndex].sfx);
        
        UpdateSprite(_data.sprites[_currentSpriteIndex].sprite);
    }
    private void UpdateSprite(Sprite sprite)
    { 
        if (_sr == null) 
            return;

        _sr.sprite = sprite;

        _currentSpriteIndex++;
    }

    private float GetFramerate(float speed) => Mathf.Lerp(0.2f, 0.05f, speed / 10f);
    #endregion
    #region Lifecycle
    private void Initialize()
    {
        _sr.sortingLayerName = "Game";
        _sr.sortingOrder = 2;

        CancellationAction = OnCancellationHandler;
    }
    private void Dispose()
    {
        OnAnimationComplete?.Invoke();
        OnAnimationComplete = null;
        CancellationAction = OnCancellationHandler;
        _sr.sprite = null;
        _currentSpriteIndex = 0;
        _data = null;
    }
    #endregion
}
