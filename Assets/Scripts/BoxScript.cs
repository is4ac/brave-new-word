using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxScript : MonoBehaviour {

	public static int gridWidthRadius = 5;
	public static int gridHeightRadius = 4;
	public static int gridWidth = gridWidthRadius*2 + 1; // -5 to 5 
	public static int gridHeight = gridHeightRadius*2 + 1; // -4 to 4
	public static Transform[,] grid = new Transform[gridWidth, gridHeight];
	float fall = 0f;
	bool falling = true;

	[SerializeField]
	float fallSpeed = 0.2f;

	// Use this for initialization
	void Start () {
		// Add the location of the block to the grid
		Vector2 v = round(transform.position);
		grid[(int)v.x + gridWidthRadius, (int)v.y + gridHeightRadius] = transform;

		if (!isValidPosition ()) {
			Application.LoadLevel (0);
			Destroy (gameObject);
		}
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.RightArrow)) {
			transform.position += new Vector3(1, 0, 0);
			if (isValidPosition())
				GridUpdate();
			else
				transform.position += new Vector3(-1, 0, 0);
		}

		else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			transform.position += new Vector3(-1, 0, 0);
			if (isValidPosition())
				GridUpdate();
			else
				transform.position += new Vector3(1, 0, 0);
		}

		else if (Input.GetKeyDown(KeyCode.UpArrow)) {
			transform.Rotate(0, 0, -90);
			if (isValidPosition())
				GridUpdate();
			else
				transform.Rotate(0, 0, 90);

		}

		else if (falling && Time.time - fall >= fallSpeed) {
			transform.position += new Vector3(0, -1, 0);
			//Debug.Log (transform.position.ToString ());
			if (isValidPosition()) {
				GridUpdate();
			} else {
				transform.position += new Vector3(0, 1, 0);
				//DeleteRow();

				// spawn a new box if the current column is not full
				int myCol = (int)transform.position.x + gridWidthRadius;
				if (!isColumnFull (myCol)) {
					if (grid [myCol, gridHeight - 1] == null) {
						GameObject spawnBoxObject = GameObject.FindWithTag ("SpawnBox" + myCol);
						SpawnBoxScript spawnBox = spawnBoxObject.GetComponent<SpawnBoxScript> ();
						spawnBox.SpawnNewBox ();
					}
				}

				falling = false;
			}
			fall = Time.time;
		}
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

	public static void Delete(int y) {
		for (int x = 0; x < gridWidth; ++x) {
			Destroy (grid [x, y].gameObject);
			grid [x, y] = null;
		}
	}

	public static bool isFull(int y) {
		for (int x = 0; x < gridWidth; ++x) {
			if (grid [x, y] == null) {
				return false;
			}
		}

		return true;
	}

	public static bool isColumnFull(int x) {
		for (int y = 0; y < gridHeight; ++y) {
			if (grid [x, y] == null) {
				return false;
			}
		}

		return true;
	}

	public static void RowDown(int y) {
		for (int x = 0; x < gridWidth; ++x) {
			if (grid [x, y] != null) {
				grid [x, y - 1] = grid [x, y];
				grid [x, y] = null;
				grid [x, y - 1].position += new Vector3 (0, -1, 0);
			}
		}
	}

	public static void RowDownAll(int y) {
		for (int i = y; i < gridHeight; ++i) {
			RowDown (i);
		}
	}
		
	bool isValidPosition() {        
		Vector2 v = round(transform.position);
		//Debug.Log (v.ToString());
		//Debug.Log (isInsideGrid (v));

		if (!isInsideGrid (v)) {
			return false;
		}
		if (grid [(int)v.x + gridWidthRadius, (int)v.y + gridHeightRadius] != null &&
		    grid [(int)v.x + gridWidthRadius, (int)v.y + gridHeightRadius] != transform) {
			return false;
		}

		return true;
	}

	void GridUpdate() {
		// Remove the previous location of this block from the grid
		for (int y = 0; y < gridHeight; ++y) {
			for (int x = 0; x < gridWidth; ++x) {
				if (grid [x, y] != null) {
					if (grid [x, y] == transform) {
						grid [x, y] = null;
					}
				}
			}
		}

		// Add the new location of the block to the grid
		Vector2 v = round(transform.position);
		grid[(int)v.x + gridWidthRadius, (int)v.y + gridHeightRadius] = transform;
	}
}
