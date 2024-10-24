using UnityEngine;

public static class Vocab
{
    public static string BattleStart(string enemy) => string.Format("{0} draws near! ", enemy);
    public static string AttackExecution(string attackerName, string abilityName) => string.Format("{0} uses {1}. ", attackerName, abilityName);
    public static string DamageReceived(string targetName, int damageAmount, int hp) => string.Format("{0} got {1} damage. {2}hp remaining. ", targetName, damageAmount, hp);
    public static string BattleFinished(string name) => string.Format("Battle won, well done {0}. ", name);    
    public static string Victory(string enemy) => string.Format("Thous hast done well in defeating the {0}. ", enemy);
    public static string GoldIncome(int amount) => string.Format("Thy gold increases by {0}. ", amount);
    public static string GoldOutcome(int amount) => string.Format("Thy gold decreases by {0}. ", amount);
    public static string Xp(int amount) => string.Format("Thy XP increases by {0}. ", amount);
}
