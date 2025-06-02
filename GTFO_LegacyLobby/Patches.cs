using System;
using System.Reflection;
using CellMenu;
using HarmonyLib;
using System.IO;
using LegacyLobby.Components;
using LegacyLobby.Extensions;
using UnityEngine;

namespace LegacyLobby;

[HarmonyWrapSafe]
[HarmonyPatch(typeof(CM_PageLoadout), nameof(CM_PageLoadout.Setup))]
[HarmonyPriority(Priority.Last)]
public static class CM_PageLoadout__Setup__Patch
{
    public static void Postfix(CM_PageLoadout __instance)
    {
        var decorText = __instance.m_readyButtonAlign.FindExactChild("DecorText");
        decorText.gameObject.SetActive(true);

        var decorTextDropButton = UnityEngine.Object.Instantiate(decorText, __instance.m_dropButton.transform);
        decorTextDropButton.localPosition = Vector3.zero;
        
        var decorTextReadyButton = UnityEngine.Object.Instantiate(decorText, __instance.m_readyButton.transform);
        decorTextReadyButton.localPosition = Vector3.zero;
        
        decorText.gameObject.SetActive(false);
    }
}

[HarmonyWrapSafe]
[HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.OnEnable))]
public static class CM_PageRundown_New__OnEnable__Patch
{
    internal static Material originalScenePostMat;
    private static readonly int NOISE_MASK = Shader.PropertyToID("_NoiseMask");
    private static readonly int BACKGROUND_DESAT = Shader.PropertyToID("_BackgroundDesat");
    private static readonly int BACKGROUND_COLOR = Shader.PropertyToID("_BackgroundColor");
    private static readonly int DISTORTION_MIN = Shader.PropertyToID("_DistortionMin");
    private static readonly int DISTORTION_MAX = Shader.PropertyToID("_DistortionMax");
    private static readonly int SCANLINE_OVERLAY = Shader.PropertyToID("_ScanlineOverlay");

    public static void Postfix(CM_PageRundown_New __instance)
    {
        if (__instance == null)
            return;
        
        if (CM_Camera.Current == null)
            return;

        var dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

        var overrideNoisePath = Path.Combine(dllPath, "Noise.png");

        var imageBytes = Resources.NoiseTexture;
        
        if (File.Exists(overrideNoisePath))
        {
            Plugin.L.LogWarning($"Using custom noise texture: {overrideNoisePath}");
            imageBytes = File.ReadAllBytes(overrideNoisePath);
        }
        
        /* Pre R6 esque lobby shading (not perfect)*/
        try
        {
            Plugin.LoadImage(imageBytes, out var texture);
            
            if (texture == null)
            {
                throw new Exception("Image error idk, send help");
            }

            var mat = CM_Camera.Current.gameObject.GetComponent<UI_ScenePost>().mat;

            originalScenePostMat = new Material(mat);
            
            // Red Channel = Main Desat applicator, Green Channel = Secondary with different scroll speed?
            mat.SetTexture(NOISE_MASK, texture); // Texture2D.whiteTexture
            mat.SetFloat(BACKGROUND_DESAT, 5);
            mat.SetColor(BACKGROUND_COLOR, new Color(0.55f, 0.7f, 0.73f, 0.2f));
            mat.SetFloat(DISTORTION_MIN, 0.01f);
            mat.SetFloat(DISTORTION_MAX, 0.03f);
            mat.SetFloat(SCANLINE_OVERLAY, 0);
        }
        catch (Exception ex)
        {
            Plugin.L.LogError("OOPS");
            Plugin.L.LogError(ex);
        }
    }
}

[HarmonyWrapSafe]
[HarmonyPatch(typeof(CM_PlayerLobbyBar), nameof(CM_PlayerLobbyBar.SetupFromPage))]
public static class CM_PlayerLobbyBar__SetupFromPage__Patch
{
    private const float BASE_POS = -166f;
    private const float OFFSET = 160f;
    
    public static void Postfix(CM_PlayerLobbyBar __instance)
    {
        if (__instance == null)
            return;

        var playerRoot = __instance.m_hasPlayerRoot.transform;
        
        var corners = __instance.m_corners;
        corners.transform.SetParent(playerRoot, true);
        // Fix sorting layer (For some reason both of those are on the 'MenuPopupSprite' layer)
        corners.transform.FindExactChild("CornerBL").GetComponent<SpriteRenderer>().sortingLayerName = "Default";
        corners.transform.FindExactChild("CornerBR").GetComponent<SpriteRenderer>().sortingLayerName = "Default";
        corners.SetActive(false);

        FixGUIX(playerRoot);
        
        // Move Ready Status text to the bottom
        if (!Plugin.LLConfig.DefaultReadyTextPosition)
        {
            __instance.m_statusText.transform.localPosition = new Vector3(-15, -620, 0);
            __instance.m_statusText.m_textAlignment = TMPro.TextAlignmentOptions.MidlineLeft;
        }

        // bye bye "Weapons" text :D
        playerRoot.FindExactChild("Inventory_Header").localPosition = new Vector3(-5000, -5000, 0);

        var clothes = __instance.m_clothesButton.transform;
        clothes.localPosition = new Vector3(clothes.localPosition.x, -700 * 100, clothes.localPosition.z);

        var slotMain = __instance.m_slotStandardAlign;
        var slotSpecial = __instance.m_slotSpecialAlign;
        var slotTool = __instance.m_slotClassAlign;
        var slotMelee = __instance.m_slotMeleeAlign;
        
        var mainX = slotMain.localPosition.x;
        var mainZ = slotMain.localPosition.z;

        slotMain.localPosition = new Vector3(mainX, BASE_POS, mainZ);
        slotSpecial.localPosition = new Vector3(mainX, BASE_POS - OFFSET * 1, mainZ);
        slotTool.localPosition = new Vector3(mainX, BASE_POS - OFFSET * 2, mainZ);
        slotMelee.localPosition = new Vector3(mainX, BASE_POS - OFFSET * 3, mainZ);

        var permButtonPos = __instance.m_permissionButton.transform.localPosition;
        
        __instance.m_permissionButton.transform.localPosition = new Vector3(permButtonPos.x, 470f, permButtonPos.z);
    }

