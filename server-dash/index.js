var app = require('express')();
var admin = require("firebase-admin");
var serviceAccount = require("/Users/vishesh/Documents/ohno-wordflood/server-dash/adminsdk-key.json");
var http = require('http').Server(app);
var io = require('socket.io')(http);
const assert = require('assert');
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

var blessed = require('blessed')
    // , contrib = require('../server-dash/blessed-deps');
var contrib = require('blessed-contrib');

var screen = blessed.screen();

var grid = new contrib.grid({rows: 16, cols: 16, screen: screen})

var log = grid.set(0, 0, 6, 6, contrib.log, 
  { fg: "green"
  , selectedFg: "green"
  , label: 'Submitted Words'})

var table =  grid.set(0, 7, 8, 5, contrib.table, 
  { keys: true
  , fg: 'green'
  , label: 'Active Users'
  , columnSpacing: 1
  , columnWidth: [38, 10]}
)

admin.initializeApp({
  credential: admin.credential.cert(serviceAccount),
  databaseURL: "https://wordflood-bf7c4.firebaseio.com"
});


var fdb = admin.database();
var ref = fdb.ref("WFLogs_V1_0_2");

// ref.orderByChild("key").equalTo("WF_Submit").limitToLast(25).on("child_added", function(snapshot, prevChildKey) {
// ref.limitToLast(25).on("child_added", function(snapshot, prevChildKey) {
ref.limitToLast(1).on("child_added", function(snapshot, prevChildKey) {
  var newPost = snapshot.val();
  var postPayload = newPost.payload;
  // console.log("Author: " + newPost.author);
  // console.log("Title: " + newPost.title);
  // console.log("Previous Post ID: " + prevChildKey);
  // console.log(newPost);
  io.sockets.emit("newWord", postPayload);
  if (newPost.key == "WF_Submit") {
    // console.log("Word " + newPost.payload.word + " success: " + newPost.payload.success + " Points: " + newPost.payload.scoreTotal);
    log.log("Word " + newPost.payload.word + " success: " + newPost.payload.success + " Points: " + newPost.payload.scoreTotal);
    // console.log();
  }

  else if (newPost.key == "WF_GameState") {

  }
  // console.log(newPost);
  if (db.allLogs.findOne(newPost, function (err, doc) {
    if (err) {

    }
    else {
      if (doc == undefined) {
        db.allLogs.insert(newPost);
      }
      else {
        // console.log("exists");
      }
    }
  }))
  

  if (newPost.parentKey == "WF_Action"){
    if (newPost.hasOwnProperty("username")) {
      // console.log(newPost["username"]);
      updateUserList(newPost);
    }
    else{
      // console.log(newPost);
    }
  }

});

function populateSubmitLog() {
  db.allLogs.find(
    {"key": "WF_Submit"},
    {"username": 1,
    "payload": 1}
  ).limit(5).sort({"timestampEpoch": -1}, function (err, docs) {
    if (err) {
      console.log(err);
    }
    else {
      for (i = 0; i < 5; i++){
        doc = docs[i];
        
        if (doc.payload != undefined){
          // console.log(doc);
          log.log("Word " + doc.payload.word + " success: " + doc.payload.success + " Points: " + doc.payload.scoreTotal);
        }  
      }
      
    }
  });
  
}

function refreshUserList() {
  db.allLogs.distinct(
     "username",
     {}, // query object
     function(err, docs){
      if (err) {}
      else {
        // console.log(typeof(docs) + "   tt.   ");
        // console.log(docs[0]);
        // userlist.push(docs);
        refreshUserTable(docs);
      }
     }
  );
}

function getScore (username, callback) {
  db.allLogs.find(
    {$and: [{"key": "WF_GameState"},
            {"username": username}]}
  ).limit(1).sort({"timestampEpoch": -1}, score = function (err, docs) {
  // db.allLogs.findOne({$and: [{"key": "WF_GameState"}, {"username": username}]}, {$orderBy: {"timestampEpoch": -1}}, function (err, res) {
    if (err) {console.log(err); callback(-5);}
    else {
      if (docs.length > 0) {
        res = docs[0];
        if (res.payload != undefined){
          // console.log(res.payload.totalScore);
          score = res.payload.totalScore;
          callback(score);
        }
        else {
          // console.log(res);
          // score = 0;
          callback(-1);
        }
      }
    }
  });

  // return score;
}

function refreshUserTable(userlist) {
  var data = [];
  // var userlist = [];

  // console.log(userlist);
  if(userlist != undefined){
    // console.log(userlist.length);
    for (var i = 0; i < userlist.length; i++) {
      var row = [];         
      row.push(userlist[i]);
      
      score = 0;
      score = getScore(userlist[i], function (res) {
        // console.log(res);
        // row.push(res);
        // score = res;
        return res;
      });
      // console.log(score);
      row.push(0);
      db.userConnects
      // row.push()
      // row.push()
      data.push(row);
      // console.log(data);
    }
  }
  // console.log(data);
  
  table.setData({headers: ['Username', 'Score'], data: data})
}

table.focus();

table.on('select',function(node){
  if (node.myCustomProperty){
    console.log(node.myCustomProperty);
  }
  console.log(node.name);
});

setInterval(function () {
  refreshUserList();
  screen.render();
}, 2000);
populateSubmitLog();

updateUserList = function (newPost) {

  db.userConnects.update(
    {"username": newPost.username},
    {$set: {
      "lastActionEpoch": newPost.timestampEpoch,
      "lastActionTime": newPost.timestamp
    } },
    {upsert: true},
    function(err, docs) {}
  );
  refreshUserTable();    
}



app.get('/', function(req, res){
  // res.send('<h1>Hello world</h1>');
  res.sendFile(__dirname + '/index.html');
});



io.on('connection', function(socket){
  // console.log('a user connected');
  socket.on('disconnect', function(){
    // console.log('user disconnected');
  });
});

http.listen(3000, function(){
  console.log('listening on *:3000');
});

screen.key(['escape', 'q', 'C-c'], function(ch, key) {
  return process.exit(0);
});

// fixes https://github.com/yaronn/blessed-contrib/issues/10
screen.on('resize', function() {
  // donut.emit('attach');
  // gauge.emit('attach');
  // gauge_two.emit('attach');
  // sparkline.emit('attach');
  // bar.emit('attach');
  table.emit('attach');
  // lcdLineOne.emit('attach');
  // errorsLine.emit('attach');
  // transactionsLine.emit('attach');
  // map.emit('attach');
  log.emit('attach');
});

screen.render()
