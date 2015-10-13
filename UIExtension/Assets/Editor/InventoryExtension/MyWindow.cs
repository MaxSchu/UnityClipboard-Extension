using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

public class MyWindow : EditorWindow

{
    private UnityEngine.Color emptyBoxColor = Color.grey;
    private int width = 4;
    private int height = 7;
    private int boxWidth = 80;
    private int boxHeight = 80;

    private int rightClickedBoxId;
    private int leftClickedBoxId;

    private GUIStyle boxStyle;
    private GUIStyle boxLabelStyle;
    private Vector2 scrollPosition;

    private bool dragStarted;
    private int dragStartedAt;

    private InventoryController inventoryController;
    private UnityEngine.Object[] objectArray;
    private string pageName;

    public void OnEnable()
    {
        Debug.Log("OnEnable");
        inventoryController = new InventoryController(width, height);
        objectArray = inventoryController.GetActivePage().GetObjectArray();
        pageName = inventoryController.GetActivePage().GetPageName();
        rightClickedBoxId = -1;
        leftClickedBoxId = -1;
        dragStarted = false;
        dragStartedAt = -1;

        InitStyles();
    }

    public void OnDisable()
    {
        inventoryController.SafePrefs();
    }

    public void OnPageChanged()
    {
        objectArray = inventoryController.GetActivePage().GetObjectArray();
        pageName = inventoryController.GetActivePage().GetPageName();
        this.Repaint();
    }

    private void InitStyles()
    {
        boxStyle = new GUIStyle();
        boxStyle.margin = new RectOffset(2, 2, 2, 2);

        boxLabelStyle = new GUIStyle();
        boxLabelStyle.alignment = TextAnchor.LowerCenter;
        boxLabelStyle.normal.textColor = Color.white;
        boxLabelStyle.wordWrap = true;
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

        GUILayout.Label(pageName);

        //scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUIStyle.none);
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
                    GUI.Label(boxRect, objectArray[boxCount].name, boxLabelStyle);
                }
                else
                {
                    Texture2D emptyBoxTexture = new Texture2D(1, 1);
                    emptyBoxTexture.SetPixel(1, 1, emptyBoxColor);
                    emptyBoxTexture.wrapMode = TextureWrapMode.Repeat;
                    emptyBoxTexture.Apply();
                    GUI.DrawTexture(boxRect, emptyBoxTexture);
                }

                if (leftClickedBoxId == boxCount)
                {
                    string borderPath = "Assets/Editor/EditorAssets/border.png";
                    Texture2D border = (Texture2D)AssetDatabase.LoadAssetAtPath(borderPath, typeof(Texture2D));
                    GUI.DrawTexture(boxRect, border);
                }

                OnDrag(evt, boxRect, boxCount);
                OnDrop(evt, boxRect, boxCount);
                OnRightClick(evt, boxRect, boxCount);
                OnLeftClick(evt, boxRect, boxCount);

                if (evt.type == EventType.DragExited)
                {
                    dragStarted = false;
                    dragStartedAt = -1;
                }
                boxCount++;
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();
        //GUILayout.EndScrollView();

        initControls();
    }

    private void initControls()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous Page"))
        {
            inventoryController.SetActivePage(-1);
            OnPageChanged();
        }
        if (GUILayout.Button("-"))
        {
            inventoryController.DeletePage();
            OnPageChanged();
        }
        string textFieldString = GUILayout.TextField(pageName, 25);
        if (textFieldString != pageName)
        {
            pageName = textFieldString;
            inventoryController.GetActivePage().SetPageName(pageName);
            OnPageChanged();
        }
        if (GUILayout.Button("+"))
        {
            inventoryController.AddPage();
            OnPageChanged();
        }
        if (GUILayout.Button("Next Page"))
        {
            inventoryController.SetActivePage(1);
            OnPageChanged();
        }
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Clear All"))
        {
            inventoryController.ClearPrefs();
            dragStarted = false;
            OnPageChanged();
        }
    }

    private void OnDrag(Event evt, Rect boxRect, int number)
    {
        if (evt.type == EventType.MouseDrag && boxRect.Contains(evt.mousePosition) && dragStarted == false && objectArray[number] != null)
        {
            Debug.Log("OnDrag");
            UnityEngine.Object[] dragArray = new UnityEngine.Object[1];
            dragArray[0] = objectArray[number];
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = dragArray;
            DragAndDrop.StartDrag(dragArray[0].ToString());
            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            dragStarted = true;
            dragStartedAt = number;
        }
    }

    private void OnDrop(Event evt, Rect boxRect, int number)
    {
        bool isAccepted = false;
        if (evt.type == EventType.DragUpdated && boxRect.Contains(evt.mousePosition) || evt.type == EventType.DragPerform && boxRect.Contains(evt.mousePosition))
        {
            Debug.Log("DragUpdate / DragPerform");
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                isAccepted = true;
            }
            Event.current.Use();

            if (isAccepted)
            {
                Debug.Log("OnDrop");
                if (DragAndDrop.objectReferences.Length == 1)
                {
                    if (dragStarted == true)
                    {
                        objectArray[dragStartedAt] = null;
                    }
                    dragStartedAt = -1;
                    dragStarted = false;
                    leftClickedBoxId = number;
                    objectArray[number] = DragAndDrop.objectReferences[0];
                }
            }
        }
    }

    private void OnRightClick(Event evt, Rect boxRect, int boxCount)
    {
        if (evt.type == EventType.MouseDown && evt.button == 1 && boxRect.Contains(evt.mousePosition))
        {
            Debug.Log("OnRightClick");
            rightClickedBoxId = boxCount;
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("DeleteItem"), false, DeleteObject);
            menu.AddItem(new GUIContent("MenuItem2"), false, Callback);
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("SubMenu/MenuItem3"), false, Callback);

            menu.ShowAsContext();

            evt.Use();
        }
    }

    private void OnLeftClick(Event evt, Rect boxRect, int boxCount)
    {
        if (evt.type == EventType.MouseDown && evt.button == 0 && boxRect.Contains(evt.mousePosition))
        {
            Debug.Log("OnLeftClick");
            UnityEngine.Object obj = objectArray[boxCount];
            UnityEditor.Selection.activeObject = obj;

            leftClickedBoxId = boxCount;
            evt.Use();
        }

    }

    private void DeleteObject()
    {
        inventoryController.DeleteObject(rightClickedBoxId);
        rightClickedBoxId = -1;
    }

    private void Callback()
    {
        Debug.Log("COnteext!");
    }


}
