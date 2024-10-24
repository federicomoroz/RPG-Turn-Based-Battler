using UnityEngine;
[CreateAssetMenu(fileName = "newUnitStatsData", menuName = "Data/Unit Data/Stats Data")]
public class SO_UnitStats : ScriptableObject
{
    #region Fields
    [field: SerializeField] public GameValue hp { get; private set; }
    [field: SerializeField] public GameValue mp { get; private set; }
    [field: SerializeField] public int level { get; private set; }
    [field: SerializeField] public int physicalStrength { get; private set; }
    [field: SerializeField] public int specialStrength { get; private set; }
    [field: SerializeField] public int physicalDefense { get; private set; }
    [field: SerializeField] public int specialDefense { get;private set; }
    [field: SerializeField] public int speed { get; private set; }
    #endregion
    #region Commands
    public SO_UnitStats SetPhysicalStrength(int value)
    {
        this.physicalStrength = value;
        return this;
    }
    public SO_UnitStats SetSpecialStrength(int value)
    {
        this.specialStrength = value;
        return this;
    }
    public SO_UnitStats SetPhysicalDefense(int value)
    {
        this.physicalDefense = value;
        return this;
    }
    public SO_UnitStats SetSpecialDefense(int value)
    {
        this.specialStrength = value;
        return this;
    }
    public SO_UnitStats SetSpeed(int value)
    {
        this.speed = value;
        return this;
    }
    #endregion
}
