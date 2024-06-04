using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CommandSystem;
using Exiled.API.Features;
using MEC;
using Server = Exiled.Events.Handlers.Server;

namespace MultiBroadcast.API;

/// <summary>
/// Class that handles all broadcasts.
/// </summary>
public static class MultiBroadcast
{
    static MultiBroadcast()
    {
        Server.RestartingRound += OnRestarting;
    }

    private static void OnRestarting()
    {
        RestartBroadcasts();
    }

    /// <summary>
    /// Dictionary that contains all broadcasts for each player.
    /// </summary>
    public static Dictionary<string, List<PlayerBroadcast>> PlayerBroadcasts { get; } = new();

    /// <summary>
    /// Gets the ID of the broadcast.
    /// </summary>
    public static int Id { get; private set; }

    /// <summary>
    /// Adds a broadcast to all players.
    /// </summary>
    /// <param name="duration">Broadcast duration.</param>
    /// <param name="text">Text of the broadcast.</param>
    /// <param name="onTop">Decides whether this broadcast must be fixed on top.</param>
    /// <returns>A group of IDs.</returns>
    public static int[] AddMapBroadcast(ushort duration, string text, bool onTop = false)
    {
        var ids = new List<int>();

        foreach (var player in Player.List)
        {
            Id++;
            Timing.RunCoroutine(AddPlayerBroadcastCoroutine(player.UserId, duration, Id, text, onTop), "MBroadcast" + Id);
            Log.Debug($"Added broadcast for {player.Nickname} with id {Id}");
            ids.Add(Id);
        }

        return ids.ToArray();
    }

    /// <summary>
    /// Adds a broadcast to a player.
    /// </summary>
    /// <param name="player">Player to send the broadcast to.</param>
    /// <param name="duration">Broadcast duration.</param>
    /// <param name="text">Text of the broadcast.</param>
    /// <param name="onTop">Decides whether this broadcast must be fixed on top.</param>
    /// <returns>The ID of the broadcast.</returns>
    public static int AddPlayerBroadcast(Player player, ushort duration, string text, bool onTop = false)
    {
        Id++;
        Timing.RunCoroutine(AddPlayerBroadcastCoroutine(player.UserId, duration, Id, text, onTop), "MBroadcast" + Id);
        Log.Debug($"Added broadcast for {player.Nickname} with id {Id}");
        return Id;
    }

    private static IEnumerator<float> AddPlayerBroadcastCoroutine(string playerId, ushort duration, int id, string text, bool onTop = false)
    {
        var player = Player.Get(playerId);
        var broadcast = new PlayerBroadcast(player, text, id, onTop);

        if (!PlayerBroadcasts.ContainsKey(playerId))
            PlayerBroadcasts.Add(playerId, [broadcast]);
        else
            PlayerBroadcasts[playerId].Add(broadcast);

        RefreshBroadcast(player);

        yield return Timing.WaitForSeconds(duration);

        if (PlayerBroadcasts.ContainsKey(playerId) && PlayerBroadcasts[playerId].Contains(broadcast))
            PlayerBroadcasts[playerId].Remove(broadcast);

        RefreshBroadcast(player);
    }

    private static void RefreshBroadcast(Player player)
    {
        if (!PlayerBroadcasts.ContainsKey(player.UserId))
            return;

        var broadcasts = PlayerBroadcasts[player.UserId]
            .OrderByDescending(x => x.OnTop)
            .ThenByDescending(y => y.Id)
            .ToList();

        var writtenText = string.Join("\n", broadcasts.Select(b => b.Text));
        player.Broadcast(120, writtenText, Broadcast.BroadcastFlags.Normal, true);
    }

    /// <summary>
    /// Edits a broadcast.
    /// </summary>
    /// <param name="text">New text for the broadcast.</param>
    /// <param name="ids">IDs of the broadcasts to edit.</param>
    /// <returns>True if the broadcast was successfully edited; otherwise, false.</returns>
    public static bool EditBroadcast(string text, params int[] ids)
    {
        foreach (var id in ids)
        {
            var broadcast = GetBroadcast(id);

            if (broadcast == null)
            {
                Log.Debug($"Error while editing: Broadcast with id {id} not found.");
                return false;
            }

            broadcast.Text = text;
            Log.Debug($"Edited broadcast with id {id} to {text}");
            RefreshBroadcast(broadcast.Player);
        }
        return true;
    }

