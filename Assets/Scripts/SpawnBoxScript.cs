using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SpawnBoxScript : MonoBehaviour {

	[SerializeField]
	public GameObject[] boxList;
	[SerializeField]
	public int myX;

	bool init = true;
	int initCount = 0;
	bool wait = true;
	List<int> letterFreq;

	// Use this for initialization
	void Start () {
		// initialize letter frequency array for spawning statistics
		letterFreq = new List<int>();
		// A = 8.1, index val = 0
		for (int i = 0; i < 81; ++i) {
			letterFreq.Add (0);
		}
		// B = 1.5, index val = 1
		for (int i = 0; i < 20; ++i) {
			letterFreq.Add (1);
		}
		// C = 2.8, index val = 2
		for (int i = 0; i < 28; ++i) {
			letterFreq.Add (2);
		}
		// D = 4.3, index val = 3
		for (int i = 0; i < 43; ++i) {
			letterFreq.Add (3);
		}
		// E = 12.7, index val = 4
		for (int i = 0; i < 90; ++i) {
			letterFreq.Add (4);
		}
		// F = 2.2, index val = 5
		for (int i = 0; i < 22; ++i) {
			letterFreq.Add (5);
		}
		// G = 2.0, index val = 6
		for (int i = 0; i < 20; ++i) {
			letterFreq.Add (6);
		}
		// H = 6.1, index val = 7
		for (int i = 0; i < 61; ++i) {
			letterFreq.Add (7);
		}
		// I = 7.0, index val = 8
		for (int i = 0; i < 70; ++i) {
			letterFreq.Add (8);
		}
		// J = 0.2, index val = 9
		for (int i = 0; i < 8; ++i) {
			letterFreq.Add (9);
		}
		// K = 0.8, index val = 10
		for (int i = 0; i < 18; ++i) {
			letterFreq.Add (10);
		}
		// L = 4.0, index val = 11
		for (int i = 0; i < 40; ++i) {
			letterFreq.Add (11);
		}
		// M = 2.4, index val = 12
		for (int i = 0; i < 24; ++i) {
			letterFreq.Add (12);
		}
		// N = 6.7, index val = 13
		for (int i = 0; i < 67; ++i) {
			letterFreq.Add (13);
		}
		// O = 7.5, index val = 14
		for (int i = 0; i < 75; ++i) {
			letterFreq.Add (14);
		}
		// P = 1.9, index val = 15
		for (int i = 0; i < 19; ++i) {
			letterFreq.Add (15);
		}
		// Q = 0.1, index val = 16
		for (int i = 0; i < 8; ++i) {
			letterFreq.Add (16);
		}
		// R = 6.0, index val = 17
		for (int i = 0; i < 60; ++i) {
			letterFreq.Add (17);
		}
		// S = 6.3, index val = 18
		for (int i = 0; i < 63; ++i) {
			letterFreq.Add (18);
		}
		// T = 9.0, index val = 19
		for (int i = 0; i < 90; ++i) {
			letterFreq.Add (19);
		}
		// U = 2.8, index val = 20
		for (int i = 0; i < 28; ++i) {
			letterFreq.Add (20);
		}
		// V = 1.0, index val = 21
		for (int i = 0; i < 10; ++i) {
			letterFreq.Add (21);
		}
		// W = 2.4, index val = 22
		for (int i = 0; i < 24; ++i) {
			letterFreq.Add (22);
		}
		// X = 0.2, index val = 23
		for (int i = 0; i < 10; ++i) {
			letterFreq.Add (23);
		}
		// Y = 2.0, index val = 24
		for (int i = 0; i < 20; ++i) {
			letterFreq.Add (24);
		}
		// Z = 0.1, index val = 25
		for (int i = 0; i < 10; ++i) {
			letterFreq.Add (25);
		}

		// import words list and put it in set
		if (BoxScript.dictionary == null) {
			BoxScript.dictionary = new HashSet<string> ();

			string line;  

			// Read the file and display it line by line.  
			StreamReader file = new StreamReader(@"Assets/Dictionaries/wordsEn.txt");  
			while((line = file.ReadLine()) != null)  
			{  
				BoxScript.dictionary.Add(line);
			}  

			file.Close();  
		}
	}

	void Update() {
		if (BoxScript.grid [myX, BoxScript.gridHeight - 1] == null &&
			!BoxScript.isBoxInColumnFalling(myX)) {
			if (init && initCount < 9) {
				++initCount;
				SpawnNewBox ();
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

	// Update is called once per frame
	public void SpawnNewBox() {
		int i = Random.Range (0, letterFreq.Count);
		GameObject box = Instantiate (boxList [letterFreq[i]], transform.position, Quaternion.identity);

		if (init) {
			box.GetComponent<BoxScript> ().fallSpeed = 0.05f;
		}
	}
}
