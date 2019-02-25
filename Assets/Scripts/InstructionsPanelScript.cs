using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InstructionsPanelScript : MonoBehaviour, IPointerClickHandler {

	public GameObject panel;

	// Use this for initialization
	void Start () {
	}

	// When this panel is clicked
	public void OnPointerClick (PointerEventData eventData)
	{
		if (panel.activeSelf) {
			GameManagerScript.BeginGame ();

            // Log the click
            GameManagerScript.LogInstructionsClick(eventData.position);

			panel.SetActive (false);
		}
	}
}
