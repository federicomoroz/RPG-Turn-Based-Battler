using System;
using Pool;
using Utils;

public class Timer : GameValue, IPoolable<Timer>
{        
    #region Fields
    private bool isLoop;
    private int
        _loops,
        _currentLoop;
    #endregion
    #region Delegates
    private Action
        onLoop,
        onComplete;
    #endregion
    #region Setup
    public Timer SetTime(float duration)
    {
        SetMax(duration);     
        SetMaxCallback(OnTimeOutHandler);
        Play();

        return this;
    }
    /// <summary>
    /// Setea la cantidad de loops que dará el timer.
    /// Si se le pasa 0 como valor, loopeará indefinidamente
    /// </summary>
    public Timer SetLoops(int value)
    {
        _loops = value;
        isLoop = true;        
        return this;
    }
    public Timer SetLoopCallback(params Action[] callbacks) => this.SetMultipleCallbacks(ref onLoop, callbacks);
    public Timer SetCompleteCallback(params Action[] callbacks) => this.SetMultipleCallbacks(ref onComplete, callbacks);
    public Timer SetStopAction(ref Action action)
    {
        action += SetOff;        
        return this;
    }
    #endregion
    #region Commands
    public void Tick() => Add(UnityEngine.Time.deltaTime);     
    public void SetOff() => TimerManager.Instance.RemoveTimer(this);                 
    private void Play() => TimerManager.Instance.AddTimer(this);
    #endregion
    #region Handlers
    private void OnTimeOutHandler()
    {
        if (!isLoop)
        {
            OnCompleteHandler();
            return;
        }        

        _currentLoop++;

        if (_loops > 0 && _currentLoop >= _loops)
        {                 
            OnCompleteHandler();
            return;
        }

        onLoop?.Invoke();
        Reset();
    }
    private void OnCompleteHandler()
    {
        onComplete?.Invoke();
        SetOff();
    }
    #endregion
    #region Lifecycle
    public Timer Initialize() => this;    
    public void Dispose()
    {
        Clean();
        PoolManager.ReturnObject(this);
    }
    protected override void Clean()
    {
        base.Clean();
        _currentLoop = 0;
        _loops = 0;
        isLoop = false;
        onLoop = null;
        onComplete = null;
    }
    #endregion
}
