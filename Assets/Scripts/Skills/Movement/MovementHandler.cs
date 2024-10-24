using System.Collections.Generic;
using UnityEngine;
using Skills;

public class MovementHandler
{
    #region Fields
    private Dictionary<MovementType, System.Action> _strategyVault = new Dictionary<MovementType, System.Action>();
    private Vector3 _targetPosition;
    #endregion
    #region Dependencies
    private Transform _transform;
    private MovementData _data;
    #endregion
    #region Delegates
    private event System.Action _onComplete;
    #endregion
    #region Constructor
    public MovementHandler(Transform transform)
    {
        _transform = transform;                 
        Initialize();
    }
    #endregion
    #region Setup
    public MovementHandler SetData(MovementData data)
    {
        _data = data;
        return this;
    }
    public MovementHandler SetTarget(Vector3 targetPosition)
    {
        this._targetPosition = targetPosition;
        return this;
    }
    #endregion
    #region Commands
    public MovementHandler Execute(MovementType type, System.Action onCompleteCallback)
    {
        _onComplete += onCompleteCallback;

        if (_strategyVault.TryGetValue(type, out System.Action strategy))         
            strategy.Invoke();        

        return this;
    }
    public void Stop() => Dispose();
    #endregion
    #region Helpers / Utils
    private Vector3 CalculateTargetPositionByDistance() =>
        _transform.position + _transform.right * _data.trajectoryLength;
    #endregion
    #region Lifecycle
    public MovementHandler Initialize()
    {
        _strategyVault.Add(
            MovementType.TargetedLinear,
            () => new LinearMovement()
            .Execute(
                _transform,
                _targetPosition,
                _data.speed,
                _data.rotate,
                OnCompleteHandler,
                Stop
                )
            );

        _strategyVault.Add(
            MovementType.TargetedCurve,
            () => new CurveMovement()
            .SetCurve(_data.trajectoryCurve)
            .Execute(
                _transform,
                _targetPosition,
                _data.speed,
                _data.rotate,
                OnCompleteHandler,
                Stop
                )
            );

        _strategyVault.Add(
            MovementType.DistanceLinear,
            () => new LinearMovement()
            .Execute(
                _transform,
                CalculateTargetPositionByDistance(),
                _data.speed,
                _data.rotate,
                OnCompleteHandler,
                Stop
                )
            );

        _strategyVault.Add(
            MovementType.None,
            () => Debug.LogWarning("No movement selected")
            );

        return this;
    }
    private void OnCompleteHandler()
    {
        _onComplete?.Invoke();
        Dispose();
    }
    private void Dispose()
    {
        _targetPosition = Vector3.zero;
        _onComplete = null;        
    }
    #endregion
}
