using UnityEngine;
using System;
using Utils;

[System.Serializable]
public class GameValue
{
    #region Fields
    [field: SerializeField] public float maxValue { get; private set; }
    private float _currentValue = 0;
    #endregion
    #region Properties
    public float Value => _currentValue;
    public float Ratio => Value / maxValue;
    #endregion
    #region Delegates
    public delegate void RationableChangeEvent(float value, float ratio);
    #endregion
    #region Events
    private event Action
        OnMax,
        OnZero;
    private event RationableChangeEvent OnValueChanged;
    #endregion
    #region Commands
    public GameValue Add(float value) => SetValue(this.Value + value);
    public GameValue Subtract(float value) => SetValue(this.Value - value);
    public GameValue IncreaseRatio(float ratio) => Add(Value * ratio);
    public GameValue DecreaseRatio(float ratio) => Subtract(Value * ratio);
    public GameValue SetMax(float amount)
    {
        maxValue = amount;
        return this;
    }
    public GameValue SetValue(float amount)
    {
        _currentValue = Mathf.Clamp(amount, 0, maxValue);

        OnValueChanged?.Invoke(_currentValue, Ratio);

        if (_currentValue == 0)
            OnZero?.Invoke();

        if (_currentValue == maxValue)
            OnMax?.Invoke();

        return this;
    }
    #endregion
    #region Setup
    public GameValue RegisterZeroCallback(params Action[] callbacks) 
        => this.RegisterMultipleCallbacks(ref OnZero, callbacks);
    public GameValue RegisterMaxCallback(params Action[] callbacks) 
        => this.RegisterMultipleCallbacks(ref OnMax, callbacks);
    public GameValue RegisterValueChangedCallback(RationableChangeEvent callback) 
    {
        OnValueChanged += callback;
        return this;
    }       
    #endregion
    #region Lifecycle
    protected virtual void Clean()
    {
        OnZero = null;
        OnValueChanged = null;
        OnMax = null;
        maxValue = 0;
        Reset();      
    }
    protected void Reset() => _currentValue = 0;
    #endregion
}