    /// <summary>
    /// Edits a broadcast with a new duration.
    /// </summary>
    /// <param name="text">New text for the broadcast.</param>
    /// <param name="duration">New duration for the broadcast.</param>
    /// <param name="ids">IDs of the broadcasts to edit.</param>
    /// <returns>True if the broadcast was successfully edited; otherwise, false.</returns>
    public static bool EditBroadcast(string text, ushort duration, params int[] ids)
    {
        foreach (var id in ids)
        {
            var broadcast = GetBroadcast(id);

            if (broadcast == null)
            {
                Log.Debug($"Error while editing: Broadcast with id {id} not found.");
                return false;
            }

            Timing.KillCoroutines("MBroadcast" + id);
            PlayerBroadcasts[broadcast.Player.UserId].Remove(broadcast);
            RefreshBroadcast(broadcast.Player);
            Timing.RunCoroutine(AddPlayerBroadcastCoroutine(broadcast.Player.UserId, duration, id, text, broadcast.OnTop), "MBroadcast" + id);
            Log.Debug($"Edited broadcast with id {id} to {text} with duration {duration}");
        }
        return true;
    }

    /// <summary>
    /// Removes a broadcast.
    /// </summary>
    /// <param name="ids">IDs of the broadcasts to remove.</param>
    /// <returns>True if the broadcasts were successfully removed; otherwise, false.</returns>
    public static bool RemoveBroadcast(params int[] ids)
    {
        foreach (var id in ids)
        {
            var broadcast = GetBroadcast(id);

            if (broadcast == null)
            {
                Log.Debug($"Error while removing: Broadcast with id {id} not found.");
                return false;
            }

            Timing.KillCoroutines("MBroadcast" + id);
            PlayerBroadcasts[broadcast.Player.UserId].Remove(broadcast);
            Log.Debug($"Removed broadcast with id {id}");
            RefreshBroadcast(broadcast.Player);
        }
        return true;
    }

    private static PlayerBroadcast GetBroadcast(int id)
    {
        var broadcast = PlayerBroadcasts.Values
            .SelectMany(broadcasts => broadcasts)
            .FirstOrDefault(broadcast => broadcast.Id == id);

        if (broadcast == null)
            Log.Debug($"Broadcast with id {id} not found.");

        return broadcast;
    }

    /// <summary>
    /// Restarts broadcasts for all players.
    /// </summary>
    private static void RestartBroadcasts()
    {
        foreach (var broadcasts in PlayerBroadcasts.Values)
        {
            broadcasts.Clear();
        }

        foreach (var player in Player.List)
        {
            RefreshBroadcast(player);
        }

        Log.Debug("Cleared all broadcasts");

        for (var i = 0; i < Id; i++)
        {
            Timing.KillCoroutines("MBroadcast" + i);
        }

        Id = 0;
        PlayerBroadcasts.Clear();
    }

    /// <summary>
    /// Clears all broadcasts.
    /// </summary>
    public static void ClearAllBroadcasts()
    {
        foreach (var broadcasts in PlayerBroadcasts.Values)
        {
            broadcasts.Clear();
        }

        foreach (var player in Player.List)
        {
            RefreshBroadcast(player);
        }

        Log.Debug("Cleared all broadcasts");

        for (var i = 0; i < Id; i++)
        {
            Timing.KillCoroutines("MBroadcast" + i);
        }
    }

    /// <summary>
    /// Clears all broadcasts for a player.
    /// </summary>
    /// <param name="player">Player to clear broadcasts for.</param>
    public static void ClearPlayerBroadcasts(Player player)
    {
        if (PlayerBroadcasts.ContainsKey(player.UserId))
        {
            var ids = PlayerBroadcasts[player.UserId].Select(broadcast => broadcast.Id).ToList();
            PlayerBroadcasts[player.UserId].Clear();
            Log.Debug($"Cleared all broadcasts for {player.Nickname}");
            RefreshBroadcast(player);

            foreach (var id in ids)
            {
                Timing.KillCoroutines("MBroadcast" + id);
            }
        }
    }
}

