using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

public class MyWindow : EditorWindow

{
    private UnityEngine.Object[] objectArray;
    private int width;
    private int height;
    private int clickedBoxId = -1;
    private int boxWidth = 100;
    private int boxHeight = 100;
    private GUIStyle boxStyle;
    private Vector2 scrollPosition;

    bool dragStarted = false;
    int dragStartedAt = -1;

    public void OnEnable()
    {
        InitUI();
        InitArray();
        LoadPrefs();
    }

    public void OnDisable()
    {
        SafePrefs();
    }

    private void InitUI()
    {
        width = 5;
        height = 5;
        boxStyle = new GUIStyle();
        boxStyle.margin = new RectOffset(5, 5, 5, 5);
    }

    private void InitArray()
    {
        objectArray = new UnityEngine.Object[width * height];
    }

    [MenuItem("Window/PrefabManager")]

    public static void ShowWindow()
    {
        var window = GetWindow(typeof(MyWindow));
        window.titleContent.text = "AwesomeWindowLongName";
    }

    void OnGUI()
    {
        var evt = Event.current;

        int boxCount = 0;

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUIStyle.none);
        GUILayout.BeginHorizontal(GUILayout.Width(width * boxWidth));
        for (int i = 0; i < width; i++)
        {
            GUILayout.BeginVertical();
            for (int j = 0; j < height; j++)
            {
                var boxRect = GUILayoutUtility.GetRect(GUIContent.none, boxStyle, GUILayout.Width(boxWidth), GUILayout.Height(boxHeight));

                if (objectArray[boxCount] != null)
                {
                    Texture2D texture = AssetPreview.GetAssetPreview(objectArray[boxCount]);
                    GUI.DrawTexture(boxRect, texture);
                }
                else
                {
                    GUI.DrawTexture(boxRect, EditorGUIUtility.whiteTexture);
                }

                OnDrag(evt, boxRect, boxCount);
                OnDrop(evt, boxRect, boxCount);
                OnRightClick(evt, boxRect, boxCount);

                if (evt.type == EventType.DragExited)
                {
                    Debug.Log("Drag Exit!");
                    dragStarted = false;
                    dragStartedAt = -1;
                }
                boxCount++;
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();
        if (GUILayout.Button("Clear All"))
        {
            ClearPrefs();
            InitArray();
            dragStarted = false;
        }
    }

    private void OnDrag(Event evt, Rect boxRect, int number)
    {
        if (evt.type == EventType.MouseDrag && boxRect.Contains(evt.mousePosition) && dragStarted == false && objectArray[number] != null)
        {
            UnityEngine.Object[] dragArray = new UnityEngine.Object[1];
            dragArray[0] = objectArray[number];
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.StartDrag("drag from window");
            DragAndDrop.objectReferences = dragArray;
            DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            dragStarted = true;
            dragStartedAt = number;
        }
    }

    private void OnDrop(Event evt, Rect boxRect, int number)
    {
        bool isAccepted = false;
        if (evt.type == EventType.DragUpdated && boxRect.Contains(evt.mousePosition) || evt.type == EventType.DragPerform && boxRect.Contains(evt.mousePosition))
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                isAccepted = true;
            }
            Event.current.Use();

            if (isAccepted)
            {
                if (DragAndDrop.objectReferences.Length == 1)
                {
                    if (dragStarted == true)
                    {
                        objectArray[dragStartedAt] = null;
                    }
                    dragStartedAt = -1;
                    dragStarted = false;
                    objectArray[number] = DragAndDrop.objectReferences[0];
                }
            }
        }
    }

    private void OnRightClick(Event evt, Rect boxRect, int boxCount)
    {        
        if (evt.type == EventType.MouseDown && evt.button == 1 && boxRect.Contains(evt.mousePosition))
        {
            clickedBoxId = boxCount;
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("DeleteItem"), false, DeleteObject);
            menu.AddItem(new GUIContent("MenuItem2"), false, Callback);
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("SubMenu/MenuItem3"), false, Callback);

            menu.ShowAsContext();

            evt.Use();
        }
    }

    private void DeleteObject()
    {
        if (clickedBoxId >= 0)
        {
            objectArray[clickedBoxId] = null;
            clickedBoxId = -1;
        }        
    }

    private void Callback()
    {
        Debug.Log("COnteext!");
    }

    private void SafePrefs()
    {
        UnityEngine.Object obj;
        for (int i = 0; i < width * height; i++)
        {
            if ((obj = objectArray[i]) != null)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                EditorPrefs.SetString("" + i, path);
                Debug.Log("Safing: " + path);
            }
        }
    }

    private void LoadPrefs()
    {
        for (int i = 0; i < width * height; i++)
        {
            String objPath;
            if ((objPath = EditorPrefs.GetString("" + i)) != "")
            {
                objectArray[i] = AssetDatabase.LoadAssetAtPath(objPath, typeof(UnityEngine.Object));
            }
        }
    }

    private void ClearPrefs()
    {
        for (int i = 0; i < width * height; i++)
        {
            EditorPrefs.DeleteKey("" + i);
        }
    }
}
