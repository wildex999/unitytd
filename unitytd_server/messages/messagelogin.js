var MessageHandler = require('../messagehandler');
var Commands = require('../commands').CommandEnum;
var Message = require('./message');

//Parser and methods for the Login message
//Format:
//username(string), password(string)

//Login reply format:
//newPlayerId, [errorMessage]
//If player id == -1, the login failed and errorMessage is included to explain why.

//Create a new Login message
exports.createLoginMessage = function()
{
	console.log("Creating a Login message on server is not implemented!");
}

//Create a new message for replying to a Login message
//playerId(int): The player Id for the user logging in, or -1 for error
//[errorMessage](string): (Excluded if login was ok) Message explaining why login failed.
exports.createLoginReplyMessage = function(playerId, errorMessage)
{	
	message = Message.createMessage(Commands.Login);
	if(playerId != -1)
		message.writeInt(playerId);
	else
	{
		message.writeInt(-1);
		message.writeString(errorMessage);
	}
	message.applyWrites(2);
	
	return message;
}

//Parser for Login messages
function messageLoginParser(message, server)
{
	message.username = message.readString();
	message.password = message.readString();
	
	console.log("Got login with username: " + message.username + " and password: " + message.password);
	
	//Emit parsed message event
	server.emit('parsedMessageLogin', message);
}

//Register parser with MessageHandler
MessageHandler.registerMessageParser(Commands.Login, messageLoginParser);