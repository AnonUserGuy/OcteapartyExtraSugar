using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace OcteapartyExtraSugar;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    public Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        harmony.PatchAll();

    }

    [HarmonyPatch(typeof(OcteapartyScript), "BeginInternal")]
    private static class OcteapartyScriptBeginInternalPatch
    {
        static readonly AccessTools.FieldRef<OcteapartyScript, Dictionary<float, int>> sugarTargetToIndexRef =
        AccessTools.FieldRefAccess<OcteapartyScript, Dictionary<float, int>>("sugarTargetToIndex");

        static readonly AccessTools.FieldRef<OcteapartyScript, Dictionary<float, Animator>> sugarTargetToTeacupRef =
        AccessTools.FieldRefAccess<OcteapartyScript, Dictionary<float, Animator>>("sugarTargetToTeacup");

        static readonly AccessTools.FieldRef<OcteapartyScript, Dictionary<float, Animator>> sugarTargetToSmallTeacupRef =
        AccessTools.FieldRefAccess<OcteapartyScript, Dictionary<float, Animator>>("sugarTargetToSmallTeacup");

        static readonly AccessTools.FieldRef<OcteapartyScript, InputManager> inputManagerRef =
        AccessTools.FieldRefAccess<OcteapartyScript, InputManager>("inputManager");

        static readonly AccessTools.FieldRef<OcteapartyScript, int> totalRef =
        AccessTools.FieldRefAccess<OcteapartyScript, int>("total");
        static void Postfix(OcteapartyScript __instance, ref int __result, bool play)
        {
            bool needsReplay = AddExtraSugar(__instance);
            if (needsReplay)
            {
                __result = Replay(__instance, play);
            }
        }

        static bool AddExtraSugar(OcteapartyScript __instance)
        {
            var sugarTargetIndex = sugarTargetToIndexRef(__instance);
            var sugarTargetToTeacup = sugarTargetToTeacupRef(__instance);
            var sugarTargetToSmallTeacup = sugarTargetToSmallTeacupRef(__instance);
            var inputManager = inputManagerRef(__instance);

            List<float> mods = new List<float>();

            foreach (KeyValuePair<float, int> entry in sugarTargetIndex)
            {
                if (entry.Value == 1)
                {
                    mods.Add(entry.Key);
                }
            }
            foreach (float n in mods)
            {
                float n0 = n - 0.5f;
                float n2 = n + 0.5f;
                sugarTargetIndex[n0] = 0;
                sugarTargetIndex[n2] = 1;
                if (sugarTargetToTeacup.ContainsKey(n))
                {
                    sugarTargetToTeacup[n0] = sugarTargetToTeacup[n];
                    sugarTargetToTeacup[n2] = sugarTargetToTeacup[n];
                }
                if (sugarTargetToSmallTeacup != null && sugarTargetToSmallTeacup.ContainsKey(n))
                {
                    sugarTargetToSmallTeacup[n0] = sugarTargetToSmallTeacup[n];
                    sugarTargetToSmallTeacup[n2] = sugarTargetToSmallTeacup[n];
                }
                inputManager.AddTarget(Action.Primary, n0);
                inputManager.AddTarget(Action.Primary, n2);
            }
            return mods.Count > 0;
        }

        static int Replay(OcteapartyScript __instance, bool play)
        {
            var inputManager = inputManagerRef(__instance);
            inputManager.autoplay.Clear();
            inputManager.autoplay.ClearCallbacks();
            return totalRef(__instance) = inputManager.Play(!play, play);
        }
    }
}
