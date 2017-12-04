using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SpawnBoxScript : MonoBehaviour {

	[SerializeField]
	public GameObject[] boxList;
	[SerializeField]
	public int myX;
	//[SerializeField]
	//BoxScript boxScript;

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
			SpawnNewBox ();
		}
	}
	
	// Update is called once per frame
	public void SpawnNewBox() {
		int i = Random.Range (0, boxList.Length);
		Instantiate (boxList [i], transform.position, Quaternion.identity);
	}
}
