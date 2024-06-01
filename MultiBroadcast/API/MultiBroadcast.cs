using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;

namespace MultiBroadcast.API;

public static class MultiBroadcast
{
    public static Dictionary<string, List<PlayerBroadcast>> PlayerBroadcast { get; } = new();
    public static List<MapBroadcast> MapBroadcast { get; } = new();

    private static int _id;
    private static int _mapId;

    /// <summary>
    /// Adds a broadcast to all players.
    /// </summary>
    /// <param name="duration">Broadcast duration.</param>
    /// <param name="text">Text.</param>
    /// <param name="onTop">Decides whether this broadcast must be fixed on top.</param>
    /// <returns>A group of ids.</returns>
    public static int[] AddMapBroadcast(ushort duration, string text, bool onTop = false)
    {
        var ids = new List<int>();

        foreach (var player in Player.List)
        {
            _id++;
            Timing.RunCoroutine(AddPlayerBroadcast(player.UserId, duration, text, onTop), _id.ToString());
            Log.Debug($"Added broadcast for {player.Nickname} with id {_id}");
            ids.Add(_id);
        }

        _mapId++;
        MapBroadcast.Add(new MapBroadcast(_mapId, ids.ToArray()));

        return ids.ToArray();
    }

    /// <summary>
    /// Adds a broadcast to a player.
    /// </summary>
    /// <param name="player">Player to send.</param>
    /// <param name="duration">Broadcast duration.</param>
    /// <param name="text">Text.</param>
    /// <param name="onTop">Decides whether this broadcast must be fixed on top.</param>
    /// <returns></returns>
    public static int AddPlayerBroadcast(Player player, ushort duration, string text, bool onTop = false)
    {
        _id++;
        Timing.RunCoroutine(AddPlayerBroadcast(player.UserId, duration, text, onTop), _id.ToString());
        Log.Debug($"Added broadcast for {player.Nickname} with id {_id}");
        return _id;
    }

    private static IEnumerator<float> AddPlayerBroadcast(string playerId, ushort duration, string text, bool onTop = false)
    {
        var player = Player.Get(playerId);
        var writtenText = string.Empty;
        var broadcast = new PlayerBroadcast(player, text, _id, onTop);
        var originalId = _id;

        if (!PlayerBroadcast.ContainsKey(playerId))
            PlayerBroadcast.Add(playerId, [broadcast]);
        else
            PlayerBroadcast[playerId].Add(broadcast);

        var broadcasts = PlayerBroadcast[playerId].OrderByDescending(x => x.OnTop).ThenByDescending(y => y.Id).ToList();

        if (broadcasts.Count > 0)
            foreach (var t in broadcasts)
            {
                writtenText += t.Text;
                writtenText += "\n";
            }

        player.Broadcast(120, writtenText, Broadcast.BroadcastFlags.Normal, true);
        yield return Timing.WaitForSeconds(duration);

        if (PlayerBroadcast.ContainsKey(playerId) && PlayerBroadcast[playerId].Contains(broadcast))
            PlayerBroadcast[playerId].Remove(broadcast);

        if (MapBroadcast.Any(x => x.Ids.Contains(originalId)))
        {
            var map = MapBroadcast.First(x => x.Ids.Contains(originalId));
            MapBroadcast.Remove(map);
        }

        RefreshBroadcast(player);
    }

    private static void RefreshBroadcast(Player player)
    {
        var writtenText = string.Empty;

        var broadcasts = PlayerBroadcast[player.UserId].OrderByDescending(x => x.OnTop).ThenByDescending(y => y.Id).ToList();

        if (broadcasts.Count > 0)
            foreach (var t in broadcasts)
            {
                writtenText += t.Text;
                writtenText += "\n";
            }

        player.Broadcast(120, writtenText, Broadcast.BroadcastFlags.Normal, true);
    }

    /// <summary>
    /// Edits a broadcast.
    /// </summary>
    /// <param name="text">Text to edit.</param>
    /// <param name="ids">Ids to edit.</param>
    /// <returns>If the broadcast was successfully edited.</returns>
    public static bool EditBroadcast(string text, params int[] ids)
    {
        foreach (var id in ids)
        {
            var broadcast = GetBroadcast(id);

            if (broadcast == null)
            {
                Log.Debug($"Err while Editing: Broadcast with id {id} not found.");
                return false;
            }

            broadcast.Text = text;
            Log.Debug($"Edited broadcast with id {id} to {text}");
            RefreshBroadcast(broadcast.Player);
        }
        return true;
    }

