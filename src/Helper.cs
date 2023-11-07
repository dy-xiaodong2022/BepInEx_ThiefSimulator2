using System.Collections.Generic;
using UnityEngine;

namespace BepInEx_ThiefSimulator2;

public abstract class Helper
{
    public static bool IsInGame()
    {
        return GetGameObjectByName<UnityEngine.Transform>("playerHands") != null;
    }

    public static ItemID GetItemID(int itemID)
    {
        ItemID returnItem = null;
        Helper.GetGameObjectsByType<ItemID>().ForEach(item =>
        {
            if (item.item_ID == itemID)
            {
                returnItem = item;
            }
        });
        return returnItem;
    }

    public static T GetGameObjectByName<T>(string name) where T : class
    {
        foreach (var o in UnityEngine.Object.FindObjectsOfType<GameObject>(true))
        {
            var obj = (GameObject)o;
            if (obj.name.Contains(name))
            {
                return obj.GetComponent<T>();
            }
        }

        return null;
    }

    public static List<T> GetGameObjectsByType<T>() where T : Object
    {
        var componentsArray = UnityEngine.Object.FindObjectsOfType<T>(true);
        var componentsList = new List<T>(componentsArray);
        return componentsList;
    }
}