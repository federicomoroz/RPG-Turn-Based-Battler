using UnityEngine;
using Skills;
using Sprites;

[CreateAssetMenu(fileName = "newUnitData", menuName = "Data/Unit Data/Base Data")]
public class SO_UnitData : ScriptableObject
{
    #region Fields
    [field: SerializeField] public string Name { get; private set; }
    public Sprite Portrait { get; private set; }
    public Sprite[] Spritesheet { get; private set; }
    [field: SerializeField] public SkillSet Skills { get; private set; } = new SkillSet();
    #endregion
    #region Properties
    public UnitSpritesSet[] SpritesVariants => _spritesVariants;
    public SO_UnitStats Stats { get; private set; }
    public SO_UnitMotions Motions => _motions;
    #endregion
    #region Dependencies
    [SerializeField] 
    private SO_UnitStats _stats;
    [SerializeField]
    private SO_UnitMotions _motions;
    [SerializeField] 
    private UnitSpritesSet[] _spritesVariants;
    public SO_Movement MovementType;
    #endregion
    #region Methods
    private void OnEnable()
    {
        if (_stats == null)
            return;

        Stats = Instantiate(_stats); // Clonación de SO para que los cambios no afecten a la instancia global
    }
    public void SetSpriteVariants(UnitSpritesSet[] variants) => _spritesVariants = variants;
    #endregion
}
