using UnityEngine;
using System;


public class GameManager : PersistentSingleton<GameManager>
{
    private System.Reflection.Assembly[] assemblies;
    [SerializeField]
    float timeScaleSlow = 0.5f;
    protected override void Awake()
    {        
        base.Awake();
        Application.targetFrameRate = 60;
        assemblies = AppDomain.CurrentDomain.GetAssemblies();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if(Time.timeScale < 1)
                Time.timeScale = 1;
            else
                Time.timeScale = timeScaleSlow;
        }
    }

    public System.Reflection.Assembly[] GetAssemblies()
    {
        return assemblies;
    }
}
