using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ShadowController : MonoBehaviour
{
    private SpriteRenderer _sr;
    private SpriteRenderer _parentSr;

    [SerializeField]
    private SO_Shadow _data;

    private void Awake()
    { 
        SetUp();
    }

    private void LateUpdate()
    {      
        _sr.sprite = _parentSr.sprite;        
    }

    private void SetUp()
    {
        _sr = GetComponent<SpriteRenderer>();
        _parentSr = transform.parent.GetComponent<SpriteRenderer>();

        var color = _data.Color;
        color.a = _data.Alpha;
        _sr.color = color;
        
        transform.localPosition = new Vector3(0f, transform.localPosition.y + _data.OffsetY, 0);
        transform.localScale = new Vector3(transform.localScale.x, _data.ScaleY, transform.localScale.z);
        transform.rotation = Quaternion.Euler(0, 0, _data.Rotation);
                
        _sr.flipX = _data.Rotation > 90;
    }

    public void UpdateSprite(Sprite sprite)
    {
        _sr.sprite = sprite;
    }
}
