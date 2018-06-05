using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class GameManagerScript : MonoBehaviour {

	// Set to true to log to Firebase database, false to turn off
	public const bool LOGGING = true;
	public const string LOGGING_VERSION = "WFLogs_V1_0_3_DEBUG";
	//public const string LOGGING_VERSION = "WFLogs_DEBUG";
	public const string APP_VERSION = "WF_1.0.3_DEBUG";

	CamShakeSimpleScript camShake;
	//private const int NUM_OF_PATHS = 6;
	public enum Versions { SwipeUI, ButtonUI, ButtonTimeUI };
	/*
	private static Versions[][] allPaths = { 
		new Versions[] { Versions.SwipeUI, Versions.ButtonUI, Versions.SwipeUI },
		new Versions[] { Versions.ButtonUI, Versions.SwipeUI, Versions.ButtonUI },
		new Versions[] { Versions.SwipeUI, Versions.SwipeUI, Versions.SwipeUI },
		new Versions[] { Versions.ButtonUI, Versions.ButtonUI, Versions.ButtonUI },
		new Versions[] { Versions.ButtonUI, Versions.ButtonUI, Versions.SwipeUI },
		new Versions[] { Versions.SwipeUI, Versions.SwipeUI, Versions.ButtonUI } 
	};
	private static Versions[] currentPath;
	private static int versionIndex;
	*/
	public static Versions currentVersion;
	GameObject playButton;
	GameObject nextButton;
	static Text usernameText;
	//Text timerText;
	public static int GAME_ID;
	public static string username;
	public static int userID;
	public static string deviceModel;
	static bool areBoxesFalling = true;
	public static GameObject gameOverPanel = null;
	static bool gameHasBegun = false;
	static bool initialLog = true;

	/**********************************
	 * Feature: Analyzing board state and immediate feedback of relative rarity of word using a trie
	 **********************************/
	public static Trie trie;

	/**********************************
	 * Feature: Timer in between words
	 **********************************/
	static float timer = 10; // 10 seconds to play a word
	static bool timerEnded = false;
	Text timerText;

	void Awake() {
		gameOverPanel = GameObject.Find ("GameOver");
		gameOverPanel.SetActive (false);
	}

	// Use this for initialization
	void Start () {
		deviceModel = SystemInfo.deviceModel;

		// Using time since epoch date as unique game ID
		System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
		GAME_ID = (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;
		Debug.Log ("Game id: " + GAME_ID);

		BoxScript.camShake = gameObject.AddComponent<CamShakeSimpleScript> ();
		playButton = GameObject.Find ("PlayButton");
		nextButton = GameObject.Find ("NextStageButton");
		if (nextButton != null) {
			nextButton.SetActive (false);
		}
		username = PlayerPrefs.GetString ("username");

		// randomize userID
		userID = Random.Range (0, int.MaxValue);

		// TODO: check to see that userID is unique

		// display the username on the screen
		usernameText = GameObject.Find("UsernameText").GetComponent<Text>();
		usernameText.text = username;

		currentVersion = (Versions) Random.Range (0, 2);

		// check version and hide/show Play Word button depending on version
		if (currentVersion == Versions.SwipeUI) {
			playButton.SetActive (false);
		} else {
			playButton.SetActive (true);
		}

		Debug.Log ("Version: " + currentVersion);

		// do various logging for the start of the game
		if (LOGGING) {
			Debug.Log ("Logging beginning of game");
			// Log beginning of game
			MetaLogEntry entry = new MetaLogEntry ();
			entry.setValues ("WF_GameStart", "WF_Meta", new MetaLogEntry.MetaPayload("start"));
			string json = JsonUtility.ToJson (entry);
			DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference (LOGGING_VERSION);
			reference.Push ().SetRawJsonValueAsync (json);

			Debug.Log ("logging user info");
			// insert new user entry into database
			reference = FirebaseDatabase.DefaultInstance.GetReference ("users");
			DatabaseReference child = reference.Child (username);
			child.Child ("gameType").SetValueAsync ((int)currentVersion);
			child.Child ("gameID").SetValueAsync (GAME_ID);
			child.Child ("userID").SetValueAsync (userID);
			child.Child ("loggingVersion").SetValueAsync (LOGGING_VERSION);
			child.Child ("appVersion").SetValueAsync (APP_VERSION);
		}
	}
	
	// Update is called once per frame
	void Update () {
		/*********************************
		 * Feature: Timer
		 *********************************/
		/*
		// timer start
		if (GameHasStarted() && !timerEnded) {
			timer -= Time.deltaTime;

			if (timer <= 0) {
				timer = 0;
				timerEnded = true;
			}

			DisplayTime ();
		}

		if (timerEnded) {
			// do something here
		}
		*/
		/********************************
		 * End Feature: Timer
		 ********************************/

		// if the enter key is pressed, then submit the word
		// check against dictionary and give it points
		if (currentVersion == Versions.ButtonUI && Input.GetKeyDown (KeyCode.Return)) {
			PlayWord ();
		}

		if (Input.GetKeyDown(KeyCode.Escape)) { 
			SceneManager.LoadScene(0);
		}

		// Log the keyframe (game state) after all the boxes have stopped falling
		if (areBoxesFalling) {
			if (!checkIfBoxesAreFalling ()) {
				areBoxesFalling = false;

				if (!initialLog) {
					LogKeyFrame ("postSubmit");
				} else {
					LogKeyFrame ("gameStart");
					initialLog = false;
					//AnalyzeGameBoard ();
				}
			}
		} else {
			if (checkIfBoxesAreFalling ()) {
				areBoxesFalling = true;
			}
		}

		// check to see if the game is over (aka, MAX_SCORE is reached)
		if (BoxScript.score >= BoxScript.MAX_SCORE) {
			// TODO: game over
		}

		/*
		// timer start
		if (SpawnBoxScript.isInitialized() && !timerEnded) {
			timer -= Time.deltaTime;

			if (timer <= 0) {
				timer = 0;
				timerEnded = true;
			}

			DisplayTime ();
		}

		if (timerEnded) {
			nextButton.SetActive (true);
		}
		*/
	}

	static bool checkIfBoxesAreFalling() {
		bool falling = false;

		for (int i = 0; i < BoxScript.gridWidth; ++i) {
			if (BoxScript.IsBoxInColumnFalling (i) || !BoxScript.IsColumnFull(i)) {
				falling = true;
				break;
			}
		}

		return falling;
	}

	void DisplayTime() {
		int minutes = Mathf.FloorToInt(timer / 60F);
		int seconds = Mathf.FloorToInt(timer - minutes * 60);
		string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);
		timerText.text = niceTime;
	}

	// Play word!
	public void PlayWord() {
		BoxScript.PlayWord ();
		Debug.Log ("Play word pressed.");
	}

	public static void BeginGame() {
		gameHasBegun = true;
	}

	public static bool GameHasStarted() {
		return gameHasBegun;
	}

	public void Reset() {
		/*
		timer = 10; // 5 minutes in seconds
		timerEnded = false;
		*/

		GameObject boxes = GameObject.Find ("SpawnBoxes");

		foreach (Transform child in boxes.transform) {
			child.gameObject.GetComponent<SpawnBoxScript> ().Reset ();
		}

		BoxScript.Reset ();

		int scene = SceneManager.GetActiveScene().buildIndex;
		SceneManager.LoadScene(scene); // eventually delete the key "currentPath"
		// PlayerPrefs.DeleteKey("currentPath");
	}

	public static void LogKeyFrame(string state) {
		Debug.Log ("Logging full game state");

		// log the current full game state
		KeyFrameLogEntry entry = new KeyFrameLogEntry ();
		KeyFrameLogEntry.KeyFramePayload payload = new KeyFrameLogEntry.KeyFramePayload ();

		payload.board = BoxScript.GetBoardPayload();
		payload.totalScore = BoxScript.score;
		payload.timeElapsed = Time.time;
		payload.totalInteractions = BoxScript.totalInteractions;
		payload.wordsPlayed = BoxScript.wordsPlayed;
		payload.state = state;

		entry.setValues ("WF_GameState", "WF_KeyFrame", payload);
		string json = JsonUtility.ToJson (entry);
		DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference (LOGGING_VERSION);
		reference.Push ().SetRawJsonValueAsync (json);
	}

	public static void LogEndOfGame() {
		Debug.Log ("Logging end of game");

		// log the end game state when the player finished the round
		LogKeyFrame("gameEnd");

		// log the end of the game
		MetaLogEntry metaEntry = new MetaLogEntry ();
		metaEntry.setValues ("WF_GameEnd", "WF_Meta", new MetaLogEntry.MetaPayload ("end"));
		DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference (LOGGING_VERSION);
		string json = JsonUtility.ToJson (metaEntry);
		reference.Push ().SetRawJsonValueAsync (json);
	}

	/**
	 * User or the OS force quits the application
	 */
	public static void LogGamePause() {
		Debug.Log("Application pausing after " + Time.time + " seconds");

		// log the game state before game pauses
		LogKeyFrame("gamePause");

		// log the pause
		MetaLogEntry metaEntry = new MetaLogEntry ();
		metaEntry.setValues ("WF_GamePaused", "WF_Meta", new MetaLogEntry.MetaPayload ("pause"));
		DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference (LOGGING_VERSION);
		string json = JsonUtility.ToJson (metaEntry);
		reference.Push ().SetRawJsonValueAsync (json);
	}

	public static void LogGameUnpause() {
		// log the unpause
		MetaLogEntry metaEntry = new MetaLogEntry ();
		metaEntry.setValues ("WF_GameUnpaused", "WF_Meta", new MetaLogEntry.MetaPayload ("unpause"));
		DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference (LOGGING_VERSION);
		string json = JsonUtility.ToJson (metaEntry);
		reference.Push ().SetRawJsonValueAsync (json);
	}

	void OnApplicationPause(bool pauseStatus)
	{
		if (pauseStatus) {
			GameManagerScript.LogGamePause ();
		} else {
			GameManagerScript.LogGameUnpause ();
		}
	}



	/**
	 * TODO: This part should be done post-hoc, after gathering log data, and maybe convert this process into
	 * a tree data structure to make it more efficient??
	 */
    /**
     * Build a Trie!
     */
    public static void LoadTrie()
    {
        trie = new Trie();

        foreach (string word in BoxScript.freqDictionary.Keys)
        {
            trie.Insert(word);
        }

        Debug.Log("Finished loading Trie");
        //Debug.Log ("Checking if cat is in Trie: " + trie.Search ("cAt"));
    }

    /**
	 * Analyze the game board and generate statistics for the potential game values
	 * Returns a dictionary of statistics, value pairs:
	 * <"max", float> // max possible score from a word
	 * <"min", float> // min possible score from a word
	 * <"median", float> // median score
	 * <"mean", float> // mean score
	 * <"count", int> // number of possible words
	 */
	public static Dictionary<string, float> AnalyzeGameBoard() {
		Dictionary<string, float> results = new Dictionary<string, float>();

		char[,] letters = BoxScript.GetBoardLetters();

		// generate all possible words from the board state
		List<string> allWords = AllPossibleWords(letters);

		// calculate max possible score and min possible score
		foreach (string word in allWords) {
			Debug.Log (word);
		}

		return results;
	}

	public static List<string> AllPossibleWords(char[,] letters) {
		List<string>[,] results = new List<string>[letters.GetLength (0), letters.GetLength (1)];
		List<string> words = new List<string> ();

		// TODO: Parallelize this section
		for (int i = 0; i < letters.GetLength (0); ++i) {
			for (int j = 0; j < letters.GetLength (1); ++j) {
				results[i, j] = AllPossibleWordsHelper (letters, i, j);
			}
		}

		// combine all results into one List
		for (int i = 0; i < letters.GetLength (0); ++i) {
			for (int j = 0; j < letters.GetLength (1); ++j) {
				words = words.Union (results [i, j]).ToList ();
			}
		}

		return words;
	}

	public static int[] ConvertSingleIndexToDoubleIndex(int index, int maxJ) {
		int[] result = new int[2];
		result [0] = index / maxJ;
		result [1] = index % maxJ;

		return result;
	}

	public static int ConvertDoubleIndexToSingleIndex(int i, int j, int maxJ) {
		return i * maxJ + j;
	}

	public static List<string> AllPossibleWordsHelper(char[,] letters, int i, int j) {
		List<string> words = new List<string> ();
		List<int> indexes = new List<int> ();
		int startIndex = ConvertDoubleIndexToSingleIndex(i, j, letters.GetLength (1));
		indexes.Add (startIndex);
		char[] flattenedLetters = FlattenArray (letters);

		// use recursion to check all neighbors
		CheckNeighbors(words, trie._root, indexes, flattenedLetters, startIndex, letters.GetLength(0), letters.GetLength(1));

		return words;
	}

	public static void CheckNeighbors(List<string> words, Node currentNode, List<int> indexes, char[] letters, int index, int maxI, int maxJ) {
		List<int> neighbors = GetNeighbors (maxI, maxJ, index);

		foreach (int neighbor in neighbors) {
			List<int> newIndexes = new List<int> (indexes);
			Node nextNode = currentNode.FindChildNode(letters[neighbor]);

			// neighboring letter does not exist in trie
			if (nextNode == null) {
				continue;
			} else {
				// check to see if neighbor already exists in newIndexes
				if (newIndexes.Contains (neighbor)) {
					continue;
				}

				newIndexes.Add (neighbor);

				// this current word is a valid word in the dictionary
				if (nextNode.FindChildNode ('$') != null) {
					words.Add (StringFromIndexes(newIndexes, letters));
				}

				// end this search if nextNode is a leaf node
				if (nextNode.IsLeaf ()) {
					continue;
				}

				CheckNeighbors (words, nextNode, newIndexes, letters, neighbor, maxI, maxJ);
			}
			
		}
	}

	public static string StringFromIndexes(List<int> indexes, char[] letters) {
		string word = "";

		foreach (int index in indexes) {
			word += letters [index];
		}

		return word;
	}

	public static List<int> GetNeighbors(int maxI, int maxJ, int index) {
		List<int> neighbors = new List<int> ();
		int[] ij = ConvertSingleIndexToDoubleIndex (index, maxJ);
		int i = ij [0];
		int j = ij [1];

		// check left
		if (i - 1 >= 0) {
			neighbors.Add (ConvertDoubleIndexToSingleIndex (i - 1, j, maxJ));

			// check upper left
			if (j + 1 < maxJ) {
				neighbors.Add(ConvertDoubleIndexToSingleIndex (i - 1, j + 1, maxJ));
			}

			// check lower left
			if (j - 1 >= 0) {
				neighbors.Add(ConvertDoubleIndexToSingleIndex (i - 1, j - 1, maxJ));
			}
		}

		// check right
		if (i + 1 < maxI) {
			neighbors.Add(ConvertDoubleIndexToSingleIndex (i + 1, j, maxJ));

			// check upper right
			if (j + 1 < maxJ) {
				neighbors.Add(ConvertDoubleIndexToSingleIndex (i + 1, j + 1, maxJ));
			}

			// check lower right
			if (j - 1 >= 0) {
				neighbors.Add(ConvertDoubleIndexToSingleIndex (i + 1, j - 1, maxJ));
			}
		}

		// check up
		if (j + 1 < maxJ) {
			neighbors.Add (ConvertDoubleIndexToSingleIndex (i, j + 1, maxJ));
		}

		// check down
		if (j - 1 >= 0) {
			neighbors.Add(ConvertDoubleIndexToSingleIndex (i, j - 1, maxJ));
		}

		return neighbors;
	}

	public static char[] FlattenArray(char[,] array) {
		char[] result = new char[array.GetLength (0) * array.GetLength (1)];
		int index = 0;

		for (int i = 0; i < array.GetLength (0); ++i) {
			for (int j = 0; j < array.GetLength (1); ++j) {
				result [index] = array [i, j];
				++index;
			}
		}

		return result;
	}

	/***************************************************
	 * Functions for checking for combos, initializing the main board, etc
	 ***************************************************/

	/*
	bool CheckValidWord(int startX, int startY, int endX, int endY) {
		string word = "";

		// check to see if word is in column or row
		if (startX == endX) {
			// column
			if (startY < endY) {
				// forward
				for (int y = startY; y <= endY; ++y) {
					word += initialBoard [startX, y];			
				}
			} else {
				// reverse
				for (int y = startY; y >= endY; --y) {
					word += initialBoard [startX, y];
				}
			}
		} else if (startY == endY) {
			// row
			if (startX < endX) {
				//forward
				for (int x = startX; x <= endX; ++x) {
					word += initialBoard [x, startY];
				}
			} else {
				// reverse
				for (int x = startX; x >= endX; --x) {
					word += initialBoard [x, startY];
				}
			}
		} else {
			Debug.Log ("Error: CheckValidWord only checks rows or columns");
			return false;
		}

		return BoxScript.IsValidWord (word);
	}

	void MarkFlaggedBoard(int startX, int startY, int endX, int endY) {
		if (startX < endX) {
			for (int x = startX; x <= endX; ++x) {
				flaggedBoard [x, startY] = true;
			}
		} else {
			for (int y = startY; y <= endY; ++y) {
				flaggedBoard [startX, y] = true;
			}
		}
	}

	bool ContainsNoValidWords(char[,] board) {
		// check all Ngrams to see if they contain valid words
		bool flag = true;
		int rLength = board.GetLength (0);
		int cLength = board.GetLength (1);

		// check rows
		for (int row = 0; row < board.GetLength (1); ++row) {
			for (int n = 0; n < rLength; ++n) {
				for (int len = 3; (n+len) <= rLength; ++len) {
					// TODO: check to see if any of the letters are flagged

					// check forwards and backwards
					if (CheckValidWord (n, row, n + len - 1, row) 
						|| CheckValidWord (n + len - 1, row, n, row)) {
						flag = false;

						MarkFlaggedBoard (n, row, n + len - 1, row);
					}
				}
			}
		}

		// check columns
		for (int col = 0; col < board.GetLength (0); ++col) {
			for (int n = 0; n < cLength; ++n) {
				for (int len = 3; (n+len) <= cLength; ++len) {
					// TODO: check to see if any of the letters are flagged

					// check forwards and backwards
					if (CheckValidWord (col, n, col, n + len - 1) 
						|| CheckValidWord (col, n + len - 1, col, n)) {
						flag = false;

						MarkFlaggedBoard (col, n, col, n + len - 1);
					}
				}
			}
		}

		return flag;
	}

	void RerollLetters() {
		// randomize all letters that are marked "true" in flaggedBoard
		for (int x = 0; x < flaggedBoard.GetLength (0); ++x) {
			for (int y = 0; y < flaggedBoard.GetLength (1); ++y) {
				if (flaggedBoard [x, y]) {
					int i = Random.Range (0, letterFreq.Count);
					initialBoard [x, y] = (char)(letterFreq [i] + 'A');
				}
			}
		}
	}

	void ResetFlaggedBoard() {
		for (int x = 0; x < flaggedBoard.GetLength (0); ++x) {
			for (int y = 0; y < flaggedBoard.GetLength (1); ++y) {
				flaggedBoard [x, y] = false;
			}
		}
	}

	// DEBUGGING PURPOSES ONLY
	void PrintInitialBoard() {
		for (int row = 0; row < initialBoard.GetLength (1); ++row) {
			string rowStr = "";
			for (int col = 0; col < initialBoard.GetLength (0); ++col) {
				rowStr += initialBoard [col, row];
			}

			Debug.Log (rowStr);
		}
	}
	*/
}
