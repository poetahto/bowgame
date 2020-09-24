using UnityEngine;
using System.Collections;

/**
 * A class for accurately displaying and updating the current FPS
 * 
 * @Author Dave Hampson
**/

// note: I didn't make this class, just yoinked it from somebodies github but its super
// nice for testing, especially cus unity doesn't display the correct fps in stats

public class FPSDisplay : MonoBehaviour {
    float deltaTime = 0.0f;

    void Update() {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI() {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperRight;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = Color.white;
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        GUI.Label(rect, text, style);
    }
}