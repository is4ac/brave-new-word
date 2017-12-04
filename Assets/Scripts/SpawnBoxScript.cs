using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
