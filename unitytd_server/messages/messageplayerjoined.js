var MessageHandler = require('../messagehandler');
var Commands = require('../commands').CommandEnum;
var Message = require('./message');

//Create Format:
//[Player]
//-PlayerName(string)
//-PlayerId(int)

//player = User
exports.createPlayerJoinedMessage = function(player)
{	
	var message = Message.createMessage(Commands.PlayerJoined);

	message.writeString(player.name);
	message.writeInt(player.id);
	
	message.applyWrites(2);

	return message;
}