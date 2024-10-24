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
            //var token = GenerateToken(_user);
        
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
                _activeSkill.invocation.accuracy
                );

            return token;
        }

        private int CalculateDamage(SO_Skill skill, BattleUnit user) 
        {
            int skillBasePower = skill.affect.basePower;
            float powerVariance = skill.affect.variance * 0.01f;
            int affectedPower = Mathf.CeilToInt(Random.Range(skillBasePower-powerVariance, skillBasePower+powerVariance));
            int power = skill.affect.type == AffectType.Physical ? user.Data.Stats.physicalStrength : user.Data.Stats.specialStrength;
            float damage = affectedPower * 4 + (user.Data.Stats.level * power * (affectedPower / 32));
            return Mathf.RoundToInt(damage);
        
        }
        #endregion
    }
}