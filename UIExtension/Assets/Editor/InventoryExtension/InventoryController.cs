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
        for (int i = pageList.Count - 1; i >= 0; i--)
        {
            InventoryPage page = (InventoryPage)pageList[i];
            if (page.GetPageId() == activePageId)
            {                
                if (pageList.Count == 1)
                {
                    ((InventoryPage)pageList[0]).ClearPage();
                    return;
                }
                else
                {
                    int index = pageList.IndexOf(page);
                    DeletePageFromPrefs(page);
                    pageList.Remove(page);
                    if (index >= pageList.Count)
                    {
                        index--;
                    }
                    Debug.Log("Page deleted, index: " + index + "Listcount :" + pageList.Count);
                    activePageId = ((InventoryPage)pageList[index]).GetPageId();
                    Debug.Log("hurrdudrudrudruuruururruru");
                    return;
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
		if ((index + num) >= 0 && (index + num) <= pageList.Count-1) 
		{
			activePageId = ((InventoryPage)pageList [index + num]).GetPageId ();
		}
	}

	public InventoryPage GetActivePage()
	{
		for(int i = 0; i < pageList.Count; i++)
		{
            InventoryPage page = (InventoryPage)pageList[i];
			if(page.GetPageId() == activePageId) 
			{
                return page;
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

    private void DeletePageFromPrefs( InventoryPage page)
    {
        int pageId = page.GetPageId();
        for ( int i = 0; i < page.GetObjectArray().Length; i++)
        {
            EditorPrefs.DeleteKey("" + pageId + "." + i);
        }
        EditorPrefs.DeleteKey("pageName" + pageId);
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
