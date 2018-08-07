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
        if (!GameManagerScript.INSTRUCTIONS_PANEL) {
            panel.SetActive(false);
        }
	}

	// When this panel is clicked
	public void OnPointerClick (PointerEventData eventData)
	{
		if (panel.activeSelf) {
			GameManagerScript.BeginGame ();

			/*************************************
			 * TODO: Do some logging actions here
			 *************************************/

			panel.SetActive (false);
		}
	}
}
