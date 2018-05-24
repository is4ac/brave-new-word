using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class LogEntry {
	[System.Serializable]
	public class Payload {
	}

	[System.Serializable]
	public class LetterPayload : Payload {
		public string letter;
		public int x;
		public int y;

		public LetterPayload() {
		}

		public LetterPayload(string letter, int x, int y) {
			this.letter = letter;
			this.x = x;
			this.y = y;
		}

		public void setValues(string letter, int x, int y) {
			this.letter = letter;
			this.x = x;
			this.y = y;
		}
	}

	// attributes handled internally
	public string logVersion;
	public string appVersion;
	public int userID;
	public string username; // e.g. Tranquil Red Panda
	public string timestamp; // the date and time that it was played
	public double timestampEpoch;
	public int gameID;
	public int gameType;
	public string deviceModel;
	public string location;

	// attributes that must be assigned
	public string key; // "WD_LetterSelected", "WD_LetterDeselected", "WD_DeselectAll", "WD_Submit"
	public string parentKey; // "WD_Action", "WD_KeyFrame", "WD_Meta"

	public LogEntry() {
		logVersion = GameManagerScript.LOGGING_VERSION;
		appVersion = GameManagerScript.APP_VERSION;
		userID = GameManagerScript.userID;
		username = GameManagerScript.username;
		gameID = GameManagerScript.GAME_ID;
		gameType = (int) GameManagerScript.currentVersion;
		deviceModel = GameManagerScript.deviceModel;
		timestamp = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
		System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
		timestampEpoch = ((System.DateTime.UtcNow - epochStart).TotalMilliseconds); // epoch time in milliseconds

		// TODO:
		location = "";
	}

	public void setValues(string key, string parentKey) {
		this.key = key;
		this.parentKey = parentKey;
	}
}

public class LetterLogEntry : LogEntry {
	public LetterPayload payload;

	public void setValues(string key, string parentKey, LetterPayload payload) {
		base.setValues (key, parentKey);
		this.payload = payload;
	}
}

[System.Serializable]
public class MetaLogEntry : LogEntry {
	[System.Serializable]
	public class MetaPayload : Payload {
		public string value;

		public MetaPayload(string value) {
			this.value = value;
		}
	}

	public MetaPayload payload;

	public void setValues(string key, string parentKey, MetaPayload payload) {
		base.setValues (key, parentKey);
		this.payload = payload;
	}
}

[System.Serializable]
public class DeselectWordLogEntry : LogEntry {
	[System.Serializable]
	public class DeselectWordPayload : Payload {
		public string word;
		public LetterPayload[] letters;
	}

	public DeselectWordPayload payload;

	public void setValues(string key, string parentKey, DeselectWordPayload payload) {
		base.setValues (key, parentKey);
		this.payload = payload;
	}
}

[System.Serializable]
public class SubmitWordLogEntry : LogEntry {
	[System.Serializable]
	public class SubmitWordPayload : Payload {
		public string word;
		public int scoreTotal;
		public int scoreBase;
		public bool success;
		public float frequency;
		public LetterPayload[] letters;
	}

	public SubmitWordPayload payload;

	public void setValues(string key, string parentKey, SubmitWordPayload payload) {
		base.setValues (key, parentKey);
		this.payload = payload;
	}
}

[System.Serializable]
public class KeyFrameLogEntry : LogEntry {
	[System.Serializable]
	public class KeyFramePayload : Payload {
		public string board;
		public float timeElapsed;
		public int totalScore;
		public int wordsPlayed;
		public int totalInteractions;
		public string state; // pre, post (submit word), gameStart, or gameEnd
	}

	public KeyFramePayload payload;

	public void setValues(string key, string parentKey, KeyFramePayload payload) {
		base.setValues (key, parentKey);
		this.payload = payload;
	}
}