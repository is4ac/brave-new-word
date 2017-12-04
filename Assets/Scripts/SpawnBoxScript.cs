using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBoxScript : MonoBehaviour {

	[SerializeField]
	public GameObject[] boxList;

	// Use this for initialization
	void Start () {
		SpawnNewBox ();
	}
	
	// Update is called once per frame
	public void SpawnNewBox() {
		int i = Random.Range (0, boxList.Length);
		Instantiate (boxList [i], transform.position, Quaternion.identity);
	}
}
