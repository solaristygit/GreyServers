using System;
using HarmonyLib;
using UnityEngine;

namespace GreyServers.Patches
{
    [HarmonyPatch(typeof(MothershipClientApiUnity), nameof(MothershipClientApiUnity.IsEnabled))]
    public static class MothershipPatch
    {
        public static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(GorillaTagScripts.SubscriptionManager), "InitializePersonalSubscriptionData")]
    public static class Patch_SubscriptionResolutionWithoutMothership
    {
        private static bool Prefix()
        {
            var initializedField = AccessTools.Field(typeof(GorillaTagScripts.SubscriptionManager), "_localSubscriptionDataInitialized");
            var resolvedField = AccessTools.Field(typeof(GorillaTagScripts.SubscriptionManager), "_localSubscriptionDataResolved");
            var initializedCallbackField = AccessTools.Field(typeof(GorillaTagScripts.SubscriptionManager), "OnLocalSubscriptionData");
            var resolvedCallbackField = AccessTools.Field(typeof(GorillaTagScripts.SubscriptionManager), "OnLocalSubscriptionDataResolved");

            if (initializedField == null || resolvedField == null || initializedCallbackField == null || resolvedCallbackField == null)
            {
                Debug.LogError("[GreyServers] Subscription fields were not found; leaving native initialization enabled.");
                return true;
            }

            if ((bool)resolvedField.GetValue(null))
            {
                return false;
            }

            initializedField.SetValue(null, true);
            resolvedField.SetValue(null, true);
            Debug.Log("[GreyServers] Mothership is disabled; resolving local subscriptions as inactive.");
            (initializedCallbackField.GetValue(null) as Action)?.Invoke();
            (resolvedCallbackField.GetValue(null) as Action)?.Invoke();
            return false;
        }
    }
}
