using UnityEngine;
using System.Collections;

public class InventoryPage {
    private const int DROP_FROM_WINDOW_EMPTY = 0;
    private const int DROP_FROM_WINDOW_ADD = 1;
    private const int DROP_FROM_WINDOW_SWITCH = 2;
    private const int DROP_FROM_EDITOR_OVERRIDE = 3;
    private const int DROP_FROM_EDITOR_ADD = 4;
    private const int DROP_WITH_STACKSPLIT = 5;
    private const int DROP_FROM_WINDOW_SELF = 6;


    private InventoryObject[] objectArray;
	public string pageName;

    private UnityEngine.Object addStackStoreObj;
    private int addStackPosition;

	public InventoryPage(string pageName, InventoryObject[] objectArray) 
	{
		this.pageName = pageName;
		this.objectArray = objectArray;
	}

	public void AddInventoryObject(InventoryObject obj, int pos) 
	{
		objectArray [pos] = obj;
	}

    public bool AddObjectOnFreeSpace(InventoryObject obj)
    {
        for(int i = 0; i < objectArray.Length; i++)
        {
            if(objectArray[i] == null)
            {
                objectArray[i] = obj;
                return true;
            }
        }
        return false;
    }

    public bool CheckPositionFilled(int pos)
    {
        if(objectArray[pos] == null)
        {
            return false;
        }
        return true;
    }

	public void SetPageName(string pageName) 
	{
		this.pageName = pageName;
	}

	public void DeleteObject(int pos)
	{
		objectArray [pos] = null;       
	}

    public void DecrementStack(int pos)
    {
        objectArray[pos].stackSize--; 
        if(objectArray[pos].stackSize <= 0)
        {
            objectArray[pos] = null;
        }     
    }

	public InventoryObject GetInventoryObjectAt(int pos) 
	{
		return objectArray[pos];
	}

    public InventoryObject[] GetObjectArray()
    {
        return objectArray;
    }

	public string GetPageName()
	{
		return pageName;
	}

    public void SplitStacks(int dragPosition, int dropPosition, int dropStackSize)
    {
        if (dropStackSize == objectArray[dragPosition].stackSize)
        {
            objectArray[dropPosition] = new InventoryObject(objectArray[dragPosition].obj, dropStackSize);
            objectArray[dragPosition] = null;
        }else if (dropStackSize != 0)
        {
            objectArray[dropPosition] = new InventoryObject(objectArray[dragPosition].obj, dropStackSize);
            objectArray[dragPosition].stackSize -= dropStackSize;
        }
    }

    public void AddStack(int stackSize)
    {
        if (CheckPositionFilled(addStackPosition))
        {
            if (objectArray[addStackPosition].Equals(addStackStoreObj))
            {
                objectArray[addStackPosition].stackSize += stackSize;
                return;
            }
        }
        objectArray[addStackPosition] = new InventoryObject(addStackStoreObj, stackSize);

    }


    public void HandleDropEvent(int dropType, int dragPosition, int dropPosition, UnityEngine.Object dragObj)
    {
        int dragStackSize = InventoryController.standardStackSize;
        switch (dropType)
        {
            case DROP_FROM_WINDOW_EMPTY:
                {
                    if(objectArray[dragPosition] != null)
                    {
                        dragStackSize = objectArray[dragPosition].stackSize;
                        objectArray[dragPosition] = null;
                        objectArray[dropPosition] = new InventoryObject(dragObj, dragStackSize);
                    }                    
                    break;
                }
            case DROP_FROM_WINDOW_ADD:
                {
                    dragStackSize = objectArray[dragPosition].stackSize;
                    if (InventoryController.stackingEnabled)
                    {
                        objectArray[dropPosition].stackSize += dragStackSize;
                        objectArray[dragPosition] = null;
                    }
                    break;
                }
            case DROP_FROM_WINDOW_SWITCH:
                {
                    InventoryObject stage = objectArray[dropPosition];
                    objectArray[dropPosition] = new InventoryObject(dragObj, objectArray[dragPosition].stackSize);
                    objectArray[dragPosition] = stage;
                    break;
                }
            case DROP_FROM_EDITOR_OVERRIDE:
                {
                    addStackStoreObj = dragObj;
                    addStackPosition = dropPosition;
                    InventoryWindow.drawAddStackWindow = true;
                    break;
                }
            case DROP_FROM_EDITOR_ADD:
                {
                    addStackStoreObj = dragObj;
                    addStackPosition = dropPosition;
                    InventoryWindow.drawAddStackWindow = true;
                    break;
                }
            case DROP_FROM_WINDOW_SELF:
                {
                    break;
                }
            case DROP_WITH_STACKSPLIT:
                {
                    break;
                }

            default:
                {
                    Debug.Log("Unknown drop type");
                    break;
                }
        }
    }

    public void DropOnTab()
    {

    }

    public void ClearPage()
    {
        for(int i = 0; i < objectArray.Length; i++)
        {
            objectArray[i] = null;           
        }
		pageName = "page0";
    }



}
