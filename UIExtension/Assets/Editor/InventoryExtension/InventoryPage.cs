using UnityEngine;
using System.Collections;

public class InventoryPage {
	private InventoryObject[] objectArray;
	private int pageId;

	public string pageName;

	public InventoryPage(string pageName, InventoryObject[] objectArray, int pageId) 
	{
		this.pageName = pageName;
		this.objectArray = objectArray;
		this.pageId = pageId;
	}

	public void AddObject(InventoryObject obj, int pos) 
	{
		objectArray [pos] = obj;
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
    }

	public InventoryObject[] GetObjectArray() 
	{
		return objectArray;
	}

	public int GetPageId() 
	{
		return pageId;
	}

	public string GetPageName()
	{
		return pageName;
	}

    public void ClearPage()
    {
        for(int i = 0; i < objectArray.Length; i++)
        {
            objectArray[i] = null;           
        }
    }



}
