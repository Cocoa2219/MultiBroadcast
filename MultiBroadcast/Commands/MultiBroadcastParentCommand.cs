using System;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using MultiBroadcast.Commands.Subcommands;

namespace MultiBroadcast.Commands;

/// <summary>
///     The base parent command.
/// </summary>
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class MultiBroadcastParentCommand : ParentCommand
{
    /// <inheritdoc />
    public MultiBroadcastParentCommand() => LoadGeneratedCommands();

    /// <inheritdoc />
    public override void LoadGeneratedCommands()
    {
        RegisterCommand(new Add());
        RegisterCommand(new Edit());
        RegisterCommand(new Remove());
        RegisterCommand(new List());
    }

    /// <inheritdoc />
    protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        response = "\nPlease enter a valid subcommand:";

        foreach (ICommand command in AllCommands)
        {
            if (sender.CheckPermission(PlayerPermissions.Broadcasting, out _))
            {
                response += $"\n\n<color=yellow><b>- {command.Command} ({string.Join(", ", command.Aliases)})</b></color>\n<color=white>{command.Description}</color>";
            }
        }

        return false;
    }

    /// <inheritdoc />
    public override string Command { get; } = "multibroadcast";
    /// <inheritdoc />
    public override string[] Aliases { get; } = ["mbc"];
    /// <inheritdoc />
    public override string Description { get; } = "The parent command for MultiBroadcast.";
}