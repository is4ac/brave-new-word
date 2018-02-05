var admin = require("firebase-admin");

// Fetch the service account key JSON file contents
var serviceAccount = require("/Users/vishesh/Documents/ohno-wordflood/server-dash/adminsdk-key.json");

// Initialize the app with a service account, granting admin privileges
admin.initializeApp({
  credential: admin.credential.cert(serviceAccount),
  databaseURL: "https://wordflood-bf7c4.firebaseio.com"
});

// As an admin, the app has access to read and write all data, regardless of Security Rules
var db = admin.database();
var ref = db.ref("words");
// ref.once("value", function(snapshot) {
//   console.log(snapshot.val());
// });

ref.on("child_added", function(snapshot, prevChildKey) {
  var newPost = snapshot.val();
  // console.log("Author: " + newPost.author);
  // console.log("Title: " + newPost.title);
  // console.log("Previous Post ID: " + prevChildKey);
  console.log(newPost);
});
