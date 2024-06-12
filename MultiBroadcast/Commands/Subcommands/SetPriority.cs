using System;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;

namespace MultiBroadcast.Commands.Subcommands;

/// <inheritdoc />
public class SetPriority : ICommand
{
    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (arguments.Count < 2)
        {
            response = "Usage: mbroadcast setpriority <id> <text>";
            return false;
        }

        if (!CommandUtilities.GetIntArguments(arguments.At(0), out var ids))
        {
            response = "Usage: mbroadcast setpriority <id> <text>";
            return false;
        }

        if (!byte.TryParse(arguments.At(1), out var priority))
        {
            response = "Usage: mbroadcast setpriority <id> <text>";
            return false;
        }


    }

    /// <inheritdoc />
    public string Command { get; } = "setpriority";

    /// <inheritdoc />
    public string[] Aliases { get; } = ["sp"];

    /// <inheritdoc />
    public string Description { get; } = "Set priority of a broadcast";

    /// <inheritdoc />
    public bool SanitizeResponse { get; } = false;
}