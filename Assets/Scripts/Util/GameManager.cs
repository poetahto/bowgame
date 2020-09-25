using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Note; scene enums should be ordered corresponding to their build order
public enum Scene
{ 
    IntroLevel
}

// Kinda lame singleton for handling game logic related stuff,
// such as loading levels (make async later)
public class GameManager : MonoBehaviour
{
    public ControllableObject startingObject = null;

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
