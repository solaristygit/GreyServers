using System.Collections.Generic;
using GorillaNetworking;
using HarmonyLib;

namespace GreyServers.Patches
{
    [HarmonyPatch(typeof(CosmeticsController), "CompleteGetCosmeticsPlayFabCatalogData")]
    internal static class CosmeticPatch
    {
        // Runs after the catalog finishes loading, so allCosmetics is populated
        private static void Postfix(CosmeticsController __instance)
        {
            if (__instance == null || __instance.allCosmetics == null)
                return;

            if (__instance.unlockedCosmetics == null)
                __instance.unlockedCosmetics = new List<CosmeticsController.CosmeticItem>();

            // HashSet makes this O(n) instead of calling FindIndex for every item
            var owned = new HashSet<string>();
            foreach (var u in __instance.unlockedCosmetics)
                owned.Add(u.itemName);

            bool changed = false;

            foreach (CosmeticsController.CosmeticItem item in __instance.allCosmetics)
            {
                if (string.IsNullOrEmpty(item.itemName) || owned.Contains(item.itemName))
                    continue;

                try
                {
                    __instance.UnlockItem(item.itemName);
                    owned.Add(item.itemName);
                    changed = true;
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogError($"[GreyServers] Failed to unlock {item.itemName}: {e}");
                }
            }

            if (changed)
                __instance.UpdateWardrobeModelsAndButtons();
        }
    }
}
