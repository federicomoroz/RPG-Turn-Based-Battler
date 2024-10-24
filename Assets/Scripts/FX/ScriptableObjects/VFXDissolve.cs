using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "newVFXDissolveData", menuName = "Data/VFX Data/Dissolve")]
public class VFXDissolve : SO_VFX
{
    #region Fields
    public AudioClip sfx => _castSfx;
    public float pitch => _pitch;
    private float _progress;

    [Header("Visual")]

    [SerializeField]
    [Range(0.1f, 5f)]
    private float _dissolveSpeed;

    [SerializeField]
    [Range(0, 500)]
    private float _spiralStrength;

    [SerializeField]
    [Range(0, 1)]
    private float _outlineThickness;

    [SerializeField]
    [Range(0, 1000)]
    private float _noiseScale;

    [SerializeField]
    [ColorUsage(true, true)]
    private Color _colorFX;

    [Header("Audio")]

    [SerializeField]
    private AudioClip _castSfx;

    [SerializeField]
    [Range(-1,3)]
    private float _pitch;
    #endregion
    #region Commands 
    public IEnumerator ChangeVisibilityCO(bool dissolve, System.Action OnCompleteCallback = null)
    {
        _progress = dissolve ? 0 : 1; 
        float targetProgress = dissolve ? 1 : 0;

        while ((_progress < targetProgress && dissolve) || (_progress > targetProgress && !dissolve))
        {
            _progress = Mathf.Clamp01(_progress + (dissolve ? Time.deltaTime * _dissolveSpeed : -Time.deltaTime * _dissolveSpeed));
            material.SetFloat("_Progress", _progress);
            yield return null;
        }

        OnCompleteCallback?.Invoke();
    }
    #endregion
}
