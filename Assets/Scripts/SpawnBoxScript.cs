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

    int[] letterDistributions = new int[] {
        81,20,28,43,90,     // A B C D E
        22,20,61,70,8,      // F G H I J
        18,40,24,67,75,     // K L M N O
        19,8,60,63,90,      // P Q R S T
        28,10,24,10,20,10}; // U V W X Y Z
    void addToLetterFreqList() {
        for (int letter_index = 0; letter_index < 26; letter_index++)
        {
            int letterFreqCount = letterDistributions[letter_index];
            for (int i = 0; i < letterFreqCount; ++i)
            {
                letterFreq.Add(letter_index);
            }
       }
    }
	// Use this for initialization
	void Start () {
		// initialize letter frequency array for spawning statistics
		letterFreq = new List<int>();

        addToLetterFreqList();


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
        // import words list and put it in set
        if (BoxScript.freqDictionary == null)
        {
            BoxScript.freqDictionary  = new Dictionary<string, float>();

            string line;

            // Read the file and display it line by line.  
            StreamReader file = new StreamReader(@"Assets/Dictionaries/sortedenddict.csv");
            while ((line = file.ReadLine()) != null)
            {
                string[] tokens = line.Split(',');
                string word = tokens[0].ToUpper();
                float val = float.Parse(tokens[1]);
                BoxScript.freqDictionary.Add(word,val);
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
