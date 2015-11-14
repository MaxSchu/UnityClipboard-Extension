using UnityEngine;
using UnityEditor;
[InitializeOnLoad]
public class OpenWindowListener
{
    public static bool windowOpen = false;    
    private static int csvId = 0;
    public static CSVLogger csvLog = new CSVLogger("testCSV" + csvId + ".csv");

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
                    if (e.keyCode == (KeyCode.I))
                    {
                        e.Use();
                        if (windowOpen == false)
                        {
                            InventoryWindow.ShowWindow();
                            windowOpen = true;
                        } else
                        {
                            EditorWindow.GetWindow<InventoryWindow>().Close();
                            windowOpen = false;
                        }                        
                    }
                    if (e.control && e.keyCode == KeyCode.RightArrow)
                    {
                        csvLog.NextTask();
                        e.Use();
                    }
                    if (e.control && e.keyCode == KeyCode.Delete)
                    {
                        csvLog.Dispose();
                        csvId++;
                        csvLog = new CSVLogger("testCSV" + csvId + ".csv");
                        InventoryWindow.setCSVLogger(csvLog);
                    }
                    break;
                }
        }        
    }

    public static CSVLogger GetLogger()
    {
        return csvLog;
    }
}