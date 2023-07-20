using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://www.youtube.com/watch?v=JQ0Jdfxo7Cg

class App
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        GameObject app = Object.Instantiate(Resources.Load("App")) as GameObject;
        if (app == null)
            throw new System.ApplicationException();

        Object.DontDestroyOnLoad(app);
    }
}