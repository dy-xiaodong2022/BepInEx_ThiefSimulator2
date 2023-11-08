using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using BepInEx;
using EVP;
using MadGoatLockpickSystem;
using UnityEngine;
using UnityEngine.UI;

namespace BepInEx_ThiefSimulator2
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private Canvas _infoCanvas;
        private Canvas _itemCanvas;
        private GameObject _playerSpeedInfo;
        private GameObject _pickupItemInfo;
        private const int MaxItems = 1000;
        private readonly Text[] _itemTexts = new Text[MaxItems];

        private readonly Config _pluginConfig = new Config();


        // Important Game Objects (Manager/Controller/...)
        private static GameManager _gameManager;
        private static Police_Manager _policeManager;
        private static Skill_Manager _skillManager;
        private static JobManager _jobManager;
        private static AbilitiesManager _abilitiesManager;
        private static ContractManager _contractManager;
        private static VehicleManager _vehicleManager;
        private static StoryManager _storyManager;
        private static DetectController _detectController;
        private static PlayerMotorBehavior _player;


        private void Awake()
        {
            // Interval call
            // InvokeRepeating(nameof(SecondLoop), 1, 1);
            InvokeRepeating(nameof(FrameInterval), 1, 0.1f);


            // Canvas setup
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
            // _playerSpeedInfo.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            _playerSpeedInfo.GetComponent<Text>().text = "PlayerSpeedInfo: 0";
            _playerSpeedInfo.GetComponent<Text>().color = Color.white;
            _playerSpeedInfo.GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Overflow;
            // width height
            // _playerSpeedInfo.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 50);
            // _playerSpeedInfo.GetComponent<Te
            // top-left
            _playerSpeedInfo.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
            _playerSpeedInfo.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
            _playerSpeedInfo.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
            _playerSpeedInfo.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            Logger.LogWarning(3);

            // Add text PickupItem
            // Canvas Text - Overlay 2D Screen
            _pickupItemInfo = new GameObject("PickupItemInfo");
            _pickupItemInfo.transform.SetParent(this._infoCanvas.transform, false);
            _pickupItemInfo.AddComponent<CanvasRenderer>();
            _pickupItemInfo.AddComponent<Text>();
            _pickupItemInfo.GetComponent<Text>().font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _pickupItemInfo.GetComponent<Text>().fontSize = 13;
            // _pickupItemInfo.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            _pickupItemInfo.GetComponent<Text>().text = "--";
            _pickupItemInfo.GetComponent<Text>().color = Color.cyan;
            _pickupItemInfo.GetComponent<Text>().fontStyle = FontStyle.Bold;
            _pickupItemInfo.GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Overflow;
            // center
            _pickupItemInfo.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            // at screen center (little down)
            _pickupItemInfo.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
            _pickupItemInfo.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            _pickupItemInfo.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -20);


            Logger.LogWarning(4);


            itemCanvasObject.transform.SetParent(mainCanvasObject.transform, false);
            // Create text items pool
            for (int i = 0; i < MaxItems; i++)
            {
                GameObject itemElementObject = new GameObject("ItemElement");
                itemElementObject.transform.SetParent(this._itemCanvas.transform, false);
                // RectTransform rectTransform = itemElementObject.GetComponent<RectTransform>();
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

            Logger.LogInfo("ThiefSimulator2 Plugin Load");
        }

        private void FrameInterval()
        {
            if (Helper.IsInGame())
            {
                GameObject managersParent = GameObject.Find("MANAGERS");
                _gameManager = _gameManager == null
                    ? managersParent.GetComponentInChildren<GameManager>()
                    : _gameManager;
                _policeManager = _policeManager == null
                    ? managersParent.GetComponentInChildren<Police_Manager>()
                    : _policeManager;
                _skillManager = _skillManager == null
                    ? managersParent.GetComponentInChildren<Skill_Manager>()
                    : _skillManager;
                _jobManager = _jobManager == null ? managersParent.GetComponentInChildren<JobManager>() : _jobManager;
                _abilitiesManager = _abilitiesManager == null
                    ? managersParent.GetComponentInChildren<AbilitiesManager>()
                    : _abilitiesManager;
                _contractManager = _contractManager == null
                    ? managersParent.GetComponentInChildren<ContractManager>()
                    : _contractManager;
                _vehicleManager = _vehicleManager == null
                    ? managersParent.GetComponentInChildren<VehicleManager>()
                    : _vehicleManager;
                _storyManager = _storyManager == null
                    ? managersParent.GetComponentInChildren<StoryManager>()
                    : _storyManager;
                _detectController = _detectController == null
                    ? managersParent.GetComponentInChildren<DetectController>()
                    : _detectController;
                _player = _player == null
                    ? Helper.GetGameObjectByName<PlayerMotorBehavior>("First-Person Character")
                    : _player;

                if (_pluginConfig.GetConfigField<bool>("LockpickAlwaysSuccess"))
                {
                    GameObject minigamesContainer = GameObject.Find("MINIGAMES CONTAINER");
                    MadGoatLockpickMaster madGoatLockpickMaster =
                        minigamesContainer.GetComponentInChildren<MadGoatLockpickMaster>();
                    if (!madGoatLockpickMaster.isQuitting)
                    {
                        madGoatLockpickMaster.OnUnlocked();
                    }
                    
                    SafeMaster safeMaster = minigamesContainer.GetComponentInChildren<SafeMaster>();
                    if (safeMaster.isEnabled)
                    {
                        safeMaster.Success();
                    }
                    
                    MediumLockMaster mediumLockMaster = Helper.GetField<MediumLockMaster>(minigamesContainer.GetComponentInChildren<Medium_Lock_Spawner>(), "spawnedMaster");
                    if (mediumLockMaster != null)
                    {
                        mediumLockMaster.Unlocked();
                    }
                }

                _player.runSpeed = 7.5f * _pluginConfig.GetConfigField<float>("PlayerSpeed");
                _player.isGhosting = _pluginConfig.GetConfigField<bool>("PlayerGhost");
                Helper.SetField(_player, "jumpsLeft", _pluginConfig.GetConfigField<bool>("PlayerInfJump") ? 999 : 1);
                if (_pluginConfig.GetConfigField<bool>("NoPolice"))
                {
                    _policeManager.current_stars = 0;
                    // destory gameobject
                    if (GameObject.Find("Policeman(Clone)"))
                    {
                        
                        GameObject.Destroy(GameObject.Find("Policeman(Clone)"));
                    }
                    // GameObject.Destroy(GameObject.Find("PoliceCar_Static01(Clone)"));
                    if (GameObject.Find("PoliceCar_Static01(Clone)"))
                    {
                        GameObject.Destroy(GameObject.Find("PoliceCar_Static01(Clone)"));
                    }
                }


                Vector2 currentSpeed = Helper.GetField<Vector2>(_player, "currentSpeed");
                float currentSpeedMagnitude = currentSpeed.magnitude * 10;
                if (currentSpeedMagnitude < 0.5)
                {
                    currentSpeedMagnitude = 0;
                }

                _playerSpeedInfo.GetComponent<Text>().text = "PlayerSpeedInfo: " + currentSpeedMagnitude;

                if (_pluginConfig.GetConfigField<bool>("TakeItemOverride"))
                {
                    // Pickup Item Info
                    Pickupable nowNearestItem = Helper.GetWatchingItem(this._itemCanvas);
                    if (nowNearestItem != null)
                    {
                        int distance = Helper.CalcItemToCameraDistance(nowNearestItem.gameObject);
                        string itemName = nowNearestItem.is_money
                            ? "$" + nowNearestItem.add_money
                            : Helper.GetItemID(nowNearestItem.item_ID).name;

                        int itemValue = nowNearestItem.is_money
                            ? nowNearestItem.add_money
                            : Helper.GetGameItem(nowNearestItem.item_ID).ItemValue;
                        _pickupItemInfo.GetComponent<Text>().text =
                            $"Press F to pickup: {itemName} | ${itemValue} [{distance}]";
                    }
                    else
                    {
                        _pickupItemInfo.GetComponent<Text>().text = "--";
                    }
                }
                else
                {
                    _pickupItemInfo.GetComponent<Text>().text = "";
                }


                // Item Show
                if (_pluginConfig.GetConfigField<bool>("ShowItems"))
                {
                    List<Pickupable> allItems = Helper.GetGameObjectsByType<Pickupable>();
                    Logger.LogWarning("Items: " + allItems.Count);
                    int index = 0;
                    allItems.ForEach(item =>
                    {
                        int distance = Helper.CalcItemToCameraDistance(item.gameObject);
                        if (distance > 80 ||
                            item.gameObject.scene.name != _gameManager.loaded_map_name ||
                            item.gameObject.activeInHierarchy == false)
                        {
                            return;
                        }

                        if (index >= MaxItems)
                        {
                            return;
                        }

                        Text textComponent = _itemTexts[index];
                        float itemValue;
                        string itemName;
                        if (item.is_money)
                        {
                            itemValue = item.add_money;
                            itemName = itemValue + " dollars";
                        }
                        else
                        {
                            int itemID = item.item_ID;
                            IGameItem gameItem = Helper.GetGameItem(itemID);
                            itemValue = gameItem.ItemValue;
                            itemName = gameItem.Name;
                        }

                        switch (itemValue)
                        {
                            case >= 500:
                                textComponent.color = Color.red;
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

                        if (
                            (textComponent.color != Color.white ||
                             _pluginConfig.GetConfigField<bool>("ShowNormalItems")) &&
                            (textComponent.color != Color.yellow ||
                             _pluginConfig.GetConfigField<bool>("ShowGoodItems")) &&
                            (textComponent.color != Color.magenta ||
                             _pluginConfig.GetConfigField<bool>("ShowValuableItems")) &&
                            (textComponent.color != Color.red ||
                             _pluginConfig.GetConfigField<bool>("ShowVeryValuableItems"))
                        )
                        {
                            textComponent.gameObject.SetActive(true);
                            textComponent.text = $"${itemValue} | {itemName} [{distance}]";
                            RectTransform rectTransform = textComponent.GetComponent<RectTransform>();
                            // Convert screen position to position in the canvas
                            rectTransform.anchoredPosition = Helper.WorldGameItemToScreenPoint(item.transform.position,
                                this._itemCanvas.transform as RectTransform);
                            // if out of screen
                            if (rectTransform.anchoredPosition.x < 0 ||
                                rectTransform.anchoredPosition.x > Screen.width ||
                                rectTransform.anchoredPosition.y < 0 ||
                                rectTransform.anchoredPosition.y > Screen.height)
                            {
                                textComponent.gameObject.SetActive(false);
                            }

                            index++;
                        }
                        else
                        {
                            textComponent.gameObject.SetActive(false);
                        }
                    });

                    // Deactivate the remaining text components in the pool
                    for (int i = index; i < MaxItems; i++)
                    {
                        _itemTexts[i].gameObject.SetActive(false);
                    }
                }
            }
        }

        private void Update()
        {
            if (Helper.IsInGame())
            {
                if (Input.GetKeyDown(KeyCode.F))
                {
                    if (_pluginConfig.GetConfigField<bool>("TakeItemOverride"))
                    {
                        Pickupable nowNearestItem = Helper.GetWatchingItem(this._itemCanvas);
                        if (nowNearestItem != null)
                        {
                            if (nowNearestItem!.is_heavy_item)
                            {
                                nowNearestItem.PickUpHeavy(1);
                                return;
                            }

                            nowNearestItem.PickUp();
                        }
                    }
                }
            }
        }
    }
}