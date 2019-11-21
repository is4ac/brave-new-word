using System;

[Serializable]
public class LogEntry {
	[Serializable]
	public class Payload {
	}

	[Serializable]
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
	public string userID;
	public string username; // e.g. Tranquil Red Panda
	public string timestamp; // the date and time that it was played
	public double timestampEpoch; // epoch time in milliseconds
    public float remainingTime; // remaining time in game seconds
	public string gameID;
    public bool obstructionProductive;
    public bool obstructionUnproductive;
    public bool juiceProductive;
    public bool juiceUnproductive;
	public string deviceModel;
	//public string location;

	// attributes that must be assigned
	public string key; // "WD_LetterSelected", "WD_LetterDeselected", "WD_DeselectAll", "WD_Submit"
	public string parentKey; // "WD_Action", "WD_KeyFrame", "WD_Meta"

	public LogEntry() {
		logVersion = GameManagerScript.LOGGING_VERSION;
		appVersion = GameManagerScript.APP_VERSION;
		userID = GameManagerScript.userID;
		username = GameManagerScript.username;
		gameID = GameManagerScript.GAME_ID;
        obstructionProductive = GameManagerScript.OBSTRUCTION_PRODUCTIVE;
        obstructionUnproductive = GameManagerScript.OBSTRUCTION_UNPRODUCTIVE;
        juiceProductive = GameManagerScript.JUICE_PRODUCTIVE;
        juiceUnproductive = GameManagerScript.JUICE_UNPRODUCTIVE;
		deviceModel = GameManagerScript.deviceModel;
		timestamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
		timestampEpoch = ((DateTime.UtcNow - GameManagerScript.epochStart).TotalMilliseconds); // epoch time in milliseconds
        remainingTime = GameManagerScript.remainingTime;
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

[Serializable]
public class MetaLogEntry : LogEntry {
	[Serializable]
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

[Serializable]
public class DeselectWordLogEntry : LogEntry {
	[Serializable]
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

[Serializable]
public class SubmitWordLogEntry : LogEntry {
	[Serializable]
	public class SubmitWordPayload : Payload {
		public string word;
		public long scoreTotal;
		public long scoreBase;
		public bool success;
		public float rarity;
		public LetterPayload[] letters;
        public float submitPromptTimer;

        public SubmitWordPayload()
        {
            submitPromptTimer = GameManagerScript.submitPromptTimer;
        }
    }

	public SubmitWordPayload payload;
    public double timeTakenInMilliseconds;

    public SubmitWordLogEntry() {
        // set the time taken since previous word submission
        timeTakenInMilliseconds = base.timestampEpoch - GameManagerScript.previousSubmissionTime;
        GameManagerScript.previousSubmissionTime = base.timestampEpoch;
    }

	public void setValues(string key, string parentKey, SubmitWordPayload payload) {
		base.setValues (key, parentKey);
		this.payload = payload;
	}
}

[Serializable]
public class ClickPlayWordButtonLogEntry : LogEntry
{
    [Serializable]
    public class ClickPlayWordButtonPayload : Payload
    {
        public string word;
        public float rarity;
        public long score;

        public ClickPlayWordButtonPayload(string word, float rarity, long score)
        {
            this.word = word;
            this.rarity = rarity;
            this.score = score;
        }
    }

    public ClickPlayWordButtonPayload payload;

    public void setValues(string key, string parentKey, ClickPlayWordButtonPayload payload)
    {
        base.setValues(key, parentKey);
        this.payload = payload;
    }
}

[Serializable]
public class CancelPlayWordLogEntry : LogEntry
{
    [Serializable]
    public class CancelPlayWordPayload : Payload
    {
        public string word;
        public float rarity;
        public long score;
        public float submitPromptTimer;

        public CancelPlayWordPayload(string word, float rarity, long score)
        {
            this.word = word;
            this.rarity = rarity;
            this.score = score;
            submitPromptTimer = GameManagerScript.submitPromptTimer;
        }
    }

    public CancelPlayWordPayload payload;

    public void setValues(string key, string parentKey, CancelPlayWordPayload payload)
    {
        base.setValues(key, parentKey);
        this.payload = payload;
    }
}

[Serializable]
public class KeyFrameLogEntry : LogEntry {
	[Serializable]
	public class KeyFramePayload : Payload {
		public string board;
		public float timeElapsed;
		public long totalScore;
		public int wordsPlayed;
		public int totalInteractions;
		public string state; // pre, post, gameStart, gamePause, or gameEnd
	}

	public KeyFramePayload payload;

	public void setValues(string key, string parentKey, KeyFramePayload payload) {
		base.setValues (key, parentKey);
		this.payload = payload;
	}
}

[Serializable]
public class ClickLogEntry : LogEntry {
    [Serializable]
    public class ClickPayload : Payload {
        public float screenX;
        public float screenY;
    }

    public ClickPayload payload;

    public void setValues(string key, string parentKey, ClickPayload payload) {
        base.setValues(key, parentKey);
        this.payload = payload;
    }
}