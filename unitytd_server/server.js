var events = require('events');
var net = require('net');
var domain = require('domain');
var Message = require('./messages/message');

function Server()
{
	events.EventEmitter.call(this); //Call EventEmitter constructor
	var self = this;
    var errorDomain = domain.create();

	this.server = net.createServer(function(c) {
		console.log("Client connected: " + c.remoteAddress + ":" + c.remotePort);
		
		c.parseBuffer = new Buffer(0);
		
		c.on('end', function () {
			console.log("Client ended connection(FIN)");
		});
		
		c.on('data', function(data) {
			//console.log("Data: " +  data.length);
            errorDomain.run(function parseInDomain() {
                //concat new buffer data to current buffer
                c.parseBuffer = Buffer.concat([c.parseBuffer, data]);
                
                var continueParse = true;
                while(continueParse == true) //Continue parsing as long as we have data
                {
                    continueParse = parseStream(self, data, c)
                }
            });
		});
        
        c.on('close', function(had_error) {
            var client = "";
            if(c.user == null)
                client = "Client";
            else
                client = c.user.name;
            
            if(had_error)
                console.log(client + " disconnected due to error!");
            else
                console.log(client + " disconnected.");
            
            if(c.user == null)
                return;
			
			c.user.userLog.close();
			
			//Disconnect user from any running games
			if(c.user.currentGame != null)
			{
				c.user.currentGame.removePlayer(c.user, "Disconnect");
				c.user.currentGame = null;
			}
			//TODO: Allow player to re-join within a timelimit
        });
        
        c.on('error', function(error) {
            console.log("Client error: " + error);
        });
	});
	this.server.listen(12000, function() {
		console.log("Server started!");
	});


}

//Server inherit from EventEmitter
Server.prototype.__proto__ = events.EventEmitter.prototype;

//Parse a stream, and call message events as it parses messages
//A message consists of:
//a 2 byte header defining the message length.(So the whole message is downloaded before it's passed on)
//a 2 byte command
//x bytes of data parsed further down depending on command(Arguments)
//Return true if whole message was parsed
function parseStream(server, dataBuffer, fromSocket)
{
	var parseBuffer = fromSocket.parseBuffer;
	
	//Check if we have the message length
	if(parseBuffer.length < Message.messageLengthSize)
    {
        //console.log("Current data: " + parseBuffer.length + ". Waiting for more");
		return false; //Wait for more data
    }
	var messageLength = parseBuffer.readUInt16LE(0);
	//console.log("Got message length: " + messageLength);
	
	//Check if we have all the data for message
	if(parseBuffer.length >= messageLength + Message.messageLengthSize)
	{
		//Create new Message and put data into it
		var newBuffer = new Buffer(messageLength);
		parseBuffer.copy(newBuffer, 0, Message.messageLengthSize, messageLength + Message.messageLengthSize);
		var newMessage = new Message(newBuffer);
		newMessage.fromSocket = fromSocket;
		
		//Emit event with the new message
		if(!server.emit('message', newMessage))
			console.log("Warning: No message listener for message - " + newMessage.getCommand());
		
		//Copy any remaining data in parseBuffer to a new buffer
		var newParseBufferLength = parseBuffer.length - (messageLength + Message.messageLengthSize);
		if(newParseBufferLength > 0)
		{
			var newParseBuffer = new Buffer(newParseBufferLength);
			parseBuffer.copy(newParseBuffer, 0, (messageLength + Message.messageLengthSize));
			fromSocket.parseBuffer = newParseBuffer;
			//console.log("Remaining data: " + parseBuffer.length);
		}
		else
		{
			fromSocket.parseBuffer = new Buffer(0);
		}
		
	}
    else
        return false; //Need more data
    
    return true;
}

var messageSizeBuffer = new Buffer(2);
Server.prototype.sendMessage = function(socket, message)
{	
	//First write the message size
	messageSizeBuffer.writeUInt16LE(message.getSize(), 0);
	socket.write(messageSizeBuffer);
	socket.write(message.buffer);
	/*if(socket.write(message.buffer))
		console.log("Send message: All data flushed");
	else
		console.log("Send message: Some data not flushed");*/
}

module.exports = Server;