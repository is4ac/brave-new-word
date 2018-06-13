using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchInputHandler : MonoBehaviour {
    
    public static bool touchEnabled = false;
    private GameManagerScript gameManager;

	// Use this for initialization
	void Start () {
        gameManager = GetComponent<GameManagerScript>();
	}

    /**
     * Returns the tile's coordinates that matches the touch input's pixel coordinates
     * 
     * returns Vector2(-1, -1) if no candidate tile is found
     */
    private Vector2 GetTilePositionFromTouchInput(Vector2 touchPos) {
        for (int i = 0; i < BoxScript.grid.GetLength(0); ++i) {
            for (int j = 0; j < BoxScript.grid.GetLength(1); ++j) {
                BoxScript boxObj = BoxScript.grid[i, j].gameObject.GetComponent<BoxScript>();

                if (boxObj.IsInsideTile(touchPos)) {
                    return new Vector2(i, j);
                }
            }
        }

        return new Vector2(-1, -1);
    }
	
	// Update is called once per frame
	void Update () {
        if (touchEnabled)
        {
            // ButtonUI touch inputs
            if (GameManagerScript.currentVersion == GameManagerScript.Versions.ButtonUI &&
                Input.touchCount > 0)
            {
                // convert pixel position to tile coordinate
                Vector2 myPos = GetTilePositionFromTouchInput(Input.GetTouch(0).position);

                if (myPos.x >= 0)
                {
                    if (Input.GetTouch(0).phase == TouchPhase.Began)
                    {
                        // there is no previously clicked box
                        if (BoxScript.currentSelection.Count == 0)
                        {
                            BoxScript.SelectTile(myPos);
                            BoxScript.LogAction("WF_LetterSelected", myPos);
                        }
                        else if (BoxScript.IsNextTo(myPos, BoxScript.currentSelection[BoxScript.currentSelection.Count - 1]) &&
                                 !BoxScript.currentSelection.Contains(myPos))
                        {
                            // add on to the current selection 
                            BoxScript.SelectTile(myPos);
                            BoxScript.LogAction("WF_LetterSelected", myPos);
                        }
                        else
                        {
                            // de-select what has already been selected
                            BoxScript.ClearAllSelectedTiles();
                            BoxScript.LogAction("WF_DeselectAll");
                        }

                    }
                    else if (Input.GetTouch(0).phase == TouchPhase.Moved)
                    {
                        // selected tile and it isn't already selected)
                        if (BoxScript.currentSelection.Count > 0 &&
                           BoxScript.IsNextTo(myPos, BoxScript.currentSelection[BoxScript.currentSelection.Count - 1]) &&
                            !BoxScript.currentSelection.Contains(myPos))
                        {
                            BoxScript.SelectTile(myPos);
                            BoxScript.LogAction("WF_LetterSelected", myPos);
                        }
                        // do nothing if you moved within the most recently selected tile
                        else if (BoxScript.currentSelection[BoxScript.currentSelection.Count - 1] == myPos)
                        {
                            // do nothing
                        }
                        else if (BoxScript.currentSelection.Contains(myPos))
                        {
                            // de-select the most recent tile(s) if you move back to an old one
                            for (int i = BoxScript.currentSelection.Count - 1; i > 0; --i)
                            {
                                if (BoxScript.currentSelection[BoxScript.currentSelection.Count - 1] != myPos)
                                {
                                    BoxScript.RemoveLastSelection();
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            // SwipeUI touch input
            else if (GameManagerScript.currentVersion == GameManagerScript.Versions.SwipeUI &&
                     Input.touchCount > 0)
            {
                // convert pixel position to tile coordinate
                Vector2 myPos = GetTilePositionFromTouchInput(Input.GetTouch(0).position);

                // If SwipeUI, automatically play word when lifting the finger, and cancel if canceled for all UI's
                if (Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    gameManager.PlayWord();
                }
                else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Canceled)
                {
                    BoxScript.ClearAllSelectedTiles();
                    BoxScript.LogAction("WF_DeselectAll");
                }

                if (myPos.x >= 0)
                {
                    if (Input.GetTouch(0).phase == TouchPhase.Began)
                    {
                        // there is no previously clicked box
                        if (BoxScript.currentSelection.Count == 0)
                        {
                            BoxScript.SelectTile(myPos);
                            BoxScript.LogAction("WF_LetterSelected", myPos);
                        }
                        else
                        {
                            // de-select what has already been selected
                            BoxScript.ClearAllSelectedTiles();
                            BoxScript.LogAction("WF_DeselectAll");
                        }
                    }
                    else if (Input.GetTouch(0).phase == TouchPhase.Moved)
                    {
                        // new tile and it isn't already selected)
                        if (BoxScript.IsNextTo(myPos, BoxScript.currentSelection[BoxScript.currentSelection.Count - 1]) &&
                            !BoxScript.currentSelection.Contains(myPos))
                        {
                            BoxScript.SelectTile(myPos);
                            BoxScript.LogAction("WF_LetterSelected", myPos);
                        }
                        // most recently selected tile (then do nothing, since you just selected it)
                        else if (BoxScript.currentSelection[BoxScript.currentSelection.Count - 1] == myPos)
                        {
                            // do nothing
                        }
                        // you've moved back to a previous tile that was already selected
                        else if (BoxScript.currentSelection.Contains(myPos))
                        {
                            // de-select the most recent tile(s) if you move back to an old one
                            for (int i = BoxScript.currentSelection.Count - 1; i > 0; --i)
                            {
                                if (BoxScript.currentSelection[BoxScript.currentSelection.Count - 1] != myPos)
                                {
                                    BoxScript.RemoveLastSelection();
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
	}
}
