using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Wrapper class for all our LeanTween needs. LeanTween is a library
// for helping animate different properties of GameObjects, and is 
// especially efficient for animating UI elements.

public class AnimationHelper
{
    // Scales target game object to the desired size.
    public static void ChangeSize(GameObject targetObject, float targetX, float targetY, float targetZ, float time, bool cancelCurrentAnimation = true)
    {
        if (cancelCurrentAnimation) LeanTween.cancel(targetObject);
        LeanTween.scaleX(targetObject, targetX, time);
        LeanTween.scaleY(targetObject, targetY, time);
        LeanTween.scaleZ(targetObject, targetZ, time);
    }
    public static void ChangeSize(GameObject targetObject, Vector3 targetScale, float time, bool cancelCurrentAnimation = true)
    {
        ChangeSize(targetObject, targetScale.x, targetScale.y, targetScale.z, time, cancelCurrentAnimation);
    }
}
