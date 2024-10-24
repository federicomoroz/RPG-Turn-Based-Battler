using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowBattleCommand : WindowBase
{
    public int current { get; private set; }

    [SerializeField] private Transform _cursor;
    [SerializeField] private ButtonPlus[] _options;
    [SerializeField] private ButtonPlus outsideArea;

    protected override void Initialize()
    {
        base.Initialize();

        for (int i = 0; i < _options.Length; i++)
        {
            _options[i].id = i;
            _options[i].onSelect.AddListener(OnOptionSelected);         
        }

    }

    private void OnOptionSelected(int buttonID)
    {
        _cursor.gameObject.SetActive(true);
        
        _cursor.SetPositionAndRotation(_options[buttonID].transform.position, _cursor.rotation);
        current = buttonID;
    }

    private void OnClickOutsideArea() 
    {
        _cursor.gameObject.SetActive(false);
    }

}
