using System.Collections.Generic;

public class TimerManager : PersistentSingleton<TimerManager>
{
    #region Fields
    private HashSet<Timer> _activeTimers = new HashSet<Timer>();
    private Queue<Timer> _inactiveTimers = new Queue<Timer>();
    private Queue<Timer> _pendingTimers = new Queue<Timer>();
    #endregion
    #region Commands
    public void AddTimer(Timer timer) => _pendingTimers.Enqueue(timer);
    public void RemoveTimer(Timer timer) => _inactiveTimers.Enqueue(timer);
    #endregion
    #region Lifecycle
    private void Update()
    {
        if (_activeTimers.Count <= 0 && _pendingTimers.Count <= 0)
            return;
           
        foreach (var timer in _activeTimers)        
            timer.Tick();
        
        while (_inactiveTimers.Count != 0)
        {
            var timer = _inactiveTimers.Dequeue();
            _activeTimers.Remove(timer);
            timer.Dispose();
        }

        while (_pendingTimers.Count != 0)         
            _activeTimers.Add(_pendingTimers.Dequeue());        
    }
    #endregion
}
