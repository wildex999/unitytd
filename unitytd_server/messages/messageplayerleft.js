var MessageHandler = require('../messagehandler');
var Commands = require('../commands').CommandEnum;
var Message = require('./message');

//Create Format:
//[Player]
//-PlayerId(int)

//player = User
exports.createPlayerLeftMessage = function(player)
{	
	var message = Message.createMessage(Commands.PlayerLeft);

	message.writeInt(player.id);
	
	message.applyWrites(2);

	return message;
}