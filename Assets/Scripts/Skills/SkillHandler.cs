using UnityEngine;
using BattleUnits;

namespace Skills
{
    public class SkillHandler
    {
        #region Fields
        private SO_Skill _activeSkill;
        private AffectToken _affectToken;
        #endregion
        #region Delegates
        private System.Action<AffectToken> sendAffectTokenCallback;
        private System.Action onCompleteCallback;
        #endregion
        #region Commands
        public void Execute(SO_Skill skill, BattleUnit user, BattleUnit target)
        {
            _activeSkill = skill;
            user.StartCoroutine(skill.Execute(user, target, onCompleteCallback));
        }
        public void Trigger()
        {                    
        
            _activeSkill.Trigger();
        
            _activeSkill = null;
        }
        #endregion
        #region Helpers / Utils    
        private AffectToken GenerateToken(BattleUnit user)
        {    
            var token = new AffectToken(
                _activeSkill.affect.action, 
                _activeSkill.affect.type,
                CalculateDamage(_activeSkill, user), 
                _activeSkill.invocation.isEvadable, 
                _activeSkill.invocation.accuracy,
                _activeSkill.affect.element,
                () => Debug.Log($"{_activeSkill.Name} finished")
                );

            return token;
        }

        private int CalculateDamage(SO_Skill skill, BattleUnit user) 
        {
            var skillBasePower = skill.affect.basePower;
            var powerVariance = skill.affect.variance * 0.01f;
            var minBasePower = skill.affect.basePower - powerVariance;
            var maxBasePower = skill.affect.basePower + powerVariance;
            var finalBasePower = Mathf.CeilToInt(Random.Range(minBasePower, maxBasePower));
            var currentStrength = skill.affect.type == AffectType.Physical ? user.Data.Stats.physicalStrength : user.Data.Stats.specialStrength;
            var damage = finalBasePower * 4 + (user.Data.Stats.level * currentStrength * (finalBasePower / 32));
            return Mathf.RoundToInt(damage);        
        }
        #endregion
    }
}