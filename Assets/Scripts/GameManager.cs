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
        InitializeSettings();
    }

    private void InitializeSettings()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 0;
    }

    public static void LoadLevel(Scene scene)
    {
        SceneManager.LoadScene((int) scene);
    }
}
