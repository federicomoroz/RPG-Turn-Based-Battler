using System.Collections.Generic;
using UnityEngine;
using Sprites;

[CreateAssetMenu(fileName = "newUnitMotionsData", menuName = "Data/Unit Data/Motions Data")]
public class SO_UnitMotions : ScriptableObject
{
    public AnimationClip
        Idle,
        Melee1,
        Melee2,
        Range1,
        Range2,
        Damage;

    public PhasedAnimation
         Jump,
         Dash;

    private Dictionary<string, AnimationClip> animationDictionary;

    private void OnEnable()
    {
        // Inicializamos el diccionario de animaciones
        animationDictionary = new Dictionary<string, AnimationClip>
        {
            { "Idle", Idle },
            { "Melee1", Melee1 },
            { "Melee2", Melee2 },
            { "Range1", Range1 },
            { "Range2", Range2 },
            { "Damage", Damage },
            { "DashStart", Dash.Start },
            { "DashLoop", Dash.Loop },
            { "DashEnd", Dash.End },
            { "JumpStart", Jump.Start },
            { "JumpLoop", Jump.Loop },
            { "JumpEnd", Jump.End }
        };
    }

    public AnimationClip GetAnimationByName(string name)
    {
        // Retornamos la animación según el nombre
        animationDictionary.TryGetValue(name, out AnimationClip clip);
        return clip; // Devuelve null si no se encuentra
    }
}
