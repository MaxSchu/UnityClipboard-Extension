using UnityEngine;
using UnityEditor;
[InitializeOnLoad]
public class OpenWindowListener
{
    public static bool windowOpen = false;

    static OpenWindowListener()
    {
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }
    public static void OnSceneGUI(SceneView view)
    {
        Event e = Event.current;
        switch (e.type)
        {
            case EventType.keyDown:
                {
                    if (Event.current.keyCode == (KeyCode.I))
                    {
                        e.Use();
                        if (windowOpen == false)
                        {
                            Debug.Log("Openwindow in listener");
                            InventoryWindow.ShowWindow();
                            windowOpen = true;
                        } else
                        {
                            Debug.Log("Closewindow in listener");
                            EditorWindow.GetWindow<InventoryWindow>().Close();
                            windowOpen = false;
                        }                        
                    }
                    break;
                }
        }        
    }
}