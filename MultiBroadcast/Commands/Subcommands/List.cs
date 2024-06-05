using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CommandSystem;

namespace MultiBroadcast.Commands.Subcommands;

/// <summary>
///     The list command.
/// </summary>
public class List : ICommand
{
    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        var sb = new StringBuilder("\n<b>Current Broadcast List:</b>\n");
        foreach (var bc in API.MultiBroadcast.PlayerBroadcasts.Values.SelectMany(broadcasts => broadcasts))
            sb.Append($" - ID: {bc.Id}, Player: {bc.Player.Nickname}, Text: {bc.Text}\n");

        if (API.MultiBroadcast.PlayerBroadcasts.Count == 0)
            sb.Append("No broadcasts found.");

        response = sb.ToString();
        return true;
    }

    /// <inheritdoc />
    public string Command { get; } = "list";
    /// <inheritdoc />
    public string[] Aliases { get; } = { "l" };
    /// <inheritdoc />
    public string Description { get; } = "List all broadcasts.";
}