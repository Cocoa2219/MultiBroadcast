# Multi Broadcasts

Plugin that allows you to send multiple broadcasts at once.

Contains easy API.

## Configs

| Name          | Default      | Type           | Description                                      |
|---------------|--------------|----------------|--------------------------------------------------|
| `is_enabled`  | `true`       | bool           | Indicates whether this plugin is enabled or not. |
| `debug`       | `false`      | bool           | Indicates whether debug log is printed or not.   |
| `order`       | `Descending` | BroadcastOrder | Indicates order of broadcasts.                   |

### BroadcastOrder

| Name         | Description                             |
|--------------|-----------------------------------------|
| `Descending` | Broadcasts are shown at top to bottom.  |
| `Ascending`  | Broadcasts are shown at bottom to top.  |

## Commands

Main command: `/multibroadcast` or `/mbc`. Requires permission `Broadcasting` (RA Permission).

_Note: This plugin also patches and replaces the original broadcast command._

| Command                                      | Description                           | Returns         |
|----------------------------------------------|---------------------------------------|-----------------|
| `/mbc add map <duration> <text>`             | Broadcasts a message to all players.  | A group of ids. |
| `/mbc add player <player> <duration> <text>` | Broadcasts a message to all players.  | A id.           |
| `/mbc edit <id> <text>`                      | Edits a broadcast.                    | Nothing.        |
| `/mbc remove all`                            | Removes all broadcasts.               | Nothing.        |
| `/mbc remove player <player>`                | Removes all broadcasts of player.     | Nothing.        |
| `/mbc remove <id>`                           | Removes a broadcast.                  | Nothing.        |
| `/mbc list`                                  | Lists all currently shown broadcasts. | A list of ids.  |

## API

All methods can be found in the `MultiBroadcast` class.

| Method                                                             | Description                            | Returns                                    |
|--------------------------------------------------------------------|----------------------------------------|--------------------------------------------|
| `IEnumerable<Broadcast> AddMapBroadcast(duration, text, priority)` | Broadcasts a message to all players.   | Broadcasts that were added.                |
| `Broadcast AddPlayerBroadcast(player, duration, text, priority)`   | Broadcasts a message to a player.      | The broadcast that was added.              |
| `IEnumerable<Broadcast> GetBroadcast(ids)`                         | Gets a broadcast.                      | The broadcast with the specified ID.       |
| `Broadcast GetBroadcast(id)`                                       | Gets a broadcast.                      | The broadcasts with the specified IDs.     |
| `Dictionary<string, List<Broadcast>> GetAllBroadcasts()`           | Gets a broadcast.                      | All broadcasts.                            |
| `IEnumerable<Broadcast> GetPlayerBroadcasts(player)`               | Gets a broadcast.                      | All broadcasts of the specified player.    |
| `bool EditBroadcast(text, ids)`                                    | Edits a broadcast.                     | If the broadcast was successfully edited.  |
| `bool EditBroadcast(text, broadcasts)`                             | Edits a broadcast.                     | If the broadcast was successfully edited.  |
| `bool EditBroadcast(text, duration, ids)`                          | Edits a broadcast with a new duration. | If the broadcast was successfully edited.  |
| `bool EditBroadcast(text, duration, broadcasts)`                   | Edits a broadcast with a new duration. | If the broadcast was successfully edited.  |
| `void SetPriority(priority, ids)`                                  | Sets a broadcast's priority.           | Nothing.                                   |
| `void SetPriority(priority, broadcasts)`                           | Sets a broadcast's priority.           | Nothing.                                   |
| `bool RemoveBroadcast(ids)`                                        | Removes a broadcast.                   | If the broadcast was successfully removed. |
| `bool RemoveBroadcast(broadcasts)`                                 | Removes a broadcast.                   | If the broadcast was successfully removed. |
| `void ClearAllBroadcasts()`                                        | Removes all broadcasts.                | Nothing.                                   |
| `void ClearPlayerBroadcasts(player)`                               | Removes all broadcasts of player.      | Nothing.                                   |

### Extension Methods (Player)

| Method                                                     | Description                         | Returns                                 |
|------------------------------------------------------------|-------------------------------------|-----------------------------------------|
| `Broadcast AddBroadcast(player, duration, text, priority)` | Broadcasts a message to a player.   | The broadcast that was added.           |
| `IEnumerable<Broadcast> GetBroadcasts(player)`             | Gets a broadcast.                   | All broadcasts of the specified player. |
| `bool ClearPlayerBroadcasts(player)`                       | Clears all broadcasts for a player. | Nothing.                                |

### Extension Methods (Broadcast)

| Method                                           | Description                            | Returns                                        |
|--------------------------------------------------|----------------------------------------|------------------------------------------------|
| `bool EditBroadcast(broadcast, text)`            | Edits a broadcast.                     | If the broadcast was successfully edited.      |
| `bool EditBroadcast(broadcast, text, duration)`  | Edits a broadcast with a new duration. | If the broadcast was successfully edited.      |
| `bool RemoveBroadcast(broadcast)`                | Removes a broadcast.                   | If the broadcast was successfully removed.     |
| `void SetPriority(broadcast, priority)`          | Sets a broadcast's priority.           | Nothing.                                       |

## API Examples

### Showing a welcome broadcast when a player joins:
```csharp
// ...

public void RegisterEvents() 
{
    Exiled.Events.Handlers.Player.Joined += OnPlayerJoined;    
}

public void UnregisterEvents() 
{
    Exiled.Events.Handlers.Player.Joined -= OnPlayerJoined;    
}

public void OnPlayerJoined(JoinedEventArgs ev) 
{
    // Using an extension method
    ev.Player.AddBroadcast(10, "Welcome to the server!", false);
    // ... or MultiBroadcast.AddPlayerBroadcast(ev.Player, 10, "Welcome to the server!");
}

// ...
```