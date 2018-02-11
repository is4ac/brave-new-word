using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InstructionsPanelScript : MonoBehaviour, IPointerClickHandler {

	GameObject panel;

	// Use this for initialization
	void Start () {
		panel = GameObject.Find ("Instructions");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// When this panel is clicked
	public void OnPointerClick (PointerEventData eventData)
	{
		panel.SetActive (false);
	}
}
