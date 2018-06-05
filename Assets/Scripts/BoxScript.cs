using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class BoxScript : MonoBehaviour {

	public static float gridWidthRadius = 2.5f;
	public static int gridHeightRadius = 4;
	public static int gridWidth = (int)(gridWidthRadius*2) + 1; // -2.5 to 2.5 
	public static int gridHeight = gridHeightRadius*2 + 1; // -4 to 4
	public static Transform[,] grid = new Transform[gridWidth, gridHeight];
	public static List<Vector2> currentSelection = new List<Vector2> ();
	public static Text scoreText = null;
	public static Text submittedWordText = null;
	public static Text submittedScoreText = null;
    public static Text selectedScore = null;
	public static int score = 0;
	private const float FALL_SPEED_CONST = 0.15f;
	public static string currentWord = "";
    public static Dictionary<string,float> freqDictionary = null;
	public static CamShakeSimpleScript camShake = null;
	public static int totalInteractions = 0;
	public static int wordsPlayed = 0;
	public static bool touchEnabled = false;

	public static float MAX_SCORE = 1000f;

	static BoxScript instance = null;

	string myLetter;
	float fall = 0f;
	bool falling = true;
	bool columnFalling = false;
	int myX = 0;
	int myY = 0;
	bool isSelected = false;

	[NonSerialized]
	public float fallSpeed = FALL_SPEED_CONST;

	void Awake() {
		if (instance == null) {
			instance = this;
		}
	}

	// Use this for initialization
	void Start () {
		// Add the location of the block to the grid
		Vector2 v = transform.position;
		myX = (int)(v.x + gridWidthRadius);
		myY = (int)v.y + gridHeightRadius;
		grid[myX, myY] = transform;
		falling = true;
		columnFalling = false;
		fall = Time.time;
		myLetter = GetLetterFromPrefab (this.gameObject.name);

		if (scoreText == null) {
			scoreText = GameObject.Find("Score").GetComponent<Text>();
		}

		if (submittedWordText == null) {
			submittedWordText = GameObject.Find ("SubmittedWord").GetComponent<Text> ();
		}

		if (submittedScoreText == null) {
			submittedScoreText = GameObject.Find ("SubmittedScore").GetComponent<Text> ();
		}

        if (selectedScore == null) {
            selectedScore = GameObject.Find("SelectedScore").GetComponent<Text>();

        }

		if (!IsValidPosition ()) {
			//SceneManager.LoadScene (0);
			Destroy (gameObject);
		}
	}

	// Update is called once per frame
	void Update () {
		if (touchEnabled) {
			// ButtonUI touch inputs
			if (GameManagerScript.currentVersion == GameManagerScript.Versions.ButtonUI &&
			   Input.touchCount > 0 && IsInsideTile (Input.GetTouch (0).position)) {
				//Debug.Log ("Inside Tile worked!");

				if (Input.GetTouch (0).phase == TouchPhase.Began) {
					// there is no previously clicked box
					if (currentSelection.Count == 0) {
						SelectThisTile ();
						LogAction ("WF_LetterSelected", myLetter, myX, myY);
					} else if (IsNextTo (currentSelection [currentSelection.Count - 1]) &&
					          !currentSelection.Contains (new Vector2 (myX, myY))) {
						// add on to the current selection 
						SelectThisTile ();
						LogAction ("WF_LetterSelected", myLetter, myX, myY);
					} else {
						// de-select what has already been selected
						ClearAllSelectedTiles ();
						LogAction ("WF_DeselectAll");
					}
				} else if (Input.GetTouch (0).phase == TouchPhase.Moved) {
					// selected tile and it isn't already selected)
					if (currentSelection.Count > 0 &&
					   IsNextTo (currentSelection [currentSelection.Count - 1]) &&
					   !currentSelection.Contains (new Vector2 (myX, myY))) {
						SelectThisTile ();
						LogAction ("WF_LetterSelected", myLetter, myX, myY);
					} else if (currentSelection.Contains (new Vector2 (myX, myY))) {
						// de-select the most recent tile(s) if you move back to an old one
						for (int i = currentSelection.Count - 1; i > 0; --i) {
							if (currentSelection [currentSelection.Count - 1] != new Vector2 (myX, myY)) {
								RemoveLastSelection ();
							} else {
								break;
							}
						}
					}
				}
			}
			// SwipeUI touch input
			else if (GameManagerScript.currentVersion == GameManagerScript.Versions.SwipeUI &&
			        Input.touchCount > 0 && IsInsideTile (Input.GetTouch (0).position)) {
				//Debug.Log ("Inside Tile worked!");

				if (Input.GetTouch (0).phase == TouchPhase.Began) {
					// there is no previously clicked box
					if (currentSelection.Count == 0) {
						SelectThisTile ();
						LogAction ("WF_LetterSelected", myLetter, myX, myY);
					} else {
						// de-select what has already been selected
						ClearAllSelectedTiles ();
						LogAction ("WF_DeselectAll");
					}
				} else if (Input.GetTouch (0).phase == TouchPhase.Moved) {
					// selected tile and it isn't already selected)
					if (IsNextTo (currentSelection [currentSelection.Count - 1]) &&
					   !currentSelection.Contains (new Vector2 (myX, myY))) {
						SelectThisTile ();
						LogAction ("WF_LetterSelected", myLetter, myX, myY);
					} else if (currentSelection.Contains (new Vector2 (myX, myY))) {
						// de-select the most recent tile(s) if you move back to an old one
						for (int i = currentSelection.Count - 1; i > 0; --i) {
							if (currentSelection [currentSelection.Count - 1] != new Vector2 (myX, myY)) {
								RemoveLastSelection ();
							} else {
								break;
							}
						}
					} else {
						// just do nothing?
					}
				}

				// If SwipeUI, automatically play word when lifting the finger, and cancel if canceled for all UI's
				else if (Input.GetTouch (0).phase == TouchPhase.Ended && isSelected) {
					PlayWord ();
				} else if (Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Canceled && isSelected) {
					ClearAllSelectedTiles ();
					LogAction ("WF_DeselectAll");
				}
			}


		}
			
		// check to see if the column needs to go down, or if it needs to be refilled
		if (!falling && myY > 0 && grid [myX, myY - 1] == null && Time.time - fall >= fallSpeed) {
			if (!IsOtherBoxInColumnFalling ()) {
				ColumnDown ();
				fall = Time.time;
			}
		} else if (columnFalling && ((myY > 0 && grid [myX, myY - 1] != null) || myY == 0)) {
			columnFalling = false;
		}

		// If a tile is falling down the screen...
		if (falling && Time.time - fall >= fallSpeed) {
			transform.position += new Vector3(0, -1, 0);

			if (IsValidPosition()) {
				GridUpdate();
			} else {
				transform.position += new Vector3 (0, 1, 0);
				falling = false;
				fallSpeed = FALL_SPEED_CONST;
			}
			fall = Time.time;
		}
	}

	// WF_LetterSelected or WF_LetterDeselected logging
	static void LogAction(string key, string letter, int x, int y) {
		if (GameManagerScript.LOGGING) {
			Debug.Log ("Attempts to log data");
			LogEntry.LetterPayload payload = new LogEntry.LetterPayload ();
			payload.setValues (letter, x, y);
			LetterLogEntry entry = new LetterLogEntry ();
			entry.setValues (key, "WF_Action", payload);
			string json = JsonUtility.ToJson (entry);
			DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference (GameManagerScript.LOGGING_VERSION);
			DatabaseReference child = reference.Push ();
			child.SetRawJsonValueAsync (json);

			++totalInteractions;
			//Debug.Log (json);
		}
	}

	// WF_DeselectAll logging
	static void LogAction(string key) {
		if (GameManagerScript.LOGGING) {
			Debug.Log ("Attempts to log data");
			LogEntry.LetterPayload[] letters = GetLetterPayloadsFromCurrentWord ();
			DeselectWordLogEntry.DeselectWordPayload wordPayload = new DeselectWordLogEntry.DeselectWordPayload ();
			wordPayload.word = currentWord;
			wordPayload.letters = letters;
			DeselectWordLogEntry entry = new DeselectWordLogEntry ();
			entry.setValues (key, "WF_Action", wordPayload);
			string json = JsonUtility.ToJson (entry);
			DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference (GameManagerScript.LOGGING_VERSION);
			DatabaseReference child = reference.Push ();
			child.SetRawJsonValueAsync (json);

			++totalInteractions;
		}
	}

	static LogEntry.LetterPayload[] GetLetterPayloadsFromCurrentWord() {
		LogEntry.LetterPayload[] toRet = new LogEntry.LetterPayload[currentSelection.Count];
		for (int i = 0; i < currentSelection.Count; ++i) {
			toRet [i] = new LogEntry.LetterPayload (currentWord [i]+"", (int)currentSelection [i].x, (int)currentSelection [i].y);
		}

		return toRet;
	}
		
	// Click on blocks to select them
	/*
	void OnMouseDown() {
		// regular left mouse click
		if (false && GameManagerScript.currentVersion != GameManagerScript.Versions.SwipeUI && Input.GetMouseButton (0)) {
			// there is no previously clicked box OR
			// this box is selectable (it is adjacent to the previously 
			// selected tile and it isn't already selected)
			if (currentSelection.Count == 0 || IsNextTo (currentSelection [currentSelection.Count - 1]) &&
			           !currentSelection.Contains (new Vector2 (myX, myY))) {
				SelectThisTile();
				LogAction ("WF_LetterSelected", myLetter, myX, myY);
			} else {
				// de-select what has already been selected
				ClearAllSelectedTiles();
				LogAction ("WF_LetterDeselected", myLetter, myX, myY);
			}
		}
	}
	*/

	public static void PlayWord() {
		SubmitWordLogEntry dbEntry = new SubmitWordLogEntry ();
		dbEntry.parentKey = "WF_Action";
		dbEntry.key = "WF_Submit";
		bool valid = UpdateScore (dbEntry);

		// Firebase logging
		if (GameManagerScript.LOGGING) {
			Debug.Log ("Attempts to log data");
			GameManagerScript.LogKeyFrame ("pre");

			string json = JsonUtility.ToJson (dbEntry);
			DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference (GameManagerScript.LOGGING_VERSION);
			DatabaseReference child = reference.Push ();
			child.SetRawJsonValueAsync (json);
			++totalInteractions;
		}

		// Screen animations baesd on if word was valid or not
		if (valid) {
			// keep track of how many words have been successfully played
			++wordsPlayed;
		} else {
			camShake.ShakeRed (1f);
		}

		Debug.Log ("BoxScript.PlayWord end");

		CheckGameEnd ();
	}
		
	public static bool UpdateScore(SubmitWordLogEntry dbEntry) {
		// firebase logging
		SubmitWordLogEntry.SubmitWordPayload payload = new SubmitWordLogEntry.SubmitWordPayload();
		payload.word = currentWord;
		payload.letters = GetLetterPayloadsFromCurrentWord ();
		dbEntry.payload = payload;

		if (IsValidWord (currentWord)) {
			int submittedScore = GetScore (currentWord, payload);
			score += submittedScore;
			scoreText.text = "Points: " + score;
			submittedWordText.text = currentWord;
			submittedScoreText.text = ": " + submittedScore + " points";

			payload.success = true;
			payload.scoreTotal = submittedScore;

			// Do something celebratory! highlight in green briefly before removing from screen
			instance.StartCoroutine(instance.AnimateSelectedTiles (submittedScore));

			UpdateScoreProgressBar ();

			return true;
		} else {
			// firebase logging
			payload.success = false;
			payload.frequency = -1;
			payload.scoreBase = -1;
			payload.scoreTotal = -1;

			ClearAllSelectedTiles ();

			return false;
		}
	}

	public static int GetScore(string word, SubmitWordLogEntry.SubmitWordPayload payload) {
		// scoring function based on freq of word + freq of letters
		// TODO: do more balance testing of scoring function to make sure it is balanced?
		float wordFreq = GetWordFreq (word);

        if (wordFreq < 0) {
            // word does not exist in dictionary
            return 0;
        }

		Debug.Log(currentWord + ": " + wordFreq);

		// base score is based on length of word (if word is 3 letters long, base is 1 + 2 + 3 points, etc)
		int baseScore = CalculateBaseScore(word.Length);

		Debug.Log ("baseScore: " + baseScore);

        // record scores and freq into log if necessary
        if (payload != null) {
            payload.frequency = wordFreq;
            payload.scoreBase = baseScore;
        }

		// freq multiplier to reward rarer words
		float freqMultiplied = wordFreq;
		for (float freq = 0.1f; freq < 0.65f; freq += 0.05f) {
			if (wordFreq > freq) {
				freqMultiplied *= 1.25f;
			}
		}

		// TODO: based on the rarity of the word, Uncommon, Rare, Super Rare, Ultra Rare?
		// give the user a fixed bonus points amount and display an animation
		int bonus = GetBonus(wordFreq);

		return (int)(baseScore * (freqMultiplied * 20)) + bonus;
	}

	static int GetBonus(float freq) {
		if (freq > 0.6) {
			// MEGA PREMIUM ULTRA RARE (probably impossible to get in game)
			return 9001; // over 9000
		} else if (freq > 0.5) {
			// PREMIUM ULTRA RARE
			return 100;
		} else if (freq > 0.45) { 
			// ULTRA RARE+
			return 80;
		} else if (freq > 0.4) {
			// ULTRA RARE
			return 70;
		} else if (freq > 0.35) { 
			// SUPER RARE
			return 60;
		} else if (freq > 0.3) {
			// RARE
			return 50;
		} else if (freq > 0.24) {
			// AVERAGE
			return 40;
		} else if (freq > 0.2) {
			// UNCOMMON
			return 30;
		} else if (freq > 0.15) {
			// COMMON
			return 20;
		} else if (freq > 0.1) {
			// VERY COMMON
			return 10;
		}

		return 0;
	}

	static int CalculateBaseScore(int length) {
		if (length == 0) {
			return 0;
		}

		return LengthScoreFunction (length) + CalculateBaseScore (length - 1);
	}

	static int LengthScoreFunction(int length) {
		if (length == 1) {
			return 1;
		}

		return 1 + LengthScoreFunction (length - 1);
	}


	/**
	 * 
	 */
	public static void CheckGameEnd() {
		if (score >= MAX_SCORE) {
			GameManagerScript.gameOverPanel.SetActive (true);

			// disable touch events
			touchEnabled = false;

			// Log the final state of the game
			GameManagerScript.LogEndOfGame();
		}
	}

    static void RemoveLastSelection()
    {
        // get the last selected letter tile and remove it from the list (and unhighlight it)
        Vector2 v = currentSelection[currentSelection.Count - 1];
        grid[(int)v.x, (int)v.y].gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        grid[(int)v.x, (int)v.y].gameObject.GetComponent<BoxScript>().isSelected = false;
        currentSelection.Remove(v);


        // log the last removed letter
        LogAction("WF_LetterDeselected", currentWord.Substring(currentWord.Length - 1, 1), (int)v.x, (int)v.y);

        // Remove the last letter
        currentWord = currentWord.Substring(0, currentWord.Length - 1);

        // select the most recent one
        v = currentSelection[currentSelection.Count - 1];
        grid[(int)v.x, (int)v.y].gameObject.GetComponent<BoxScript>().isSelected = true;

        /*****************************************************************
         * FEATURE: Highlighting color gradient based on frequency feature
         *****************************************************************/
        // Rehighlight the currently selected word with the correct color
        Color highlightColor = GetHighlightColor(currentWord);
        foreach (Vector2 vec in currentSelection)
        {
            grid[(int)vec.x, (int)vec.y].gameObject.GetComponent<SpriteRenderer>().color = highlightColor;
        }

        /******************************************************************
         * FEATURE: Display currently selected score
         ******************************************************************/
        // Calculate currently selected score and change the text on screen
        if (currentWord.Length >= 3)
        {
            int currentScore = GetScore(currentWord, null);
            if (currentScore == 0)
            {
                selectedScore.text = "";
            }
            else
            {
                selectedScore.text = currentScore + " points";
            }
        } else {
            selectedScore.text = "";
        }
    }

	bool IsInsideTile(Vector2 pos) {
		Vector2 realPos = Camera.main.ScreenToWorldPoint (pos);
		float trueX = myX - gridWidthRadius;
		int trueY = myY - gridHeightRadius;
		float radius = 0.37f;

		// slight border around edge to make it easier to get diagonals
		return (realPos.x > trueX - radius && realPos.x <= trueX + radius &&
			realPos.y > trueY - radius && realPos.y <= trueY + radius);
	}

	bool IsNoBoxAboveMe() {
		for (int y = myY+1; y < gridHeight; ++y) {
			if (grid [myX, y] != null) {
				return false;
			}
		}

		return true;
	}

	// Checks to see if there is another box in my column that is falling with me in a 'column fall'
	bool IsOtherBoxInColumnFalling() {
		for (int y = myY-1; y >= 0; --y) {
			if (grid [myX, y] != null && grid [myX, y].gameObject.GetComponent<BoxScript> ().columnFalling) {
				return true;
			}
		}

		return false;
	}

	// Checks to see if there are any boxes in column x that is currently falling (or column falling)
	public static bool IsBoxInColumnFalling(int x) {
		for (int y = 0; y < gridHeight; ++y) {
			if (grid [x, y] != null && (grid [x, y].gameObject.GetComponent<BoxScript> ().falling ||
										grid[x, y].gameObject.GetComponent<BoxScript>().columnFalling)) {
				return true;
			}
		}

		return false;
	}

	public static void UpdateScoreProgressBar() {
		float scale = score * 1.0f / MAX_SCORE;

		if (scale > 1) {
			scale = 1.0f;
		}

		GameObject.Find("ProgressBarFG").transform.localScale = new Vector3(scale, 1.0f, 1.0f);
	}

	public IEnumerator AnimateSelectedTiles(int submittedScore) {
		// animate different congratulatory messages based on score
		TextFaderScript textFader = GameObject.Find("SuccessMessage").GetComponent<TextFaderScript>();
		if (submittedScore >= 50) {
			// PHENOMENAL!
			textFader.FadeText (0.7f, "Phenomenal!");
		} else if (submittedScore >= 40) {
			// FANTASTIC!
			textFader.FadeText (0.7f, "Fantastic!");
		} else if (submittedScore >= 30) {
			// GREAT!
			textFader.FadeText (0.7f, "Great!");
		} else if (submittedScore >= 20) {
			// NICE!
			textFader.FadeText (0.7f, "Nice!");
		}

		// animate each selected tile
		foreach (Vector2 v in currentSelection) {
			GameObject gameObject = grid [(int)v.x, (int)v.y].gameObject;
			gameObject.GetComponent<BoxScript> ().AnimateSuccess ();
		}

		// brief pause for the color to change before removing them from screen
		yield return new WaitForSeconds(0.17f);
		DeleteAllSelectedTiles ();
	}

	public static void DeleteAllSelectedTiles() {
		// delete all tiles in list
		foreach (Vector2 v in currentSelection) {
			GameObject gameObject = grid [(int)v.x, (int)v.y].gameObject;
			gameObject.GetComponent<BoxScript> ().AnimateSuccess ();
			Destroy (gameObject);
			grid [(int)v.x, (int)v.y] = null;
		}
		currentWord = "";
		currentSelection.Clear ();

        /******************************************************************
         * FEATURE: Display currently selected score
         ******************************************************************/
        selectedScore.text = "";
	}

	public void AnimateSuccess() {
		// highlight in green briefly when successfully played
		grid[myX, myY].gameObject.GetComponent<SpriteRenderer>().color = Color.green;
	}

	public void AnimateSelect() {
		// TODO: little animation from each tile when it gets selected??
	}

	public static bool IsValidWord(string word) {
		if (word.Length < 3) {
			// Error message
			TextFaderScript textFader = GameObject.Find("SuccessMessage").GetComponent<TextFaderScript>();
			textFader.FadeErrorText (0.8f, "Word must be at least 3 letters");
		}

		return word.Length >= 3 && freqDictionary.ContainsKey(word);
	}

    public static float GetWordFreq(string word) {
        if (freqDictionary.ContainsKey(word)) {
            return freqDictionary[word];
        }

        return -1;
    }

    /*****************************************************************
     * FEATURE: Highlighting color gradient based on frequency feature
     *****************************************************************/
    public static Color IntToColor(int HexVal)
    {
        byte R = (byte)((HexVal >> 16) & 0xFF);
        byte G = (byte)((HexVal >> 8) & 0xFF);
        byte B = (byte)((HexVal) & 0xFF);

        return new Color(R/255f, G/255f, B/255f, 1);
    }

    public static Color GetColorGradient(float freq) {
        float[] highs = new float[] { 0.15f, 0.24f, 0.36f };
        float[] lows = new float[] { 0.0f, 0.15f, 0.24f };

        // determine which third the freq is in
        int index = -1;
        if (freq > lows[2]) {
            index = 2;
        } else if (freq > lows[1]) {
            index = 1;
        } else {
            index = 0;
        }

        float input = (freq - lows[index]) / (highs[index] - lows[index]);
        if (input > 1) input = 1.0f;
        int[] rs = new int[] { 241, 46, 52, 155 };
        int[] gs = new int[] { 196, 204, 152, 89 };
        int[] bs = new int[] { 15, 113, 219, 182 };

        int rStart = rs[index],
        gStart = gs[index],
        bStart = bs[index],
        rEnd = rs[index+1],
        gEnd = gs[index+1],
        bEnd = bs[index+1];

        float r = rStart,
        g = gStart,
        b = bStart;

        r -= (rStart - rEnd) * input;
        g -= (gStart - gEnd) * input;
        b -= (bStart - bEnd) * input;

        Debug.Log(r + " " + g + " " + b);
        return new Color(r / 255f, g / 255f, b / 255f, 1);
    }

    /**
     * Returns a gradient of the highlighted letters color 
     */
    public static Color GetHighlightColor(string word) {
        Color highlightColor;

        if (word.Length >= 3 && freqDictionary.ContainsKey(word)) {
            float freq = freqDictionary[word];

            // increase the color gradient by inverse gaussian as the freq increases linearly
            return GetColorGradient(freq);
        } else {
            if (ColorUtility.TryParseHtmlString("#c0392b", out highlightColor))
                return highlightColor;

            return Color.red;
        }
    }

    /**
     * Highlight the tile when it is selected and add the selection to the list
     */
	void SelectThisTile() {
		currentSelection.Add (new Vector2 (myX, myY));
		currentWord += myLetter;

        /*****************************************************************
         * FEATURE: Highlighting color gradient based on frequency feature
         *****************************************************************/
		//grid [myX, myY].gameObject.GetComponent<SpriteRenderer>().color = Color.yellow;
        Color highlightColor = GetHighlightColor(currentWord);
        foreach (Vector2 v in currentSelection)
        {
            grid[(int)v.x, (int)v.y].gameObject.GetComponent<SpriteRenderer>().color = highlightColor;
        }

		isSelected = true;

        /******************************************************************
         * FEATURE: Display currently selected score
         ******************************************************************/
        // Calculate currently selected score and change the text on screen
        if (currentWord.Length >= 3)
        {
            int currentScore = GetScore(currentWord, null);
            if (currentScore == 0)
            {
                selectedScore.text = "";
            }
            else
            {
                selectedScore.text = currentScore + " points";
            }
        } else {
            selectedScore.text = "";
        }
	}

	// Checks to see if the other tile is adjacent (or diagonal) to the current location
	public bool IsNextTo(Vector2 otherLoc) {
		int otherX = (int)otherLoc.x;
		int otherY = (int)otherLoc.y;

		// check to the right
		if (myX + 1 == otherX && myY == otherY) {
			return true;
		} 
		// check to the left
		else if (myX - 1 == otherX && myY == otherY) {
			return true;
		}
		// check up
		else if (myX == otherX && myY + 1 == otherY) {
			return true;
		}
		// check down
		else if (myX == otherX && myY - 1 == otherY) {
			return true;
		}
		// check diagonal top right
		else if (myX + 1 == otherX && myY + 1 == otherY) {
			return true;
		}
		// check diagonal top left
		else if (myX - 1 == otherX && myY + 1 == otherY) {
			return true;
		}
		// check diagonal bottom right
		else if (myX + 1 == otherX && myY - 1 == otherY) {
			return true;
		}
		// check diagonal bottom left
		else if (myX - 1 == otherX && myY - 1 == otherY) {
			return true;
		}

		return false;
	}

	public static void ClearAllSelectedTiles() {
		currentWord = "";

		// remove all coloring
		foreach (Vector2 v in currentSelection) {
			grid [(int)v.x, (int)v.y].gameObject.GetComponent<SpriteRenderer> ().color = Color.white;
			grid [(int)v.x, (int)v.y].gameObject.GetComponent<BoxScript> ().isSelected = false;
		}

		currentSelection.Clear ();

        /******************************************************************
         * FEATURE: Display currently selected score
         ******************************************************************/
        // Calculate currently selected score and change the text on screen
        selectedScore.text = "";
	}

	public static string GetLetterFromPrefab(string name) {
		// kind of a hack.. the prefab names' 7th character is the letter of the block
		return name.Substring(6, 1);
	}

	public static bool IsInsideGrid(Vector2 pos) {
		float x = pos.x;
		int y = (int)pos.y;
		return (x >= -gridWidthRadius && x <= gridWidthRadius && y >= -gridHeightRadius && y <= gridHeightRadius);
	}

	public static bool IsColumnFull(int x) {
		for (int y = 0; y < gridHeight; ++y) {
			if (grid [x, y] == null) {
				return false;
			}
		}

		return true;
	}
		
	bool IsValidPosition() {        
		Vector2 v = transform.position;

		if (!IsInsideGrid (v)) {
			return false;
		}
		if (grid [(int)(v.x + gridWidthRadius), (int)v.y + gridHeightRadius] != null &&
			grid [(int)(v.x + gridWidthRadius), (int)v.y + gridHeightRadius] != transform) {
			return false;
		}

		return true;
	}

	void ColumnDown() {
		columnFalling = true;

		// move every other block on top of this block down 1 as well
		for (int y = myY; y < gridHeight; ++y) {
			if (grid [myX, y] != null) {
				grid [myX, y].position += new Vector3 (0, -1, 0);
				grid [myX, y].gameObject.GetComponent<BoxScript> ().myY -= 1;
				grid [myX, y - 1] = grid [myX, y];
			} else {
				grid [myX, y - 1] = null;
			}
		}

		grid [myX, gridHeight - 1] = null;
	}

	void GridUpdate() {
		// Remove the previous location of this block from the grid
		grid [myX, myY] = null;

		// Add the new location of the block to the grid
		Vector2 v = transform.position;
		myX = (int)(v.x + gridWidthRadius);
		myY = (int)v.y + gridHeightRadius;
		grid[myX, myY] = transform;
	}

	public static void Reset() {
		foreach (Transform transform in grid) {
			Destroy (transform.gameObject);
		}

		grid = new Transform[gridWidth, gridHeight];
		currentSelection.Clear ();
		score = 0;
		scoreText.text = "Points: " + score;
		submittedScoreText.text = "";
		submittedWordText.text = "";
	}

	public static string GetBoardPayload() {
		string boardString = "";

		for (int j = gridHeight-1; j >= 0; --j) {
			for (int i = 0; i < gridWidth; ++i) {
				if (grid [i, j] != null) {
					boardString += grid [i, j].gameObject.GetComponent<BoxScript> ().myLetter;
				}
			}

			if (j > 0) {
				boardString += "\n";
			}
		}

		return boardString;
	}

	public static char[,] GetBoardLetters() {
		char[,] letters = new char[gridWidth, gridHeight];

		for (int j = 0; j < gridHeight; ++j) {
			for (int i = 0; i < gridWidth; ++i) {
				if (grid [i, j] != null) {
					letters[i, j] = grid [i, j].gameObject.GetComponent<BoxScript> ().myLetter[0];
				}
			}
		}

		return letters;
	}
}
