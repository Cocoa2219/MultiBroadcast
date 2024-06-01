using Exiled.API.Features;

namespace MultiBroadcast.API;

public class PlayerBroadcast(Player player, string text, int id, bool onTop)
{
    public Player Player { get; } = player;
    public string Text { get; set; } = text;
    public int Id { get; } = id;
    public bool OnTop { get; } = onTop;
}

public class MapBroadcast(int id, int[] ids)
{
    public int Id { get; } = id;
    public int[] Ids { get; } = ids;
}