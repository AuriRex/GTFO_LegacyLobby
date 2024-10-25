using System;
using CellMenu;
using HarmonyLib;
using LegacyLobby.Extensions;
using UnityEngine;

namespace LegacyLobby;

[HarmonyWrapSafe]
[HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.OnEnable))]
public static class CM_PageRundown_New__OnEnable__Patch
{
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
        
        /* Pre R6 esque lobby shading (not perfect)*/
        try
        {
            Plugin.LoadImage(Resources.NoiseTexture, out var texture);
            
            if (texture == null)
            {
                throw new Exception("Image error idk, send help");
            }

            var mat = CM_Camera.Current.gameObject.GetComponent<UI_ScenePost>().mat;

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
        corners.SetActive(false);

        FixGUIX(playerRoot);
        
        // Move Ready Status text to the bottom
        __instance.m_statusText.transform.localPosition = new Vector3(-115, -630, 0);
        
        // TODO: Move them
        playerRoot.FindExactChild("BoosterImplantButtons").gameObject.SetActive(false);
        
        // bye bye "Weapons" text :D
        playerRoot.FindExactChild("Inventory_Header").localPosition = new Vector3(-3000, 0, 0);

        var clothes = __instance.m_clothesButton.transform;
        clothes.localPosition = new Vector3(clothes.localPosition.x, -700, clothes.localPosition.z);

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
    }
}