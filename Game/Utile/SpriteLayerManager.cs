using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteLayerManager : CSingleton<SpriteLayerManager>
{
    int order;
    public int Order
    {
        get
        {
            return ++order;
        }
    }


    public void reset()
    {
        this.order = 0;
    }
}
