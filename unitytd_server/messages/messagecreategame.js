var MessageHandler = require('../messagehandler');
var Commands = require('../commands').CommandEnum;
var Message = require('./message');

//Format:
//game name(string)
//game version(int)
//max players(int)
//game mode(string)
//map id(int)
//map name(string) (Empty if map id != 0)

//The reply will tell if the game was created or not, and includes the id of the created game
//CreateGame reply format:
//Results(byte) (1/0)
//[game id](int) (Only sent if success = 1)
//[error](string) (Only sent if success = 0)
 	

//Create message for replying to CreateGame
//success(byte 0/1)
//gameId(int)
//error(string)
exports.createCreateGameReplyMessage = function(success, gameId, error)
{	
	var message = Message.createMessage(Commands.CreateGame);
	
	message.writeByte(success);
	if(success == 1)
		message.writeInt(gameId);
	else
		message.writeString(error);

	message.applyWrites(2);

	return message;
}

//Parser for Login messages
function messageCreateGameParser(message, server)
{
	message.gameName = message.readString();
	message.gameVersion = message.readInt();
	message.maxPlayers = message.readInt();
	message.gameMode = message.readString();
	message.mapId = message.readInt();
	message.mapName = message.readString();
	
	console.log("Got CreateGame request:");
	console.log("Game Name: " + message.gameName);
	console.log("Game Mode: " + message.gameMode);
	console.log("Game Version: " + message.gameVersion);
	console.log("Max players: " + message.maxPlayers);
	console.log("Map Id: " + message.mapId);
	console.log("Map name: " + message.mapName);
	
	//Emit parsed message event
	server.emit('parsedMessageCreateGame', message);
}

//Register parser with MessageHandler
MessageHandler.registerMessageParser(Commands.CreateGame, messageCreateGameParser);