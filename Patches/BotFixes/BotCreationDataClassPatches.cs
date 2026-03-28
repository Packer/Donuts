using Donuts.Utils;
using EFT;
using HarmonyLib;
using JetBrains.Annotations;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Donuts.Patches.BotFixes;

[UsedImplicitly]
public class BotCreationDataClassPatches
{
	private static readonly Type s_targetType = typeof(BotCreationDataClass);
	
	/*
	 * As of SPT 3.11.X
	 * ___list_0 is a List<SpawnPointData> where SpawnPointData has a constructor signature:
	 *	public GClass660(Vector3 pos, int corePointId, bool isUsed)
	 *	
	 * SpawnPointData also has 3 fields:
	 *	public Vector3 position;
	 *	public bool alreadyUsedToSpawn
	 *	public int CorePointId;
	 *
	 * Purpose of this patch:
	 * Donuts currently only adds one spawn position to the BotCreationDataClass;
	 * The current method performs unnecessary code executions after the first GetPosition() call.
	 */
	[UsedImplicitly]
	public class GetPositionPatch : ModulePatch
	{
		protected override MethodBase GetTargetMethod() =>
			AccessTools.Method(s_targetType, nameof(BotCreationDataClass.GetPosition));
		
		[PatchPrefix]
		private static bool PatchPrefix(ref GClass682 __result, List<GClass682> ___List_0)
		{
            __result = ___List_0.RandomElement();
			return false;
		}
	}
	
	// TODO: Move this patch to its own file
	[UsedImplicitly]
	public class ChooseProfilePatch : ModulePatch
	{
		protected override MethodBase GetTargetMethod() =>
			AccessTools.Method(typeof(BotProfileDataClass), nameof(BotProfileDataClass.ChooseProfile));

		[PatchPrefix]
		private static bool PatchPrefix(
            BotProfileDataClass __instance,
			ref Profile __result,
			List<Profile> profiles2Select,
			bool withDelete)
		{
			int profileCount = profiles2Select.Count;
			List<Profile> list = new(profileCount);
			
			for (var i = 0; i < profileCount; i++)
			{
				Profile profile = profiles2Select[i];
				ProfileInfoSettingsClass profileSettings = profile.Info.Settings;
				
				if (profileSettings.Role == __instance.WildSpawnType_0 &&
					profileSettings.BotDifficulty == __instance.BotDifficulty_0)
				{
					list.Add(profile);
				}
			}
			
			if (list.Count == 0)
			{
				__result = null;
				return false;
			}
			
			Profile chosenProfile = list.PickRandom();
			
			if (withDelete)
			{
				profiles2Select.Remove(chosenProfile);
			}
			
			__result = chosenProfile;
			return false;
		}
	}
}