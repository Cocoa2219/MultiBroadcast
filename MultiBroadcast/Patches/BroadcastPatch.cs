using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using CommandSystem.Commands.RemoteAdmin.Broadcasts;
using Exiled.API.Features;
using HarmonyLib;
using NorthwoodLib.Pools;
using Utils;
using BroadcastCommand = CommandSystem.Commands.RemoteAdmin.Broadcasts.BroadcastCommand;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MultiBroadcast.Patches;

[HarmonyPatch(typeof(ClearBroadcastCommand), nameof(ClearBroadcastCommand.Execute))]
internal class ClearBroadcastPatch
{
    public static bool Prefix(ArraySegment<string> arguments, ICommandSender sender, ref string response,
        ref bool __result)
    {
        if (!Plugin.Instance.Config.ReplaceBroadcastCommand)
        {
            Log.Info("Broadcast command is not replaced");
            return true;
        }

        if (!sender.CheckPermission(PlayerPermissions.Broadcasting, out response))
        {
            __result = false;
            return false;
        }

        ServerLogs.AddLog(ServerLogs.Modules.Administrative, sender.LogName + " cleared all broadcasts.",
            ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
        API.MultiBroadcast.ClearAllBroadcasts();
        response = "All broadcasts cleared.";
        __result = true;
        return false;
    }
}

[HarmonyPatch(typeof(BroadcastCommand), nameof(BroadcastCommand.OnExecute))]
internal class BroadcastPatch
{
    public static bool Prefix(BroadcastCommand __instance, ArraySegment<string> arguments, ICommandSender sender,
        ref string response, ref bool __result)
    {
        if (!Plugin.Instance.Config.ReplaceBroadcastCommand)
        {
            Log.Info("Broadcast command is not replaced");
            return true;
        }

        var text = arguments.At(0);
        if (!__instance.IsValidDuration(text, out var time))
        {
            response = string.Concat("Invalid argument for duration: ", text, " Usage: ", arguments.At(0), " ",
                __instance.DisplayCommandUsage());
            __result = false;
            return false;
        }

        var flag = __instance.HasInputFlag(arguments.At(1), out var broadcastFlags, arguments.Count);
        var text2 = RAUtils.FormatArguments(arguments, flag ? 2 : 1);
        var bcs = API.MultiBroadcast.AddMapBroadcast(time, text2);
        var ids = bcs?.Select(bc => bc.Id).ToList();
        ServerLogs.AddLog(ServerLogs.Modules.Administrative,
            $"{sender.LogName} broadcast text \"{text2}\". Duration: {text} seconds. Broadcast Flag: {broadcastFlags}.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
        response = bcs == null ? "Error on adding broadcast" : $"Added broadcast for all players with id {string.Join(", ", ids)}";
        __result = true;
        return false;
    }
}

[HarmonyPatch(typeof(PlayerBroadcastCommand), nameof(PlayerBroadcastCommand.OnExecute))]
internal class PlayerBroadcastPatch
{
    public static bool Prefix(PlayerBroadcastCommand __instance, ArraySegment<string> arguments, ICommandSender sender,
        ref string response, ref bool __result)
    {
        if (!Plugin.Instance.Config.ReplaceBroadcastCommand)
        {
            Log.Info("Broadcast command is not replaced");
            return true;
        }

        var list = RAUtils.ProcessPlayerIdOrNamesList(arguments, 0, out var array);
        if (array == null || array.Length < __instance.MinimumArguments)
        {
            response = "To execute this command provide at least 3 arguments!\nUsage: " + arguments.At(0) + " " +
                       __instance.DisplayCommandUsage();
            __result = false;
            return false;
        }

        var text = array[0];
        if (!__instance.IsValidDuration(text, out var num))
        {
            response = string.Concat("Invalid argument for duration: ", text, " Usage: ", arguments.At(0), " ",
                __instance.DisplayCommandUsage());
            return false;
        }

        var flag = __instance.HasInputFlag(array[1], out var broadcastFlags, array.Length);
        var text2 = RAUtils.FormatArguments(array.Segment(1), flag ? 1 : 0);
        var stringBuilder = StringBuilderPool.Shared.Rent();
        var num2 = 0;
        var ids = ListPool<int>.Shared.Rent();
        var ls = ListPool<Player>.Shared.Rent();

        foreach (var referenceHub in list)
        {
            if (num2++ != 0) stringBuilder.Append(", ");
            stringBuilder.Append(referenceHub.LoggedNameFromRefHub());
            var player = Player.Get(referenceHub);
            var broadcast = API.MultiBroadcast.AddPlayerBroadcast(player, num, text2);
            ids.Add(broadcast.Id);
            ls.Add(player);
        }

        ServerLogs.AddLog(ServerLogs.Modules.Administrative,
            $"{sender.LogName} broadcast text \"{text2}\" to {num2} players. Duration: {num} seconds. Affected players: {stringBuilder}. Broadcast Flag: {broadcastFlags}.",
            ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
        StringBuilderPool.Shared.Return(stringBuilder);
        response = num2 >= 2
            ? $"Added broadcast for {num2} players with id {string.Join(", ", ids)}"
            : "Added broadcast for " + ls[0].Nickname + " with id " + ids[0];
        ListPool<Player>.Shared.Return(ls);
        ListPool<int>.Shared.Return(ids);
        __result = true;
        return false;
    }
}

// [HarmonyPatch(typeof(Player), nameof(Player.Broadcast), [typeof(ushort), typeof(string), typeof(Broadcast.BroadcastFlags), typeof(bool)])]
// public class ExiledBroadcastPatch
// {
//     public static bool Prefix(Player __instance, ushort duration, string message, Broadcast.BroadcastFlags type, bool shouldClearPrevious)
//     {
//         if (!Plugin.Instance.Config.CompatibilityMode)
//         {
//             return true;
//         }
//
//         if (type != Broadcast.BroadcastFlags.Normal) return true;
//         if (shouldClearPrevious) API.MultiBroadcast.ClearPlayerBroadcasts(__instance);
//
//         API.MultiBroadcast.AddPlayerBroadcast(__instance, duration, message);
//         return false;
//     }
// }
//
// [HarmonyPatch(typeof(Player), nameof(Player.Broadcast), [typeof(Exiled.API.Features.Broadcast), typeof(bool)])]
// public class ExiledBroadcastPatch2
// {
//     public static bool Prefix(Player __instance, Exiled.API.Features.Broadcast broadcast, bool shouldClearPrevious)
//     {
//         if (!Plugin.Instance.Config.CompatibilityMode)
//         {
//             return true;
//         }
//
//         if (broadcast.Type != Broadcast.BroadcastFlags.Normal) return true;
//
//         if (!broadcast.Show) return false;
//         if (shouldClearPrevious) API.MultiBroadcast.ClearPlayerBroadcasts(__instance);
//
//         API.MultiBroadcast.AddPlayerBroadcast(__instance, broadcast.Duration, broadcast.Content);
//         return false;
//     }
// }