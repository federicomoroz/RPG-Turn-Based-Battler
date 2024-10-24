[System.Serializable]
public class Stats
{
    public int level { get; set; }
    public int hpCurrent { get; set; }
    public int hpMax { get; set; }
    public int mpCurrent { get; set; }
    public int mpMax { get; set; }
    public int physicalStrength { get; set; }
    public int specialStrength { get; set; }
    public int physicalDefense { get; set; }
    public int specialDefense { get; set; }
    public int speed { get; set; }

    public Stats(int level, int hpMax, int mpMax, int physicalStrength, int specialStrength, int physicalDefense, int specialDefense, int speed)
    {
        this.level = level;
        this.hpMax = hpMax;
        hpCurrent = hpMax;
        this.mpMax = mpMax;
        mpCurrent = mpMax;
        this.physicalStrength = physicalStrength;
        this.specialStrength = specialStrength;
        this.physicalDefense = physicalDefense;
        this.specialDefense = specialDefense;
        this.speed = speed;
    }

    public Stats SetLevel(int value)
    {
        this.level = value;
        return this;
    }

    public Stats SetHp(int value)
    {
        this.hpCurrent = value;
        return this;
    }
}
