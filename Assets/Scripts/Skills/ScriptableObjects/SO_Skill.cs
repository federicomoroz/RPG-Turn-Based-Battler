using System.Collections;
using UnityEngine;
using BattleUnits;

namespace Skills
{
    [CreateAssetMenu(fileName = "newSkillData", menuName = "Data/Skill Data/Skill")]
    public abstract class SO_Skill : ScriptableObject
    {
        #region Fields
        public Sprite icon;

        public string
            Name,
            description;

        public int costMP;
        public Scope scope = new Scope();
        public Invocation invocation = new Invocation();
        public Message message = new Message();
        public Affect affect = new Affect();
        public Priority priority;
        public bool DoMovement;
        [Range(1,2)]
        public ushort animationVariant;

        #endregion
        #region Commands
        public abstract IEnumerator Execute(BattleUnit user, BattleUnit target, System.Action completeCallback);
        public abstract void Trigger();
        #endregion
    }
}
