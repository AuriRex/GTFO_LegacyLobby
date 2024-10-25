﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LegacyLobby.Extensions;

internal static class ExtensionMethods
{
    public static IEnumerable<Transform> Children(this Transform self)
    {
        for (int i = 0; i < self.childCount; i++)
        {
            yield return self.GetChild(i);
        }
    }

    public static Transform FindExactChild(this Transform self, string name)
    {
        return self.Children().FirstOrDefault(c => c.name == name);
    }
    
    public static T GetOrAddComponent<T>(this GameObject self) where T : Component
    {
        var comp = self.GetComponent<T>();

        if(comp == null)
        {
            comp = self.AddComponent<T>();
        }

        return comp;
    }

    public static void DontDestroyAndSetHideFlags(this UnityEngine.Object obj)
    {
        UnityEngine.Object.DontDestroyOnLoad(obj);
        obj.hideFlags = HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
    }
}
