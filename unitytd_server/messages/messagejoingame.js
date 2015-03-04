var MessageHandler = require('../messagehandler');
var Commands = require('../commands').CommandEnum;
var Message = require('./message');
var MessageGetGame = require('./messagegetgame');

//Format:
//GameId(string)

//The reply will simply re-send the current game info, so it just sends a GetGameReply message, with the id changed.


//JoinGame reply format(Game NOT found):
//Results(byte)(0 = game not found)
//Error(string)
 	

//Create a new message for replying to a JoinGame message
//gameinfo(GameInfo)
//error(string)
exports.createJoinGameReplyMessage = function(gameinfo, error)
{	
	var message = null;
	
	if(gameinfo == null)
	{
		message = Message.createMessage(Commands.JoinGame);
		message.writeByte(0);
		message.writeString(error);
		message.applyWrites(2);
	}
	else
	{
		message = MessageGetGame.createGetGameReplyMessage(gameinfo);
		message.setCommand(Commands.JoinGame);
	}
	
	return message;
}

//Parser for JoinGame messages
function messageJoinGameParser(message, server)
{
	message.gameName = message.readString();
	
	console.log("Got JoinGame request with GameId: " + message.gameName + " What: " + Commands.JoinGame);
	
	//Emit parsed message event
	server.emit('parsedMessageJoinGame', message);
}

//Register parser with MessageHandler
MessageHandler.registerMessageParser(Commands.JoinGame, messageJoinGameParser);