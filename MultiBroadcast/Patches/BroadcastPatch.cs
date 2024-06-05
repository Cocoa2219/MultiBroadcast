using System;
using CommandSystem;
using CommandSystem.Commands.RemoteAdmin.Broadcasts;
using Exiled.API.Features;
using HarmonyLib;
using NorthwoodLib.Pools;
using Utils;
using BroadcastCommand = CommandSystem.Commands.RemoteAdmin.Broadcasts.BroadcastCommand;

namespace MultiBroadcast.Patches;

[HarmonyPatch(typeof(ClearBroadcastCommand), nameof(ClearBroadcastCommand.Execute))]
internal class ClearBroadcastPatch
{
    public static bool Prefix(ArraySegment<string> arguments, ICommandSender sender, out string response,
        ref bool __result)
    {
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
        out string response, ref bool __result)
    {
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
        var ids = API.MultiBroadcast.AddMapBroadcast(time, text2);
        ServerLogs.AddLog(ServerLogs.Modules.Administrative,
            $"{sender.LogName} broadcast text \"{text2}\". Duration: {text} seconds. Broadcast Flag: {broadcastFlags}.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
        response = ids == null ? "Error on adding broadcast" : $"Added broadcast for all players with id {string.Join(", ", ids)}";
        __result = true;
        return false;
    }
}

[HarmonyPatch(typeof(PlayerBroadcastCommand), nameof(PlayerBroadcastCommand.OnExecute))]
internal class PlayerBroadcastPatch
{
    public static bool Prefix(PlayerBroadcastCommand __instance, ArraySegment<string> arguments, ICommandSender sender,
        out string response, ref bool __result)
    {
        var list = RAUtils.ProcessPlayerIdOrNamesList(arguments, 0, out var array);
        if (array == null || array.Length < __instance.MinimumArguments)
        {
            response = "To execute this command provide at least 3 arguments!\nUsage: " + arguments.At(0) + " " +
                       __instance.DisplayCommandUsage();
            __result = false;
            return false;
        }

        var text = array[0];
        ushort num;
        if (!__instance.IsValidDuration(text, out num))
        {
            response = string.Concat("Invalid argument for duration: ", text, " Usage: ", arguments.At(0), " ",
                __instance.DisplayCommandUsage());
            return false;
        }

        var flag = __instance.HasInputFlag(array[1], out var broadcastFlags, array.Length);
        var text2 = RAUtils.FormatArguments(array.Segment(1), flag ? 1 : 0);
        var stringBuilder = StringBuilderPool.Shared.Rent();
        var num2 = 0;
        int[] ids = [];
        foreach (var referenceHub in list)
        {
            if (num2++ != 0) stringBuilder.Append(", ");
            stringBuilder.Append(referenceHub.LoggedNameFromRefHub());
            ids.AddItem(API.MultiBroadcast.AddPlayerBroadcast(Player.Get(referenceHub), num, text2));
        }

        ServerLogs.AddLog(ServerLogs.Modules.Administrative,
            $"{sender.LogName} broadcast text \"{text2}\" to {num2} players. Duration: {num} seconds. Affected players: {stringBuilder}. Broadcast Flag: {broadcastFlags}.",
            ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
        StringBuilderPool.Shared.Return(stringBuilder);
        response = num2 > 2
            ? $"Added broadcast for {num2} players with id {string.Join(", ", ids)}"
            : "Added broadcast for " + stringBuilder + " with id " + string.Join(", ", ids);
        __result = true;
        return false;
    }
}