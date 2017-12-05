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

	// Use this for initialization
	void Start () {
		//SpawnNewBox ();
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
		int i = Random.Range (0, boxList.Length);
		GameObject box = Instantiate (boxList [i], transform.position, Quaternion.identity);

		if (init) {
			box.GetComponent<BoxScript> ().fallSpeed = 0.075f;
		} else {
			box.GetComponent<BoxScript> ().fallSpeed = 0.4f;
		}
	}
}
