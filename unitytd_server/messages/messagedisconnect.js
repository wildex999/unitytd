var MessageHandler = require('../messagehandler');
var Commands = require('../commands').CommandEnum;
var Message = require('./message');

//Send a reason for disconnect(User disonnect, Error, etc.)
//There is no reply, but the message can be sent by both the main server and the clients.

//Format:
//DisconnectReason(string)



//reason(string)
exports.createDisconnectMessage = function(reason)
{	
	var message = Message.createMessage(Commands.Disconnect);
	
    message.writeString(reason);
	
	message.applyWrites(2);

	return message;
};

//Parser for Disconnect messages
function messageDisconnectParser(message, server)
{
	message.reason = message.readString();
	
	console.log("Got Disconnect message with reason: " + message.reason);
	
	//Emit parsed message event
	server.emit('parsedMessageGetGame', message);
}

//Register parser with MessageHandler
MessageHandler.registerMessageParser(Commands.Disconnect, messageDisconnectParser);