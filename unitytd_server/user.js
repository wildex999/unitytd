var MessageDisconnect = require('./messages/messagedisconnect');


function User(name, id)
{
	this.name = name;
	this.id = id;
	
	this.socket = null;
	this.userLog = null;
	
	this.currentGame = null; //The current game this user is in(GameInfo)
}

//game = GameInfo
User.prototype.setCurrentGame = function(game) {
	//Remove from current game
	if(this.currentGame != null)
	{
		this.currentGame.removePlayer(this, "Left the game");
		this.currentGame = null;
	}
	
	if(game == null)
		return;
		
	//Add to new game
	game.addPlayer(this);
	this.currentGame = game;
}

User.prototype.writeLog = function(log) {
	if(this.userLog == null)
		return;
	this.userLog.write(log + "\n");
}

User.prototype.disconnect = function(reason) {
    if(this.socket == null)
        return; //Already disconnected
    
    //Create disconnect message
    var message = MessageDisconnect.createDisconnectMessage(reason);
    
    mainServer.sendMessage(this.socket, message);
    this.socket.end();
    this.socket = null; //When not connected, set socket to null
}

module.exports = User;