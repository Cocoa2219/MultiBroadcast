using System;
using System.Collections.Generic;
using System.Text;
using CommandSystem;
using CommandSystem.Commands.RemoteAdmin.Broadcasts;
using Exiled.API.Features;
using HarmonyLib;
using NorthwoodLib.Pools;
using Utils;
using BroadcastCommand = CommandSystem.Commands.RemoteAdmin.Broadcasts.BroadcastCommand;

namespace MultiBroadcast.Patches;

[HarmonyPatch(typeof(ClearBroadcastCommand), nameof(ClearBroadcastCommand.Execute))]
public class ClearBroadcastPatch
{
    public static bool Prefix(ArraySegment<string> arguments, ICommandSender sender, out string response, ref bool __result)
    {
        if (!sender.CheckPermission(PlayerPermissions.Broadcasting, out response))
        {
            __result = false;
            return false;
        }
        ServerLogs.AddLog(ServerLogs.Modules.Administrative, sender.LogName + " cleared all broadcasts.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging, false);
        API.MultiBroadcast.ClearBroadcasts();
        response = "All broadcasts cleared.";
        __result = true;
        return false;
    }
}

[HarmonyPatch(typeof(BroadcastCommand), nameof(BroadcastCommand.OnExecute))]
public class BroadcastPatch
{
    public static bool Prefix(BroadcastCommand __instance, ArraySegment<string> arguments, ICommandSender sender, out string response, ref bool __result)
    {
        string text = arguments.At(0);
        ushort time;
        if (!__instance.IsValidDuration(text, out time))
        {
            response = string.Concat(new string[]
            {
                "Invalid argument for duration: ",
                text,
                " Usage: ",
                arguments.Array[0],
                " ",
                __instance.DisplayCommandUsage()
            });
            __result = false;
            return false;
        }
        Broadcast.BroadcastFlags broadcastFlags;
        bool flag = __instance.HasInputFlag(arguments.At(1), out broadcastFlags, arguments.Count);
        string text2 = RAUtils.FormatArguments(arguments, flag ? 2 : 1);
        var ids = API.MultiBroadcast.AddMapBroadcast(time, text2);
        ServerLogs.AddLog(ServerLogs.Modules.Administrative, string.Format("{0} broadcast text \"{1}\". Duration: {2} seconds. Broadcast Flag: {3}.", new object[]
        {
            sender.LogName,
            text2,
            text,
            broadcastFlags
        }), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging, false);
        response = $"Added broadcast for all players with id {string.Join(", ", ids)}";
        __result = true;
        return false;
    }
}

[HarmonyPatch(typeof(PlayerBroadcastCommand), nameof(PlayerBroadcastCommand.OnExecute))]
public class PlayerBroadcastPatch
{
    public static bool Prefix(PlayerBroadcastCommand __instance, ArraySegment<string> arguments, ICommandSender sender, out string response, ref bool __result)
    {
        string[] array;
        List<ReferenceHub> list = RAUtils.ProcessPlayerIdOrNamesList(arguments, 0, out array, false);
        if (array == null || array.Length < __instance.MinimumArguments)
        {
            response = "To execute this command provide at least 3 arguments!\nUsage: " + arguments.Array[0] + " " + __instance.DisplayCommandUsage();
            __result = false;
            return false;
        }
        string text = array[0];
        ushort num;
        if (!__instance.IsValidDuration(text, out num))
        {
            response = string.Concat(new string[]
            {
                "Invalid argument for duration: ",
                text,
                " Usage: ",
                arguments.Array[0],
                " ",
                __instance.DisplayCommandUsage()
            });
            return false;
        }
        Broadcast.BroadcastFlags broadcastFlags;
        bool flag = __instance.HasInputFlag(array[1], out broadcastFlags, array.Length);
        string text2 = RAUtils.FormatArguments(array.Segment(1), flag ? 1 : 0);
        StringBuilder stringBuilder = StringBuilderPool.Shared.Rent();
        Broadcast singleton = Broadcast.Singleton;
        int num2 = 0;
        int[] ids = [];
        foreach (ReferenceHub referenceHub in list)
        {
            if (num2++ != 0)
            {
                stringBuilder.Append(", ");
            }
            stringBuilder.Append(referenceHub.LoggedNameFromRefHub());
            ids.AddItem(API.MultiBroadcast.AddPlayerBroadcast(Player.Get(referenceHub), num, text2));
        }
        ServerLogs.AddLog(ServerLogs.Modules.Administrative, string.Format("{0} broadcast text \"{1}\" to {2} players. Duration: {3} seconds. Affected players: {4}. Broadcast Flag: {5}.", new object[]
        {
            sender.LogName,
            text2,
            num2,
            num,
            stringBuilder,
            broadcastFlags
        }), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging, false);
        StringBuilderPool.Shared.Return(stringBuilder);
        response = $"Added broadcast for {num2} players with id {string.Join(", ", ids)}";
        __result = true;
        return false;
    }
}