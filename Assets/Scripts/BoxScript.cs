using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

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
	private static int score = 0;
	private const float FALL_SPEED_CONST = 0.15f;
	public static string currentWord = "";
    public static Dictionary<string,float> freqDictionary = null;
	public static CamShakeSimpleScript camShake = null;

	float fall = 0f;
	bool falling = true;
	bool columnFalling = false;
	int myX = 0;
	int myY = 0;
	bool isSelected = false;

	[NonSerialized]
	public float fallSpeed = FALL_SPEED_CONST;

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

		if (scoreText == null) {
			scoreText = GameObject.Find("Score").GetComponent<Text>();
		}

		if (submittedWordText == null) {
			submittedWordText = GameObject.Find ("SubmittedWord").GetComponent<Text> ();
		}

		if (submittedScoreText == null) {
			submittedScoreText = GameObject.Find ("SubmittedScore").GetComponent<Text> ();
		}

		if (!IsValidPosition ()) {
			//SceneManager.LoadScene (0);
			Destroy (gameObject);
		}
	}

	// Update is called once per frame
	void Update () {
		// ButtonUI touch inputs
		if (GameManagerScript.currentVersion == GameManagerScript.Versions.ButtonUI && 
			Input.touchCount > 0 && IsInsideTile (Input.GetTouch (0).position)) {
			//Debug.Log ("Inside Tile worked!");

			if (Input.GetTouch (0).phase == TouchPhase.Began) {
				// there is no previously clicked box
				if (currentSelection.Count == 0) {
					SelectThisTile ();
				} else if (IsNextTo (currentSelection[currentSelection.Count-1]) && 
						   !currentSelection.Contains (new Vector2 (myX, myY))) {
					// add on to the current selection 
					SelectThisTile();
				} else {
					// de-select what has already been selected
					ClearAllSelectedTiles ();
				}
			} else if (Input.GetTouch (0).phase == TouchPhase.Moved) {
				// selected tile and it isn't already selected)
				if (currentSelection.Count > 0 && 
					IsNextTo (currentSelection [currentSelection.Count - 1]) &&
					!currentSelection.Contains (new Vector2 (myX, myY))) {
					SelectThisTile ();
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
		}
		// SwipeUI touch input
		else if (GameManagerScript.currentVersion == GameManagerScript.Versions.SwipeUI && 
			Input.touchCount > 0 && IsInsideTile (Input.GetTouch (0).position)) {
			//Debug.Log ("Inside Tile worked!");

			if (Input.GetTouch (0).phase == TouchPhase.Began) {
				// there is no previously clicked box
				if (currentSelection.Count == 0) {
					SelectThisTile ();
				} else {
					// de-select what has already been selected
					ClearAllSelectedTiles ();
				}
			} else if (Input.GetTouch (0).phase == TouchPhase.Moved) {
				// selected tile and it isn't already selected)
				if (IsNextTo (currentSelection [currentSelection.Count - 1]) &&
					!currentSelection.Contains (new Vector2 (myX, myY))) {
					SelectThisTile ();
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
		}

		// If SwipeUI, automatically play word when lifting the finger, and cancel if canceled for all UI's
		if (GameManagerScript.currentVersion == GameManagerScript.Versions.SwipeUI &&
			Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Ended && isSelected) {
			PlayWord ();
		} else if (Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Canceled && isSelected) {
			ClearAllSelectedTiles ();
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

	/*
	// Click on blocks to select them
	void OnMouseDown() {
		// regular left mouse click
		if (Input.GetMouseButton (0)) {
			// there is no previously clicked box OR
			// this box is selectable (it is adjacent to the previously 
			// selected tile and it isn't already selected)
			if (currentSelection.Count == 0 || IsNextTo (currentSelection [currentSelection.Count - 1]) &&
			           !currentSelection.Contains (new Vector2 (myX, myY))) {
				SelectThisTile();
			} else {
				// de-select what has already been selected
				ClearAllSelectedTiles();
			}
		}
	}
	*/

	public static void PlayWord() {
		bool valid = UpdateScore ();

		if (valid) {
			// do something celebratory! like sparkles?
		} else {
			camShake.ShakeRed (1f);
		}
	}

	static void RemoveLastSelection() {
		Vector2 v = currentSelection [currentSelection.Count - 1];
		grid [(int)v.x, (int)v.y].gameObject.GetComponent<SpriteRenderer> ().color = Color.white;
		grid [(int)v.x, (int)v.y].gameObject.GetComponent<BoxScript> ().isSelected = false;
		currentSelection.Remove (v);
		currentWord = currentWord.Substring (0, currentWord.Length - 1);

		// select the most recent one
		v = currentSelection[currentSelection.Count - 1];
		grid [(int)v.x, (int)v.y].gameObject.GetComponent<BoxScript> ().isSelected = true;
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

	public static bool UpdateScore() {
		if (IsValidWord (currentWord)) {
			int submittedScore = GetScoringFunction (currentWord);
			score += submittedScore;
			scoreText.text = "Points: " + score;
			submittedWordText.text = currentWord;
			submittedScoreText.text = ": " + submittedScore + " points";

			AnimateSelectedTiles (submittedScore);
			DeleteAllSelectedTiles ();

			return true;
		} else {
			ClearAllSelectedTiles ();

			return false;
		}
	}

	public static int GetScoringFunction(string word) {
		// scoring function based on freq of word + freq of letters
		// TODO: do more balance testing of scoring function to make sure it is balanced?
		float wordFreq = GetWordFreq (word);
		Debug.Log(currentWord + ": " + wordFreq);

		int baseScore = 0;
		for (int i = 0; i < word.Length; ++i) {
			baseScore += SpawnBoxScript.MAX_LETTER_FREQ / SpawnBoxScript.letterDistributions[word[i]-'A'];
		}
		Debug.Log ("baseScore: " + baseScore);

		return baseScore + (int)(baseScore * (wordFreq * 20));
	}

	public static void AnimateSelectedTiles(int submittedScore) {
		// animate different congratulatory messages based on score
		TextFaderScript textFader = GameObject.Find("SuccessMessage").GetComponent<TextFaderScript>();
		if (submittedScore >= 45) {
			// PHENOMENAL!
			textFader.FadeText (0.5f, "Phenomenal!");
		} else if (submittedScore >= 35) {
			// FANTASTIC!
			textFader.FadeText (0.5f, "Fantastic!");
		} else if (submittedScore >= 25) {
			// GREAT!
			textFader.FadeText (0.5f, "Great!");
		} else if (submittedScore >= 15) {
			// NICE!
			textFader.FadeText (0.5f, "Nice!");
		}

		// animate each selected tile
		foreach (Vector2 v in currentSelection) {
			GameObject gameObject = grid [(int)v.x, (int)v.y].gameObject;
			gameObject.GetComponent<BoxScript> ().AnimateSuccess ();
		}
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
	}

	public void AnimateSuccess() {
		// TODO: little animation from each tile when it gets submitted
	}

	public void AnimateSelect() {
		// TODO: little animation from each tile when it gets selected??
	}

	public static bool IsValidWord(string word) {
		return word.Length >= 3 && freqDictionary.ContainsKey(word);
	}

    public static float GetWordFreq(string word) {
        if (freqDictionary.ContainsKey(word)) {
            return freqDictionary[word];
        }

        return -1;
    }

	void SelectThisTile() {
		currentSelection.Add (new Vector2 (myX, myY));
		currentWord += GetLetterFromPrefab (this.gameObject.name);
		grid [myX, myY].gameObject.GetComponent<SpriteRenderer>().color = Color.yellow;
		isSelected = true;
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
	}

	public static string GetLetterFromPrefab(string name) {
		// kind of a hack.. the prefab names' 7th character is the letter of the block
		return name.Substring (6, 1);
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
}