/// <summary>
/// Class that provides extension methods for player broadcasts.
/// </summary>
public static class BroadcastExtensions
{
    /// <summary>
    /// Adds a broadcast to a player.
    /// </summary>
    /// <param name="player">Player to send the broadcast to.</param>
    /// <param name="duration">Broadcast duration.</param>
    /// <param name="message">Text of the broadcast.</param>
    /// <param name="onTop">Decides whether this broadcast must be fixed on top.</param>
    /// <returns>The ID of the broadcast.</returns>
    public static int Broadcast(this Player player, ushort duration, string message, bool onTop = false)
    {
        return MultiBroadcast.AddPlayerBroadcast(player, duration, message, onTop);
    }
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class BroadcastCommand : ICommand
{
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (!sender.CheckPermission(PlayerPermissions.Broadcasting, out response))
        {
            return false;
        }

        if (arguments.Count < 1)
        {
            response = "Usage: mbroadcast <add/edit/remove/list>";
            return false;
        }

        var arg = arguments.At(0).ToLower();

        int id;
        string text;
        string arg2;
        bool result;

        switch (arg[0])
        {
            case 'a':
                if (arguments.Count < 2)
                {
                    response = "Usage: mbroadcast add <map/player>";
                    return false;
                }

                arg2 = arguments.At(1).ToLower();

                switch (arg2[0])
                {
                    case 'm':
                        if (arguments.Count < 4)
                        {
                            response = "Usage: mbroadcast add map <duration> <text>";
                            return false;
                        }

                        if (!ushort.TryParse(arguments.At(2), out var duration))
                        {
                            response = "Usage: mbroadcast add map <duration> <text>";
                            return false;
                        }

                        text = string.Join(" ", arguments.Skip(3));

                        var ids = MultiBroadcast.AddMapBroadcast(duration, text);
                        response = $"Added broadcast for all players with id {string.Join(", ", ids)}";
                        return true;
                    case 'p':
                        if (arguments.Count < 5)
                        {
                            response = "Usage: mbroadcast add player <player> <duration> <text>";
                            return false;
                        }

                        var player = Player.Get(arguments.At(2));

                        if (player == null)
                        {
                            response = "Player not found";
                            return false;
                        }

                        if (!ushort.TryParse(arguments.At(3), out duration))
                        {
                            response = "Usage: mbroadcast add player <player> <duration> <text>";
                            return false;
                        }

                        text = string.Join(" ", arguments.Skip(4));

                        id = MultiBroadcast.AddPlayerBroadcast(player, duration, text);

                        response = $"Added broadcast for {player.Nickname} with id {id}";
                        return true;
                    default:
                        response = "Usage: mbroadcast add <map/player>";
                        return false;
                }
            case 'e':
                if (arguments.Count < 3)
                {
                    response = "Usage: mbroadcast edit <id> <text>";
                    return false;
                }

                if (!int.TryParse(arguments.At(1), out id))
                {
                    response = "Usage: mbroadcast edit <id> <text>";
                    return false;
                }

                text = string.Join(" ", arguments.Skip(2));

                result = MultiBroadcast.EditBroadcast(text, id);
                response = !result ? $"Error on editing broadcast with id {id}" : $"Edited broadcast with id {id} to {text}";
                return true;
            case 'r':
                if (arguments.Count < 2)
                {
                    response = "Usage: mbroadcast remove <all/player/id>";
                    return false;
                }

                arg2 = arguments.At(1).ToLower();

                switch (arg2[0])
                {
                    case 'a':
                        MultiBroadcast.ClearAllBroadcasts();
                        response = "Removed all broadcasts";
                        return true;
                    case 'p':
                        if (arguments.Count < 3)
                        {
                            response = "Usage: mbroadcast remove player <player>";
                            return false;
                        }

                        var player = Player.Get(arguments.At(2));

                        MultiBroadcast.ClearPlayerBroadcasts(player);
                        response = $"Removed all broadcasts for {player.Nickname}";
                        return true;
                    default:
                        if (!int.TryParse(arg2, out id))
                        {
                            response = "Usage: mbroadcast remove <all/player/id>";
                            return false;
                        }

                        result = MultiBroadcast.RemoveBroadcast(id);
                        response = !result ? $"Error on removing broadcast with id {id}" : $"Removed broadcast with id {id}";
                        return true;
                }
            case 'l':
                var sb = new StringBuilder("\n<b>Current Broadcast List:</b>\n");
                foreach (var bc in MultiBroadcast.PlayerBroadcasts.Values.SelectMany(broadcasts => broadcasts))
                {
                    sb.Append($" - ID: {bc.Id}, Player: {bc.Player.Nickname}, Text: {bc.Text}\n");
                }

                response = sb.ToString();
                return true;
            default:
                response = "Usage: mbroadcast <add/edit/remove/list>";
                return false;
        }
    }

    public string Command { get; } = "mbroadcast";
    public string[] Aliases { get; } = ["mbc"];
    public string Description { get; } = "Broadcasts a message to all players or a specific player.";
}
