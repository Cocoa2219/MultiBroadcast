﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CommandSystem;

namespace MultiBroadcast.Commands.Subcommands;

/// <summary>
///     The edit command.
/// </summary>
public class Edit : ICommand
{
    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (arguments.Count < 2)
        {
            response = "Usage: multibroadcast edit <id> <text>";
            return false;
        }

        if (!CommandUtilities.GetIntArguments(arguments.At(0), out var ids))
        {
            response = "Usage: multibroadcast edit <id> <text>";
            return false;
        }

        var text = string.Join(" ", arguments.Skip(1));

        var result = API.MultiBroadcast.EditBroadcast(text, ids);
        var str = CommandUtilities.GetStringFromArray(ids);
        response = !result
            ? $"Error on editing broadcast with id {str}"
            : $"Edited broadcast with id {str} to {text}";
        return true;
    }

    /// <inheritdoc />
    public string Command { get; } = "edit";

    /// <inheritdoc />
    public string[] Aliases { get; } = ["e"];

    /// <inheritdoc />
    public string Description { get; } = "Edit a broadcast.";

    /// <inheritdoc />
    public bool SanitizeResponse { get; } = false;
}