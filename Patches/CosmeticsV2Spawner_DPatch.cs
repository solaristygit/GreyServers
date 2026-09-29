using HarmonyLib;
using GorillaNetworking;
using System.Collections.Generic;
using UnityEngine;
using System;

[HarmonyPatch(typeof(CosmeticsV2Spawner_Dirty), "ProcessLoadOpInfos")]
public class CosmeticsV2Spawner_DPatch
{
    static bool Prefix(VRRig rig, string playfabId)
    {
        var traverse = Traverse.Create(typeof(CosmeticsV2Spawner_Dirty));
        var dict = traverse.Field("_gVRRigDatasIndexByRig").GetValue<Dictionary<VRRig, int>>();
        if (dict == null) return true;

        if (!dict.ContainsKey(rig))
        {
            if (rig.isOfflineVRRig)
            {
                Debug.LogWarning($"{rig.gameObject.name} not found in dict, registering...");
                dict[rig] = 0;
            }
            else
            {
                Debug.Log("[GreyServers] Remote rig not in local cosmetic cache; letting native handler continue.");
                return true;
            }
        }

        int index = dict[rig];
        var allLoadOpDicts = traverse.Field("_g_loadOpInfosForRigAndCosmeticIDDicts").GetValue() as Array;
        if (allLoadOpDicts == null || index >= allLoadOpDicts.Length || allLoadOpDicts.GetValue(index) == null)
        {
            Debug.LogWarning($"cosmetic data for rig index {index} is null; attempting force prepare...");
            traverse.Method("PrepareLoadOpInfos").GetValue();
            allLoadOpDicts = traverse.Field("_g_loadOpInfosForRigAndCosmeticIDDicts").GetValue() as Array;
        }

        if (allLoadOpDicts == null || index >= allLoadOpDicts.Length)
        {
            Debug.LogWarning($"cosmetic data for rig index {index} is unavailable; letting native handler continue.");
            return true;
        }

        var rigDict = allLoadOpDicts.GetValue(index) as System.Collections.IDictionary;
        if (rigDict == null)
        {
            Debug.LogError($"cosmetic data for rig index {index} is STILL null; letting native handler continue.");
            return true;
        }

        if (!rigDict.Contains(playfabId))
        {
            Debug.LogWarning($"cosmetic id {playfabId} not found in spawner data; letting native handler continue.");
            return true;
        }

        return true;
    }
}
