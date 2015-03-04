var MessageHandler = require('../messagehandler');
var Commands = require('../commands').CommandEnum;
var Message = require('./message');


//Format:
//Sender player id(int)
//includeSender(byte 1/0) - Whether to also send to sender
//Message(byte[]) - The serialized message


//Parser for Broadcast messages
function messageBroadcastParser(message, server)
{
    //We are just to overwrite the sender player id and then forward it
	//console.log("Got Broadcast request");
    
    //Skip PlayerId and read includeSender
    message.readInt();
    
    var includeSender = message.readByte();
    if(includeSender == 1)
        message.includeSender = true;
    else   
        message.includeSender = false;
    
	
	//Emit parsed message event
	server.emit('parsedMessageBroadcast', message);
}

//Register parser with MessageHandler
MessageHandler.registerMessageParser(Commands.Broadcast, messageBroadcastParser);