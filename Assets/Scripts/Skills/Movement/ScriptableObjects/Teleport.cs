using System.Collections;
using UnityEngine;
using Managers;
using BattleUnits;

[CreateAssetMenu(fileName = "newTeleportData", menuName = "Data/Movement Data/Teleport Data")]
public class Teleport : SO_Movement
{
    public override IEnumerator Execute(BattleUnit user, Vector3 target)
    {
        //1 SETEO
        // 1.a Conseguir el VFX
        var vfx = (VFXDissolve)FXManager.Instance.GetVFX("VFX_Teleport");
        // 1.b Cachear material y layer originales
        var cachedMaterial = user.Sr.sharedMaterial;
        var cachedLayerMask = user.Sr.gameObject.layer;
        // 1.c Setear objeto en la layer de postPro
        user.Sr.gameObject.layer = 7;
        // 1.d Overridear material con el del VFX    
        user.Sr.material = vfx.material;

        //2 PROCESO
        // 2.a Desaparición
        // 2.a.a audio
        SoundManager.PlaySound(sfxCast);
        // 2.a.b visual
        yield return FXManager.Instance.StartCoroutine(            
            vfx.ChangeVisibilityCO(true, () => user.transform.position = target)
            );

        // 2.b Reparición
        // 2.b.a audio
        SoundManager.PlaySound(sfxCast);
        // 2.b.b visual
        yield return FXManager.Instance.StartCoroutine(
            vfx.ChangeVisibilityCO(
                false,
                () =>
                {
                    user.Sr.material = cachedMaterial;
                    user.Sr.gameObject.layer = cachedLayerMask;             
                }
                )
            );
        OnComplete?.Invoke();
    }
}
