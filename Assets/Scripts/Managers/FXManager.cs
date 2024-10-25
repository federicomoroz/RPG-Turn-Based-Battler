using Pool;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class FXManager : PersistentSingleton<FXManager>
    {
        #region Fields
        private Dictionary<string, SO_VFX> _vfxVault = new Dictionary<string, SO_VFX>();

        [SerializeField]
        private List<PoolablePrefabSlot> _fxPoolItems;
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
        #region Lifecycle
        protected void Start()
        {            
            FillVault();
            foreach (var item in _fxPoolItems)            
                PoolManager.CreatePrefabPool<Impact>(item.prefab, item.amount);
            
        }
        #endregion
    }
}