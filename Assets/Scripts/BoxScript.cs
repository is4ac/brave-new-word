using System.Collections;
using System.Collections.Generic;
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

	public static string currentWord = "";
	float fall = 0f;
	bool falling = true;
	bool columnFalling = false;
	int myX;
	int myY;
	private static int score = 0;
	bool spawnFlag = false;

	[SerializeField]
	float fallSpeed = 0.2f;

	// Use this for initialization
	void Start () {
		// Add the location of the block to the grid
		Vector2 v = round(transform.position);
		myX = (int)v.x + gridWidthRadius;
		myY = (int)v.y + gridHeightRadius;
		grid[myX, myY] = transform;
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
		// if the enter key is pressed, then submit the word
		// check against dictionary and give it points
		if (Input.GetKeyDown (KeyCode.Return) && currentSelection.Count > 0 &&
			(int)currentSelection[currentSelection.Count - 1].x == myX &&
			(int)currentSelection[currentSelection.Count - 1].y == myY) {
			updateScore ();
		}

		// check to see if the column needs to go down, or if it needs to be refilled
		if (!falling && myY > 0 && grid [myX, myY - 1] == null && Time.time - fall >= fallSpeed) {
			if (!isOtherBoxInColumnFalling ()) {
				columnDown ();
				fall = Time.time;
			}
		} else if (!falling && !isColumnFull (myX) && columnFalling) {
			columnFalling = false;

			if (grid [myX, gridHeight - 1] == null) {
				GameObject spawnBoxObject = GameObject.FindWithTag ("SpawnBox" + myX);
				SpawnBoxScript spawnBox = spawnBoxObject.GetComponent<SpawnBoxScript> ();
				spawnBox.SpawnNewBox ();
			}
		} else if (!falling && !isOtherBoxInColumnFalling () && myY < gridHeight - 1 && grid [myX, myY + 1] == null && !spawnFlag) {
			GameObject spawnBoxObject = GameObject.FindWithTag ("SpawnBox" + myX);
			SpawnBoxScript spawnBox = spawnBoxObject.GetComponent<SpawnBoxScript> ();
			spawnBox.SpawnNewBox ();
			spawnFlag = true;
		}

		// If a tile is falling down the screen...
		if (falling && Time.time - fall >= fallSpeed) {
			transform.position += new Vector3(0, -1, 0);

			if (isValidPosition()) {
				GridUpdate();
			} else {
				transform.position += new Vector3(0, 1, 0);

				// spawn a new box if the current column is not full
				if (!isColumnFull (myX)) {
					if (grid [myX, gridHeight - 1] == null) {
						GameObject spawnBoxObject = GameObject.FindWithTag ("SpawnBox" + myX);
						SpawnBoxScript spawnBox = spawnBoxObject.GetComponent<SpawnBoxScript> ();
						spawnBox.SpawnNewBox ();
						spawnFlag = true;
					}
				}

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

	bool isOtherBoxInColumnFalling() {
		for (int y = myY; y >= 0; --y) {
			if (grid[myX, y] != null && grid [myX, y].gameObject.GetComponent<BoxScript> ().columnFalling) {
				return true;
			}
		}

		return false;
	}

	void updateScore() {
		if (isValidWord (currentWord)) {
			score += currentWord.Length;
			scoreText.text = "Points: " + score;

			deleteAllSelectedTiles ();
		} else {
			clearAllSelectedTiles ();
		}
	}

	public static void deleteAllSelectedTiles() {
		// delete all tiles in list
		foreach (Vector2 v in currentSelection) {
			// set the spawn flag so it is possible to spawn new boxes in the column that was deleted
			if ((int)v.y > 0 && grid[(int)v.x, (int)v.y-1] != null) {
				grid [(int)v.x, (int)v.y - 1].gameObject.GetComponent<BoxScript> ().spawnFlag = false;
			}

			Destroy (grid [(int)v.x, (int)v.y].gameObject);
			grid [(int)v.x, (int)v.y] = null;
		}
			
		currentSelection.Clear ();
	}

	public static bool isValidWord(string word) {
		// TODO: use nltk corpora dictionary to check
		return true;
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
		//Debug.Log ((int)pos.x + "," + (int)pos.y);
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
		Debug.Log ("grid updating");

		columnFalling = true;

		// move every other block on top of this block down 1 as well
		for (int y = myY; y < gridHeight; ++y) {
			if (grid [myX, y] != null) {
				Debug.Log (grid [myX, y].gameObject.GetComponent<BoxScript> ().myX + "," +
				grid [myX, y].gameObject.GetComponent<BoxScript> ().myY);
				grid [myX, y].position += new Vector3 (0, -1, 0);
				grid [myX, y].gameObject.GetComponent<BoxScript> ().myY -= 1;
				grid [myX, y - 1] = grid [myX, y];
				Debug.Log (grid [myX, y].gameObject.GetComponent<BoxScript> ().myX + "," +
				grid [myX, y].gameObject.GetComponent<BoxScript> ().myY);
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
