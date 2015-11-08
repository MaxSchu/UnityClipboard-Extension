using UnityEngine;
using UnityEditor;
using System.Collections;

public class InventoryController {
	public readonly static string pageCountKey = "page_count";
    public readonly static bool stackingEnabled = true;
    public readonly static int standardStackSize = 1;
    public readonly static KeyCode splitStackKey = KeyCode.LeftAlt;

	private ArrayList pageList = new ArrayList();
	public int activePageId;

	private int height, width;

	public InventoryController(int width, int height) 
	{
		this.height = height;
		this.width = width;
		LoadPrefs ();
	}

	public void AddPage() 
	{
		InventoryPage page = new InventoryPage ("page" + pageList.Count, new InventoryObject[width*height]);
		pageList.Add (page);
		activePageId = pageList.Count-1;
	}

	private void LoadPage(InventoryPage page) 
	{
		pageList.Add (page);
	}

	public void DeletePage(int id) 
	{               

    	if (pageList.Count == 1)
        {
			((InventoryPage)pageList[0]).ClearPage();
			return;
		}
		else
		{
			DeletePageFromPrefs((InventoryPage)pageList[id]);
			pageList.RemoveAt(id);
			if (id >= pageList.Count)
			{
				id--;
			}
			activePageId = id;
		}
    }

	public void SetActivePage(int id)
	{
		activePageId = id;
	}

	public InventoryPage GetActivePage()
	{
		return (InventoryPage)pageList[activePageId];
	}
	
	public void SafePrefs()
	{
        InventoryObject[] objectArray;
        InventoryObject obj;
		InventoryPage page;

		for (int i = 0; i < pageList.Count; i++) 
		{
			page = (InventoryPage) pageList[i];
			objectArray = page.GetObjectArray();
			for (int u = 0; u < width * height; u++)
			{
				if ((obj = objectArray[u]) != null)
				{
					string path = AssetDatabase.GetAssetPath(obj.obj);
					EditorPrefs.SetString("" + i + "." + u, path);
					if(stackingEnabled)
					{
						EditorPrefs.SetInt("Stack" + i + "." + u, obj.stackSize);
					}
				} else
				{
					EditorPrefs.DeleteKey("" + i + "." + u);
					EditorPrefs.DeleteKey("Stack" + i + "." + u);
				}
			}
			EditorPrefs.SetString("pageName" + i, page.GetPageName());	
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
                InventoryObject[] objectArray = new InventoryObject[width * height];
				
				for (int i = 0; i < width * height; i++)
				{
					string objPath;
					if ((objPath = EditorPrefs.GetString("" + pageId + "." + i)) != "")
					{
                        int stackSize = 0;
                        if(stackingEnabled)
                        {
                            stackSize = EditorPrefs.GetInt("Stack" + pageId + "." + i);
                        }
                        objectArray[i] = new InventoryObject(AssetDatabase.LoadAssetAtPath(objPath, typeof(UnityEngine.Object)), stackSize);
                    }
				}
				LoadPage (new InventoryPage(EditorPrefs.GetString("pageName"+ pageId), objectArray));
			}

		}
		activePageId = 0;
	}

    private void DeletePageFromPrefs(InventoryPage page)
    {
		int pageId = pageList.IndexOf (page);
        for ( int i = 0; i < page.GetObjectArray().Length; i++)
        {
            EditorPrefs.DeleteKey("" + pageId + "." + i);
            EditorPrefs.DeleteKey("Stack" + pageId + "." + i);
        }
        EditorPrefs.DeleteKey("pageName" + pageId);
    }

	public void ClearPrefs()
	{
		for(int pageId = 0; pageId < pageList.Count; pageId++)
		{
			for(int i = 0; i < width*height; i++)
			{
				EditorPrefs.DeleteKey("" + pageId + "." + i);
                EditorPrefs.DeleteKey("Stack" + pageId + "." + i);
            }
			EditorPrefs.DeleteKey("pageName" + pageId);
		}
		EditorPrefs.DeleteKey(pageCountKey);
		pageList = new ArrayList ();
		AddPage ();
	}

	public string GetPageName(int id)
	{
		if (id >= pageList.Count) 
		{
			return "PageNotAddedYet";
		}
		return ((InventoryPage)pageList [id]).GetPageName ();
	}

	public int GetPageCount()
	{
		return pageList.Count;
	}
}
