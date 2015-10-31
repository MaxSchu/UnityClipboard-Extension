using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

public class InventoryWindow : EditorWindow

{
    private const int DROP_FROM_WINDOW_EMPTY = 0;
    private const int DROP_FROM_WINDOW_ADD = 1;
    private const int DROP_FROM_WINDOW_SWITCH = 2;
    private const int DROP_FROM_EDITOR_OVERRIDE = 3;
    private const int DROP_FROM_EDITOR_ADD = 4;
    private const int DROP_WITH_STACKSPLIT = 5;
    private const int DROP_FROM_WINDOW_SELF = 6;

    private readonly UnityEngine.Color emptyBoxColor = Color.grey;
    private static readonly float windowHeight = 640;
    private static readonly float windowWidth = 328;
    private readonly int width = 4;
    private readonly int height = 7;
    private readonly int boxWidth = 80;
    private readonly int boxHeight = 80;
    private readonly float stackSplitwindowHeight = 110;
    private readonly float stackSplitwindowWidth = 150;

    private int rightClickedBoxId;
    private int leftClickedBoxId;

    private GUIStyle boxStyle;
    private GUIStyle boxLabelStyle;
    private GUIStyle stackLabelStyle;
    private Vector2 scrollPosition;

    private bool dragStarted;    
    private int dragStartedAt;

    private InventoryController inventoryController;
    private InventoryObject[] objectArray;
    private string pageName;

    private bool stackSplitKeyPressed = false;
    private bool drawStackSplitWindow = false;
    private bool stackSplitWindowOpen = false;
    private float popUpX = 0;
    private float popUpY = 0;
    int sliderValue = 0;
    int dragStackField = 0;
    int dropStackField = 0;
    int dropPosition = -1;
    int dragPosition = -1;

    public void OnEnable()
    {
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

        stackLabelStyle = new GUIStyle();
        stackLabelStyle.alignment = TextAnchor.UpperRight;
        stackLabelStyle.normal.textColor = Color.white;
    }

    [MenuItem("Window/PrefabManager")]

    public static void ShowWindow()
    {
        var window = GetWindow(typeof(InventoryWindow));
        window.minSize = new Vector2(windowWidth, windowHeight);
        window.maxSize = new Vector2(windowWidth, windowHeight);

        window.titleContent.text = "Inventory";
    }

