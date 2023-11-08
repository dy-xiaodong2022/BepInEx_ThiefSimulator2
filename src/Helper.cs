using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Vector2 = System.Numerics.Vector2;

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

    private static readonly IGameItem[] ItemDB = new IGameItem[500];

    private static void SetupItemDB()
    {
        GetGameObjectsByType<ItemID>().ForEach(item =>
        {
            ItemDB[item.item_ID] = new GameItem(item);
        });
    }
    
    public static IGameItem GetGameItem(int itemID)
    {
        if (ItemDB[0] == null)
        {
            SetupItemDB();
        }
        return ItemDB![itemID];
    }
    
    public static UnityEngine.Vector2 WorldGameItemToScreenPoint(Vector3 worldPoint,RectTransform canvasRectTransform)
    {
        // convert item in world to item in screen (for show on canvas)
        UnityEngine.Vector3 screenPoint = Camera.main!.WorldToScreenPoint(worldPoint);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPoint, null, out UnityEngine.Vector2 localPoint);
        return localPoint;
    }
    
    public static int CalcItemToCameraDistance(GameObject item)
    {
        UnityEngine.Vector3 cameraPosition = Camera.main!.transform.position;
        UnityEngine.Vector3 itemPosition = item.transform.position;
        return (int)UnityEngine.Vector3.Distance(cameraPosition, itemPosition);
    }
    
    public static bool ShowItemOrNot(GameObject item)
    {
        if (CalcItemToCameraDistance(item) > 80 ||
            item.activeInHierarchy == false)
        {
            return false;
        }
        return true;
    }
    
    public static Pickupable GetWatchingItem(Canvas ItemCanvas)
    {
        // Get All Pickupable
        List<Pickupable> allItems = Helper.GetGameObjectsByType<Pickupable>().FindAll(item => Helper.ShowItemOrNot(item.gameObject));
        // Convert to Position and find the nearest one to mid point of screen
        UnityEngine.Vector2[] allItemsPosition = new UnityEngine.Vector2[allItems.Count];
        allItems.ForEach(item =>
        {
            UnityEngine.Vector2 itemPosition = Helper.WorldGameItemToScreenPoint(item.transform.position,
                ItemCanvas.transform as RectTransform);
            allItemsPosition[allItems.IndexOf(item)] = itemPosition;
        });
        UnityEngine.Vector2 screenMidPoint = new UnityEngine.Vector2(Screen.width / 2f, Screen.height / 2f);
        Pickupable nowNearestItem = null;
        float nowNearestDistance = 1000;
        for (int i = 0; i < allItems.Count; i++)
        {
            float itemDistance = Math.Abs(allItemsPosition[i].x) + Math.Abs(allItemsPosition[i].y);
            if (itemDistance < nowNearestDistance)
            {
                nowNearestDistance = itemDistance;
                nowNearestItem = allItems[i];
            }
        }
        return nowNearestDistance > 150 ? null : nowNearestItem;
    }
}