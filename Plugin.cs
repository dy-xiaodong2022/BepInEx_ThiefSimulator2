using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;

namespace BepInEx_ThiefSimulator2
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private Canvas InfoCanvas;
        private Canvas ItemCanvas;
        private GameObject PlayerSpeedInfo;
        private const int MaxItems = 1000;
        private Text[] itemTexts = new Text[MaxItems];

        private void Awake()
        {
            InvokeRepeating(nameof(SecondLoop), 1, 1);
            InvokeRepeating(nameof(FrameInterval), 1, 0.5f);
            Logger.LogInfo("ThiefSimulator2 Plugin Load");
            // Main Object
            GameObject MainCanvasObject = new GameObject("ModCanvas");
            // Don't destroy on load
            DontDestroyOnLoad(MainCanvasObject);
            // ModCanvas(Object) -> InfoCanvas/ItemCanvas(Object)
            GameObject InfoCanvasObject = new GameObject("InfoCanvas");
            GameObject ItemCanvasObject = new GameObject("ItemCanvas");
            InfoCanvasObject.transform.SetParent(MainCanvasObject.transform, false);
            ItemCanvasObject.transform.SetParent(MainCanvasObject.transform, false);
            Logger.LogWarning(1);

            // Info Canvas
            this.InfoCanvas = InfoCanvasObject.AddComponent<Canvas>();
            this.InfoCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // Items Canvas
            this.ItemCanvas = ItemCanvasObject.AddComponent<Canvas>();
            this.ItemCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            Logger.LogWarning(2);
            InfoCanvasObject.AddComponent<CanvasScaler>();
            InfoCanvasObject.AddComponent<GraphicRaycaster>();
            ItemCanvasObject.AddComponent<CanvasScaler>();
            ItemCanvasObject.AddComponent<GraphicRaycaster>();
            ItemCanvasObject.transform.SetParent(MainCanvasObject.transform, false);
            Logger.LogWarning(2);


            // Add text PlayerSpeedInfo
            // Canvas Text - Overlay 2D Screen
            PlayerSpeedInfo = new GameObject("PlayerSpeedInfo");
            PlayerSpeedInfo.transform.SetParent(this.InfoCanvas.transform, false);
            PlayerSpeedInfo.AddComponent<CanvasRenderer>();
            PlayerSpeedInfo.AddComponent<Text>();
            PlayerSpeedInfo.GetComponent<Text>().font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            PlayerSpeedInfo.GetComponent<Text>().fontSize = 20;
            PlayerSpeedInfo.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            PlayerSpeedInfo.GetComponent<Text>().text = "PlayerSpeedInfo: 0";
            PlayerSpeedInfo.GetComponent<Text>().color = Color.white;
            // width height
            PlayerSpeedInfo.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 50);
            // top-left
            PlayerSpeedInfo.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            PlayerSpeedInfo.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            PlayerSpeedInfo.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
            PlayerSpeedInfo.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            Logger.LogWarning(3);


            ItemCanvasObject.transform.SetParent(MainCanvasObject.transform, false);
            // Create text items pool
            for (int i = 0; i < MaxItems; i++) {
                GameObject itemText = new GameObject("ItemText");
                itemText.transform.SetParent(this.ItemCanvas.transform, false);
                itemText.AddComponent<CanvasRenderer>();
                Text textComponent = itemText.AddComponent<Text>();
                textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                textComponent.fontSize = 20;
                textComponent.alignment = TextAnchor.MiddleCenter;
                textComponent.color = Color.white;
                RectTransform rectTransform = itemText.GetComponent<RectTransform>();
                // width height
                rectTransform.sizeDelta = new Vector2(200, 50);
                // top-left
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.pivot = new Vector2(0, 1);
                itemTexts[i] = textComponent;
            }
        }

        private void FrameInterval()
        {
            if (Helper.IsInGame())
            {
                PlayerMotorBehavior playerMotor =
                    Helper.GetGameObjectByName<PlayerMotorBehavior>("First-Person Character");
                var currentSpeedField =
                    typeof(PlayerMotorBehavior).GetField("currentSpeed",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                Vector2 currentSpeed = (Vector2)currentSpeedField.GetValue(playerMotor);
                float currentSpeedMagnitude = currentSpeed.magnitude * 10;
                if (currentSpeedMagnitude < 0.5)
                {
                    currentSpeedMagnitude = 0;
                }

                PlayerSpeedInfo.GetComponent<Text>().text = "PlayerSpeedInfo: " + currentSpeedMagnitude;


                // clear items (active)
                for (int i = 0; i < MaxItems; i++)
                {
                    itemTexts[i].gameObject.SetActive(false);
                }
                // add items
                List<Pickupable> allItems = Helper.GetGameObjectsByType<Pickupable>();
                Logger.LogWarning("Items: " + allItems.Count);
                int index = 0;
                allItems.ForEach(item =>
                {
                    // Make sure we don't exceed our object pool size
                    if (index >= MaxItems) {
                        return;
                    }

                    Vector2 itemInfoOnScreenPoint;
                    // convert item in world to item in screen
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(this.ItemCanvas.transform as RectTransform,
                        Camera.main.WorldToScreenPoint(item.transform.position), null, out itemInfoOnScreenPoint);
    
                    Text textComponent = itemTexts[index];
                    textComponent.gameObject.SetActive(true);
                    float itemValue;
                    string itemName;
                    if (item.is_money)
                    {
                        itemValue = item.add_money;
                        itemName = itemValue + " dollars";
                        // Logger.LogInfo("Money: " + itemName);
                    }
                    else
                    {
                        int itemID = item.item_ID;
                        // Logger.LogInfo("ItemID: " + itemID);
                        ItemID gameItem = Helper.GetItemID(itemID);
                        // Logger.LogInfo(itemID + "_: " + gameItem);
                        itemValue = gameItem.item_value;
                        // Logger.LogInfo("ItemValue: " + itemValue);
                        itemName = item.name;
                        // Logger.LogInfo("ItemName: " + itemName);
                    }

                    textComponent.text = itemName + " | $" + itemValue;
                    textComponent.color = Color.white;

                    RectTransform rectTransform = textComponent.GetComponent<RectTransform>();
                    // Convert screen position to position in the canvas
                    rectTransform.anchoredPosition = itemInfoOnScreenPoint;

                    index++;
                });

// Deactivate the remaining text components in the pool
                for (int i = index; i < MaxItems; i++) {
                    itemTexts[i].gameObject.SetActive(false);
                }
            }
        }

        private void SecondLoop()
        {
            if (Helper.IsInGame())
            {
                Logger.LogInfo("IsInGame");
                PlayerMotorBehavior playerMotor =
                    Helper.GetGameObjectByName<PlayerMotorBehavior>("First-Person Character");
                Logger.LogInfo("PlayerMotorBehavior: " + playerMotor.ToString());
                playerMotor.runSpeed = 30;
            }
        }
    }

    public class Helper
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

        public static List<T> GetGameObjectsByType<T>() where T : Component
        {
            var componentsArray = UnityEngine.Object.FindObjectsOfType<T>(true);
            var componentsList = new List<T>(componentsArray);
            return componentsList;
        }
    }
}