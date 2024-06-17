using System;
using System.Linq;
using HarmonyLib;
using PluginAPI.Events;
using RemoteAdmin;
using RemoteAdmin.Communication;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MultiBroadcast.Patches;

[HarmonyPatch(typeof(CommandProcessor), nameof(CommandProcessor.ProcessQuery))]
public class ProcessQueryPatch
{
    public static bool Prefix(string q, CommandSender sender, ref string __result)
    {
	    if (q.StartsWith("$", StringComparison.Ordinal))
        {
	        var array = q.Remove(0, 1).Split([' '], StringSplitOptions.None);
	        if (array.Length <= 1)
	        {
		        __result = null;
		        return false;
	        }

	        if (!int.TryParse(array[0], out var key))
	        {
		        __result = null;
		        return false;
	        }

	        if (CommunicationProcessor.ServerCommunication.TryGetValue(key, out var serverCommunication))
	        {
		        serverCommunication.ReceiveData(sender, string.Join(" ", array.Skip(1)));
	        }
	        __result = null;
	        return false;
        }

	    var array2 = q.Trim().Split(QueryProcessor.SpaceArray, 512, StringSplitOptions.RemoveEmptyEntries);
	    if (!EventManager.ExecuteEvent(new RemoteAdminCommandEvent(sender, array2[0], array2.Skip(1).ToArray())))
	    {
		    __result = null;
		    return false;
	    }

	    if (CommandProcessor.RemoteAdminCommandHandler.TryGetCommand(array2[0], out var command))
	    {
		    try
		    {
			    var flag = command.Execute(array2.Segment(1), sender, out var text);
			    if (command.SanitizeResponse)
			    {
				    // Why
				    // text = Misc.SanitizeRichText(text, "", "");
			    }
			    if (!EventManager.ExecuteEvent(new RemoteAdminCommandExecutedEvent(sender, array2[0], array2.Skip(1).ToArray(), flag, text)))
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
			    var text2 = "Command execution failed! Error: " + Misc.RemoveStacktraceZeroes(ex.ToString());
			    if (!EventManager.ExecuteEvent(new RemoteAdminCommandExecutedEvent(sender, array2[0], array2.Skip(1).ToArray(), false, text2)))
			    {
				    __result = null;
				    return false;
			    }
			    sender.RaReply(text2, false, true, array2[0].ToUpperInvariant() + "#" + text2);
			    __result = text2;
			    return false;
		    }
	    }
	    if (!EventManager.ExecuteEvent(new RemoteAdminCommandExecutedEvent(sender, array2[0], array2.Skip(1).ToArray(), false, "Unknown command!")))
	    {
		    __result = null;
		    return false;
	    }
	    sender.RaReply("SYSTEM#Unknown command!", false, true, string.Empty);
	    __result = "Unknown command!";
	    return false;
    }
}