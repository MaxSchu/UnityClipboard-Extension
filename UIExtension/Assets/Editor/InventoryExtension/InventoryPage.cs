using UnityEngine;
using System.Collections;

public class InventoryPage {
	private UnityEngine.Object[] objectArray;
	private int pageId;

	public string pageName;

	public InventoryPage(string pageName, UnityEngine.Object[] objectArray, int pageId) 
	{
		this.pageName = pageName;
		this.objectArray = objectArray;
		this.pageId = pageId;
	}

	public void AddObject(UnityEngine.Object obj, int pos) 
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

	public UnityEngine.Object[] GetObjectArray() 
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

}
