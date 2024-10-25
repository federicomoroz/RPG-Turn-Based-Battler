using System.Collections;
using UnityEngine;
using Pool;

[RequireComponent(typeof(ParticleSystem))]
public class Impact : MonoBehaviour, IPoolable<Impact>, IPoolablePrefab
{
    #region Dependencies
    private ParticleSystem _particleSystem;
    #endregion
    #region Events
    private event System.Action _onCompleteCallback;
    #endregion
    #region Fields
    [SerializeField] ushort playTimes = 1;
    [Range(0,1)]
    [SerializeField] float repeatOnLifecycleTimeRatio = 1;
    private WaitUntil _waitUntil;
    #endregion
    #region Setup
    public Impact SetCompleteCallback(System.Action callback)
    {
        _onCompleteCallback += callback;
        return this;
    }
    public Impact SetPlayTimes(ushort playTimes)
    {
        this.playTimes = playTimes;
        return this;
    }
    public Impact SetRepeatTimeOnRatio(float value)
    {
        repeatOnLifecycleTimeRatio = value;
        return this;
    }
    #endregion
    #region Commands
    public Impact Execute()
    {
        StartCoroutine(PlayParticles());        
        return this;
    }
    #endregion
    #region Helpers / Utils
    private IEnumerator PlayParticles()
    {
        for (int i = 0; i < playTimes; i++)
        {
            _particleSystem.Play();
            var particleDuration = _particleSystem.main.duration;
            var thresholdTime = particleDuration * repeatOnLifecycleTimeRatio;            
           
            yield return new WaitUntil(() => _particleSystem.time >= thresholdTime);

            if (i == playTimes - 1)
                yield return _waitUntil;          
        }

        _onCompleteCallback?.Invoke();
    }
    private void OnCompleteHandler()
    {
        Dispose();
    }
    #endregion
    #region Lifecycle
    private void Start()
    {
        Initialize();
        Execute();
    }
    public Impact Initialize()
    { 
        _particleSystem = GetComponent<ParticleSystem>();
        _onCompleteCallback = OnCompleteHandler;
        _waitUntil = new WaitUntil(() => !_particleSystem.isPlaying);
        return this;
    }
    public void Dispose()
    {
        _onCompleteCallback = OnCompleteHandler;        
        PoolManager.ReturnObject(this);

    }
    #endregion

}