    /// <summary>
    /// Removes a broadcast.
    /// </summary>
    /// <param name="ids">Ids to remove.</param>
    /// <returns>If the broadcast was successfully removed.</returns>
    public static bool RemoveBroadcast(params int[] ids)
    {
        foreach (var id in ids)
        {
            Timing.KillCoroutines(id.ToString());

            var broadcast = GetBroadcast(id);

            if (broadcast == null)
            {
                Log.Debug($"Err while Removing: Broadcast with id {id} not found.");
                return false;
            }

            PlayerBroadcast[broadcast.Player.UserId].Remove(broadcast);
            Log.Debug($"Removed broadcast with id {id}");
            RefreshBroadcast(broadcast.Player);
        }
        return true;
    }

    private static PlayerBroadcast GetBroadcast(int id)
    {
        var text = PlayerBroadcast.Values.SelectMany(broadcasts => broadcasts).Aggregate("Getting Broadcast with id {id}...:", (current, broadcast) => current + $"\n{broadcast.Id} - {broadcast.Player.Nickname} - {broadcast.Text}");
        Log.Debug(text);
        return PlayerBroadcast.Values.SelectMany(broadcasts => broadcasts).FirstOrDefault(broadcast => broadcast.Id == id);
    }

    /// <summary>
    /// Clears all broadcasts.
    /// </summary>
    public static void ClearBroadcasts()
    {
        foreach (var broadcasts in PlayerBroadcast.Values)
        {
            broadcasts.Clear();
        }

        foreach (var player in Player.List)
        {
            RefreshBroadcast(player);
        }

        Log.Debug("Cleared all broadcasts");

        for (var i = 0; i < _id; i++)
        {
            Timing.KillCoroutines(i.ToString());
        }
    }

    /// <summary>
    /// Clears all broadcasts for a player.
    /// </summary>
    /// <param name="player">Player to clear broadcasts.</param>
    public static void ClearPlayerBroadcasts(Player player)
    {
        var ids = new List<int>();

        if (PlayerBroadcast.ContainsKey(player.UserId))
        {
            ids.AddRange(PlayerBroadcast[player.UserId].Select(broadcast => broadcast.Id));
            PlayerBroadcast[player.UserId].Clear();
            Log.Debug($"Cleared all broadcasts for {player.Nickname}");
            RefreshBroadcast(player);
        }

        foreach (var id in ids)
        {
            Timing.KillCoroutines(id.ToString());
        }
    }
}

public static class BroadcastExtensions
{
    /// <summary>
    /// Adds a broadcast to a player.
    /// </summary>
    /// <param name="player">Player to send.</param>
    /// <param name="duration">Broadcast duration.</param>
    /// <param name="message">Text.</param>
    /// <param name="onTop">Decides whether this broadcast must be fixed on top.</param>
    /// <returns></returns>
    public static int Broadcast(this Player player, ushort duration, string message, bool onTop = false)
    {
        return MultiBroadcast.AddPlayerBroadcast(player, duration, message, onTop);
    }
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
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
            response = "Usage: mbroadcast <add/edit/remove>";
            return false;
        }

        var arg = arguments.At(0).ToLower();

        int id;
        int[] ids;
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

                        ids = MultiBroadcast.AddMapBroadcast(duration, text);
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
                        MultiBroadcast.ClearBroadcasts();
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

                sb.Append("<b>Map Broadcasts:</b>\n");

                foreach (var bc in MultiBroadcast.MapBroadcast)
                {
                    sb.Append($" - Map Broadcast ID: {bc.Id}, Broadcast ID: {string.Join(", ", bc.Ids)}\n");
                }

                sb.Append("<b>Player Broadcasts:</b>\n");

                foreach (var bc in MultiBroadcast.PlayerBroadcast.Values.SelectMany(broadcasts => broadcasts))
                {
                    sb.Append($" - Player Broadcast ID: {bc.Id}, Player: {bc.Player.Nickname}, Text: {bc.Text}\n");
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