using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using Server = Exiled.Events.Handlers.Server;

namespace MultiBroadcast.API;

/// <summary>
///     Class that handles all broadcasts.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class MultiBroadcast
{
    static MultiBroadcast()
    {
        Server.RestartingRound += OnRestarting;
        Exiled.Events.Handlers.Player.Left += OnLeft;
        IsDependency = Plugin.Instance == null;
        Log.Debug("MultiBroadcast class initialized.");
    }

    private static void OnRestarting()
    {
        Log.Debug("OnRestarting event triggered.");
        RestartBroadcasts();
    }

    private static void OnLeft(LeftEventArgs ev)
    {
        if (ev.Player == null)
            return;

        PlayerBroadcasts.Remove(ev.Player.UserId);
    }

    private static readonly bool IsDependency;

    /// <summary>
    ///     Dictionary that contains all broadcasts for each player.
    /// </summary>
    private static Dictionary<string, List<Broadcast>> PlayerBroadcasts { get; } = new();

    /// <summary>
    ///     Gets the ID of the broadcast that was last added.
    /// </summary>
    public static int Id { get; private set; }

    /// <summary>
    ///     Adds a broadcast to all players.
    /// </summary>
    /// <param name="duration">Broadcast duration.</param>
    /// <param name="text">Text of the broadcast.</param>
    /// <param name="priority">Priority of the broadcast.</param>
    /// <param name="tag">Tag of the broadcast.</param>
    /// <returns>Broadcasts that were added.</returns>
    /// <remarks>Duration must be between 1 and 300. (NW Moment)</remarks>
    public static IEnumerable<Broadcast> AddMapBroadcast(ushort duration, string text, byte priority = 0,
        string tag = "")
    {
        Log.Debug($"AddMapBroadcast called with duration: {duration}, text: {text}, priority: {priority}, tag: {tag}");

        if (duration is 0 or > 300)
        {
            Log.Debug($"AddMapBroadcast early return due to invalid duration: {duration}");
            return null;
        }

        var broadcasts = new List<Broadcast>();

        foreach (var player in Player.List)
        {
            if (player == null)
                continue;

            if (player.IsNPC)
                continue;

            Id++;

            var broadcast = new Broadcast(player, text, Id, duration, priority, tag);

            Timing.RunCoroutine(AddPlayerBroadcastCoroutine(broadcast, duration),
                "MBroadcast" + Id);
            Log.Debug($"Added broadcast for {player.Nickname} with id {Id}");

            broadcasts.Add(broadcast);
        }

        return broadcasts;
    }

    /// <summary>
    ///     Adds a broadcast to a player.
    /// </summary>
    /// <param name="player">Player to send the broadcast to.</param>
    /// <param name="duration">Broadcast duration.</param>
    /// <param name="text">Text of the broadcast.</param>
    /// <param name="priority">Priority of the broadcast.</param>
    /// <param name="tag">Tag of the broadcast.</param>
    /// <returns>The broadcast that was added.</returns>4
    /// <remarks>Duration must be between 1 and 300. (NW Moment)</remarks>
    public static Broadcast AddPlayerBroadcast(Player player, ushort duration, string text, byte priority = 0,
        string tag = "")
    {
        Log.Debug(
            $"AddPlayerBroadcast called for player {player?.Nickname}, duration: {duration}, text: {text}, priority: {priority}, tag: {tag}");

        if (player == null || player.IsNPC || duration == 0 || duration > 300)
        {
            Log.Debug($"AddPlayerBroadcast early return for player {player?.Nickname} due to invalid parameters.");
            return null;
        }

        Id++;

        var broadcast = new Broadcast(player, text, Id, duration, priority, tag);
        Timing.RunCoroutine(AddPlayerBroadcastCoroutine(broadcast, duration),
            "MBroadcast" + Id);
        Log.Debug($"Added broadcast for {player.Nickname} with id {Id}");

        return broadcast;
    }

    private static IEnumerator<float> AddPlayerBroadcastCoroutine(Broadcast broadcast, ushort duration)
    {
        Log.Debug(
            $"AddPlayerBroadcastCoroutine started for player {broadcast.Player.Nickname} with duration {duration}");

        var player = broadcast.Player;
        var playerId = player.UserId;

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
        Log.Debug($"Refreshing broadcasts for player {player.Nickname}");

        if (!PlayerBroadcasts.ContainsKey(player.UserId))
            return;

        var sortOrder = IsDependency ? BroadcastOrder.Descending : Plugin.Instance.Config.Order;

        var broadcasts = sortOrder == BroadcastOrder.Descending
            ? PlayerBroadcasts[player.UserId]
                .OrderByDescending(x => x.Priority)
                .ThenByDescending(y => y.Id)
                .ToList()
            : PlayerBroadcasts[player.UserId]
                .OrderByDescending(x => x.Priority)
                .ThenBy(y => y.Id)
                .ToList();

        var writtenText = string.Join("\n", broadcasts.Select(b => b.Text));

        PluginAPI.Core.Server.Broadcast.TargetClearElements(player.Connection);
        PluginAPI.Core.Server.Broadcast.TargetAddElement(player.Connection, writtenText, 300,
            global::Broadcast.BroadcastFlags.Normal);
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
    ///     Edits a broadcast.
    /// </summary>
    /// <param name="text">New text for the broadcast.</param>
    /// <param name="broadcasts">Broadcasts to edit.</param>
    /// <returns>True if the broadcast was successfully edited; otherwise, false.</returns>
    public static bool EditBroadcast(string text, params Broadcast[] broadcasts)
    {
        foreach (var broadcast in broadcasts)
        {
            if (broadcast == null)
            {
                Log.Debug("Error while editing: Broadcast not found.");
                return false;
            }

            broadcast.Text = text;
            Log.Debug($"Edited broadcast with id {broadcast.Id} to {text}");
            RefreshBroadcast(broadcast.Player);
        }

        return true;
    }

    /// <summary>
    ///     Edits a broadcast.
    /// </summary>
    /// <param name="text">New text for the broadcast.</param>
    /// <param name="tag">Tag of the broadcast to edit.</param>
    /// <returns>True if the broadcast was successfully edited; otherwise, false.</returns>
    public static bool EditBroadcast(string text, string tag)
    {
        var broadcasts = PlayerBroadcasts.Values
            .SelectMany(broadcasts => broadcasts)
            .Where(broadcast => broadcast.Tag == tag)
            .ToList();

        if (broadcasts.Count == 0)
        {
            Log.Debug($"Error while editing: Broadcast with tag {tag} not found.");
            return false;
        }

        foreach (var broadcast in broadcasts)
        {
            broadcast.Text = text;
            Log.Debug($"Edited broadcast with tag {tag} to {text}");
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
                Log.Debug("Error while editing: Duration exceeds the maximum allowed value.");
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
                AddPlayerBroadcastCoroutine(new Broadcast(broadcast.Player, text, id, duration, broadcast.Priority), duration),
                "MBroadcast" + id);
            Log.Debug($"Edited broadcast with id {id} to {text} with duration {duration}");
        }

        return true;
    }

    /// <summary>
    ///     Edits a broadcast with a new duration.
    /// </summary>
    /// <param name="text">New text for the broadcast.</param>
    /// <param name="duration">New duration for the broadcast.</param>
    /// <param name="broadcasts">Broadcasts to edit.</param>
    /// <returns>True if the broadcast was successfully edited; otherwise, false.</returns>
    public static bool EditBroadcast(string text, ushort duration, params Broadcast[] broadcasts)
    {
        switch (duration)
        {
            case 0:
                return EditBroadcast(text, broadcasts);
            case > 300:
                Log.Debug("Error while editing: Duration exceeds the maximum allowed value.");
                return false;
        }

        foreach (var broadcast in broadcasts)
        {
            if (broadcast == null)
            {
                Log.Debug("Error while editing: Broadcast not found.");
                return false;
            }

            Timing.KillCoroutines("MBroadcast" + broadcast.Id);
            PlayerBroadcasts[broadcast.Player.UserId].Remove(broadcast);
            RefreshBroadcast(broadcast.Player);
            Timing.RunCoroutine(
                AddPlayerBroadcastCoroutine(new Broadcast(broadcast.Player, text, duration, broadcast.Id, broadcast.Priority),
                    duration),
                "MBroadcast" + broadcast.Id);
            Log.Debug($"Edited broadcast with id {broadcast.Id} to {text} with duration {duration}");
        }

        return true;
    }

    /// <summary>
    ///    Edits a broadcast with a new duration.
    /// </summary>
    /// <param name="text">New text for the broadcast.</param>
    /// <param name="duration">New duration for the broadcast.</param>
    /// <param name="tag">Tag of the broadcast to edit.</param>
    /// <returns>True if the broadcast was successfully edited; otherwise, false.</returns>
    public static bool EditBroadcast(string text, ushort duration, string tag)
    {
        switch (duration)
        {
            case 0:
                return EditBroadcast(text, tag);
            case > 300:
                Log.Debug("Error while editing: Duration exceeds the maximum allowed value.");
                return false;
        }

        var broadcasts = PlayerBroadcasts.Values
            .SelectMany(broadcasts => broadcasts)
            .Where(broadcast => broadcast.Tag == tag)
            .ToList();

        if (broadcasts.Count == 0)
        {
            Log.Debug($"Error while editing: Broadcast with tag {tag} not found.");
            return false;
        }

        foreach (var broadcast in broadcasts)
        {
            Timing.KillCoroutines("MBroadcast" + broadcast.Id);
            PlayerBroadcasts[broadcast.Player.UserId].Remove(broadcast);
            RefreshBroadcast(broadcast.Player);
            Timing.RunCoroutine(
                AddPlayerBroadcastCoroutine(new Broadcast(broadcast.Player, text, broadcast.Id, duration, broadcast.Priority, tag),
                    duration),
                "MBroadcast" + broadcast.Id);
            Log.Debug($"Edited broadcast with tag {tag} to {text} with duration {duration}");
        }

        return true;
    }

    /// <summary>
    ///     Sets the priority of a broadcast.
    /// </summary>
    /// <param name="priority">Priority to set.</param>
    /// <param name="ids">Id of broadcasts to set the priority for.</param>
    /// <returns>True if the broadcasts were priority was successfully set; otherwise, false.</returns>
    public static bool SetPriority(byte priority, params int[] ids)
    {
        foreach (var id in ids)
        {
            var broadcast = GetBroadcast(id);

            if (broadcast == null)
            {
                Log.Debug($"Error while setting priority: Broadcast with id {id} not found.");
                return false;
            }

            broadcast.Priority = priority;
            Log.Debug($"Set priority of broadcast with id {id} to {priority}");
            RefreshBroadcast(broadcast.Player);
        }

        return true;
    }

    /// <summary>
    ///     Sets the priority of a broadcast.
    /// </summary>
    /// <param name="priority">Priority to set.</param>
    /// <param name="broadcasts">Broadcasts to set the priority for.</param>
    /// <returns>True if the broadcasts were priority was successfully set; otherwise, false.</returns>
    public static bool SetPriority(byte priority, params Broadcast[] broadcasts)
    {
        foreach (var broadcast in broadcasts)
        {
            if (broadcast == null)
            {
                Log.Debug("Error while setting priority: Broadcast not found.");
                return false;
            }

            broadcast.Priority = priority;
            Log.Debug($"Set priority of broadcast with id {broadcast.Id} to {priority}");
            RefreshBroadcast(broadcast.Player);
        }

        return true;
    }

    /// <summary>
    ///     Sets the tag of the broadcast.
    /// </summary>
    /// <param name="tag">Tag to set.</param>
    /// <param name="broadcasts">Broadcasts to set the tag for.</param>
    /// <returns>True if the broadcasts were tag was successfully set; otherwise, false.</returns>
    public static bool SetTag(string tag, params Broadcast[] broadcasts)
    {
        foreach (var broadcast in broadcasts)
        {
            if (broadcast == null)
            {
                Log.Debug("Error while setting tag: Broadcast not found.");
                return false;
            }

            broadcast.Tag = tag;
            Log.Debug($"Set tag of broadcast with id {broadcast.Id} to {tag}");
            RefreshBroadcast(broadcast.Player);
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
    ///     Removes a broadcast.
    /// </summary>
    /// <param name="broadcasts">Broadcasts to remove.</param>
    /// <returns>True if the broadcasts were successfully removed; otherwise, false.</returns>
    public static bool RemoveBroadcast(params Broadcast[] broadcasts)
    {
        foreach (var broadcast in broadcasts)
        {
            if (broadcast == null)
            {
                Log.Debug("Error while removing: Broadcast not found.");
                return false;
            }

            Timing.KillCoroutines("MBroadcast" + broadcast.Id);
            PlayerBroadcasts[broadcast.Player.UserId].Remove(broadcast);
            Log.Debug($"Removed broadcast with id {broadcast.Id}");
            RefreshBroadcast(broadcast.Player);
        }

        return true;
    }

    /// <summary>
    ///     Gets a broadcast with the specified ID.
    /// </summary>
    /// <param name="id">ID of the broadcast.</param>
    /// <returns>The broadcast with the specified ID.</returns>
    public static Broadcast GetBroadcast(int id)
    {
        Log.Debug($"Getting broadcast with id {id}");

        var broadcast = PlayerBroadcasts.Values
            .SelectMany(broadcasts => broadcasts)
            .FirstOrDefault(broadcast => broadcast.Id == id);

        if (broadcast == null)
            Log.Debug($"Broadcast with id {id} not found.");

        return broadcast;
    }

    /// <summary>
    ///     Gets broadcasts with the specified IDs.
    /// </summary>
    /// <param name="ids">IDs of the broadcasts.</param>
    /// <returns>The broadcasts with the specified IDs.</returns>
    public static IEnumerable<Broadcast> GetBroadcast(params int[] ids)
    {
        Log.Debug($"Getting broadcasts with ids: {string.Join(", ", ids)}");

        var broadcasts = PlayerBroadcasts.Values
            .SelectMany(broadcasts => broadcasts)
            .Where(broadcast => ids.Contains(broadcast.Id))
            .ToList();

        foreach (var id in ids)
        {
            if (broadcasts.All(broadcast => broadcast.Id != id))
            {
                Log.Debug($"Broadcast with id {id} not found.");
            }
        }

        return broadcasts;
    }


    /// <summary>
    ///     Gets all broadcasts for a player.
    /// </summary>
    /// <param name="player">Player to get broadcasts for.</param>
    /// <returns>All broadcasts for the player.</returns>
    public static IEnumerable<Broadcast> GetPlayerBroadcasts(Player player)
    {
        return PlayerBroadcasts.TryGetValue(player.UserId, out var broadcast) ? broadcast.ToArray() : null;
    }

    /// <summary>
    ///     Gets all broadcasts.
    /// </summary>
    /// <returns>All broadcasts.</returns>
    public static Dictionary<string, List<Broadcast>> GetAllBroadcasts()
    {
        return PlayerBroadcasts;
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
    /// <param name="priority">Priority of the broadcast.</param>
    /// <returns>The broadcast that was added.</returns>
    public static Broadcast AddBroadcast(this Player player, ushort duration, string message, byte priority = 0)
    {
        return MultiBroadcast.AddPlayerBroadcast(player, duration, message, priority);
    }

    /// <summary>
    ///     Clears all broadcasts for a player.
    /// </summary>
    /// <param name="player">Player to clear broadcasts for.</param>
    public static void ClearPlayerBroadcasts(this Player player)
    {
        MultiBroadcast.ClearPlayerBroadcasts(player);
    }

    /// <summary>
    ///     Gets all broadcasts for a player.
    /// </summary>
    /// <param name="player">Player to get broadcasts for.</param>
    /// <returns>All broadcasts of the specified player.</returns>
    public static IEnumerable<Broadcast> GetBroadcasts(this Player player)
    {
        return MultiBroadcast.GetPlayerBroadcasts(player);
    }
}

/// <summary>
///     Class that provides extension methods for player broadcasts.
/// </summary>
public static class PlayerBroadcastExtensions
{
    /// <summary>
    ///     Edits a broadcast.
    /// </summary>
    /// <param name="broadcast">The broadcast to edit.</param>
    /// <param name="text">New text for the broadcast.</param>
    /// <returns>True if the broadcast was successfully edited; otherwise, false.</returns>
    public static bool Edit(this Broadcast broadcast, string text)
    {
        return MultiBroadcast.EditBroadcast(text, broadcast);
    }

    /// <summary>
    ///     Edits a broadcast with a new duration.
    /// </summary>
    /// <param name="broadcast">The broadcast to edit.</param>
    /// <param name="text">New text for the broadcast.</param>
    /// <param name="duration">New duration for the broadcast.</param>
    /// <returns>True if the broadcast was successfully edited; otherwise, false.</returns>
    public static bool Edit(this Broadcast broadcast, string text, ushort duration)
    {
        return MultiBroadcast.EditBroadcast(text, duration, broadcast);
    }

    /// <summary>
    ///     Removes a broadcast.
    /// </summary>
    /// <param name="broadcast">The broadcast to remove.</param>
    /// <returns>True if the broadcast was successfully removed; otherwise, false.</returns>
    public static bool Remove(this Broadcast broadcast)
    {
        return MultiBroadcast.RemoveBroadcast(broadcast);
    }

    /// <summary>
    ///     Sets the priority of a broadcast.
    /// </summary>
    /// <param name="broadcast">The broadcast to set the priority for.</param>
    /// <param name="priority">Priority to set.</param>
    /// <returns>True if the broadcast priority was successfully set; otherwise, false.</returns>
    public static bool SetPriority(this Broadcast broadcast, byte priority)
    {
        return MultiBroadcast.SetPriority(priority, broadcast);
    }

    /// <summary>
    ///     Sets the tag of the broadcast.
    /// </summary>
    /// <param name="broadcast">The broadcast to set the tag for.</param>
    /// <param name="tag">Tag to set.</param>
    /// <returns>True if the broadcast tag was successfully set; otherwise, false.</returns>
    public static bool SetTag(this Broadcast broadcast, string tag)
    {
        return MultiBroadcast.SetTag(tag, broadcast);
    }
}