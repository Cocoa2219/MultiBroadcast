using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using HarmonyLib;
using MEC;
using Server = Exiled.Events.Handlers.Server;

namespace MultiBroadcast.API;

/// <summary>
///     Class that handles all broadcasts.
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
    ///     Dictionary that contains all broadcasts for each player.
    /// </summary>
    public static Dictionary<string, List<PlayerBroadcast>> PlayerBroadcasts { get; } = new();

    /// <summary>
    ///     Gets the ID of the broadcast.
    /// </summary>
    private static int Id { get; set; }

    /// <summary>
    ///     Adds a broadcast to all players.
    /// </summary>
    /// <param name="duration">Broadcast duration.</param>
    /// <param name="text">Text of the broadcast.</param>
    /// <param name="onTop">Decides whether this broadcast must be fixed on top.</param>
    /// <returns>A group of IDs.</returns>
    public static int[] AddMapBroadcast(ushort duration, string text, bool onTop = false)
    {
        if (duration > 300)
            return null;

        var ids = new List<int>();

        foreach (var player in Player.List)
        {
            if (player.IsNPC)
                continue;
            Id++;
            Timing.RunCoroutine(AddPlayerBroadcastCoroutine(player.UserId, duration, Id, text, onTop),
                "MBroadcast" + Id);
            Log.Debug($"Added broadcast for {player.Nickname} with id {Id}");
            ids.Add(Id);
        }

        return ids.ToArray();
    }

    /// <summary>
    ///     Adds a broadcast to a player.
    /// </summary>
    /// <param name="player">Player to send the broadcast to.</param>
    /// <param name="duration">Broadcast duration.</param>
    /// <param name="text">Text of the broadcast.</param>
    /// <param name="onTop">Decides whether this broadcast must be fixed on top.</param>
    /// <returns>The ID of the broadcast.</returns>
    public static int AddPlayerBroadcast(Player player, ushort duration, string text, bool onTop = false)
    {
        if (player.IsNPC)
            return -1;
        if (duration > 300)
            return -1;

        Id++;
        Timing.RunCoroutine(AddPlayerBroadcastCoroutine(player.UserId, duration, Id, text, onTop), "MBroadcast" + Id);
        Log.Debug($"Added broadcast for {player.Nickname} with id {Id}");
        return Id;
    }

    private static IEnumerator<float> AddPlayerBroadcastCoroutine(string playerId, ushort duration, int id, string text,
        bool onTop = false)
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
    ///     Edits a broadcast.
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
    ///     Edits a broadcast with a new duration.
    /// </summary>
    /// <param name="text">New text for the broadcast.</param>
    /// <param name="duration">New duration for the broadcast.</param>
    /// <param name="ids">IDs of the broadcasts to edit.</param>
    /// <returns>True if the broadcast was successfully edited; otherwise, false.</returns>
    public static bool EditBroadcast(string text, ushort duration, params int[] ids)
    {
        switch (duration)
        {
            case 0:
                return EditBroadcast(text, ids);
            case > 300:
                return false;
        }

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
            Timing.RunCoroutine(
                AddPlayerBroadcastCoroutine(broadcast.Player.UserId, duration, id, text, broadcast.OnTop),
                "MBroadcast" + id);
            Log.Debug($"Edited broadcast with id {id} to {text} with duration {duration}");
        }

        return true;
    }

    /// <summary>
    ///     Removes a broadcast.
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

    /// <summary>
    ///     Checks if a broadcast exists with the specified ID to player.
    /// </summary>
    /// <param name="player">Player to check for.</param>
    /// <param name="id">ID of the broadcast.</param>
    /// <returns>True if the broadcast exists; otherwise, false.</returns>
    public static bool BroadcastExists(Player player, int id)
    {
        return PlayerBroadcasts.ContainsKey(player.UserId) &&
               PlayerBroadcasts[player.UserId].Any(broadcast => broadcast.Id == id);
    }

    /// <summary>
    ///     Gets a broadcast with the specified ID.
    /// </summary>
    /// <param name="id">ID of the broadcast.</param>
    /// <returns>The broadcast with the specified ID.</returns>
    public static PlayerBroadcast GetBroadcast(int id)
    {
        var broadcast = PlayerBroadcasts.Values
            .SelectMany(broadcasts => broadcasts)
            .FirstOrDefault(broadcast => broadcast.Id == id);

        if (broadcast == null)
            Log.Debug($"Broadcast with id {id} not found.");

        return broadcast;
    }

    /// <summary>
    ///    Gets the text of a broadcast with the specified ID.
    /// </summary>
    /// <param name="id">ID of the broadcast.</param>
    /// <returns>The text of the broadcast with the specified ID.</returns>
    public static string GetBroadcastText(int id)
    {
        var broadcast = GetBroadcast(id);

        return broadcast?.Text;
    }

    /// <summary>
    ///     Restarts broadcasts for all players.
    /// </summary>
    private static void RestartBroadcasts()
    {
        foreach (var broadcasts in PlayerBroadcasts.Values) broadcasts.Clear();

        foreach (var player in Player.List) RefreshBroadcast(player);

        Log.Debug("Cleared all broadcasts");

        for (var i = 0; i < Id; i++) Timing.KillCoroutines("MBroadcast" + i);

        Id = 0;
        PlayerBroadcasts.Clear();
    }

    /// <summary>
    ///     Clears all broadcasts.
    /// </summary>
    public static void ClearAllBroadcasts()
    {
        foreach (var broadcasts in PlayerBroadcasts.Values) broadcasts.Clear();

        foreach (var player in Player.List) RefreshBroadcast(player);

        Log.Debug("Cleared all broadcasts");

        for (var i = 0; i < Id; i++) Timing.KillCoroutines("MBroadcast" + i);
    }

    /// <summary>
    ///     Clears all broadcasts for a player.
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

            foreach (var id in ids) Timing.KillCoroutines("MBroadcast" + id);
        }
    }
}

/// <summary>
///     Class that provides extension methods for player broadcasts.
/// </summary>
public static class BroadcastExtensions
{
    /// <summary>
    ///     Adds a broadcast to a player.
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
