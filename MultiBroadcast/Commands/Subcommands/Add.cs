using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;

namespace MultiBroadcast.Commands.Subcommands;

/// <inheritdoc />
public class Add : ICommand
{
    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (arguments.Count < 1)
        {
            response = "Usage: multibroadcast add <map/player>";
            return false;
        }

        string text;
        var arg2 = arguments.At(0).ToLower();

        switch (arg2[0])
        {
            case 'm':
                if (arguments.Count < 3)
                {
                    response = "Usage: multibroadcast add map <duration> <text>";
                    return false;
                }

                if (!ushort.TryParse(arguments.At(1), out var duration))
                {
                    response = "Usage: multibroadcast add map <duration> <text>";
                    return false;
                }

                text = string.Join(" ", arguments.Skip(2));

                var bcs = API.MultiBroadcast.AddMapBroadcast(duration, text);
                var ids = bcs?.Select(bc => bc.Id).ToArray();

                response = bcs == null ? "Error on adding broadcast" : $"Added broadcast for all players with id {string.Join(", ", ids)}";
                return true;
            case 'p':
                if (arguments.Count < 4)
                {
                    response = "Usage: multibroadcast add player <player> <duration> <text>";
                    return false;
                }

                var player = Player.Get(arguments.At(1));

                if (player == null)
                {
                    response = "Player not found";
                    return false;
                }

                if (!ushort.TryParse(arguments.At(2), out duration))
                {
                    response = "Usage: multibroadcast add player <player> <duration> <text>";
                    return false;
                }

                text = string.Join(" ", arguments.Skip(3));

                var bc = API.MultiBroadcast.AddPlayerBroadcast(player, duration, text);
                var id = bc?.Id;

                response = bc == null ? $"Error on adding broadcast to {player.Nickname}" : $"Added broadcast for {player.Nickname} with id {id}";
                return true;
            default:
                response = "Usage: multibroadcast add <map/player>";
                return false;
        }
    }

    /// <inheritdoc />
    public string Command { get; } = "add";

    /// <inheritdoc />
    public string[] Aliases { get; } = ["a"];

    /// <inheritdoc />
    public string Description { get; } = "Add a broadcast.";

    /// <inheritdoc />
    public bool SanitizeResponse { get; } = false;
}