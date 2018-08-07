using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SpawnBoxScript : MonoBehaviour {

	public TextAsset dictionary;

    public Transform boxPrefab;

	[SerializeField]
	public int myX;

	//static int width = 6;
	//static int height = 9;

	static bool isInit = false;
	bool init = true;
	int initCount = 0;
	bool wait = true;
	List<int> letterFreq;
	//static char[,] initialBoard = new char[width, height];
	//static bool[,] flaggedBoard = new bool[width, height];

    public static int[] letterDistributions = new int[] {
        81,20,28,43,90,     // A B C D E
        22,20,61,70,8,      // F G H I J
        18,40,24,67,75,     // K L M N O
        19,8,60,63,90,      // P Q R S T
        28,10,24,10,20,10}; // U V W X Y Z
	public const int MAX_LETTER_FREQ = 90;

	public static bool isInitialized() {
		return isInit;
	}

    void AddToLetterFreqList() {
        for (int letter_index = 0; letter_index < 26; letter_index++)
        {
            int letterFreqCount = letterDistributions[letter_index];
            for (int i = 0; i < letterFreqCount; ++i)
            {
                letterFreq.Add(letter_index);
            }
       }
    }

	void ImportDictionary() {
		// import words list and put it in set
		if (BoxScript.freqDictionary == null)
		{
			BoxScript.freqDictionary  = new Dictionary<string, float>();

			string line;

			// Read the file and store it line by line.  
			string[] lines = dictionary.text.Split ('\n');
			for (int i = 0; i < lines.Length; ++i)
			{
				line = lines [i];
				string[] tokens = line.Split(',');
				if (tokens.Length > 1) {
					string word = tokens [0].ToUpper ();
					float val = float.Parse (tokens [1]);
					BoxScript.freqDictionary.Add (word, val);
				}
			}

			//GameManagerScript.LoadTrie ();
		}
	}

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

	// Use this for initialization
	void Start () {
		//Debug.Log ("spawn box start");

		// initialize letter frequency array for spawning statistics
		if (letterFreq == null) {
			letterFreq = new List<int> ();

			AddToLetterFreqList ();
		}
			
		// import the dictionary and assign it to the BoxScript.freqDictionary variable
		ImportDictionary ();

		/*
		// initialize flaggedBoard to all false
		// randomly initialize the beginning of the board
		for (int x = 0; x < initialBoard.GetLength (0); ++x) {
			for (int y = 0; y < initialBoard.GetLength (1); ++y) {
				int i = Random.Range (0, letterFreq.Count);
				initialBoard [x, y] = (char) (letterFreq[i] + 'A');
				flaggedBoard [x, y] = false;
			}
		}
        
		// check for any letters that form words and re-roll them until they don't form words
		while (!ContainsNoValidWords (initialBoard)) {
			// ContainsNoValidWords should have marked up the flaggedBoard
			RerollLetters();

			// reset flaggedBoard
			ResetFlaggedBoard();
		}

		PrintInitialBoard ();
		*/
	}

	void Update() {
		if (BoxScript.grid [myX, BoxScript.gridHeight - 1] == null &&
			!BoxScript.IsBoxInColumnFalling(myX)) {
			if (init && initCount < 9) {
				++initCount;
				SpawnNewBox ();

				if (initCount == 9) {
					isInit = true;
				}
			} else if (wait) {
				StartCoroutine(WaitForSpawn ());
				wait = false;
				init = false;
			}

		}
	}

	IEnumerator WaitForSpawn() {
		yield return new WaitForSeconds(0.15f);
		SpawnNewBox ();
		wait = true;
	}

	/**
	 * Creates a new spawnbox with a random letter at this location
	 */
	public void SpawnNewBox() {
        // TODO: fix this!!!
		int i = Random.Range (0, letterFreq.Count);
        SpawnNewBox((char)('A' + letterFreq[i]));
	}

	/**
	 * Creates a new spawnbox with the given letter (in caps, e.g. 'A', 'B', 'C', etc) at this location
	 */
	public void SpawnNewBox(char letter) {
        Transform box = Instantiate (boxPrefab, transform.position, Quaternion.identity);

        BoxScript script = box.GetComponent<BoxScript>();
        script.SetLetter(letter);

		if (init) {
			script.fallSpeed = 0.05f;
		}
	}

	public void Reset() {
		init = true;
		initCount = 0;
		isInit = false;
	}
}
