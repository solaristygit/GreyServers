
using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace GreyServers.Patches
{
    [HarmonyPatch(typeof(MothershipClientApiUnity), nameof(MothershipClientApiUnity.IsEnabled))]
    public static class MothershipPatch
    {
        private static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(
        typeof(GorillaTagScripts.SubscriptionManager),
        "InitializePersonalSubscriptionData"
    )]
    public static class SubscriptionManagerPatch
    {
        private static bool _alreadyResolved;

        private static bool Prefix()
        {
            if (_alreadyResolved)
                return false;

            Type manager = typeof(GorillaTagScripts.SubscriptionManager);

            FieldInfo initializedField =
                AccessTools.Field(manager, "_localSubscriptionDataInitialized");

            FieldInfo resolvedField =
                AccessTools.Field(manager, "_localSubscriptionDataResolved");

            FieldInfo initializedCallbackField =
                AccessTools.Field(manager, "OnLocalSubscriptionData");

            FieldInfo resolvedCallbackField =
                AccessTools.Field(manager, "OnLocalSubscriptionDataResolved");

            if (initializedField == null || resolvedField == null)
            {
                Debug.LogWarning(
                    "[GreyServers] SubscriptionManager fields not found."
                );

                return true;
            }

            try
            {
                bool alreadyResolved =
                    resolvedField.GetValue(null) is bool value && value;

                if (alreadyResolved)
                {
                    _alreadyResolved = true;
                    return false;
                }

                initializedField.SetValue(null, true);
                resolvedField.SetValue(null, true);

                _alreadyResolved = true;

                Debug.Log(
                    "[GreyServers] Local subscription resolved without Moth
