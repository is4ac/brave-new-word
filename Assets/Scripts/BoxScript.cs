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
	public static string currentWord = "";
	public static HashSet<string> dictionary = null;

	float fall = 0f;
	bool falling = true;
	bool columnFalling = false;
	int myX = 0;
	int myY = 0;

	[NonSerialized]
	public float fallSpeed = 0.4f;

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

			if (fallSpeed == 0.5f) {
				Debug.Log ("fallSpeed = 0.5f");
			}

			transform.position += new Vector3(0, -1, 0);

			if (isValidPosition()) {
				GridUpdate();
			} else {
				transform.position += new Vector3 (0, 1, 0);
				falling = false;
			}
			fall = Time.time;
		}
	}

	// Click on blocks to select them
	void OnMouseDown() {
		// regular left mouse click
		if (Input.GetMouseButton (0)) {
			// there is no previously clicked box OR
			// this box is selectable (it is adjacent to the previously 
			// selected tile and it isn't already selected)
			if (currentSelection.Count == 0 || isNextTo (currentSelection [currentSelection.Count - 1]) &&
			           !currentSelection.Contains (new Vector2 (myX, myY))) {
				selectThisTile();
			} else {
				// de-select what has already been selected
				clearAllSelectedTiles();
			}
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
			// TODO: different scoring function based on freq of word + freq of letters?
			score += currentWord.Length;
			scoreText.text = "Points: " + score;

			deleteAllSelectedTiles ();

			return true;
		} else {
			clearAllSelectedTiles ();

			return false;
		}
	}

	public static void deleteAllSelectedTiles() {
		//List<BoxScript> scripts = new List<BoxScript> ();

		// delete all tiles in list
		foreach (Vector2 v in currentSelection) {
			Destroy (grid [(int)v.x, (int)v.y].gameObject);
			grid [(int)v.x, (int)v.y] = null;
		}
		currentWord = "";
		currentSelection.Clear ();
	}

	public static bool isValidWord(string word) {
		return dictionary.Contains(word.ToLower());
	}

	void selectThisTile() {
		currentSelection.Add (new Vector2 (myX, myY));
		currentWord += getLetterFromPrefab (this.gameObject.name);
		grid [myX, myY].gameObject.GetComponent<SpriteRenderer>().color = Color.yellow;
	}

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
