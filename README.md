# Multi Broadcasts

Plugin that allows you to send multiple broadcasts at once.

Contains easy API.

## Configs

| Name                       | Default      | Type           | Description                                      |
|----------------------------|--------------|----------------|--------------------------------------------------|
| `is_enabled`               | `true`       | bool           | Indicates whether this plugin is enabled or not. |
| `debug`                    | `false`      | bool           | Indicates whether debug log is printed or not.   |
| `order`                    | `Descending` | BroadcastOrder | Indicates order of broadcasts.                   |
| `close_tags`               | `true`       | bool           | Automatically close tags in broadcasts.          |

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

| Method                                                             | Description                            | Returns                                           |
|--------------------------------------------------------------------|----------------------------------------|---------------------------------------------------|
| `IEnumerable<Broadcast> AddMapBroadcast(duration, text, priority)` | Broadcasts a message to all players.   | Broadcasts that were added.                       |
| `Broadcast AddPlayerBroadcast(player, duration, text, priority)`   | Broadcasts a message to a player.      | The broadcast that was added.                     |
| `IEnumerable<Broadcast> GetBroadcast(ids)`                         | Gets a broadcast.                      | The broadcast with the specified ID.              |
| `Broadcast GetBroadcast(id)`                                       | Gets a broadcast.                      | The broadcasts with the specified IDs.            |
| `Dictionary<string, List<Broadcast>> GetAllBroadcasts()`           | Gets a broadcast.                      | All broadcasts.                                   |
| `IEnumerable<Broadcast> GetPlayerBroadcasts(player)`               | Gets a broadcast.                      | All broadcasts of the specified player.           |
| `bool EditBroadcast(text, ids)`                                    | Edits a broadcast.                     | If the broadcast was successfully edited.         |
| `bool EditBroadcast(text, broadcasts)`                             | Edits a broadcast.                     | If the broadcast was successfully edited.         |
| `bool EditBroadcast(text, duration, ids)`                          | Edits a broadcast with a new duration. | If the broadcast was successfully edited.         |
| `bool EditBroadcast(text, duration, broadcasts)`                   | Edits a broadcast with a new duration. | If the broadcast was successfully edited.         |
| `bool SetPriority(priority, ids)`                                  | Sets a broadcast's priority.           | If the broadcast's priority was successfully set. |
| `bool SetPriority(priority, broadcasts)`                           | Sets a broadcast's priority.           | If the broadcast's priority was successfully set. |
| `bool RemoveBroadcast(ids)`                                        | Removes a broadcast.                   | If the broadcast was successfully removed.        |
| `bool RemoveBroadcast(broadcasts)`                                 | Removes a broadcast.                   | If the broadcast was successfully removed.        |
| `void ClearAllBroadcasts()`                                        | Removes all broadcasts.                | Nothing.                                          |
| `void ClearPlayerBroadcasts(player)`                               | Removes all broadcasts of player.      | Nothing.                                          |

### Extension Methods (Player)

| Method                                                     | Description                         | Returns                                 |
|------------------------------------------------------------|-------------------------------------|-----------------------------------------|
| `Broadcast AddBroadcast(player, duration, text, priority)` | Broadcasts a message to a player.   | The broadcast that was added.           |
| `IEnumerable<Broadcast> GetBroadcasts(player)`             | Gets a broadcast.                   | All broadcasts of the specified player. |
| `bool ClearPlayerBroadcasts(player)`                       | Clears all broadcasts for a player. | Nothing.                                |

### Extension Methods (Broadcast)

| Method                                  | Description                            | Returns                                           |
|-----------------------------------------|----------------------------------------|---------------------------------------------------|
| `bool Edit(broadcast, text)`            | Edits a broadcast.                     | If the broadcast was successfully edited.         |
| `bool Edit(broadcast, text, duration)`  | Edits a broadcast with a new duration. | If the broadcast was successfully edited.         |
| `bool Remove(broadcast)`                | Removes a broadcast.                   | If the broadcast was successfully removed.        |
| `bool SetPriority(broadcast, priority)` | Sets a broadcast's priority.           | If the broadcast's priority was successfully set. |

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
    ev.Player.AddBroadcast(10, "Welcome to the server!", 0);
    // ... or MultiBroadcast.AddPlayerBroadcast(ev.Player, 10, "Welcome to the server!", 0);
}

// ...
```

### Editing and removing broadcasts:
```csharp
// ...

public void OnPlayerJoined(JoinedEventArgs ev) 
{
    // Adding and storing the broadcast
    var broadcast = ev.Player.AddBroadcast(10, "Welcome to the server!", 0);
    
    // Editing the broadcast
    broadcast.Edit("Welcome to the server! You are a VIP!");
    
    // Removing the broadcast
    broadcast.Remove();
    
    // Clearing all broadcasts of the player
    ev.Player.ClearPlayerBroadcasts();
    
    // Clearing all broadcasts
    MultiBroadcast.ClearAllBroadcasts();
}

// ...

```

### Using a priority system:
```csharp
// ...

public void OnPlayerJoined(JoinedEventArgs ev) 
{
    // Second broadcast will be shown upwards (top to bottom, can be changed in the config)
    // Using this assembly as dependency will fix priority to descending
    ev.Player.AddBroadcast(10, "Welcome to the server!", 0);
    ev.Player.AddBroadcast(10, "You are a VIP!", 1);
    
    ev.Player.ClearPlayerBroadcasts();
    
    // If two broadcasts have the same priority, the one that was added last will be shown first
    ev.Player.AddBroadcast(10, "This is first broadcast.", 1);
    ev.Player.AddBroadcast(10, "This is second broadcast.", 1);
    // will result:
    // This is second broadcast.
    // This is first broadcast.
    
    ev.Player.ClearPlayerBroadcasts();
    
    // Also you can change priority of a broadcast
    var broadcast = ev.Player.AddBroadcast(10, "Welcome to the server!", 0);
    
    broadcast.SetPriority(1);
}

// ...
```