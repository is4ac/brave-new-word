import firebase_admin
from firebase_admin import credentials
from firebase_admin import db

# Fetch the service account key JSON file contents
cred = credentials.Certificate('adminsdk-key.json')

# Initialize the app with a custom auth variable, limiting the server's access
firebase_admin.initialize_app(cred, {
    'databaseURL': 'https://wordflood-bf7c4.firebaseio.com/'
})

# The app only has access as defined in the Security Rules
ref = db.reference('/words')
print(ref.get())
lastTime = 0
snapshot = ref.order_by_child('dateTime').start_at(0).get()
