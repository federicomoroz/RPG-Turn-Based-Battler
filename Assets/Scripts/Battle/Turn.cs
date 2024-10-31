using System.Collections;
using Skills;
using BattleUnits;

namespace BattleSystem
{
    public class Turn
    {
        SO_Skill skill;
        BattleUnits.BattleUnit user;
        AffectToken affectToken;
        BattleUnits.BattleUnit[] targets;

        event System.Action OnComplete;

        public IEnumerator Execute()
        {
            // Ejecutar la skill
            yield return skill.Execute(user, targets[0], () => UnityEngine.Debug.Log("Skill complete"));
            // Obtener lista de targets y affectToken
            
        }

        public void ApplyAffectToken(AffectToken affectToken, BattleUnit target)
        {

        }

        // Fases: 1) aplicar afecto de estado
        //        2) Ejecutar skill. Al terminar se debe devolver 1 affectToken y 1 array de BattleUnit (Targets : BattleUnit[])
        //        3) Por cada unit en BattleUnit, entregarle una copia del affectToken y un evento para avisar que terminó de procesarlo
        //        4) Cuando uno BattleUnit termina de procesar su affectToken, pasa al siguiente
        //        5) Cuando el último termina de procesar su affectToken, se da por completado el turno y se pasa al siguiente

           
    }
}