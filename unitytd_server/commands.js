/*Commands:

MultiCommand - Multiple commands in one message
Log(type, message) - Log entry from player(Info, Warning, Error, Stacktrace etc.) that will be saved to file playername-ip-datetime.log (datetime will be when the player logged in)
Disconnect(reason) - A reason for disconnect. Sent as the socket is disconnected(Before FIN).

--Player
Login("name") - Returns a id that is used by games to verify the player is logged in
Register - Register a new player

GetPlayer("name") - Returns permission and info about a player

SendMessage("userid", "message") - Send message to player

--Custom server(player server)
CreateGame("name") - Create a new game with name
UpdateGame("name") - Update game info for lobby(Player count, map name, status)
DestroyGame("name") - Remove game from lobby
GetGame("name") - Returns info about the named game. Owner, players, map, mode etc.
JoinGame("name") - Join the game and update your playerinfo to show which game player is in
VerifyPlayer("playername", "id") - Verify that player exists, has that name, and is allowed to join the game(Lobby game, LAN/Direct would ignore this)

MapChange("mapname", "mapid", mapversion, "gamemode") - The server has switched to a new map(And possibly new gamemode)
MapDownload() - When Map id is 0(Unpublished) or system is offline(LAN), the client will request a map download from the host

--Game
PlayerJoined - Sent to all players of a game when a new player joins
PlayerLeft - Sent to all players of a game when a player leaves
KickPlayer - Sent to player when the player is forced to leave the game(Or game is ending)

Broadcast(message, includeSender) - Broadcast a Game message to all connected players(Except original sender if includeSender = false)
SendMessage(userIdTo, userIdFrom, message) - Send a message to a specific player in the game
EndGame - Send to everyone in game when the game ends(Owner leaves/disconnects)


*/
exports.CommandEnum = Object.freeze({
	MultiCommand: 0,
	Login: 1,
	Register: 2,
	GetPlayer: 3,
	GetGame: 4,
	JoinGame: 5,
	CreateGame: 6,
	UpdateGame: 7,
	DestroyGame: 8,
	VerifyPlayer: 9,
	MapChange: 10,
	MapDownload: 11,
	PlayerJoined: 12,
	PlayerLeft: 13,
	Broadcast: 14,
	SendMessage: 15,
	EndGame: 17,
    Disconnect: 18,
    KickPlayer: 19,
	
	Log: 21, //TODO: Disable for release, only using during beta/debugging(Maybe allow for debug clients for finding problems after release)(Allow errors/exceptions to be sent?)
});