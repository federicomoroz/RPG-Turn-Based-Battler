using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitMotions", menuName = "Data/Unit Motions")]
public class SO_UnitMotions : ScriptableObject
{
    public AnimationClip 
        Idle,
        Melee,
        Range,
        Range2,
        Damage,
        DashStart,
        DashLoop,
        DashEnd,
        JumpStart,
        JumpLoop,
        JumpEnd;

    private Dictionary<string, AnimationClip> animationDictionary;

    private void OnEnable()
    {
        // Inicializamos el diccionario de animaciones
        animationDictionary = new Dictionary<string, AnimationClip>
        {
            { "Idle", Idle },
            { "Melee", Melee },
            { "Range", Range },
            { "Range2", Range2 },
            { "Damage", Damage },
            { "DashStart", DashStart },
            { "DashLoop", DashLoop },
            { "DashEnd", DashEnd },
            { "JumpStart", JumpStart },
            { "JumpLoop", JumpLoop },
            { "JumpEnd", JumpEnd }
        };
    }

    public AnimationClip GetAnimationByName(string name)
    {
        // Retornamos la animación según el nombre
        animationDictionary.TryGetValue(name, out AnimationClip clip);
        return clip; // Devuelve null si no se encuentra
    }
}
