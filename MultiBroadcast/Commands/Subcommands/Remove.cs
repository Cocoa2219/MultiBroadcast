using System;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using Exiled.API.Features;

namespace MultiBroadcast.Commands.Subcommands;

/// <summary>
///     The remove command.
/// </summary>
public class Remove : ICommand
{
    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (arguments.Count < 1)
        {
            response = "Usage: multibroadcast remove <all/player/id>";
            return false;
        }

        var arg2 = arguments.At(0).ToLower();

        switch (arg2[0])
        {
            case 'a':
                API.MultiBroadcast.ClearAllBroadcasts();
                response = "Removed all broadcasts";
                return true;
            case 'p':
                if (arguments.Count < 2)
                {
                    response = "Usage: multibroadcast remove player <player>";
                    return false;
                }

                var player = Player.Get(arguments.At(1));

                API.MultiBroadcast.ClearPlayerBroadcasts(player);
                response = $"Removed all broadcasts for {player.Nickname}";
                return true;
            default:
                if (!CommandUtilities.GetIntArguments(arguments.At(0), out var ids))
                {
                    response = "Usage: multibroadcast remove <id> <text>";
                    return false;
                }

                var result = API.MultiBroadcast.RemoveBroadcast(ids);
                var str = CommandUtilities.GetStringFromArray(ids);
                response = !result
                    ? $"Error on removing broadcast with id {str}"
                    : $"Removed broadcast with id {str}";
                return true;
        }
    }

    /// <inheritdoc />
    public string Command { get; } = "remove";

    /// <inheritdoc />
    public string[] Aliases { get; } = ["r"];

    /// <inheritdoc />
    public string Description { get; } = "Remove a broadcast.";

    /// <inheritdoc />
    public bool SanitizeResponse { get; } = false;
}