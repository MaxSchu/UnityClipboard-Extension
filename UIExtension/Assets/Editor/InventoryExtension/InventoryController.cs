using UnityEngine;
using UnityEditor;
using System.Collections;

public class InventoryController : MonoBehaviour{
	public static string pageCountKey = "page_count";

	private ArrayList pageList = new ArrayList();
	private InventoryPage activePage;
	private int pageCount = 0;

	private int height, width;

	public InventoryController(int width, int height) 
	{
		this.height = height;
		this.width = width;
		LoadPrefs ();
	}

	public void AddPage(InventoryPage page) 
	{
		pageCount++;
		pageList.Add (page);
	}

	public void DeletePage(int pageId) 
	{
		foreach (InventoryPage page in pageList) 
		{
			if(page.GetPageId() == pageId) 
			{
				pageList.Remove(page);
			}	
		}
	}

	public InventoryPage GetActivePage() 
	{
		return activePage;
	}

	public void SetActivePage()
	{

	}
	
	private void SafePrefs()
	{
		UnityEngine.Object[] objectArray;
		UnityEngine.Object obj;
		int pageId;

		foreach (InventoryPage page in pageList) 
		{
			pageId = page.GetPageId();
			objectArray = page.GetObjectArray();
			for (int i = 0; i < width * height; i++)
			{
				if ((obj = objectArray[i]) != null)
				{
					string path = AssetDatabase.GetAssetPath(obj);
					EditorPrefs.SetString("" + pageId + "." + i, path);
				} else
				{
					EditorPrefs.DeleteKey("" + pageId + "." + i);
				}
			}
			EditorPrefs.SetString("pageName" + page.GetPageId(), page.GetPageName());
		}
		EditorPrefs.SetInt (pageCountKey, pageList.Count);
	}
	
	private void LoadPrefs()
	{
		int pageAmount = EditorPrefs.GetInt (pageCountKey);
		if (pageAmount == null) 
		{
			activePage = new InventoryPage("Page1", new UnityEngine.Object[width*height], 0);
			AddPage( activePage);
		} else 
		{
			for (int pageId = 0; pageId < pageAmount; pageId++) 
			{
				UnityEngine.Object[] objectArray = new UnityEngine.Object[width * height];
				
				for (int i = 0; i < width * height; i++)
				{
					string objPath;
					if ((objPath = EditorPrefs.GetString("" + pageId + "." + i)) != "")
					{
						objectArray[i] = AssetDatabase.LoadAssetAtPath(objPath, typeof(UnityEngine.Object));
					}
				}
				AddPage (new InventoryPage(EditorPrefs.GetString("pageName"+ pageId), objectArray, pageId));
			}
			activePage = (InventoryPage) pageList[0];
		}
	}
	
	public void ClearPrefs()
	{
		for(int pageId = 0; pageId < pageCount; pageId++)
		{
			for(int i = 0; i < width*height; i++)
			{
				EditorPrefs.DeleteKey("" + pageId + "." + i);
			}
			EditorPrefs.DeleteKey("pageName" + pageId);
		}
		EditorPrefs.DeleteKey(pageCountKey);
	}	
}
