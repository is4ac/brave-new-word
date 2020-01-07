using UnityEngine;
using UnityEngine.EventSystems;

public class InstructionsPanelScript : MonoBehaviour, IPointerClickHandler
{

    public GameObject panel;

    // When this panel is clicked
    public void OnPointerClick(PointerEventData eventData)
    {
        if (panel.activeSelf)
        {
            GameManagerScript.BeginGame();

            // Log the click
            Logger.LogInstructionsClick(eventData.position);

            panel.SetActive(false);
        }
    }
}
