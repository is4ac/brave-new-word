using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SpawnBoxScript : MonoBehaviour
{

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

    public static int[] letterDistributions = {
        81,20,28,43,90,     // A B C D E
        22,20,61,70,8,      // F G H I J
        18,40,24,67,75,     // K L M N O
        19,8,60,63,90,      // P Q R S T
        28,10,24,10,20,10}; // U V W X Y Z
    public const int MAX_LETTER_FREQ = 90;

    public static bool isInitialized()
    {
        return isInit;
    }

    void AddToLetterFreqList()
    {
        for (int letter_index = 0; letter_index < 26; letter_index++)
        {
            int letterFreqCount = letterDistributions[letter_index];
            for (int i = 0; i < letterFreqCount; ++i)
            {
                letterFreq.Add(letter_index);
            }
        }
    }

    void ImportDictionary()
    {
        // import words list and put it in set
        if (BoxScript.freqDictionary == null)
        {
            BoxScript.freqDictionary = new Dictionary<string, Vector2>();

            string line;

            // Read the file and store it line by line.  
            string[] lines = dictionary.text.Split('\n');
            // skip first line, it is the header
            for (int i = 1; i < lines.Length; ++i)
            {
                line = lines[i];
                string[] tokens = line.Split(',');
                if (tokens.Length > 2)
                {
                    string word = tokens[0].ToUpper();
                    float freq = float.Parse(tokens[1]);
                    float rank = float.Parse(tokens[2]);
                    try
                    {
                        BoxScript.freqDictionary.Add(word, new Vector2(freq, rank));
                    }
                    catch (ArgumentException e)
                    {
                        Debug.LogWarning("Exception: " + e + " at line: " + line);
                    }
                }
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        // initialize letter frequency array for spawning statistics
        if (letterFreq == null)
        {
            letterFreq = new List<int>();

            AddToLetterFreqList();
        }

        // import the dictionary and assign it to the BoxScript.freqDictionary variable
        ImportDictionary();
    }

    void Update()
    {
        if (BoxScript.grid[myX, BoxScript.gridHeight - 1] == null &&
            !BoxScript.IsBoxInColumnFalling(myX))
        {
            if (init && initCount < 9)
            {
                ++initCount;
                SpawnNewBox();

                if (initCount == 9)
                {
                    isInit = true;
                }
            }
            else if (wait)
            {
                StartCoroutine(WaitForSpawn());
                wait = false;
                init = false;
            }

        }
    }

    IEnumerator WaitForSpawn()
    {
        yield return new WaitForSeconds(0.15f);
        SpawnNewBox();
        wait = true;
    }

    /**
	 * Creates a new spawnbox with a random letter at this location
	 */
    public void SpawnNewBox()
    {
        // TODO: fix this!!!
        int i = UnityEngine.Random.Range(0, letterFreq.Count);
        SpawnNewBox((char)('A' + letterFreq[i]));

        // Make a "woosh" sound effect when spawning
        AudioManager.instance.Play("Woosh");
    }

    /**
	 * Creates a new spawnbox with the given letter (in caps, e.g. 'A', 'B', 'C', etc) at this location
	 */
    public void SpawnNewBox(char letter)
    {
        Transform box = Instantiate(boxPrefab, transform.position, Quaternion.identity);

        BoxScript script = box.GetComponent<BoxScript>();
        script.SetLetter(letter);
    }

    public void Reset()
    {
        init = true;
        initCount = 0;
        isInit = false;
    }
}
