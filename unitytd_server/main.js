var Server = require('./server');
var MessageHandler = require('./messagehandler');
var MessageLogin = require('./messages/messagelogin');
var MessageGetGame = require('./messages/messagegetgame');
var MessageCreateGame = require('./messages/messagecreategame');
var MessageJoinGame = require('./messages/messagejoingame');
var MessageLog = require('./messages/messagelog');
var MessagePlayerJoined = require('./messages/messageplayerjoined');
var MessagePlayerLeft = require('./messages/messageplayerleft');
var MessageBroadcast = require('./messages/messagebroadcast');
var MessageSendMessage = require('./messages/messagesendmessage');

var Commands = require('./commands').CommandEnum;
var GameInfo = require('./gameinfo');
var User = require('./user');
var Map = require('./map');

var fs = require('fs');
var domain = require('domain');
var async = require('async');

//Players
loggedInPlayers = {}; //Hashmap of players logged in
var playerLogsPath = "playerLogs/";

//Current game version(Used to only allow fully updated games to connect)
var currentGameVersion = 0;

//Create custom games group
customGames = {};
currentGameId = 0;

//Friends group
//The root group for all friend circles
//Each player is the owner of his own friends group. Everyone who is a friend of this player is a member of this friends group.


//MAIN
console.log("Starting server...");

//Make sure the logging directory is there
fs.stat(playerLogsPath, function (err, stats) {
	if(err)
	{
		//Try to create the directory
		fs.mkdir(playerLogsPath, function() {
			console.log("Created dir: " + playerLogsPath);
		});
	}
});

mainServer = new Server();
messageHandler = new MessageHandler(mainServer);
//MAIN END


//Login message handler
var nextPlayerId = 0;
mainServer.on('parsedMessageLogin', function handleLoginMessage(message) {
	console.log("Parsed Login message");
	var playerId = nextPlayerId++;

	//TODO: Get player id from player profile on storage. An ID is given to a player upon creating an account.
	var loginReply = null;

	if(playerId in loggedInPlayers) //Check if player already logged in
	{
		loginReply = MessageLogin.createLoginReplyMessage(-1, "You are already logged in! Try again later.");
		//TODO: Start a check on the logged in player to reduce waiting time before "lost" connections are timed out.
	}
	else
	{
        console.log("Login given player id: " + playerId);
		loginReply = MessageLogin.createLoginReplyMessage(playerId);
		var userObj = new User(message.username, playerId);
		loggedInPlayers[playerId] = userObj;
		
		//Put player object on the socket for future quick reference
		message.fromSocket.user = userObj;
		userObj.socket = message.fromSocket;
		
		//Create a log for the session
		var date = new Date();
		var filename = userObj.name + "-" + userObj.socket.remoteAddress + "-" + date.getDate() + "D" + date.getMonth() + "M" + date.getYear() + "Y" + date.getHours() + "h" + date.getMinutes() + "m";
		
		//Catch any errors
		var d = domain.create();
		d.run(function () {
			//TODO: SECURITY: Clean this so no username will be able to damage the system.
			userObj.userLog = fs.createWriteStream(playerLogsPath + filename);
		});
		
		d.on('error', function(err) {
			console.log("Error creating player log: " + err);
		});
	}
	
	//Send reply to player
	mainServer.sendMessage(message.fromSocket, loginReply);
});

//GetGame message handler
mainServer.on('parsedMessageGetGame', function handleGetGameMessage(message) {
	console.log("Parsed GetGame message");
	
	if(message.fromSocket.user == null)
	{
		console.log("Not logged in!");
		replyMessage = MessageGetGame.createGetGameReplyMessage(null);
	}
	
	//Get game if it exists
	var replyMessage = null;
	var game = customGames[message.gameName];
	if(game == null)
	{
		console.log("No game found with name");
		replyMessage = MessageGetGame.createGetGameReplyMessage(null);
	}
	else
	{	
		replyMessage = MessageGetGame.createGetGameReplyMessage(game);
	}
	
	mainServer.sendMessage(message.fromSocket, replyMessage);
});

