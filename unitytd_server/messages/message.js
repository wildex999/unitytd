//Create a class for messages, with functions to read the message
function Message(dataBuffer)
{
	this.buffer = dataBuffer;
	this.writeList = new Array();
	this.command = dataBuffer.readUInt16LE(0);
	this.offset = 2;
	this.fromSocket = null; //Socket of the user who sent the message. Set in server.js when a message is received.
	
	/*getSocket(user?)

	//Will internally maintain an offset pointer, so you can call readByte twice in a row, and not get the same byte.
	readByte
	readShort
	readInt
	unsigned etc. //Remember to check endianness, have option. All targets use little-endian, but network is big-endian(Same for nodejs?)
	readString //a string will start with a 2 byte length, and then the string defined by it's length
	getCurrentBuffer //Return the remaining of the buffer using Buffer.slice*/
}

Message.messageLengthSize = 2; //Size of message Length description(in bytes)

//Create a new message with command
Message.createMessage = function(command) {
	var newBuffer = new Buffer(Message.messageLengthSize);
	newBuffer.writeUInt16LE(command, 0);
	var newMessage = new Message(newBuffer);
	
	return newMessage;
}

//command = Command
Message.prototype.setCommand = function(command) {
	this.command = command;
	if(this.buffer != null) //Write new command
		this.buffer.writeUInt16LE(this.command, 0);
}

Message.prototype.getCommand = 	function() {
	return this.command;
} 

Message.prototype.getOffset = function() {
	return this.offset;
}

Message.prototype.setOffset = function(newOffset) {
	this.offset = newOffset;
}

Message.prototype.getSize = function() {
	return this.buffer.length;
}

Message.prototype.readByte = function() {
	var out = this.buffer.readInt8(this.offset);
	this.offset += 1;
	return out;
}

Message.prototype.readShort = function() {
	var out = this.buffer.readInt16LE(this.offset);
	this.offset += 2;
	return out;
}

Message.prototype.readUShort = function() {
	var out = this.buffer.readUInt16LE(this.offset);
	this.offset += 2;
	return out;
}

Message.prototype.readInt = function() {
	var out = this.buffer.readInt32LE(this.offset);
	this.offset += 4;
	return out;
}

Message.prototype.readUInt = function() {
	var out = this.buffer.readUInt32LE(this.offset);
	this.offset += 4;
	return out;
}

Message.prototype.readString = function() {
	var stringLength = this.readUShort();
	var out = this.buffer.toString('utf8', this.offset, this.offset + stringLength);
	this.offset += stringLength;
	return out;
}

Message.prototype.writeByte = function(value) {
	var partBuffer = new Buffer(1);
	partBuffer.writeInt8(value, 0);
	this.writeList.push(partBuffer);
}

//Add Int to pending writes
Message.prototype.writeInt = function(value) {
	var partBuffer = new Buffer(4);
	partBuffer.writeInt32LE(value, 0);
	this.writeList.push(partBuffer);
}

//Add string to pending writes
Message.prototype.writeString = function(string) {
	var lengthBuffer = new Buffer(2);
	var stringLength = Buffer.byteLength(string, 'utf8');
	lengthBuffer.writeUInt16LE(stringLength, 0);
	this.writeList.push(lengthBuffer);
	
	var stringBuffer = new Buffer(stringLength);
	stringBuffer.write(string, 0, stringLength, 'utf8');
	this.writeList.push(stringBuffer);
}

//Apply any writes to the internal buffer starting at the given offset
Message.prototype.applyWrites = function(offset) {
	//Get existing string with start 0 and end = offset, and add to beginning of writeList
	//do buffer concat to create a new buffer from all the parts
	var startBuffer = this.buffer.slice(0, offset);
	this.writeList.unshift(startBuffer);
	this.buffer = Buffer.concat(this.writeList);
	console.log("Final buffer: " + this.buffer);
}

module.exports = Message;