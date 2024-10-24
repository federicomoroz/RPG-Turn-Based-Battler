using Managers;
using System.Collections;
using UnityEngine;
using Pool;

[RequireComponent(typeof(AudioSource))]
public class SFX : MonoBehaviour, IPoolable<SFX>
{
    #region Fields
    private float _savedTime;
    private WaitForSeconds _timer;
    #endregion
    #region Dependencies
    private AudioSource _source;
    #endregion
    #region Delegates
    private System.Action _stopCallback;
    #endregion
    #region Setup
    public SFX Set(AudioClip clip, float volume = 1, float pitch = 1)
    {
        _source.playOnAwake = false;
        _source.clip = clip;
        _source.volume = volume;
        _source.pitch = pitch;
                       
        if (_timer == null)
            _timer = new WaitForSeconds(0);          
        
        return this;
    }
    #endregion
    #region Commands
    public SFX Play()
    {        
       StartCoroutine(PlayCo());        
       return this;
    }
    public SFX PlayLoop(System.Action stopAction)
    {
        stopAction += 
            _stopCallback += 
            () => SoundManager.RemoveLoopSfx(this);

        _source.loop = true;
        _source.Play();

        return this;
    }
    public void Pause()
    {
        if (_source.isPlaying) 
        {
            _savedTime = _source.time; 
            _source.Pause(); 
        }
    }
    public void Resume()
    {
        if (_source.clip != null) 
        {
            _source.time = _savedTime; 
            _source.Play(); 
            _savedTime = 0; 
        }
    }
    #endregion
    #region Helpers / Utils
    private IEnumerator PlayCo()
    {        
        _source.Play();        
        _timer = new WaitForSeconds(_source.clip.length);        
        yield return _timer; 

        Dispose();
    }
    #endregion
    #region Lifecycle
    public void Dispose()
    {
        _source.Stop();
        _savedTime = 0;   
        _source.loop = false;
        _source.clip = null;
        _stopCallback = null;

        PoolManager.ReturnObject(this);
    }
    public SFX Initialize()
    {
        return this;
    }
    private void OnEnable()
    {
        if (_source == null)
            _source = GetComponent<AudioSource>();

        _stopCallback = Dispose;
    }
    #endregion
}
