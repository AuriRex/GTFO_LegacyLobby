using System;
using System.Linq;
using CellMenu;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using LegacyLobby.Extensions;
using Localization;
using TMPro;
using UnityEngine;


namespace LegacyLobby.Components;

public class ClothesButton : MonoBehaviour
{
    public const string CLOTHES_BUTTON_NAME = "Custom_Clothes_Button_Booster_Style";
    private static Sprite _icon;
    
    [HideFromIl2Cpp]
    public bool HasBlinkedIn { get; set; }
    
    [HideFromIl2Cpp]
    private CM_PlayerLobbyBar _playerLobbyBar
    {
        get => _rf_playerLobbyBar.Get();
        set => _rf_playerLobbyBar.Set(value);
    }
    [HideFromIl2Cpp]
    private CM_Item _item
    {
        get => _rf_item.Get();
        set => _rf_item.Set(value);
    }
    [HideFromIl2Cpp]
    private GameObject _box
    {
        get => _rf_box.Get();
        set => _rf_box.Set(value);
    }
    [HideFromIl2Cpp]
    private TextMeshPro _text
    {
        get => _rf_text.Get();
        set => _rf_text.Set(value);
    }
    [HideFromIl2Cpp]
    private GameObject _attentionIcon
    {
        get => _rf_attentionIcon.Get();
        set => _rf_attentionIcon.Set(value);
    }
    
    public Il2CppReferenceField<CM_PlayerLobbyBar> _rf_playerLobbyBar;
    public Il2CppReferenceField<CM_Item> _rf_item;
    public Il2CppReferenceField<GameObject> _rf_box;
    public Il2CppReferenceField<TextMeshPro> _rf_text;
    public Il2CppReferenceField<GameObject> _rf_attentionIcon;
    
    public void Awake()
    {
        if (_playerLobbyBar == null)
            return;
        
        CheckUpdateAndToggleState();
    }
    
    public static ClothesButton GetOrSetupFromLobbyBar(CM_PlayerLobbyBar lobbyBar)
    {
        if (Plugin.IsForceDefaultVanityInstalled)
            return null;

        if (lobbyBar == null || lobbyBar.m_hasPlayerRoot == null)
            return null;
        
        var playerRoot = lobbyBar.m_hasPlayerRoot.transform;
        
        var clothesButton = playerRoot.FindExactChild(CLOTHES_BUTTON_NAME)?.gameObject;
        if (clothesButton != null)
        {
            return clothesButton.GetComponent<ClothesButton>();
        }

        var boosterImplantSlot = GOUtil.SpawnChildAndGetComp<CM_BoosterImplantSlot>(lobbyBar.m_boosterImplantSlotHolder.m_boosterImplantSlotPrefab, playerRoot);
        clothesButton = boosterImplantSlot.gameObject;
        clothesButton.name = CLOTHES_BUTTON_NAME;

        clothesButton.DontDestroyAndSetHideFlags();
        
        var trans = clothesButton.transform;
        trans.localPosition = new Vector3(342, -500, 0);
        
        Destroy(boosterImplantSlot);

        trans.FindExactChild("ImplantStateText").gameObject.SetActive(false);
        trans.FindExactChild("BoosterIcon").gameObject.SetActive(false);

        if (_icon == null)
        {
            var pageRundown = MainMenuGuiLayer.Current.PageRundownNew;
            
            var drops = pageRundown.GetComponentInChildren<CM_RundownVanityItemDropsNext>(includeInactive: true);

            _icon = drops.m_icon.sprite;
        }
        
        return clothesButton.AddComponent<ClothesButton>().Setup();
    }
    
    private ClothesButton Setup()
    {
        _playerLobbyBar = transform.GetComponentInParent<CM_PlayerLobbyBar>();
        _item = gameObject.AddComponent<CM_Item>();
        _item.Setup();
        _box = transform.FindExactChild("Box").gameObject;

        _item.m_hoverSpriteArray = _box.transform.GetComponentsInChildren<SpriteRenderer>(true).Cast<Il2CppReferenceArray<SpriteRenderer>>();
        _item.m_alphaSpriteOnHover = true;
        _item.m_alphaTextOnHover = false;
        
        _item.m_onBtnPress = new();
        _item.OnBtnPressCallback = new Action<int>((_) =>
        {
            Plugin.L.LogWarning("Clothes Button pressed!");
            _playerLobbyBar.ShowClothesSelect();
        });

        _text = transform.FindExactChild("ImplantCategoryName").GetComponent<TextMeshPro>();

        _attentionIcon = transform.FindExactChild("AttentionIcon").gameObject;

        var emptyIconGo = transform.FindExactChild("EmptyIcon");
        emptyIconGo.localRotation = Quaternion.identity;
        var iconRenderer = emptyIconGo.GetComponent<SpriteRenderer>();
        iconRenderer.sprite = _icon;
        iconRenderer.size = new Vector2(125, 125);
        
        CheckUpdateAndToggleState();
        
        gameObject.SetActive(false);
        
        return this;
    }

    public void CheckUpdateAndToggleState()
    {
        _text.SetText(Text.Get(GameData.GD.Text.MainMenu_Lobby_PlayerBar_Apparel));
        
        var isLocal = _playerLobbyBar?.m_player?.IsLocal ?? false;
        var showForBot = (_playerLobbyBar?.m_player?.IsBot ?? false) && transform.parent.FindExactChild("BotCustomization_ClothesButton") != null;
        
        var enableClothesButton = (isLocal || showForBot)
            && GameStateManager.CurrentStateName != eGameStateName.Generating
            && GameStateManager.CurrentStateName != eGameStateName.InLevel
            && !GameStateManager.IsReady;

        if (isActiveAndEnabled || HasBlinkedIn)
        {
            gameObject.SetActive(isLocal || showForBot);
        }
        
        _item.SetButtonEnabled(enableClothesButton);
        _box.SetActive(enableClothesButton);
        
        var shouldShow = isLocal && AnyNewItems();
        _attentionIcon.SetActive(shouldShow);
    }

    private static bool AnyNewItems()
    {
        var items = PersistentInventoryManager.Current.m_vanityItemsInventory?.m_backednItems;

        if (items == null)
            return false;
        
        return items.ToArray().Any(item => (int)(item.flags & VanityItemFlags.Touched) == 0);
    }
}