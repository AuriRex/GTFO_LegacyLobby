using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Reflection;
using System.Text.Json;
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
    public const string VERSION = "1.3.1";

    public const string FORCE_DEFAULT_VANITY_GUID = "JarheadHME.ForceDefaultVanity";
    private const string CONFIG_FILE_NAME = $"{nameof(LegacyLobby)}_Config.json";
    
    internal static ManualLogSource L;

    internal static Config LLConfig = new();
    
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

        try
        {
            var configPath = Path.Combine(Paths.ConfigPath, CONFIG_FILE_NAME);
            if (File.Exists(configPath))
            {
                LLConfig = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath));
            }
            else
            {
                File.WriteAllText(configPath, JsonSerializer.Serialize(LLConfig, new JsonSerializerOptions()
                {
                    WriteIndented = true
                }));
            }
        }
        catch (Exception ex)
        {
            L.LogWarning("Config file loading failed.");
            L.LogError($"{ex.GetType().FullName}: {ex.Message}");
            L.LogWarning($"Stacktrace:\n{ex.StackTrace}");
            LLConfig = new();
        }
    }

    public static void LoadImage(byte[] bytes, out Texture2D tex)
    {
        tex = new Texture2D(2, 2);
        tex.LoadImage(bytes, false);

        tex.DontDestroyAndSetHideFlags();
    }
}