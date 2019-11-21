using UnityEngine;

public class TextMeshScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
        this.gameObject.GetComponent<Renderer>().sortingLayerID = 
            this.transform.parent.gameObject.GetComponent<Renderer>().sortingLayerID;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
