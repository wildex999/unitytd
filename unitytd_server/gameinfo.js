var MessagePlayerJoined = require('./messages/messageplayerjoined');
var MessagePlayerLeft = require('./messages/messageplayerleft');
var MessageKickPlayer = require('./messages/messagekickplayer');

//Info about a running game

//Create new game group
//name = string
//gameId = int
//gameVersion = int
//owner = User
//maxPlayers = int
//mode = string
//map = Map
function GameInfo(name, gameId, gameVersion, owner, maxPlayers, gameMode, map)
{
	this.gameName = name;
	this.gameId = gameId;
	this.gameVersion = gameVersion;
	this.owner = owner;
	this.maxPlayers = maxPlayers;
	this.gameMode = gameMode;
	this.map = map;
    
    this.isActive = true;
	
	this.players = {};
    
    owner.setCurrentGame(this);
 }
 
 //player = User
 GameInfo.prototype.addPlayer = function(player)
 {
	if(player.id in this.players || this.isActive == false)
		return;
 
	//Inform players
	var message = MessagePlayerJoined.createPlayerJoinedMessage(player);
	this.broadcast(message);
	
	console.log("AddPlayer: " + player.name);
	this.players[player.id] = player;
 }
 
 //player = User
 //reason = string(TODO: enum?)
 GameInfo.prototype.removePlayer = function(player, reason)
 {
	if(!(player.id in this.players) && player != this.owner || this.isActive == false)
		return;
 
	//If player is owner, end the game
	if(player == this.owner)
	{
        this.endGame();
	}
	else
	{
		delete this.players[player.id];
		//Broadcast player disconnect
		var message = MessagePlayerLeft.createPlayerLeftMessage(player);
		this.broadcast(message);
	}
 }
 
 //Get player in game by id
 //playerId = int
 //Return: User if found, or null if no player with that Id is in the game
 GameInfo.prototype.getPlayer = function(playerId)
 {
     for(var userId in this.players)
     {
         if(userId == playerId)
             return this.players[userId];
     }
     return null;
 }
 
 //Set a new map, and broadcast the map change to all members
 //map = Map
 GameInfo.prototype.setMap = function(map)
 {	
	
 }
 
 //Broadcast a message to all players in game
 //message = Message
 //notPlayer = User - User who should not be sent to
 GameInfo.prototype.broadcast = function(message, notPlayer)
 {    
	//Send to everyone
     for(var userId in this.players)
     {
         var user = this.players[userId];
         if(user == notPlayer)
             continue;
         mainServer.sendMessage(user.socket, message);
     }
     
 }
 
 //Ends the game, informing all players.
 GameInfo.prototype.endGame = function()
 {
     this.isActive = false; 
     
     var kickMessage = MessageKickPlayer.createKickPlayerMessage("Host disconnected!");
     
     for(var userId in this.players)
     {
         var user = this.players[userId];
         user.setCurrentGame(null);
         
         //Inform the player(Kick them)
         mainServer.sendMessage(user.socket, kickMessage);
     }
     
     //Remove the game from list
     delete customGames[this.gameName]; //TODO: use Id
     console.log("Removed game: " + this.gameId);
 }
 
 module.exports = GameInfo;