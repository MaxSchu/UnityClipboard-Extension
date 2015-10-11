using UnityEngine;
using UnityEditor;
using System.Collections;

public class InventoryController {
	public static string pageCountKey = "page_count";

	private ArrayList pageList = new ArrayList();
	private int activePageId;
	private int pageCount = 0;

	private int height, width;

	public InventoryController(int width, int height) 
	{
		this.height = height;
		this.width = width;
		LoadPrefs ();
	}

	public void AddPage() 
	{
		InventoryPage page = new InventoryPage ("page" + pageCount, new UnityEngine.Object[width*height], pageCount);
		pageList.Add (page);
		pageCount++;
		activePageId = page.GetPageId();
	}

	private void LoadPage(InventoryPage page) 
	{
		pageList.Add (page);
		pageCount++;
	}

	public void DeletePage() 
	{
		foreach (InventoryPage page in pageList) 
		{
			if(page.GetPageId() == activePageId) 
			{
				int index = pageList.IndexOf(page);
				pageList.Remove(page);
				if(pageList.Count == 0) 
				{
					AddPage();
				}else 
				{
					activePageId = ((InventoryPage) pageList[index]).GetPageId();
				}
			}	
		}
	}

	public void DeleteObject(int position) 
	{
		GetActivePage ().DeleteObject (position);
	}

	public void SetActivePage(int num)
	{
		int index = pageList.IndexOf (GetActivePage ());
		Debug.Log ("index of active page: " + index + " , Amount of pages :" + pageList.Count);
		if ((index + num) >= 0 && (index + num) <= pageList.Count-1) 
		{
			Debug.Log ("new activepage setted");
			activePageId = ((InventoryPage)pageList [index + num]).GetPageId ();
		}
	}

	public InventoryPage GetActivePage()
	{
		foreach (InventoryPage page in pageList) 
		{
			if(page.GetPageId() == activePageId) 
			{
				return (InventoryPage) pageList[activePageId];
			}
		}
		return null;
	}
	
	public void SafePrefs()
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
		if (pageAmount == 0) 
		{
			Debug.Log ("Prefs Empty. Loaded new empty page.");
			AddPage ();
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
				LoadPage (new InventoryPage(EditorPrefs.GetString("pageName"+ pageId), objectArray, pageId));
			}

		}
		activePageId = ((InventoryPage) pageList[0]).GetPageId();
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
		pageList = new ArrayList ();
		pageCount = 0;
		AddPage ();
	}	
}
