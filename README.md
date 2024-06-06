# Multi Broadcasts

Plugin that allows you to send multiple broadcasts at once.

Contains easy API.

## Configs

| Name         | Type | Description                                      |
|--------------|------|--------------------------------------------------|
| `is_enabled` | bool | Indicates whether this plugin is enabled or not. |
| `debug`      | bool | Indicates whether debug log is printed or not.   |

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

| Method                                           | Description                           | Returns                                         |
|--------------------------------------------------|---------------------------------------|-------------------------------------------------|
| `int[] AddMapBroadcast(duration, text)`          | Broadcasts a message to all players.  | A group of ids.                                 |
| `int AddPlayerBroadcast(player, duration, text)` | Broadcasts a message to all players.  | A id. (returns -1 if error happen while adding) |
| `PlayerBroadcast GetBroadcast(id)`               | Gets a broadcast.                     | If the broadcast was found.                     |
| `string GetBroadcastText(id)`                    | Gets text of broadcast.               | The text of the broadcast.                      |
| `bool EditBroadcast(text, ids)`                  | Edits a broadcast.                    | If the broadcast was successfully edited.       |
| `bool EditBroadcast(text, duration, ids)`        | Edits a broadcast with a new duration | If the broadcast was successfully edited.       |
| `bool RemoveBroadcast(ids)`                      | Removes a broadcast.                  | If the broadcast was successfully removed.      |
| `bool HasBroadcast(player, id)`                  | Checks if player has broadcast.       | If the player has the broadcast.                |
| `void RemoveAllBroadcasts()`                     | Removes all broadcasts.               | Nothing.                                        |
| `void RemovePlayerBroadcast(player)`             | Removes all broadcasts of player.     | Nothing.                                        |
