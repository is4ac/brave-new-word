using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RandomNameScript : MonoBehaviour
{

    public TextAsset usernamesText;

    public TextMeshProUGUI welcomeText;
    private static List<string> attributes = null;
    private static List<string> colors = null;
    private static List<string> animals = null;
    public static Firebase.Auth.FirebaseAuth auth;
    private Firebase.Auth.FirebaseUser _newUser;
    private bool _updateText = false;

    public void Awake()
    {
        // Generate a random username
        InitializeUsernameList();
        DbManager.OnUsernameChange += OnUsernameChange;
        StartGameScript.OnUsernameChange += OnUsernameChange;
        StartGameScript.OnRandomizeName += RandomizeName;
    }

    private void Update()
    {
        if (_updateText)
        {
            welcomeText.text = "Welcome,<br>" + GameManagerScript.username + "!";
            _updateText = false;
        }
    }

    private void OnUsernameChange(string username)
    {
        GameManagerScript.username = username;
        _updateText = true;
    }

    void InitializeUsernameList()
    {
        // import username list and put it into corresponding lists
        string[] lines = usernamesText.text.Split('\n');

        if (attributes == null)
        {
            attributes = new List<string>();

            // read first line of the file
            string line = lines[0];

            if (line != null)
            {
                string[] tokens = line.Split(',');
                for (int i = 0; i < tokens.Length; ++i)
                {
                    attributes.Add(tokens[i]);
                }
            }
        }

        if (colors == null)
        {
            colors = new List<string>();

            // read second line of the file
            string line = lines[1];

            if (line != null)
            {
                string[] tokens = line.Split(',');
                for (int i = 0; i < tokens.Length; ++i)
                {
                    colors.Add(tokens[i]);
                }
            }
        }

        if (animals == null)
        {
            animals = new List<string>();

            // read third line of the file
            string line = lines[2];

            if (line != null)
            {
                string[] tokens = line.Split(',');
                for (int i = 0; i < tokens.Length; ++i)
                {
                    animals.Add(tokens[i]);
                }
            }
        }
    }

    public void RandomizeName()
    {
        int i = UnityEngine.Random.Range(0, attributes.Count);
        int j = UnityEngine.Random.Range(0, colors.Count);
        int k = UnityEngine.Random.Range(0, animals.Count);

        GameManagerScript.username = attributes[i].Trim() + " " + colors[j].Trim() + " " + animals[k].Trim();
        _updateText = true;
    }
}
