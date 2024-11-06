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
    public static void ExecuteTokens() => Instance.StartCoroutine(Instance.Perform()); 
    public static void TriggerSkill(BattleUnit user)
    {
        if (Instance._currentSkill == null)
            return;

        Instance._currentSkill.Trigger(user);
    }
    public static void AppleAffectToken() { }
    #endregion
    #region Helpers / Utils
    public void SortCommandTokens()
    {
        _commandTokens = _commandTokens
            .OrderByDescending(token => token.Skill.priority)
            .ThenByDescending(token => token.User.host.Data.Stats.speed) 
            .ToList();
    }
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
                    target.transform.position,
                    target.transform.position,
                    () => Debug.Log(message)
                    ));

            var affectToken = GenerateAffectToken(_currentSkill, token.User.host, token.Target.host);
            affectToken.OnProcessCompleteCallback?.Invoke();
            _currentSkill = null;
            yield return new WaitForSeconds(_timeBetweenTurns);
        }        
    }
    private AffectToken GenerateAffectToken(SO_Skill skill, BattleUnit user, BattleUnit target)
    {
        System.Action debugCompleteMessage = () => Debug.Log($"Hey {target.name}! {user} sends you this AffectToken. Process it and tell when you're done");

        var token = new AffectToken(
            skill.affect.action,
            skill.affect.type,
            CalculateDamage(skill, user),
            skill.invocation.isEvadable,
            skill.invocation.accuracy,
            skill.affect.element, 
            debugCompleteMessage
            );

        return token;
    }

    private int CalculateDamage(SO_Skill skill, BattleUnit user)
    {
        var skillBasePower = skill.affect.basePower;
        var powerVariance = skill.affect.variance * 0.01f;
        var minBasePower = skill.affect.basePower - powerVariance;
        var maxBasePower = skill.affect.basePower + powerVariance;
        var finalBasePower = Mathf.CeilToInt(UnityEngine.Random.Range(minBasePower, maxBasePower));
        var currentStrength = skill.affect.type == AffectType.Physical ? user.Data.Stats.physicalStrength : user.Data.Stats.specialStrength;
        var damage = finalBasePower * 4 + (user.Data.Stats.level * currentStrength * (finalBasePower / 32));
        return Mathf.RoundToInt(damage);
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