    void OnGUI()
    {
        var evt = Event.current;        

        int boxCount = 0;

        GUILayout.Label(pageName);

        GUILayout.BeginHorizontal(GUILayout.Width(width * boxWidth));
        for (int i = 0; i < width; i++)
        {
            GUILayout.BeginVertical();
            for (int j = 0; j < height; j++)
            {
                var boxRect = GUILayoutUtility.GetRect(GUIContent.none, boxStyle, GUILayout.Width(boxWidth), GUILayout.Height(boxHeight));

                if (objectArray[boxCount] != null)
                {
                    if(objectArray[boxCount].obj != null)
                    {
                        Texture2D texture = AssetPreview.GetAssetPreview(objectArray[boxCount].obj);
                        GUI.DrawTexture(boxRect, texture);
                        GUI.Label(boxRect, objectArray[boxCount].obj.name, boxLabelStyle);
                        if (InventoryController.stackingEnabled)
                        {
                            var labelRect = boxRect;
                            labelRect.width = boxRect.width - 4;
                            GUI.Label(labelRect, objectArray[boxCount].stackSize.ToString(), stackLabelStyle);
                        }
                    } else
                    {
                        Debug.Log("Prefab Missing");
                    }                    
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

                if (drawStackSplitWindow)
                {
                    DrawStackSplitWindow(evt);
                }
                OnSplitStackPressed(evt, boxRect, boxCount);
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
        initControls();
        
        CheckKeyInput(evt);
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

    private void DrawStackSplitWindow(Event evt)
    {
        int maxValue = objectArray[dragPosition].stackSize;
        if (stackSplitWindowOpen == false)
        {
            sliderValue = (int) Math.Round((double)(maxValue / 2));
            dropStackField = sliderValue;
            dragStackField = maxValue - sliderValue;
            popUpX = evt.mousePosition.x;
            popUpY = evt.mousePosition.y;
            if(popUpX + stackSplitwindowWidth > windowWidth)
            {
                popUpX -= stackSplitwindowWidth;
            }
            if(popUpY + stackSplitwindowHeight > windowHeight)
            {
                popUpY -= stackSplitwindowHeight;
            }
            stackSplitWindowOpen = true;
        }        
        Rect stackSplitBox = new Rect(popUpX, popUpY, stackSplitwindowWidth, stackSplitwindowHeight);
        GUI.Box(stackSplitBox, "Split Stack");

        int dropStackSize = DrawTextFieldSliderWidget(popUpX, popUpY, evt, maxValue);      

        if (GUI.Button(new Rect(popUpX + 10, popUpY + 80, 60, 20), "OK"))
        {
            Debug.Log("dropPos: " + dropPosition);
            if(dropStackSize != 0)
            {
                objectArray[dropPosition] = new InventoryObject(objectArray[dragPosition].obj, dropStackSize);
                objectArray[dragPosition].stackSize -= dropStackSize;
            }
            if(dropStackSize == maxValue)
            {
                objectArray[dropPosition] = new InventoryObject(objectArray[dragPosition].obj, dropStackSize);
                objectArray[dragPosition] = null;
            }
            ResetStackSplitVariables();
        }
        if (GUI.Button(new Rect(popUpX + 80, popUpY + 80, 60, 20), "Cancel"))
        {
            ResetStackSplitVariables();
        }
    }

    private void ResetStackSplitVariables()
    {
        stackSplitWindowOpen = false;
        stackSplitKeyPressed = false;
        drawStackSplitWindow = false;
        sliderValue = 0;
        dropStackField = 0;
        dragStackField = 0;
    }

    private int DrawTextFieldSliderWidget(float x, float y, Event evt, int maxValue)
    {

        int sliderValueCheck = (int) GUI.HorizontalSlider(new Rect(popUpX + 40, popUpY + 50, 70, 30), sliderValue, 0, maxValue);
        int dragStackFieldCheck = EditorGUI.IntField(new Rect(popUpX + 10, popUpY + 50, 20, 20), dragStackField);
        int dropStackFieldCheck = EditorGUI.IntField(new Rect(popUpX + 120, popUpY + 50, 20, 20), dropStackField);
        if (sliderValueCheck != sliderValue)
        {
            sliderValue = sliderValueCheck;
            dragStackField = maxValue - sliderValue;
            dragStackFieldCheck = dragStackField;
            dropStackField = sliderValue;
            dropStackFieldCheck = dropStackField;
        }        
        if(!dragStackFieldCheck.Equals(dragStackField))
        {
            if(dragStackFieldCheck > maxValue)
            {
                dragStackFieldCheck = maxValue;
            }
            if(dragStackFieldCheck < 0)
            {
                dragStackFieldCheck = 0;
            }
            dragStackField = dragStackFieldCheck;
            dropStackField = maxValue - dragStackField;
            dropStackFieldCheck = dropStackField;
            sliderValue = dropStackField;
            sliderValueCheck = sliderValue;           
            
        }
        if(!dropStackFieldCheck.Equals(dropStackField))
        {
            if (dropStackFieldCheck > maxValue)
            {
                dropStackFieldCheck = maxValue;
            }
            if (dropStackFieldCheck < 0)
            {
                dropStackFieldCheck = 0;
            }
            dropStackField = dropStackFieldCheck;
            dragStackField = maxValue - dragStackField;
            dragStackFieldCheck = dragStackField;
            sliderValue = dropStackField;
            sliderValueCheck = sliderValue;
        }
        return dropStackField;
    }

    private void OnDrag(Event evt, Rect boxRect, int number)
    {
        if (evt.type == EventType.MouseDown && boxRect.Contains(evt.mousePosition) && dragStarted == false && objectArray[number] != null)
        {
            UnityEngine.Object[] dragArray = new UnityEngine.Object[1];
            dragArray[0] = objectArray[number].obj;
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
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                isAccepted = true;
            }
            Event.current.Use();

            if (isAccepted)
            {
                int dropType = CheckDropType(number, dragStartedAt, DragAndDrop.objectReferences[0]);
                int dragStackSize = InventoryController.standardStackSize;

                switch (dropType)
                {
                    case DROP_FROM_WINDOW_EMPTY:
                        {
                            dragStackSize = objectArray[dragStartedAt].stackSize;
                            objectArray[dragStartedAt] = null;
                            objectArray[number] = new InventoryObject(DragAndDrop.objectReferences[0], dragStackSize);
                            break;
                        }
                    case DROP_FROM_WINDOW_ADD:
                        {
                            dragStackSize = objectArray[dragStartedAt].stackSize;
                            if (InventoryController.stackingEnabled)
                            {
                                objectArray[number].stackSize += dragStackSize;
                                if (dragStarted == true)
                                {
                                    objectArray[dragStartedAt] = null;
                                }
                            }
                            break;
                        }
                    case DROP_FROM_WINDOW_SWITCH:
                        {
                            InventoryObject stage = objectArray[number];
                            objectArray[number] = new InventoryObject(DragAndDrop.objectReferences[0], objectArray[dragStartedAt].stackSize);
                            objectArray[dragStartedAt] = stage;
                            break;
                        }
                    case DROP_WITH_STACKSPLIT:
                        {
                            dragPosition = dragStartedAt;
                            dropPosition = number;
                            dragStackSize = objectArray[dragStartedAt].stackSize;
                            drawStackSplitWindow = true;
                            break;
                        }
                    case DROP_FROM_EDITOR_OVERRIDE:
                        {
                            objectArray[number] = new InventoryObject(DragAndDrop.objectReferences[0], dragStackSize);
                            break;
                        }
                    case DROP_FROM_EDITOR_ADD:
                        {
                            if (InventoryController.stackingEnabled)
                            {
                                objectArray[number].stackSize++;
                            }                            
                            break;
                        }
                    case DROP_FROM_WINDOW_SELF:
                        {
                            break;
                        }
                    default:
                        {
                            Debug.Log("Unknown drop type");
                            break;
                        }
                }
                dragStartedAt = -1;
                dragStarted = false;
                leftClickedBoxId = number;
            }
        }
    }

    private void OnRightClick(Event evt, Rect boxRect, int boxCount)
    {
        if (evt.type == EventType.MouseDown && evt.button == 1 && boxRect.Contains(evt.mousePosition))
        {
            rightClickedBoxId = boxCount;
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("DeleteItem"), false, DeleteObject);

            menu.ShowAsContext();

            evt.Use();
        }
    }

    private void OnLeftClick(Event evt, Rect boxRect, int boxCount)
    {
        if (evt.type == EventType.MouseDown && evt.button == 0 && boxRect.Contains(evt.mousePosition))
        {
            if (objectArray[boxCount] != null)
            {
                UnityEditor.Selection.activeObject = objectArray[boxCount].obj;
            }
            leftClickedBoxId = boxCount;
            evt.Use();
        }

    }

    private void OnSplitStackPressed(Event evt, Rect boxRect, int boxCount)
    {

        if (evt.type == EventType.KeyDown && evt.keyCode == InventoryController.splitStackKey && stackSplitKeyPressed == false)
        {
            stackSplitKeyPressed = true;
            evt.Use();
        }
        if (evt.type == EventType.KeyUp && evt.keyCode == InventoryController.splitStackKey && stackSplitKeyPressed == true)
        {
            stackSplitKeyPressed = false;
            evt.Use();
        }
        

    }

    private void DeleteObject()
    {
        inventoryController.DeleteObject(rightClickedBoxId);
        rightClickedBoxId = -1;
    }

    private int CheckDropType(int dropFieldId, int dragFieldId, UnityEngine.Object draggedObject)
    {
        if(dragStarted == true)
        {
            if(objectArray[dropFieldId] == null)
            {
                if(stackSplitKeyPressed)
                {
                    return DROP_WITH_STACKSPLIT;
                }else
                {
                    return DROP_FROM_WINDOW_EMPTY;
                }                
            } else
            {
                if(UnityEngine.Object.ReferenceEquals(objectArray[dropFieldId].obj, draggedObject))
                {
                    if(dropFieldId == dragFieldId)
                    {
                        return DROP_FROM_WINDOW_SELF;
                    }
                    return DROP_FROM_WINDOW_ADD;
                }else
                {
                    return DROP_FROM_WINDOW_SWITCH;
                }
            }
        } else
        {
            if (objectArray[dropFieldId] != null)
            {
                if (UnityEngine.Object.ReferenceEquals(objectArray[dropFieldId].obj, draggedObject))
                {
                    return DROP_FROM_EDITOR_ADD;
                }
            }
            return DROP_FROM_EDITOR_OVERRIDE;
        }
    }

    private void CheckKeyInput(Event evt)
    {
        switch (evt.type)
        {
            case EventType.keyDown:
                {
                    if (Event.current.keyCode == (KeyCode.I))
                    {
                        OpenWindowListener.windowOpen = false;
                        this.Close();
                        evt.Use();
                    }
                    break;
                }
        }
    }


}
