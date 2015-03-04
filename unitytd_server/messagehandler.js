
function MessageHandler(server)
{
	this.mainServer = server;
	server.on('message', function(message) {
		//Call the parser for the message using command
		var parser = MessageHandler.messageParsers[message.getCommand()];
		if(parser == null)
		{
			console.log("Warning: No parser for message with command " + message.getCommand());
			return;
		}
		parser(message, server);
	});
}

//Global list(hashmap) of message parsers
MessageHandler.messageParsers = {};

//Static function for registering a new Message parser
//commandId: The command the parser will handle
MessageHandler.registerMessageParser = function(commandId, parseMethod)
{	
	MessageHandler.messageParsers[commandId] = parseMethod;
}


module.exports = MessageHandler;