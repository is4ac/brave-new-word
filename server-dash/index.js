var app = require('express')();
var admin = require("firebase-admin");
var serviceAccount = require("/Users/vishesh/Documents/ohno-wordflood/server-dash/adminsdk-key.json");
var http = require('http').Server(app);
var io = require('socket.io')(http);
const assert = require('assert');

// const mongoose = require('mongoose');
const MongoClient = require('mongodb').MongoClient;
 
// mongoose.connect('mongodb://localhost/my_database');
const url = 'mongodb://localhost:27017';
const dbName = 'wordflood';

var mongoose = require('mongoose');
mongoose.connect('mongodb://localhost:27017/test');

// Use connect method to connect to the Server

admin.initializeApp({
  credential: admin.credential.cert(serviceAccount),
  databaseURL: "https://wordflood-bf7c4.firebaseio.com"
});


var fdb = admin.database();
var ref = fdb.ref("WFLogs_V1_0_2");


// ref.orderByChild("key").equalTo("WF_Submit").limitToLast(25).on("child_added", function(snapshot, prevChildKey) {
ref.limitToLast(25).on("child_added", function(snapshot, prevChildKey) {
  var newPost = snapshot.val();
  var postPayload = newPost.payload;
  // console.log("Author: " + newPost.author);
  // console.log("Title: " + newPost.title);
  // console.log("Previous Post ID: " + prevChildKey);
  // console.log(newPost);
  io.sockets.emit("newWord", postPayload);
  if (newPost.key == "WF_Submit") {
    console.log("Word " + newPost.payload.word + "success: " + newPost.payload.success + "Points: " + newPost.payload.scoreTotal);
    console.log();
  }

  if (newPost.key == "WF_GameState") {

  }

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