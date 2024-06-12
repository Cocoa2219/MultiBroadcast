using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
        RegisterCommand(new SetPriority());
    }

    /// <inheritdoc />
    protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        response = AllCommands.Where(command => sender.CheckPermission(PlayerPermissions.Broadcasting, out _)).Aggregate("\nPlease enter a valid subcommand:", (current, command) => current + $"\n\n<color=yellow><b>- {command.Command} ({string.Join(", ", command.Aliases)})</b></color>\n<color=white>{command.Description}</color>");

        return false;
    }

    /// <inheritdoc />
    public override string Command { get; } = "multibroadcast";
    /// <inheritdoc />
    public override string[] Aliases { get; } = ["mbc"];
    /// <inheritdoc />
    public override string Description { get; } = "The parent command for MultiBroadcast.";
}