//CreateGame handler
//Create a new group as subgroup of custom games.
//Add the game info/settings as group data
mainServer.on('parsedMessageCreateGame', function handleCreateGameMessage(message) {
	console.log("Parsed CreateGame message");
	
	if(message.fromSocket.user == null)
	{
		console.log("Not logged in!");
		var replyMessage = MessageCreateGame.createCreateGameReplyMessage(0, 0, "Not logged in!");
		mainServer.sendMessage(message.fromSocket, replyMessage);
		return;
	}

	var map = null;
	if(message.mapId != 0)
	{
		//TODO: Get map from mapId and check if it's valid(An Async request to a DB?)
		console.log("Error: Non-custom map not yet implemented!");
		var replyMessage = MessageCreateGame.createCreateGameReplyMessage(0, 0, "Non-custom map not yet implemented!!");
		mainServer.sendMessage(message.fromSocket, replyMessage);
		return;
	}
	else
	{
		//TODO: Make client send map information(Name and version) when creating game with 0 id map.
		map = new Map(message.mapName, 0, 1);
	}
	
	//Check if a game with that name already exists(TODO: Allow multiple games with same name, and use unique id as key)
	if(customGames[message.gameName] != null)
	{
		console.log("Game with name already exists: " + message.gameName);
		var replyMessage = MessageCreateGame.createCreateGameReplyMessage(0, 0, "A game with the given name already exists!");
		mainServer.sendMessage(message.fromSocket, replyMessage);
		return;
	}
	
	//If all is ok with the given data, create the game
	var user = message.fromSocket.user;
	var newGameId = currentGameId++; //TODO: Have list of released game id's to pick from when creating new game. If list is empty then get from currentGameId count.
	
	var newGame = new GameInfo(message.gameName, newGameId, message.gameVersion, user, message.maxPlayers, message.gameMode, map);
	customGames[message.gameName] = newGame;
	
	//Send the reply message
	var replyMessage = MessageCreateGame.createCreateGameReplyMessage(1, newGameId, "");
	mainServer.sendMessage(message.fromSocket, replyMessage);
});

mainServer.on('parsedMessageJoinGame', function handleJoinGameMessage(message) {
	console.log("Parsed JoinGame message");
	
	var user = message.fromSocket.user;
	if(user == null)
	{
		console.log("Not logged in!");
		var replyMessage = MessageJoinGame.createJoinGameReplyMessage(null, "Not logged in!");
		mainServer.sendMessage(message.fromSocket, replyMessage);
		return;
	}
	
	var gameInfo = customGames[message.gameName];
	if(gameInfo == null)
	{
		console.log("No game with given name exists: " + message.gameName);
		var replyMessage = MessageJoinGame.createJoinGameReplyMessage(null, "Game does not exist");
		mainServer.sendMessage(message.fromSocket, replyMessage);
		return;
	}
	
	//TODO: Check if game is full
	
	user.setCurrentGame(gameInfo);
	
	var replyMessage = MessageJoinGame.createJoinGameReplyMessage(gameInfo);
	mainServer.sendMessage(message.fromSocket, replyMessage);
	
});

//Log handler
//Write log to player log file
//TODO, move towards a single logfile per player, and simply append to it.
mainServer.on('parsedMessageLog', function handleLogMessage(message) {

	var user = message.fromSocket.user;
	if(user == null)
		return;
	
	var date = new Date();
	user.writeLog(date.getDate() + "D" + date.getHours() + "h" + date.getMinutes() + "m" + date.getSeconds() + "s :: " + message.logType);
	user.writeLog(message.logMessage);
	user.writeLog("|--------------------------------------------------|"); //Try to separate them visibly
});

mainServer.on('parsedMessageBroadcast', function handleBroadcastMessage(message) {
    
    //Only a logged in player in a game can broadcast
    var user = message.fromSocket.user;
    if(user == null || user.currentGame == null)
    {
        if(user != null)
            user.disconnect("Error: Sent Broadcast in an invalid state!(Outside game)");
        else
            message.fromSocket.end();
        return;
    }
    
    //Write in the id of the sender(So original sender can not spoof/forge messages),
    //and then send it to everyone.
    message.buffer.writeInt32LE(user.id, 2); //The first two bytes are the command id
    
    //console.log("Sending broadcasts. Include Sender: " + message.includeSender);
    
    for(var userId in user.currentGame.players)
    {
        var gameUser = user.currentGame.players[userId];
        
        if(gameUser == user && message.includeSender != true)
            continue;
        
        mainServer.sendMessage(gameUser.socket, message);
    }
    
});

mainServer.on('parsedMessageSendMessage', function handleSendMessage(message) {
    console.log("Parsed send message");
    
    //Only a logged in player in a game can send a message
    var user = message.fromSocket.user;
    if(user == null || user.currentGame == null)
    {
        console.log("User or game = null" + user);
        //User should not be null, and must be in a game.
        if(user != null)
            user.disconnect("Error: Sent message in an invalid state!(Outside game)");
        else
            message.fromSocket.end();
        return;
    }
    
    var game = user.currentGame;
    //Can only send a message to another playr in the same game
    var recipient = game.getPlayer(message.recipientUserId);
    if(recipient == null)
        return; //Recipient can possibly have left after we sent the message, so just ignore.
    
    //Write id of sender to avoid spoofing/forging of messages.
    message.buffer.writeInt32LE(user.id, 2);
    mainServer.sendMessage(recipient.socket, message);
    console.log("Sent to recipient");
});

