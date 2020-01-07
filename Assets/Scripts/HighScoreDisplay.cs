using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighScoreDisplay : MonoBehaviour
{

    class Triple
    {
        public long score;
        public string username;
        public string userID;

        public Triple(long score, string username, string userID)
        {
            this.score = score;
            this.username = username;
            this.userID = userID;
        }
    }

    public class CompareTriples : IComparer
    {
        // Subtract the two scores to return a correct value for compare
        int IComparer.Compare(object x, object y)
        {
            return (int)(((Triple)y).score - ((Triple)x).score);
        }
    }

    public static HighScoreDisplay instance;

    public GameObject contentGrid;
    public GameObject myScoreContent;
    public Transform elementPrefab;
    public Sprite elementSprite;
    public GameObject scrollBar;

    //ArrayList highScores;

    //private float contentWidth;

    // Initialization
    void Awake()
    {
        /*
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        RectTransform contentTransform = (RectTransform)contentGrid.transform;
        contentWidth = contentTransform.rect.width;

        highScores = new ArrayList();
        */
    }

    void OnEnable()
    {
        // Update and display the data
        //DBManager.instance.RetrieveTopScores();
    }

    public void UpdateHighScores()
    {
        /*
        Debug.Log("UpdateHighScores being called");
        highScores.Clear();

        // iterate through the dictionary
        foreach (KeyValuePair<string, long> entry in DBManager.instance.userToScore)
        {
            // add each score from the database, getting the username from the userIDtoUsername dictionary
            // and the userID from the Key
            highScores.Add(new Triple(entry.Value,
                                      DBManager.instance.userIDToUsernames[entry.Key],
                                      entry.Key));
        }

        CompareTriples comparer = new CompareTriples();
        highScores.Sort(comparer);
        */
    }

    public void DisplayHighScores()
    {
        /*
        Debug.Log("DisplayHighScores being called");

        // reset all children
        contentGrid.transform.DetachChildren();

        int rank = 1;
        foreach (Triple item in highScores)
        {
            AddHighScoreElement(rank++, item.username, item.score, item.userID);
        }
        */
    }

    void AddHighScoreElement(int rank, string username, long score, string userID)
    {
        /*
        // instantiate the prefab
        Transform element = Instantiate(elementPrefab);
        element.SetParent(contentGrid.transform, false);

        // update the text of the score and username
        element.GetChild(0).GetComponent<Text>().text = rank + "";
        element.GetChild(1).GetComponent<Text>().text = username;
        element.GetChild(2).GetComponent<Text>().text = score + "";
        element.GetComponent<LayoutElement>().minWidth = contentWidth;

        // highlight the panel if the user is the current user
        if (userID.Equals(GameManagerScript.userID))
        {
            element.GetComponent<Image>().color = Color.yellow;

            // update the personal score
            myScoreContent.transform.DetachChildren();
            Transform myScoreElement = Instantiate(elementPrefab);
            myScoreElement.SetParent(myScoreContent.transform, false);

            // update the text of the score and username
            myScoreElement.GetChild(0).GetComponent<Text>().text = rank + "";
            myScoreElement.GetChild(1).GetComponent<Text>().text = username;
            myScoreElement.GetChild(2).GetComponent<Text>().text = score + "";
            myScoreElement.GetComponent<LayoutElement>().minWidth = contentWidth;
            myScoreElement.GetComponent<Image>().color = Color.yellow;
        }
        */
    }
}
