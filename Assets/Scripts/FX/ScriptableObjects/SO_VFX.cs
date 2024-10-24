using UnityEngine;

public abstract class SO_VFX : ScriptableObject
{
    #region Properties
    public Material material
    {
        get
        {
            if (_currentMaterial == null)
                _currentMaterial = new Material(_material);

            return _currentMaterial;
        }
    }
    #endregion
    #region Fields
    private Material _currentMaterial;

    [SerializeField] 
    private Material _material;
    #endregion

}
