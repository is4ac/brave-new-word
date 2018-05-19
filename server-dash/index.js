var express = require('express');
var app = express();
var admin = require("firebase-admin");
var serviceAccount = require("./adminsdk-key.json");
var http = require('http').Server(app);
var io = require('socket.io')(http);
const assert = require('assert');

app.use(express.static('public/'));

// const db = require('./config/db');
// var ObjectID = require('mongodb').ObjectID

// var mdb = {};

// const mongoose = require('mongoose');
const MongoClient = require('mongodb').MongoClient;
 
// mongoose.connect('mongodb://localhost/my_database');
const url = 'mongodb://localhost:27017';
const dbName = 'wordflood';
var mongojs = require('mongojs');

var db = mongojs("mongodb://localhost:27017/wordflood");


admin.initializeApp({
  credential: admin.credential.cert(serviceAccount),
  databaseURL: "https://wordflood-bf7c4.firebaseio.com"
});


var fdb = admin.database();
var ref = fdb.ref("WFLogs_V1_0_2");

// ref.orderByChild("key").equalTo("WF_Submit").limitToLast(25).on("child_added", function(snapshot, prevChildKey) {
// ref.limitToLast(25).on("child_added", function(snapshot, prevChildKey) {
// ref.limitToLast(1).on("child_added", function(snapshot, prevChildKey) {
ref.on("child_added", function(snapshot, prevChildKey) {  
  var newPost = snapshot.val();
  var postPayload = newPost.payload;
  if (newPost.key == "WF_Submit") {
  }

  else if (newPost.key == "WF_GameState") {
    if (newPost.hasOwnProperty("username")) {
      // console.log(newPost["username"]);
      updateUserList(newPost);
    }
  }
  // console.log(newPost);
  db.allLogs.findOne(newPost, function (err, doc) {
    if (err) {

    }
    else {
      if (doc == undefined) {
        db.allLogs.insert(newPost);
        if (newPost.key == "WF_Submit") {
          io.sockets.emit('new-logs', newPost);
        }
        // else if (newPost.parentKey == "WF_Action"){
        else if (newPost.key == "WF_GameState"){
          if (newPost.hasOwnProperty("username")) {
            // console.log(newPost["username"]);
            updateUserList(newPost);
          }
          else{
            // console.log(newPost);
          }
        }
      }
      else {
        // console.log("exists");
      }
    }
  })
});



updateUserList = function (newPost) {
  db.allLogs.update({$and: [
    {"username": newPost.username},
    {"key": "dash_ScoreLog"},
    {"parentKey": "WF_Dashboard"}]}, 
  {$set: {
    "score": newPost.payload.totalScore, 
    "wordsPlayed": newPost.payload.wordsPlayed,
    "fullLog": newPost}
  }, {upsert: true, safe: false},
  function (err, data) {
    if (err) {console.log(err)}
  });
  setUsers();
}

setUsers = function () {
  latest = db.allLogs.find({"key": "dash_ScoreLog"});
  latest.toArray(function (err, res) {
    if (err){ return err; }
    else {
      // console.log("new user");
      // console.log(res);
      io.sockets.emit("setScoreList", res);
    }
  });
}

app.get('/', function(req, res){
  // res.send('<h1>Hello world</h1>');
  res.sendFile(__dirname + '/index.html');
});



io.on('connection', function(socket){
  console.log('a user connected');

  socket.broadcast.emit('hi');

  socket.on('join', function () {
    // console.log("joined in");
    latest = db.allLogs.find({"key": "WF_Submit"}).limit(5);
    latest.toArray(function (err, res) {
      if (err){
        return err;
      }
      else {
        // console.log("new entry");
        // console.log(res);
        for (i = 0; i < res.length; i++) {
          io.emit('new-logs', res[i]);
        }
      }
     });

    setUsers();
    
  });

  socket.on('getUserMoves', function (msg) {
    console.log("make user scores: " + msg);
    latest = db.allLogs.find({$and: [
      {"key": "WF_Submit"}, 
      {"username": msg}]
    }).sort({"payload.scoreTotal": -1})
    .limit(5)
    .toArray(function (err, res) {
      if (err) {console.log(err)}
      else {socket.emit("user-moves",res);}
    });
    // return latest
    
  })

  socket.on('disconnect', function(){
    // console.log('user disconnected');
  });
});

http.listen(3000, function(){
  console.log('listening on *:3000');
});
