var MessageHandler = require('../messagehandler');
var Commands = require('../commands').CommandEnum;
var Message = require('./message');

//Format:
//GameId(string)

//The reply should contain the immediately visible information for someone looking at the games
//Game name, player names, player count, map name etc.
//But also contain the id's for detailed lookup of information(Map version/description etc.)
//GetGame reply format(Game found):
//Results(byte)(1 = found game)
//Game name(string)
//game version(int)
//[Owner]
//-player id(int)
//maxPlayers(int)
//playerCount(int)
//{for each player} (Not including game owner)
//-player Name(string)
//-player Id(int)
//game mode(string)
//[game map]
//-map name(string)
//-map id(int)
//-map version(int)

//GetGame reply format(Game NOT found):
//Results(byte)(0 = game not found)

//Create a new message for replying to a GetGame message
//gameinfo(GameInfo)
exports.createGetGameReplyMessage = function(gameinfo)
{	
	var message = Message.createMessage(Commands.GetGame);
	
	if(gameinfo == null)
	{
		message.writeByte(0);
	}
	else
	{
		message.writeByte(1);
		console.log("GameName: " + gameinfo.gameName);
		message.writeString(gameinfo.gameName);
		message.writeInt(gameinfo.gameVersion);
		message.writeInt(gameinfo.owner.id);
		message.writeInt(gameinfo.maxPlayers);
		
		var players = Object.keys(gameinfo.players);
		message.writeInt(players.length);
		players.forEach(function (index) {
			var player = gameinfo.players[index];
			message.writeString(player.name);
			message.writeInt(player.id);
		});
		
		message.writeString(gameinfo.gameMode);
		message.writeString(gameinfo.map.name);
		message.writeInt(gameinfo.map.id);
		message.writeInt(gameinfo.map.version);
	}
	
	message.applyWrites(2);

	return message;
};

//Parser for GetGame messages
function messageGetGameParser(message, server)
{
	message.gameName = message.readString();
	
	console.log("Got GetGame request with GameId: " + message.gameName);
	
	//Emit parsed message event
	server.emit('parsedMessageGetGame', message);
}

//Register parser with MessageHandler
MessageHandler.registerMessageParser(Commands.GetGame, messageGetGameParser);