using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Commander;
using Skills;
using BattleUnits;

public partial class BattleManager : PersistentSingleton<BattleManager>
{
    #region Fields
    [SerializeField]
    private List<CommandToken> _commandTokens = new List<CommandToken>();

    [SerializeField]
    private float _timeBetweenTurns = 0.75f;

    private SO_Skill _currentSkill;
    private Action _completeTokenCallback;
    #endregion
    #region Commands
    public void SortCommandTokens()
    {
        _commandTokens = _commandTokens
            .OrderByDescending(token => token.Skill.priority)
            .ThenByDescending(token => token.User.host.Data.Stats.speed) 
            .ToList();
    }
    public static void ExecuteTokens() => Instance.StartCoroutine(Instance.Perform()); 
    public static void TriggerSkill()
    {
        if (Instance._currentSkill == null)
            return;

        Instance._currentSkill.Trigger();
    }

    #endregion
    #region Helpers / Utils
    private IEnumerator Perform()
    {
        foreach (var token in _commandTokens)
        {
            _currentSkill = token.Skill;            

            var user = token.User.host;
            var target = token.Target.host;
            var message = $"{user.Data.Name} {_currentSkill.name.ToLower()}ed {target.Data.Name}";

            yield return StartCoroutine(
                token.Skill.Execute(
                    user,
                    target,
                    () => Debug.Log(message)
                    ));

            _currentSkill = null;
            yield return new WaitForSeconds(_timeBetweenTurns);
        }        
    }
    #endregion

}
#region Background*
public partial class BattleManager : PersistentSingleton<BattleManager>
{
    #region Off

    [SerializeField]
    private Sprite _background;
    private GameObject _backgroundContainer;


    private void CreateBackground(Sprite sprite)
    {
        if (_backgroundContainer == null)
        {
            _backgroundContainer = new GameObject("o_Background");
            _backgroundContainer.transform.SetParent(null);
            _backgroundContainer.AddComponent<FullScreenSprite>().SetSprite(sprite);
        }

        _backgroundContainer.GetComponent<FullScreenSprite>().SetSprite(sprite);

    }
    public void CreateBackgroundInEditor()
    {
        if (_background == null)
        {
            Debug.LogError("No se ha asignado ningún Sprite de fondo.");
            return;
        }

        // Crea el nuevo background
        CreateBackground(_background);
    }
    #endregion
}
#endregion
