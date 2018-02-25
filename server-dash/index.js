var app = require('express')();
var admin = require("firebase-admin");
var serviceAccount = require("/Users/vishesh/Documents/ohno-wordflood/server-dash/adminsdk-key.json");
var http = require('http').Server(app);
var io = require('socket.io')(http);

admin.initializeApp({
  credential: admin.credential.cert(serviceAccount),
  databaseURL: "https://wordflood-bf7c4.firebaseio.com"
});

var db = admin.database();
var ref = db.ref("WFLogs_V0_1_2");

ref.orderByChild("key").equalTo("WF_Submit").limitToLast(1).on("child_added", function(snapshot, prevChildKey) {
  var newPost = snapshot.val();
  var postPayload = newPost.payload;
  // console.log("Author: " + newPost.author);
  // console.log("Title: " + newPost.title);
  // console.log("Previous Post ID: " + prevChildKey);
  console.log(postPayload);
  io.sockets.emit("newWord", postPayload);
});

app.get('/', function(req, res){
  // res.send('<h1>Hello world</h1>');
  res.sendFile(__dirname + '/index.html');
});



io.on('connection', function(socket){
  console.log('a user connected');
  socket.on('disconnect', function(){
    console.log('user disconnected');
  });
});

http.listen(3000, function(){
  console.log('listening on *:3000');
});