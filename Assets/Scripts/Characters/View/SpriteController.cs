using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class SpriteController : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    private event System.Action<Sprite> onSpriteChange;
    private event System.Action onTriggerSkill;


    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
    }

    public SpriteController RegisterTriggerSkillCallback(System.Action callback)
    {
        onTriggerSkill = callback;
        return this;
    }

    public void OnSpriteChangeHandler() => onSpriteChange?.Invoke(_spriteRenderer.sprite);   
    public void TriggerSkillHandler() => onTriggerSkill?.Invoke();

    public void Dispose()
    {
        onSpriteChange = null;
        onTriggerSkill = null;
    }
    
}
