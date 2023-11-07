using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace BepInEx_ThiefSimulator2
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private Canvas _infoCanvas;
        private Canvas _itemCanvas;
        private GameObject _playerSpeedInfo;
        private const int MaxItems = 1000;
        private readonly Text[] _itemTexts = new Text[MaxItems];
        // ReSharper disable once MemberCanBePrivate.Global
        public static GameManager GameManager;

        private void Awake()
        {
            InvokeRepeating(nameof(SecondLoop), 1, 1);
            InvokeRepeating(nameof(FrameInterval), 1, 0.2f);
            Logger.LogInfo("ThiefSimulator2 Plugin Load");
            // Main Object
            GameObject mainCanvasObject = new GameObject("ModCanvas");
            // Don't destroy on load
            DontDestroyOnLoad(mainCanvasObject);
            // ModCanvas(Object) -> InfoCanvas/ItemCanvas(Object)
            GameObject infoCanvasObject = new GameObject("InfoCanvas");
            GameObject itemCanvasObject = new GameObject("ItemCanvas");
            infoCanvasObject.transform.SetParent(mainCanvasObject.transform, false);
            itemCanvasObject.transform.SetParent(mainCanvasObject.transform, false);
            Logger.LogWarning(1);

            // Info Canvas
            this._infoCanvas = infoCanvasObject.AddComponent<Canvas>();
            this._infoCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // Items Canvas
            this._itemCanvas = itemCanvasObject.AddComponent<Canvas>();
            this._itemCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // full screen
            this._infoCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);
            this._itemCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);

            Logger.LogWarning(2);
            infoCanvasObject.AddComponent<CanvasScaler>();
            infoCanvasObject.AddComponent<GraphicRaycaster>();
            itemCanvasObject.AddComponent<CanvasScaler>();
            itemCanvasObject.AddComponent<GraphicRaycaster>();
            itemCanvasObject.transform.SetParent(mainCanvasObject.transform, false);
            Logger.LogWarning(2);


            // Add text PlayerSpeedInfo
            // Canvas Text - Overlay 2D Screen
            _playerSpeedInfo = new GameObject("PlayerSpeedInfo");
            _playerSpeedInfo.transform.SetParent(this._infoCanvas.transform, false);
            _playerSpeedInfo.AddComponent<CanvasRenderer>();
            _playerSpeedInfo.AddComponent<Text>();
            _playerSpeedInfo.GetComponent<Text>().font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _playerSpeedInfo.GetComponent<Text>().fontSize = 13;
            _playerSpeedInfo.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            _playerSpeedInfo.GetComponent<Text>().text = "PlayerSpeedInfo: 0";
            _playerSpeedInfo.GetComponent<Text>().color = Color.white;
            _playerSpeedInfo.GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Overflow;
            // width height
            _playerSpeedInfo.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 50);
            // top-left
            _playerSpeedInfo.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            _playerSpeedInfo.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            _playerSpeedInfo.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
            _playerSpeedInfo.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            Logger.LogWarning(3);


            itemCanvasObject.transform.SetParent(mainCanvasObject.transform, false);
            // Create text items pool
            for (int i = 0; i < MaxItems; i++)
            {
                GameObject itemElementObject = new GameObject("ItemElement");
                itemElementObject.transform.SetParent(this._itemCanvas.transform, false);
                RectTransform rectTransform = itemElementObject.GetComponent<RectTransform>();
                // width height
                // rectTransform.sizeDelta = new Vector2(200, 50);
                // // top-left
                // rectTransform.anchorMin = new Vector2(0, 1);
                // rectTransform.anchorMax = new Vector2(0, 1);
                // rectTransform.pivot = new Vector2(0, 1);
                Text textComponent = itemElementObject.AddComponent<Text>();
                textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                textComponent.fontSize = 13;
                // width no max
                textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
                textComponent.alignment = TextAnchor.MiddleCenter;
                textComponent.color = Color.white;
                _itemTexts[i] = textComponent;
            }
        }

        private void FrameInterval()
        {
            if (Helper.IsInGame())
            {
                if (GameManager == null)
                {
                    GameManager = UnityEngine.Object.FindObjectOfType<GameManager>();
                }
                PlayerMotorBehavior playerMotor =
                    Helper.GetGameObjectByName<PlayerMotorBehavior>("First-Person Character");
                var currentSpeedField =
                    typeof(PlayerMotorBehavior).GetField("currentSpeed",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                if (currentSpeedField != null)
                {
                    Vector2 currentSpeed = (Vector2)currentSpeedField.GetValue(playerMotor);
                    float currentSpeedMagnitude = currentSpeed.magnitude * 10;
                    if (currentSpeedMagnitude < 0.5)
                    {
                        currentSpeedMagnitude = 0;
                    }

                    _playerSpeedInfo.GetComponent<Text>().text = "PlayerSpeedInfo: " + currentSpeedMagnitude;
                }


                // add items
                List<Pickupable> allItems = Helper.GetGameObjectsByType<Pickupable>();
                Logger.LogWarning("Items: " + allItems.Count);
                int index = 0;
                allItems.ForEach(item =>
                {
                    int distance = Helper.CalcItemToCameraDistance(item.gameObject);
                    if (distance > 80 ||
                        item.gameObject.scene.name != GameManager.loaded_map_name ||
                        item.gameObject.activeInHierarchy == false)
                    {
                        return;
                    }
                    // Make sure we don't exceed our object pool size
                    if (index >= MaxItems)
                    {
                        return;
                    }

                    // convert item in world to item in screen
                    // RectTransformUtility.ScreenPointToLocalPointInRectangle(this._itemCanvas.transform as RectTransform,
                    // Camera.main!.WorldToScreenPoint(item.transform.position), null, out var itemInfoOnScreenPoint);
                    Text textComponent = _itemTexts[index];
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
                        IGameItem gameItem = Helper.GetGameItem(itemID);
                        itemValue = gameItem.ItemValue;
                        itemName = gameItem.Name;
                    }

                    // textComponent.text = itemName + " | $" + itemValue;
                    textComponent.text = String.Format("${0} | {1} [{2}]", itemValue, itemName, distance);
                    switch (itemValue)
                    {
                        case >= 500:
                            textComponent.color =Color.red;
                            break;
                        case >= 200:
                            textComponent.color = Color.magenta;
                            break;
                        case >= 100:
                            textComponent.color = Color.yellow;
                            break;
                        default:
                            textComponent.color = Color.white;
                            break;
                    }

                    RectTransform rectTransform = textComponent.GetComponent<RectTransform>();
                    // Convert screen position to position in the canvas
                    rectTransform.anchoredPosition = Helper.WorldGameItemToScreenPoint(item.transform.position,
                        this._itemCanvas.transform as RectTransform);

                    index++;
                });

// Deactivate the remaining text components in the pool
                for (int i = index; i < MaxItems; i++)
                {
                    _itemTexts[i].gameObject.SetActive(false);
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
                Logger.LogInfo("PlayerMotorBehavior: " + playerMotor);
                playerMotor.runSpeed = 30;
            }
        }
    }
}