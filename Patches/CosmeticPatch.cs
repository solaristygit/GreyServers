﻿using GorillaNetworking;
using HarmonyLib;

namespace GreyServers.Patches
{
    [HarmonyPatch(typeof(CosmeticsController), "CompleteGetCosmeticsPlayFabCatalogData")]
    internal static class CosmeticPatch
    {
        private static void Postfix(CosmeticsController __instance)
        {
            if (__instance == null || __instance.allCosmetics == null)
                return;

            foreach (CosmeticsController.CosmeticItem item in __instance.allCosmetics)
            {
                if (string.IsNullOrEmpty(item.itemName))
                    continue;

                if (__instance.unlockedCosmetics.FindIndex(unlocked => unlocked.itemName == item.itemName) < 0)
                {
                    __instance.UnlockItem(item.itemName);
                }
            }

            __instance.UpdateWardrobeModelsAndButtons();
        }
    }
}
