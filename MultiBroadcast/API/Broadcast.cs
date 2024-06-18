using Exiled.API.Features;

namespace MultiBroadcast.API;

/// <summary>
/// Represents a broadcast that is sent to a player.
/// </summary>
public class Broadcast
{
    /// <summary>
    /// Gets the player to whom the broadcast is sent.
    /// </summary>
    public Player Player { get; set; }

    /// <summary>
    /// Gets or sets the text of the broadcast.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Gets the ID of the broadcast.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the duration of the broadcast.
    /// </summary>
    public int Duration { get; }

    /// <summary>
    /// Gets or sets the priority of the broadcast.
    /// </summary>
    public byte Priority { get; set; }

    /// <summary>
    /// Gets or sets the tag of the broadcast.
    /// </summary>
    public string Tag { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Broadcast"/> class.
    /// </summary>
    /// <param name="player">The player to whom the broadcast is sent.</param>
    /// <param name="text">The text of the broadcast.</param>
    /// <param name="id">The ID of the broadcast.</param>
    /// <param name="duration">The duration of the broadcast.</param>
    /// <param name="priority">The priority of the broadcast.</param>
    /// <param name="tag">The tag of the broadcast.</param>
    public Broadcast(Player player, string text, int id, int duration, byte priority = 0, string tag = "")
    {
        Player = player;
        Text = text;
        Id = id;
        Priority = priority;
        Duration = duration;
        Tag = tag;
    }
}
