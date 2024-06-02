# Multi Broadcasts

Plugin that allows you to send multiple broadcasts at once.

## Configs

`is_enabled`: Indicates whether this plugin is enabled or not.

`debug`: Indicates whether debug log is printted or not.

## Commands

Main command: `/multibroadcast` or `/mbc`. Requires permission `Broadcasting` (RA Permission).

_Note: This plugin also patches and replaces the original broadcaat command_

| Command                                      | Description                           | Returns         |
| -------------------------------------------- | ------------------------------------- | --------------- |
| `/mbc add map <duration> <text>`             | Broadcasts a message to all players.  | A group of ids. |
| `/mbc add player <player> <duration> <text>` | Broadcasts a message to all players.  | A id.           |
| `/mbc edit <id> <text>`                      | Edits a broadcast.                    | Nothing.        |
| `/mbc remove all`                            | Removes all broadcasts.               | Nothing.        |
| `/mbc remove player <player>`                | Removes all broadcasts of player.     | Nothing.        |
| `/mbc remove <id>`                           | Removes a broadcast.                  | Nothing.        |
| `/mbc list`                                  | Lists all currently shown broadcasts. | A list of ids.  |

## API

All methods can be found in the `MultiBroadcast` class.

| Method                                       | Description                          | Returns                                    |
| -------------------------------------------- | ------------------------------------ | ------------------------------------------ |
| `AddMapBroadcast(duration, text)`            | Broadcasts a message to all players. | A group of ids.                            |
| `AddPlayerBroadcast(player, duration, text)` | Broadcasts a message to all players. | A id                                       |
| `EditBroadcast(id, text)`                    | Edits a broadcast.                   | If the broadcast was successfully edited.  |
| `RemoveAllBroadcasts()`                      | Removes all broadcasts.              | Nothing.                                   |
| `RemovePlayerBroadcast(player)`              | Removes all broadcasts of player.    | Nothing.                                   |
| `RemoveBroadcast(id)`                        | Removes a broadcast.                 | If the broadcast was successfully removed. |
