using UnityEngine;
using System.Collections;

public class InventoryObject {
    public UnityEngine.Object obj;
    public int stackSize = 0;

    public InventoryObject(UnityEngine.Object obj, int stackSize)
    {
        this.obj = obj;
        this.stackSize = stackSize;
    }
}
