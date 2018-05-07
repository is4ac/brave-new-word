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
    , contrib = require('../server-dash/blessed-deps');
// var contrib = require('blessed-contrib');

var screen = blessed.screen();

var grid = new contrib.grid({rows: 16, cols: 16, screen: screen})

// var map = grid.set(0, 0, 4, 4, contrib.map, {label: 'World Map'})
var log = grid.set(8, 6, 4, 2, contrib.log, 
  { fg: "green"
  , selectedFg: "green"
  , label: 'Submitted Words'})


// MongoClient.connect(db.url, (err, client) => {
//   if (err) return console.log(err)
//   else {
//     // console.log(database);
//     // console.log(db);
//     console.log("connection success!")
//     const mdb = client.db(dbName)
//   }
//   // Make sure you add the database name and not the collection name
//   // db = database.db("wordflood")
//   // console.log(mdb);

// })
// console.log(mdb);


// mdb.collection('notes').find(details, (err, item) => {
//   if (err) {
//     console.log("error~ " + err);
//   }
//   else {
//     console.log(item);
//   }
// })

// var mongoose = require('mongoose');
// mongoose.connect('mongodb://localhost:27017/test');
// var db = mongoose.connection;
// db.on('error', console.error.bind(console, 'connection error:'));
// db.once('open', function() {
//   console.log("connected to server!");
// });

// var loginSchema = mongoose.Schema({
//   username: String;
//   timestamp: Number;
//   day: Number;
//   hour: Number;
//   minute: Number;
//   second: Number;
// });
// Use connect method to connect to the Server

admin.initializeApp({
  credential: admin.credential.cert(serviceAccount),
  databaseURL: "https://wordflood-bf7c4.firebaseio.com"
});


// var listOfPlayers = db.userConnects.find({}, {"username": 1}, {tailable:true, timeout:false});

// listOfPlayers.on('data', function (doc) {
//   console.log(doc);
// });



var fdb = admin.database();
var ref = fdb.ref("WFLogs_V1_0_2");

// ref.orderByChild("key").equalTo("WF_Submit").limitToLast(25).on("child_added", function(snapshot, prevChildKey) {
// ref.limitToLast(25).on("child_added", function(snapshot, prevChildKey) {
ref.on("child_added", function(snapshot, prevChildKey) {
  var newPost = snapshot.val();
  var postPayload = newPost.payload;
  // console.log("Author: " + newPost.author);
  // console.log("Title: " + newPost.title);
  // console.log("Previous Post ID: " + prevChildKey);
  // console.log(newPost);
  io.sockets.emit("newWord", postPayload);
  if (newPost.key == "WF_Submit") {
    // console.log("Word " + newPost.payload.word + " success: " + newPost.payload.success + " Points: " + newPost.payload.scoreTotal);
    // lo
    // console.log();
  }

  else if (newPost.key == "WF_GameState") {

  }


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

// userList = db.userConnects.distinct("username", {});
// var userList = []
// console.log(userList);
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
    
    db.collection('userConnects').distinct(
       "username",
       {}, // query object
       (function(err, docs){
            if(err){
                // return console.log(err);
            }
            if(docs){  
                // console.log(docs);
            }
       })
    );

  // console.log(db.userConnects.distinct("username", {}), (function (err, doc) {
  //     if (err) {
  //       console.log(err);
  //     }
  //     else {
  //       console.log(doc);
  //     }
  //   }) 
  // );
  // au = false
  // db.userConnects.findOne({"key": "activeUsers"}, function (err, doc) {
  //   if (err) {
  //     console.log(err);
  //   }
  //   else {
  //     console.log(doc);
  //     au = true;
  //   }
  // });
  // if (au == true) {
  //   db.userConnects.update(
  //     {"key": "activeUsers",
  //     "user": username}, 
  //     {$addToSet: {}}, 
  //     function (err, doc) {
  //       if (err) {
  //         console.log(err);
  //       }
  //       else {
  //         console.log(doc);
  //       }
  //     }
  //   );
  //   // console.log("adding to active users list");
  // }
  // else {
  //   // console.log("making active users list");
  //   db.userConnects.insert({"key": "activeUsers", "userList": [username]}, function (err, doc) {
  //     console.log(doc);
  //   });
  // }
  // // if (userList.length == 0) {
  // //   userList = db.userConnects.distinct("username", {});
  // //   console.log(userList);
  // // }
  // // if 
  // console.log(db.userConnects.findOne({"key": "activeUsers"}, function (err, doc) {}));
}

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