using System;
using System.Linq;
using CommandSystem;
using HarmonyLib;
using PluginAPI.Events;
using RemoteAdmin;
using RemoteAdmin.Communication;
using RemoteAdmin.Interfaces;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MultiBroadcast.Patches;

[HarmonyPatch(typeof(CommandProcessor), nameof(CommandProcessor.ProcessQuery))]
public class ProcessQueryPatch
{
    public static bool Prefix(string q, CommandSender sender, ref string __result)
    {
        if (q.StartsWith("$", StringComparison.Ordinal))
        {
	        string[] array = q.Remove(0, 1).Split([' '], StringSplitOptions.None);
	        if (array.Length <= 1)
	        {
		        __result = null;
		        return false;
	        }
	        int key;
	        if (!int.TryParse(array[0], out key))
	        {
		        __result = null;
		        return false;
	        }
	        IServerCommunication serverCommunication;
	        if (CommunicationProcessor.ServerCommunication.TryGetValue(key, out serverCommunication))
	        {
		        serverCommunication.ReceiveData(sender, string.Join(" ", array.Skip(1)));
	        }
	        __result = null;
	        return false;
        }
        else
        {
	        string[] array2 = q.Trim().Split(QueryProcessor.SpaceArray, 512, StringSplitOptions.RemoveEmptyEntries);
	        if (!EventManager.ExecuteEvent(new RemoteAdminCommandEvent(sender, array2[0], array2.Skip(1).ToArray<string>())))
	        {
		        __result = null;
		        return false;
	        }
	        ICommand command;
	        if (CommandProcessor.RemoteAdminCommandHandler.TryGetCommand(array2[0], out command))
	        {
		        try
		        {
			        string text;
			        bool flag = command.Execute(array2.Segment(1), sender, out text);
			        if (command.SanitizeResponse)
			        {
				        // Oh Man, Why the hell you added this?
				        // text = Misc.SanitizeRichText(text, "", "");
			        }
			        if (!EventManager.ExecuteEvent(new RemoteAdminCommandExecutedEvent(sender, array2[0], array2.Skip(1).ToArray<string>(), flag, text)))
			        {
				        __result = null;
				        return false;
			        }
			        if (!string.IsNullOrEmpty(text))
			        {
				        sender.RaReply(array2[0].ToUpperInvariant() + "#" + text, flag, true, "");
			        }
			        __result = text;
			        return false;
		        }
		        catch (Exception ex)
		        {
			        string text2 = "Command execution failed! Error: " + Misc.RemoveStacktraceZeroes(ex.ToString());
			        if (!EventManager.ExecuteEvent(new RemoteAdminCommandExecutedEvent(sender, array2[0], array2.Skip(1).ToArray<string>(), false, text2)))
			        {
				        __result = null;
				        return false;
			        }
			        sender.RaReply(text2, false, true, array2[0].ToUpperInvariant() + "#" + text2);
			        __result = text2;
			        return false;
		        }
	        }
	        if (!EventManager.ExecuteEvent(new RemoteAdminCommandExecutedEvent(sender, array2[0], array2.Skip(1).ToArray<string>(), false, "Unknown command!")))
	        {
		        __result = null;
		        return false;
	        }
	        sender.RaReply("SYSTEM#Unknown command!", false, true, string.Empty);
	        __result = "Unknown command!";
	        return false;
        }
    }
}