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
    private static readonly float windowHeight = 597;
    private static readonly float windowWidth = 387;
    private readonly int width = 4;
    private readonly int height = 7;
    private readonly int boxWidth = 80;
    private readonly int boxHeight = 80;
	private readonly int maxTabNumber = 10;
    private readonly float stackSplitwindowHeight = 110;
    private readonly float stackSplitwindowWidth = 150;

    private int rightClickedBoxId;
    private int leftClickedBoxId;
    private int selectedTabId = 0;
    private static EditorWindow window;

    private GUIStyle boxStyle;
    private GUIStyle boxLabelStyle;
    private GUIStyle stackLabelStyle;
	private GUIStyle tabLabelStyle;
    private GUIStyle pageTitleInputStyle;
	private Texture2D emptyBoxTexture;

    private bool dragStartedInWindow;
    private bool dropPerformed;
    private int dragStartedAt;

    private InventoryController inventoryController;
    private InventoryPage activePage;

    private bool stackSplitKeyPressed = false;
    private bool drawStackSplitWindow = false;
    private bool stackSplitWindowOpen = false;
    public static bool drawAddStackWindow = false;
    private bool addStackWindowOpen = false;
    private float popUpX = 0;
    private float popUpY = 0;
    int sliderValue = 0;
    int dragStackField = 0;
    int dropStackField = 0;
    private int addStackField = 1;
    int dropPosition = -1;
    int dragPosition = -1;

    public static CSVLogger csvLog;

    public void OnEnable()
    {
        csvLog = OpenWindowListener.GetLogger();
        SceneView.onSceneGUIDelegate += OnSceneGUI;
        inventoryController = new InventoryController(width, height);
        activePage = inventoryController.GetActivePage();
        rightClickedBoxId = -1;
        leftClickedBoxId = -1;
        dragStartedInWindow = false;
        dragStartedAt = -1;

        csvLog.WriteRow(new List<string>(new string[] { "Enable Inventory Window" }));
        
    }

    public void OnDisable()
    {
        csvLog.WriteRow(new List<string>(new string[] { "Disable Inventory Window" }));
        inventoryController.SafePrefs();
    }

    public void OnPageChanged()
    {
        activePage = inventoryController.GetActivePage();
        selectedTabId = inventoryController.activePageId;
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

		tabLabelStyle = new GUIStyle ();
		tabLabelStyle.alignment = TextAnchor.MiddleCenter;
		tabLabelStyle.normal.textColor = Color.white;
		tabLabelStyle.wordWrap = true;

		emptyBoxTexture = new Texture2D(1, 1);
		emptyBoxTexture.SetPixel(1, 1, emptyBoxColor);
		emptyBoxTexture.wrapMode = TextureWrapMode.Repeat;
		emptyBoxTexture.Apply();

		pageTitleInputStyle = new GUIStyle(GUI.skin.textField);
		pageTitleInputStyle.fixedWidth = (int)windowWidth/2;
    }

    [MenuItem("Window/PrefabManager")]

    public static void ShowWindow()
    {
        window = GetWindow(typeof(InventoryWindow));
        window.minSize = new Vector2(windowWidth, windowHeight);
        window.maxSize = new Vector2(windowWidth, windowHeight);
        window.titleContent.text = "Inventory";
    }

    void OnGUI()
    {
		InitStyles ();
        var evt = Event.current;      
        int boxCount = 0;
		int tabId = 0;


		GUILayout.BeginHorizontal ();
		string textFieldString = GUILayout.TextField(activePage.GetPageName(), 15, pageTitleInputStyle);
		if(!textFieldString.Equals(activePage.GetPageName()))
		{
            csvLog.WriteRow(new List<string>(new string[] { "Rename page", activePage.GetPageName(), textFieldString }));
            activePage.SetPageName(textFieldString);
		}
		if (GUILayout.Button("Delete Page"))
		{
            GUIUtility.keyboardControl = 0;
            csvLog.WriteRow(new List<string>(new string[] { "Delete Page", activePage.GetPageName() }));
            inventoryController.DeletePage(inventoryController.activePageId);
			OnPageChanged();
		}
		GUILayout.EndHorizontal ();



        GUILayout.BeginHorizontal(GUILayout.Width(width * boxWidth));

		GUILayout.BeginVertical ();
		int tabHeight =(int) ((windowHeight-34) / maxTabNumber);
		
		for (int i = 0; i < inventoryController.GetPageCount(); i++) 
		{
			var boxRect = GUILayoutUtility.GetRect(GUIContent.none, boxStyle, GUILayout.Width(tabHeight), GUILayout.Height(tabHeight));
            Texture2D tab;
            if (selectedTabId == tabId)
            {
                tab = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Editor/EditorAssets/tab_icon_selected.png", typeof(Texture2D));
            }else
            {
                tab = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Editor/EditorAssets/tab_icon_unselected.png", typeof(Texture2D));
            }
            
            GUI.DrawTexture(boxRect, tab);
			GUI.Label(boxRect, inventoryController.GetPageName(tabId), tabLabelStyle);

			OnTabClick(evt, boxRect, tabId);
            OnTabDrop(evt, boxRect, tabId);
			tabId++;
		}

		if (inventoryController.GetPageCount () < maxTabNumber) 
		{
			var boxRect = GUILayoutUtility.GetRect(GUIContent.none, boxStyle, GUILayout.Width(tabHeight), GUILayout.Height(tabHeight));
            Texture2D addTab = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Editor/EditorAssets/tab_icon_add.png", typeof(Texture2D));
            GUI.DrawTexture(boxRect, addTab);			
			OnAddTabClick(evt, boxRect);
		}
		
		
		GUILayout.EndVertical ();

        for (int i = 0; i < width; i++)
        {
            GUILayout.BeginVertical();
            for (int j = 0; j < height; j++)
            {
                var boxRect = GUILayoutUtility.GetRect(GUIContent.none, boxStyle, GUILayout.Width(boxWidth), GUILayout.Height(boxHeight));
                InventoryObject invObject = activePage.GetInventoryObjectAt(boxCount);

                if (invObject != null)
                {
                    if(invObject.obj != null)
                    {
                        Texture2D texture = null;

                        texture = AssetPreview.GetAssetPreview(invObject.obj);
                        if (invObject.obj is UnityEditor.MonoScript)
                        {
                            string iconPath = "Assets/Editor/EditorAssets/script_icon.png";
                            texture = (Texture2D)AssetDatabase.LoadAssetAtPath(iconPath, typeof(Texture2D));
                        }

                        GUI.DrawTexture(boxRect, texture);
                        GUI.Label(boxRect, invObject.obj.name, boxLabelStyle);
                        if (InventoryController.stackingEnabled)
                        {
                            var labelRect = boxRect;
                            labelRect.width = boxRect.width - 4;
                            GUI.Label(labelRect, invObject.stackSize.ToString(), stackLabelStyle);
                        }
                    } else
                    {
                        Debug.Log("Prefab Missing");
                    }                    
                }
                else
                {
                    GUI.DrawTexture(boxRect, emptyBoxTexture);
                }

                if (leftClickedBoxId == boxCount)
                {
                    string borderPath = "Assets/Editor/EditorAssets/border.png";
                    Texture2D border = (Texture2D)AssetDatabase.LoadAssetAtPath(borderPath, typeof(Texture2D));
                    if (activePage.CheckPositionFilled(boxCount))
                    {
                        UnityEditor.Selection.activeObject = activePage.GetInventoryObjectAt(boxCount).obj;
                    } else
                    {
                        UnityEditor.Selection.activeObject = null;
                    }
                    
                    GUI.DrawTexture(boxRect, border);
                }

                if (drawStackSplitWindow)
                {
                    DrawStackSplitWindow(evt);
                }
                if (drawAddStackWindow)
                {
                    DrawAddStackWindow(evt);
                }
                OnSplitStackPressed(evt, boxRect, boxCount);
                OnDrag(evt, boxRect, boxCount);
                OnDrop(evt, boxRect, boxCount);
                OnRightClick(evt, boxRect, boxCount);
                OnLeftClick(evt, boxRect, boxCount);

                boxCount++;
            }
            GUILayout.EndVertical();
        }
		GUILayout.EndHorizontal ();
        CheckKeyInput(evt);
        if (evt.type == EventType.DragExited)
        {
            OnDropPerformed();
            if(dragStartedInWindow == true && dropPerformed == false && EditorWindow.mouseOverWindow.Equals(window))
            {
                dragStartedInWindow = false;
                dragStartedAt = -1;
            }
        }
    }

    public void OnSceneGUI(SceneView sceneView)
    {
        var evt = Event.current;
        OnDropSceneGUI(evt);
    }

    private void InitControls()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous Page"))
        {
            inventoryController.SetActivePage(-1);
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
            dragStartedInWindow = false;
            OnPageChanged();
        }
    }

    private void DrawAddStackWindow(Event evt)
    {
        drawStackSplitWindow = false;
        if (addStackWindowOpen == false)
        {
            addStackWindowOpen = true;
            popUpX = evt.mousePosition.x;
            popUpY = evt.mousePosition.y;
            if (popUpX + stackSplitwindowWidth > windowWidth)
            {
                popUpX -= stackSplitwindowWidth;
            }
            if (popUpY + stackSplitwindowHeight > windowHeight)
            {
                popUpY -= stackSplitwindowHeight;
            }
        }        

        Rect addStackBox = new Rect(popUpX, popUpY, stackSplitwindowWidth, stackSplitwindowHeight);
        GUI.Box(addStackBox, "Add Stack");
        addStackField = EditorGUI.IntField(new Rect(popUpX + stackSplitwindowWidth/2-10, popUpY + 30, 20, 20), addStackField);

        if (GUI.Button(new Rect(popUpX + 10, popUpY + 80, 60, 20), "OK"))
        {
            Debug.Log("ok");
            activePage.AddStack(addStackField);
            drawAddStackWindow = false;

        }
        if (GUI.Button(new Rect(popUpX + 80, popUpY + 80, 60, 20), "Cancel"))
        {
            addStackField = 1;
            drawAddStackWindow = false;
        }
    }

    private void DrawStackSplitWindow(Event evt)
    {
        drawAddStackWindow = false;
        int maxValue = activePage.GetInventoryObjectAt(dragPosition).stackSize;
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
            activePage.SplitStacks(dragPosition, dropPosition, dropStackSize);
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
        GUIUtility.keyboardControl = 0;
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
        if (evt.type == EventType.MouseDown && boxRect.Contains(evt.mousePosition) && dragStartedInWindow == false && activePage.CheckPositionFilled(number))
        {
            GUIUtility.keyboardControl = 0;
            UnityEngine.Object[] dragArray = new UnityEngine.Object[1];
            dragArray[0] = activePage.GetInventoryObjectAt(number).obj;
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = dragArray;
            DragAndDrop.StartDrag(dragArray[0].ToString());
            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            if (activePage.CheckPositionFilled(number))
            {
                csvLog.WriteRow(new List<string>(new string[] { "Drag Object from Window", dragArray[0].ToString(), activePage.GetInventoryObjectAt(number).stackSize.ToString(), "-1", activePage.GetPageName() }));
            }
            dragStartedInWindow = true;
            dragStartedAt = number;
        }
    }

    private void OnDrop(Event evt, Rect boxRect, int number)
    {
        bool isAccepted = false;
        if (evt.type == EventType.DragUpdated && boxRect.Contains(evt.mousePosition) || evt.type == EventType.DragPerform && boxRect.Contains(evt.mousePosition))
        {
            GUIUtility.keyboardControl = 0;
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                isAccepted = true;
                Event.current.Use();
            }
           

            if (isAccepted)
            {
                int dropType = CheckDropType(number, dragStartedAt, DragAndDrop.objectReferences[0]);
                if(dragStartedInWindow == false)
                {
                    csvLog.WriteRow(new List<string>(new string[] { "Drop Object in Window", DragAndDrop.objectReferences[0].ToString(), "no stacksize",dropType.ToString(), activePage.GetPageName() }));
                }
                else
                {
                    if (activePage.CheckPositionFilled(dragStartedAt))
                    {
                        csvLog.WriteRow(new List<string>(new string[] { "Drop Object in Window", DragAndDrop.objectReferences[0].ToString(), activePage.GetInventoryObjectAt(dragStartedAt).stackSize.ToString(), dropType.ToString(), activePage.GetPageName() }));
                    }
                }
                
                activePage.HandleDropEvent(dropType, dragStartedAt, number, DragAndDrop.objectReferences[0]);
                
                leftClickedBoxId = number;
                dropPerformed = true;
            }
        }
    }

    private void OnTabDrop(Event evt, Rect boxRect, int tabId)
    {
        if (evt.type == EventType.DragUpdated && boxRect.Contains(evt.mousePosition) || evt.type == EventType.DragPerform && boxRect.Contains(evt.mousePosition))
        {
            bool isAccepted = false;
            GUIUtility.keyboardControl = 0;
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                isAccepted = true;
            }
            Event.current.Use();

            if (isAccepted)
            {
                if (dragStartedInWindow)
                {
                    bool succes = inventoryController.GetPageAt(tabId).AddObjectOnFreeSpace(activePage.GetInventoryObjectAt(dragStartedAt));
                    if(succes)
                    {
                        activePage.DeleteObject(dragStartedAt);
                    }                    
                } else
                {
                    inventoryController.GetPageAt(tabId).AddObjectOnFreeSpace(new InventoryObject(DragAndDrop.objectReferences[0], 1));
                }

                dropPerformed = true;
            }
        }
    }

    private void OnDropSceneGUI(Event evt)
    {
        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            GUIUtility.keyboardControl = 0;
            Debug.Log("in on scenedrop");
            
            if (evt.type == EventType.DragPerform)
            {
                if (dragStartedInWindow == true)
                {
                    if (activePage.CheckPositionFilled(dragStartedAt))
                    {
                        csvLog.WriteRow(new List<string>(new string[] { "Drop Object from Window in Scene", activePage.GetInventoryObjectAt(dragStartedAt).obj.ToString(), activePage.GetInventoryObjectAt(dragStartedAt).stackSize.ToString(), "-1", activePage.GetPageName() }));
                    }
                    activePage.DecrementStack(dragStartedAt);
                    this.Repaint();
                }
                dropPerformed = true;
                OnDropPerformed();
            }
        }
    }


    private void OnRightClick(Event evt, Rect boxRect, int boxCount)
    {
        if (evt.type == EventType.MouseDown && evt.button == 1 && boxRect.Contains(evt.mousePosition))
        {
            GUIUtility.keyboardControl = 0;
            rightClickedBoxId = boxCount;
            GenericMenu menu = new GenericMenu();
            if(activePage.CheckPositionFilled(rightClickedBoxId))
            {
                csvLog.WriteRow(new List<string>(new string[] { "Right Click on Object", activePage.GetInventoryObjectAt(rightClickedBoxId).obj.ToString(), activePage.GetInventoryObjectAt(rightClickedBoxId).stackSize.ToString(), activePage.GetPageName() }));
            }else
            {
                csvLog.WriteRow(new List<string>(new string[] { "Right Click on EmptyField" }));

            }
            menu.AddItem(new GUIContent("Delete Item"), false, DeleteObject);

            menu.ShowAsContext();

            evt.Use();
        }
    }

    private void OnDropPerformed()
    {
        if(dropPerformed == true)
        {
            Debug.Log("exited");
            dragStartedAt = -1;
            dragStartedInWindow = false;
            dropPerformed = false;
        }        
    }

    private void OnLeftClick(Event evt, Rect boxRect, int fieldId)
    {
        if (evt.type == EventType.MouseDown && evt.button == 0 && boxRect.Contains(evt.mousePosition))
        {
            GUIUtility.keyboardControl = 0;
            if (activePage.CheckPositionFilled(fieldId))
            {
                csvLog.WriteRow(new List<string>(new string[] { "Left Click on Object", activePage.GetInventoryObjectAt(fieldId).obj.ToString(), activePage.GetInventoryObjectAt(fieldId).stackSize.ToString(), "-1",activePage.GetPageName() }));
            }else
            {
                csvLog.WriteRow(new List<string>(new string[] { "Left Click on EmptyField" }));
                leftClickedBoxId = fieldId;
            }
            evt.Use();
        }

    }

    private void OnSplitStackPressed(Event evt, Rect boxRect, int boxCount)
    {

        if (evt.type == EventType.KeyDown && evt.keyCode == InventoryController.splitStackKey && stackSplitKeyPressed == false)
        {
            GUIUtility.keyboardControl = 0;
            stackSplitKeyPressed = true;
            evt.Use();
        }
        if (evt.type == EventType.KeyUp && evt.keyCode == InventoryController.splitStackKey && stackSplitKeyPressed == true)
        {
            GUIUtility.keyboardControl = 0;
            stackSplitKeyPressed = false;
            evt.Use();
        }
    }

	private void OnTabClick(Event evt, Rect boxRect, int tabId) 
	{
		if (evt.type == EventType.MouseDown && evt.button == 0 && boxRect.Contains (evt.mousePosition)) 
		{
            GUIUtility.keyboardControl = 0;
            inventoryController.SetActivePage(tabId);
            csvLog.WriteRow(new List<string>(new string[] { "Switched to Bag", inventoryController.GetActivePage().GetPageName() }));
            OnPageChanged();
		}
	}

	private void OnAddTabClick(Event evt, Rect boxRect)
	{
		if (evt.type == EventType.MouseDown && evt.button == 0 && boxRect.Contains (evt.mousePosition)) 
		{
            GUIUtility.keyboardControl = 0;
            inventoryController.AddPage();
            csvLog.WriteRow(new List<string>(new string[] { "Added new Bag", inventoryController.GetActivePage().GetPageName() }));
            OnPageChanged();
		}
	}

    private void DeleteObject()
    {
        if(activePage.CheckPositionFilled(rightClickedBoxId))
        {
            csvLog.WriteRow(new List<string>(new string[] { "Deleting Object", activePage.GetInventoryObjectAt(rightClickedBoxId).obj.ToString(), activePage.GetInventoryObjectAt(rightClickedBoxId).stackSize.ToString(), "-1", activePage.GetPageName() }));
        }
        activePage.DeleteObject(rightClickedBoxId);
        rightClickedBoxId = -1;
        UnityEditor.Selection.activeObject = null;
    }

    private int CheckDropType(int dropFieldId, int dragFieldId, UnityEngine.Object draggedObject)
    {
        if(dragStartedInWindow == true)
        {
            if(activePage.CheckPositionFilled(dropFieldId) == false)
            {
                if(stackSplitKeyPressed)
                {
                    dragPosition = dragFieldId;
                    dropPosition = dropFieldId;
                    drawStackSplitWindow = true;
                    return DROP_WITH_STACKSPLIT;
                }
                else
                {
                    return DROP_FROM_WINDOW_EMPTY;
                }                
            } else
            {
                if(UnityEngine.Object.ReferenceEquals(activePage.GetInventoryObjectAt(dropFieldId).obj, draggedObject))
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
            if (activePage.CheckPositionFilled(dropFieldId))
            {
                if (UnityEngine.Object.ReferenceEquals(activePage.GetInventoryObjectAt(dropFieldId).obj, draggedObject))
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

    public static void setCSVLogger(CSVLogger csv)
    {
        csvLog = csv;
    }
}
