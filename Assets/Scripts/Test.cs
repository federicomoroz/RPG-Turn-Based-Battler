using UnityEngine;

public class Test : MonoBehaviour
{
    public string end = "end";
    public string loop = "loop";

    private Timer timer;
    
    void Start()
    {
        timer = new Timer()
            .SetTime(2)
            .SetLoops(3)
            .SetLoopCallback(() => Debug.Log(loop))
            .SetCompleteCallback(() => Debug.Log(end)
            );
    }

}
