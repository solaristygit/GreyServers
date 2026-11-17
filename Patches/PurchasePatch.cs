using GorillaNetworking;
using HarmonyLib;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace GreyServers.Patches
{
    [HarmonyPatch(typeof(CosmeticsController), "PurchaseItem")]
    public static class Patch_PurchaseItem
    {
        private static bool Prefix(CosmeticsController __instance)
        {
            if (__instance == null)
                return false;

            CosmeticsController controller = __instance;

            if (controller.itemToBuy.isNullItem ||
                controller.itemToBuy.itemName == controller.nullItem.itemName)
            {
                return false;
            }

            string id = controller.itemToBuy.itemName;
            var cosmetic = controller.itemToBuy;

            var request = new ExecuteCloudScriptRequest
            {
                FunctionName = "buyFreeCosmetic",
                FunctionParameter = new { itemID = id },
                GeneratePlayStreamEvent = true
            };

            PlayFabClientAPI.ExecuteCloudScript(
                request,
                result =>
                {
                    // CloudScript can "succeed" at the HTTP level but still throw inside the script
                    if (result.Error != null)
                    {
                        Debug.LogError($"[GreyServers] CloudScript error for {id}: {result.Error.Message}");
                        controller.itemToBuy = controller.nullItem;
                        return;
                    }

                    if (!controller.allCosmeticsDict.ContainsKey(id))
                        controller.allCosmeticsDict.Add(id, cosmetic);

                    // This is the part that makes it appear without restarting:
                    // add to the unlocked list the wardrobe actually reads
                    if (controller.unlockedCosmetics.FindIndex(c => c.itemName == id) < 0)
                        controller.UnlockItem(id);

                    controller.itemToBuy = controller.nullItem;
                    controller.UpdateWardrobeModelsAndButtons();
                },
                error =>
                {
                    Debug.LogError($"[GreyServers] Purchase failed for {id}: {error.GenerateErrorReport()}");
                    controller.itemToBuy = controller.nullItem;
                }
            );

            return false;
        }
    }
}
