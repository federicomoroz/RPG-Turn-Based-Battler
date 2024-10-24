using UnityEngine;

public class Debugger : MonoBehaviour
{
    private void Update() 
    {
        if(Input.GetKeyDown(KeyCode.V))
            BattleManager.Instance.SortCommandTokens();

        if (Input.GetKeyDown(KeyCode.Space))
            BattleManager.ExecuteTokens();
    }
}
