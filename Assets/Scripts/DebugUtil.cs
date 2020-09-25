using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// Singleton for creating debug text and helpful tools

public class DebugUtil : MonoBehaviour
{
    public static Dictionary<GameObject, List<string>> messages;

    private void Awake()
    {
        messages = new Dictionary<GameObject, List<string>>();
    }

#if (UNITY_EDITOR)
    private void OnDrawGizmos()
    {
        if (messages != null)
        {
            foreach (GameObject obj in messages.Keys)
            {
                StringWriter writer = new StringWriter();

                foreach (string msg in messages[obj])
                {
                    writer.WriteLine(msg);
                }

                Handles.Label(obj.transform.position, writer.ToString());
            }
        }
    }
#endif

    public static void AddMessage(GameObject parent, string message)
    {
        if (messages.ContainsKey(parent))
        {
            messages[parent].Add(message);
        }
        else
        { 
            messages.Add(parent, new List<string> { message });
        }
    }

    public static void ClearAllMessages(GameObject parent)
    {
        messages[parent].RemoveAll(s => true);
    }

    public static void ClearMessage(GameObject parent, string message)
    {
        messages[parent].RemoveAll(s => s.Equals(message));
    }
}
