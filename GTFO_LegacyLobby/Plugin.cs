using System.Diagnostics.CodeAnalysis;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Reflection;
using Il2CppInterop.Runtime.Injection;
using LegacyLobby.Components;
using LegacyLobby.Extensions;
using UnityEngine;

[assembly: AssemblyVersion(LegacyLobby.Plugin.VERSION)]
[assembly: AssemblyFileVersion(LegacyLobby.Plugin.VERSION)]
[assembly: AssemblyInformationalVersion(LegacyLobby.Plugin.VERSION)]

namespace LegacyLobby;

[BepInPlugin(GUID, NAME, VERSION)]
[BepInDependency(FORCE_DEFAULT_VANITY_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class Plugin : BasePlugin
{
    public const string GUID = "dev.aurirex.gtfo.legacylobby";
    public const string NAME = "LegacyLobby";
    public const string VERSION = "1.1.0";

    public const string FORCE_DEFAULT_VANITY_GUID = "JarheadHME.ForceDefaultVanity";
    
    internal static ManualLogSource L;

    private static Harmony _harmony;
    
    internal static bool IsForceDefaultVanityInstalled { get; private set; }

    public override void Load()
    {
        L = Log;
        Log.LogMessage($"Loading {NAME}");

        IsForceDefaultVanityInstalled = IL2CPPChainloader.Instance.Plugins.ContainsKey(FORCE_DEFAULT_VANITY_GUID);

        ClassInjector.RegisterTypeInIl2Cpp<ClothesButton>();
        
        _harmony = new Harmony(GUID);
        _harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
    
    public static void LoadImage(byte[] bytes, out Texture2D tex)
    {
        tex = new Texture2D(2, 2);
        tex.LoadImage(bytes, false);

        tex.DontDestroyAndSetHideFlags();
    }
}