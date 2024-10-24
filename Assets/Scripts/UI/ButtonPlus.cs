using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonPlus : Button, ISelectHandler, IDeselectHandler
{
    [Serializable]
    public class ButtonSelectedEvent : UnityEvent<int> { }
    [Serializable]
    public class ButtonDeselectedEvent : UnityEvent<int> { }

    [SerializeField]
    private ButtonSelectedEvent m_OnSelect = new ButtonSelectedEvent();
    [SerializeField]
    private ButtonDeselectedEvent m_OnDeselect = new ButtonDeselectedEvent();

    private int m_id;
    protected ButtonPlus() { }

    public ButtonSelectedEvent onSelect
    {
        get { return m_OnSelect; }
        set { m_OnSelect = value; }
    }

    public ButtonDeselectedEvent onDeselect
    {
        get { return m_OnDeselect; }
        set { m_OnDeselect = value; }
    }

    public int id
    {
        get { return m_id; }
        set { m_id = value; }
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);

        if (!IsActive() || !IsInteractable()) 
            return;

        m_OnSelect.Invoke(m_id);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);

        if (!IsActive() || !IsInteractable())
            return;

        m_OnDeselect.Invoke(m_id);
    }
}
