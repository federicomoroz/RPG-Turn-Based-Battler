using System.Collections;
using UnityEngine;
using BattleUnits;

public abstract class SO_Movement : ScriptableObject
{
    #region Dependencies
    [SerializeField]
    protected AudioClip sfxCast;
    #endregion
    #region Delegates
    protected System.Action OnComplete;
    #endregion
    #region Commands
    public abstract IEnumerator Execute(BattleUnit user, Vector3 target);
    #endregion
}