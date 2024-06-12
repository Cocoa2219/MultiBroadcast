using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CommandSystem;
using MultiBroadcast.API;

namespace MultiBroadcast.Commands.Subcommands;

/// <summary>
///     The list command.
/// </summary>
public class List : ICommand
{
    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (arguments.Count > 0)
        {
            var str = string.Join(" ", arguments);
            var player = Exiled.API.Features.Player.Get(str);

            if (player == null)
            {
                response = "Player not found.";
                return false;
            }

            var strb = new StringBuilder($"\n<b>{player.Nickname}'s Broadcast List:</b>\n");
            foreach (var bc in player.GetBroadcasts().Select(broadcast => broadcast))
                strb.Append($" - ID: {bc.Id}, Duration: {bc.Duration}, Priority: {bc.Priority}, Text: {bc.Text}\n");

            if (player.GetBroadcasts().ToList().Count == 0)
                strb.Append("No broadcasts found.");

            response = strb.ToString();
            return true;
        }

        var sb = new StringBuilder("\n<b>Current Broadcast List:</b>\n");
        foreach (var bc in API.MultiBroadcast.GetAllBroadcasts().Values.SelectMany(broadcasts => broadcasts))
            sb.Append($" - ID: {bc.Id}, Player: {bc.Player.Nickname}, Duration: {bc.Duration}, Priority: {bc.Priority}, Text: {bc.Text}\n");

        if (API.MultiBroadcast.GetAllBroadcasts().Count == 0)
            sb.Append("No broadcasts found.");

        response = sb.ToString();
        return true;
    }

    /// <inheritdoc />
    public string Command { get; } = "list";

    /// <inheritdoc />
    public string[] Aliases { get; } = ["l"];

    /// <inheritdoc />
    public string Description { get; } = "List all broadcasts.";

    /// <inheritdoc />
    public bool SanitizeResponse { get; } = false;
}