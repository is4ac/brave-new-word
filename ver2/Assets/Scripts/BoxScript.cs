using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class BoxScript : MonoBehaviour {

	public static int gridWidthRadius = 5;
	public static int gridHeightRadius = 4;
	public static int gridWidth = gridWidthRadius*2 + 1; // -5 to 5 
	public static int gridHeight = gridHeightRadius*2 + 1; // -4 to 4
	public static Transform[,] grid = new Transform[gridWidth, gridHeight];
	public static List<Vector2> currentSelection = new List<Vector2> ();
	public static Text scoreText;
	private static int score = 0;
	private const float FALL_SPEED_CONST = 0.15f;
	public static string currentWord = "";
	public static HashSet<string> dictionary = null;
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
		Vector2 v = round(transform.position);
		myX = (int)v.x + gridWidthRadius;
		myY = (int)v.y + gridHeightRadius;
		grid[myX, myY] = transform;
		falling = true;
		columnFalling = false;
		fall = Time.time;

		if (scoreText == null) {
			scoreText = GameObject.Find("Score").GetComponent<Text>();
		}

		if (!isValidPosition ()) {
			//SceneManager.LoadScene (0);
			Destroy (gameObject);
		}
	}

	// Update is called once per frame
	void Update () {
		// Check touch input updates

		if (Input.touchCount > 0 && isInsideTile (Input.GetTouch (0).position)) {
			//Debug.Log ("Inside Tile worked!");

			if (Input.GetTouch (0).phase == TouchPhase.Began) {
				// there is no previously clicked box
				if (currentSelection.Count == 0) {
					selectThisTile ();
				} else {
					// de-select what has already been selected
					clearAllSelectedTiles ();
				}
			} else if (Input.GetTouch (0).phase == TouchPhase.Moved) {
				// selected tile and it isn't already selected)
				if (isNextTo (currentSelection [currentSelection.Count - 1]) &&
				    !currentSelection.Contains (new Vector2 (myX, myY))) {
					selectThisTile ();
				} else if (currentSelection.Contains (new Vector2 (myX, myY))) {
					// de-select the most recent tile(s) if you move back to an old one
					for (int i = currentSelection.Count - 1; i > 0; --i) {
						if (currentSelection [currentSelection.Count - 1] != new Vector2 (myX, myY)) {
							removeLastSelection ();
						} else {
							break;
						}
					}
				} else {
					// just do nothing?
				}
			}
		} else if (Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Ended && isSelected) {
			playWord ();
		} else if (Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Canceled && isSelected) {
			clearAllSelectedTiles ();
		}

		// check to see if the column needs to go down, or if it needs to be refilled
		if (!falling && myY > 0 && grid [myX, myY - 1] == null && Time.time - fall >= fallSpeed) {
			if (!isOtherBoxInColumnFalling ()) {
				columnDown ();
				fall = Time.time;
			}
		} else if (columnFalling && ((myY > 0 && grid [myX, myY - 1] != null) || myY == 0)) {
			columnFalling = false;
		}

		// If a tile is falling down the screen...
		if (falling && Time.time - fall >= fallSpeed) {
			transform.position += new Vector3(0, -1, 0);

			if (isValidPosition()) {
				GridUpdate();
			} else {
				transform.position += new Vector3 (0, 1, 0);
				falling = false;
				fallSpeed = FALL_SPEED_CONST;
			}
			fall = Time.time;
		}
	}

	static void removeLastSelection() {
		Vector2 v = currentSelection [currentSelection.Count - 1];
		grid [(int)v.x, (int)v.y].gameObject.GetComponent<SpriteRenderer> ().color = Color.white;
		grid [(int)v.x, (int)v.y].gameObject.GetComponent<BoxScript> ().isSelected = false;
		currentSelection.Remove (v);
		currentWord = currentWord.Substring (0, currentWord.Length - 1);

		// select the most recent one
		v = currentSelection[currentSelection.Count - 1];
		grid [(int)v.x, (int)v.y].gameObject.GetComponent<BoxScript> ().isSelected = true;
	}

	bool isInsideTile(Vector2 pos) {
		Vector2 realPos = Camera.main.ScreenToWorldPoint (pos);
		int trueX = myX - gridWidthRadius;
		int trueY = myY - gridHeightRadius;

		// slight border around edge to make it easier to get diagonals
		return (realPos.x > trueX - 0.40 && realPos.x <= trueX + 0.40 &&
				realPos.y > trueY - 0.40 && realPos.y <= trueY + 0.40);
	}

	public static void playWord() {
		bool valid = updateScore ();

		if (valid) {
			// do something celebratory! like sparkles?
		} else {
			camShake.ShakeRed (1f);
		}
	}

	bool isNoBoxAboveMe() {
		for (int y = myY+1; y < gridHeight; ++y) {
			if (grid [myX, y] != null) {
				return false;
			}
		}

		return true;
	}

	bool isOtherBoxInColumnFalling() {
		for (int y = myY-1; y >= 0; --y) {
			if (grid [myX, y] != null && grid [myX, y].gameObject.GetComponent<BoxScript> ().columnFalling) {
				return true;
			}
		}

		return false;
	}

	public static bool isBoxInColumnFalling(int x) {
		for (int y = 0; y < gridHeight; ++y) {
			if (grid [x, y] != null && (grid [x, y].gameObject.GetComponent<BoxScript> ().falling ||
										grid[x, y].gameObject.GetComponent<BoxScript>().columnFalling)) {
				return true;
			}
		}

		return false;
	}

	public static bool updateScore() {
		if (isValidWord (currentWord)) {
			score += getScoringFunction(currentWord);
			scoreText.text = "Points: " + score;

			deleteAllSelectedTiles ();

			return true;
		} else {
			clearAllSelectedTiles ();

			return false;
		}
	}

	public static int getScoringFunction(string word) {
		// scoring function based on freq of word + freq of letters
		// TODO: do more balance testing of scoring function to make sure it is balanced?
		float wordFreq = getWordFreq (word);
		Debug.Log(currentWord + ": " + wordFreq);

		int baseScore = 0;
		for (int i = 0; i < word.Length; ++i) {
			baseScore += SpawnBoxScript.MAX_LETTER_FREQ / SpawnBoxScript.letterDistributions[word[i]-'A'];
		}
		Debug.Log ("baseScore: " + baseScore);

		return baseScore + (int)(baseScore * (wordFreq * 20));
	}

	public static void deleteAllSelectedTiles() {
		// delete all tiles in list
		foreach (Vector2 v in currentSelection) {
			Destroy (grid [(int)v.x, (int)v.y].gameObject);
			grid [(int)v.x, (int)v.y] = null;
		}
		currentWord = "";
		currentSelection.Clear ();
	}

	public static bool isValidWord(string word) {
		return word.Length >= 3 && dictionary.Contains(word.ToLower());
	}

    public static float getWordFreq(string word) {
        if (freqDictionary.ContainsKey(word)) {
            return freqDictionary[word];
        }
        return -1;
    }

	void selectThisTile() {
		currentSelection.Add (new Vector2 (myX, myY));
		currentWord += getLetterFromPrefab (this.gameObject.name);
		grid [myX, myY].gameObject.GetComponent<SpriteRenderer>().color = Color.yellow;
		isSelected = true;
	}

	// Checks to see if the other tile is adjacent (or diagonal) to the current location
	public bool isNextTo(Vector2 otherLoc) {
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

	public static void clearAllSelectedTiles() {
		currentWord = "";

		// remove all coloring
		foreach (Vector2 v in currentSelection) {
			grid [(int)v.x, (int)v.y].gameObject.GetComponent<SpriteRenderer> ().color = Color.white;
			grid [(int)v.x, (int)v.y].gameObject.GetComponent<BoxScript> ().isSelected = false;
		}

		currentSelection.Clear ();
	}

	public static string getLetterFromPrefab(string name) {
		// kind of a hack.. the prefab names' 7th character is the letter of the block
		return name.Substring (6, 1);
	}

	public static Vector2 round(Vector2 v) {
		return new Vector2(Mathf.Round(v.x), Mathf.Round(v.y));
	}

	public static bool isInsideGrid(Vector2 pos) {
		int x = (int)pos.x;
		int y = (int)pos.y;
		return (x >= -gridWidthRadius && x <= gridWidthRadius && y >= -gridHeightRadius && y <= gridHeightRadius);
	}

	public static bool isColumnFull(int x) {
		for (int y = 0; y < gridHeight; ++y) {
			if (grid [x, y] == null) {
				return false;
			}
		}

		return true;
	}

	public static bool isColumnEmpty(int x) {
		for (int y = 0; y < gridHeight; ++y) {
			if (grid[x, y] != null) {
				return false;
			}
		}

		return true;
	}
		
	bool isValidPosition() {        
		Vector2 v = round(transform.position);

		if (!isInsideGrid (v)) {
			return false;
		}
		if (grid [(int)v.x + gridWidthRadius, (int)v.y + gridHeightRadius] != null &&
		    grid [(int)v.x + gridWidthRadius, (int)v.y + gridHeightRadius] != transform) {
			return false;
		}

		return true;
	}

	void columnDown() {
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
		Vector2 v = round(transform.position);
		myX = (int)v.x + gridWidthRadius;
		myY = (int)v.y + gridHeightRadius;
		grid[myX, myY] = transform;
	}
}
