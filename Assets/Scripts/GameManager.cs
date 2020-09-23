using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Scene
{ 
    IntroLevel
}

public class GameManager : MonoBehaviour
{

    void Start()
    {
          
    }

    public static void RestartLevel(Scene scene)
    {
        SceneManager.LoadScene((int) scene);
    }

}
