using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class FXManager : PersistentSingleton<FXManager>
    {
        #region Fields
        private Dictionary<string, SO_VFX> _vfxVault = new Dictionary<string, SO_VFX>();
        #endregion
        #region Lifecycle
        protected override void Awake()
        {
            base.Awake();
            FillVault();
        }
        #endregion
        #region Setup
        private void FillVault()
        {       
            SO_VFX[] vfxArray = Resources.LoadAll<SO_VFX>("ScriptableObjects/VFX");
                
            foreach (SO_VFX vfx in vfxArray)
            {            
                string key = vfx.name;
                        
                if (!_vfxVault.ContainsKey(key))            
                    _vfxVault.Add(key, vfx);                      
                else            
                    Debug.LogWarning($"La key {key} ya existe en el diccionario.");            
            }
        }
        #endregion
        #region Commands
        public SO_VFX GetVFX(string key)
        {
            if (_vfxVault.TryGetValue(key, out SO_VFX vfx))        
                return vfx;
        
            Debug.LogWarning($"VFX con la key {key} no encontrado.");
            return null;
        }
        #endregion
    }
}


