using UnityEngine;

[CreateAssetMenu(fileName = "newShadowData", menuName = "Data/VFX Data/Shadow")]
public class SO_Shadow : ScriptableObject
{
    #region Properties
    public Color Color => _color;
    public float Alpha => _alpha;
    public float ScaleY => _scaleY;
    public float Rotation => _rotation;
    public float OffsetY => _yOffset;
    #endregion
    #region Fields
    [SerializeField]
    private Color _color = Color.black;
    [SerializeField]
    private float _alpha = 0.5f;
    [SerializeField]
    private float _scaleY = 0.1f;
    [SerializeField]
    private float _rotation;
    [SerializeField]
    private float _yOffset = 0.03f;
    #endregion
}
