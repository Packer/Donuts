using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

// From acidphantasm's botplacementsystem-csharp

namespace Donuts.Patches;

internal class IsPlayerEnemyPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotsGroup), nameof(BotsGroup.IsPlayerEnemy));
    }

    [PatchPrefix]
    private static bool PatchPrefix(BotsGroup __instance, IPlayer player, ref bool __result)
    {
        if (player.IsAI && player.Profile.Info.Settings.Role is WildSpawnType.pmcBEAR or WildSpawnType.pmcUSEC)
        {
            string leaderId = __instance.InitialBot.Profile.ProfileId;
            string thisBotId = player.Profile.ProfileId;
            
            if (__instance.InitialBot.BotsGroup.Contains(player.AIData.BotOwner))
            {
                __result = false;
                return false;
            }

            __result = true;
            return false;
        }

        return true;
    }
}