    private static void FixGUIX(Transform playerRoot)
    {
        var guix = playerRoot.FindExactChild("GuixPillar");

        guix.localPosition = new Vector3(25, -25, 0);
        
        guix.gameObject.SetActive(false);
        guix.FindExactChild("BrainScan (1)").gameObject.SetActive(true);
        guix.FindExactChild("lobby-brainxray-top").gameObject.SetActive(true);
        
        var corners3 = guix.FindExactChild("Corners (3)");

        var tr = corners3.FindExactChild("CornerTR");
        tr.localPosition = new Vector3(tr.localPosition.x, 132, tr.localPosition.z);
        
        var tl = corners3.FindExactChild("CornerTL");
        tl.localPosition = new Vector3(tl.localPosition.x, 132, tl.localPosition.z);
    }
}

[HarmonyWrapSafe]
// /* INLINED */[HarmonyPatch(typeof(CM_BoosterImplantSlotHolder), nameof(CM_BoosterImplantSlotHolder.SetupSlotItems))]
[HarmonyPatch(typeof(CM_BoosterImplantSlotHolder), nameof(CM_BoosterImplantSlotHolder.UpdateBoosterImplantInventory))]
public static class CM_BoosterImplantSlotHolder__SetupSlotItems__Patch
{
    public const string CLOTHES_BUTTON_NAME = "Custom_Clothes_Button_Booster_Style";
    public static void Postfix(CM_BoosterImplantSlotHolder __instance)
    {
        var forceDefaultVanity = Plugin.IsForceDefaultVanityInstalled;
        
        var c = forceDefaultVanity ? 2 : 3;
        foreach (var kvp in __instance.m_categorySlots)
        {
            var implantItem = kvp.Value;
            
            implantItem.transform.localPosition = new Vector3(400, -10 + 160 * c, 0);

            c--;
        }

        if (forceDefaultVanity)
            return;
        
        var clothesButton = ClothesButton.GetOrSetupFromLobbyBar(__instance.GetComponentInParent<CM_PlayerLobbyBar>());
        
        clothesButton?.CheckUpdateAndToggleState();
    }
}

[HarmonyWrapSafe]
[HarmonyPatch(typeof(CM_ExtraCamera), nameof(CM_ExtraCamera.CheckInitialized))]
public static class CM_ExtraCamera_CheckInitialized_Patch
{
    public static void Postfix()
    {
        CM_ExtraCamera.Current.GetComponent<UI_ScenePost>().mat = CM_PageRundown_New__OnEnable__Patch.originalScenePostMat;
    }
}

[HarmonyWrapSafe]
[HarmonyPatch(typeof(CM_PlayerLobbyBar._DoPlayIntro_d__111), nameof(CM_PlayerLobbyBar._DoPlayIntro_d__111.MoveNext))]
public static class CM_PlayerLobbyBar__DoIntro__Patch
{
    public static void Postfix(CM_PlayerLobbyBar._DoPlayIntro_d__111 __instance)
    {
        Plugin.L.LogDebug($"Hi from enumerator patch uwu - Blinking in GUIX & Corner UI elements.");
        
        if (__instance.__1__state != 1)
            return;

        var lobbyBar = __instance.__4__this;

        var playerRoot = lobbyBar.m_hasPlayerRoot.transform;
        var guix = playerRoot.FindExactChild("GuixPillar").gameObject;
        guix.SetActive(false);
        
        CoroutineManager.BlinkIn(guix, 0.2f * 5.0f + 0.5f);
        CoroutineManager.BlinkIn(lobbyBar.m_corners, 0.2f * 5.0f + 0.6f);

        if (!(lobbyBar.m_player?.IsLocal ?? false))
            return;
        
        var clothesButton = ClothesButton.GetOrSetupFromLobbyBar(lobbyBar);
        
        if (clothesButton != null)
            CoroutineManager.BlinkIn(clothesButton.gameObject, 0.2f * 5.0f + 0.1f);
    }
}