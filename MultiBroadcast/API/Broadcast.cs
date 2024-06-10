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
    public Player Player { get; }

    /// <summary>
    /// Gets or sets the text of the broadcast.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Gets the ID of the broadcast.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets a value indicating whether the broadcast should be displayed on top.
    /// </summary>
    public bool OnTop { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Broadcast"/> class.
    /// </summary>
    /// <param name="player">The player to whom the broadcast is sent.</param>
    /// <param name="text">The text of the broadcast.</param>
    /// <param name="id">The ID of the broadcast.</param>
    /// <param name="onTop">Whether the broadcast should be displayed on top.</param>
    public Broadcast(Player player, string text, int id, bool onTop)
    {
        Player = player;
        Text = text;
        Id = id;
        OnTop = onTop;
    }
}
