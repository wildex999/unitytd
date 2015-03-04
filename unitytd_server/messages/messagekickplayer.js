var MessageHandler = require('../messagehandler');
var Commands = require('../commands').CommandEnum;
var Message = require('./message');

//Used when player is forced to leave a game.
//This is also used when the game is ending.

//Create Format:
//Kick reason(string)

//reason = string
exports.createKickPlayerMessage = function(reason)
{	
	var message = Message.createMessage(Commands.KickPlayer);

	message.writeString(reason);
	
	message.applyWrites(2);

	return message;
}