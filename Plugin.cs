using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace OcteapartyExtraSugar;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    public Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

    private static ConfigEntry<int> howMany;
    private static ConfigEntry<bool> mutateEventTemplates;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        howMany = Config.Bind("General",
                              "HowMany",
                              3,
                              "How many sugars per sugar cue");

        mutateEventTemplates = Config.Bind("General",
                                           "MutateEventTemplates",
                                           true,
                                           "Add \"amount\" parameter to the Octeaparty sugar event in editor");

        harmony.PatchAll();
        if (mutateEventTemplates.Value)
        {
            OcteapartyEventTemplates.templates[2].properties["amount"] = -1;
            OcteapartyFireEventTemplates.templates[2].properties["amount"] = -1;
        }
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

        static readonly MethodInfo gameNameGetter = AccessTools.PropertyGetter(typeof(OcteapartyScript), "GameName");
        static string GameName(OcteapartyScript obj) => (string)gameNameGetter.Invoke(obj, null);

        static readonly MethodInfo isMixtapeOrCustomGetter = AccessTools.PropertyGetter(typeof(GameplayScript), "IsMixtapeOrCustom");
        static bool IsMixtapeOrCustom(GameplayScript obj) => (bool)isMixtapeOrCustomGetter.Invoke(obj, null);

        static readonly AccessTools.FieldRef<InputManager, Dictionary<Action, List<Target>>> targetsRef =
        AccessTools.FieldRefAccess<InputManager, Dictionary<Action, List<Target>>>("targets");

        static void Postfix(OcteapartyScript __instance, ref int __result, bool play, Entity[] entities)
        {
            bool needsReplay = BeginInternalPostfix(__instance, entities);
            if (needsReplay)
            {
                __result = Replay(__instance, play);
            }
        }

        static bool BeginInternalPostfix(OcteapartyScript __instance, Entity[] entities)
        {
            if (IsMixtapeOrCustom(__instance))
            {
                return BeginInternalPostfixRemix(__instance, entities);
            }
            else
            {
                return BeginInternalPostfixNonRemix(__instance);
            }
        }

        static bool BeginInternalPostfixNonRemix(OcteapartyScript __instance)
        {
            List<float> mods = [];
            foreach (KeyValuePair<float, int> entry in sugarTargetToIndexRef(__instance))
            {
                if (entry.Value == 0)
                {
                    mods.Add(entry.Key);
                }
            }
            foreach (float n in mods)
            {
                AddExtraSugar(__instance, n, howMany.Value);
            }
            return mods.Count > 0;
        }

        static bool BeginInternalPostfixRemix(OcteapartyScript __instance, Entity[] entities)
        {
            bool result = false;
            string gameName = GameName(__instance);
            foreach (Entity entity in entities)
            {
                string[] args = entity.dataModel.Split(['/'], 2);
                if (args[0] == gameName && args[1] == "sugar")
                {
                    result = true;
                    if (entity.dynamicData.ContainsKey("amount"))
                    {
                        int amount = entity.GetInt("amount");
                        AddExtraSugar(__instance, entity.beat + 3, amount >= 0 ? amount : howMany.Value);
                    }
                    else
                    {
                        AddExtraSugar(__instance, entity.beat + 3, howMany.Value);
                    }
                }
            }
            return result;
        }

        static void RemoveSugar(OcteapartyScript __instance, float when)
        {
            sugarTargetToIndexRef(__instance).Remove(when);
            sugarTargetToTeacupRef(__instance).Remove(when);
            sugarTargetToSmallTeacupRef(__instance)?.Remove(when);
            targetsRef(inputManagerRef(__instance))[Action.Primary].Remove(when);
        }

        static void AddSugar(OcteapartyScript __instance, float when, float oldWhen, int index)
        {
            sugarTargetToIndexRef(__instance)[when] = index;
            if (sugarTargetToTeacupRef(__instance).ContainsKey(oldWhen))
            {
                sugarTargetToTeacupRef(__instance)[when] = sugarTargetToTeacupRef(__instance)[oldWhen];
            }
            if (sugarTargetToSmallTeacupRef(__instance) != null && sugarTargetToSmallTeacupRef(__instance).ContainsKey(oldWhen))
            {
                sugarTargetToSmallTeacupRef(__instance)[when] = sugarTargetToSmallTeacupRef(__instance)[oldWhen];
            }
            inputManagerRef(__instance).AddTarget(Action.Primary, when);
        }

        static void AddExtraSugar(OcteapartyScript __instance, float when, int amount)
        {
            if (amount % 2 == 0)
            {
                RemoveSugar(__instance, when + 1);
            }
            if (amount < 2)
            {
                RemoveSugar(__instance, when);
                RemoveSugar(__instance, when + 2);
            }
            else
            {
                for (int i = 1; i < amount - 1; i++)
                {
                    float sugarIndex = i * 2f / (amount - 1);
                    if (sugarIndex == 1f)
                    {
                        continue;
                    }
                    AddSugar(__instance, when + sugarIndex, when, sugarIndex < 1 ? 0 : 1);
                }
            }
        }

        static int Replay(OcteapartyScript __instance, bool play)
        {
            var inputManager = inputManagerRef(__instance);
            inputManager.autoplay.Stop();
            inputManager.autoplay.Clear();
            inputManager.autoplay.ClearCallbacks();
            return totalRef(__instance) = inputManager.Play(!play, play);
        }
    }
}
