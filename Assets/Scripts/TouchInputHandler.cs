using UnityEngine;

public class TouchInputHandler : MonoBehaviour
{
    public static bool inputEnabled = false;
    public static bool touchEnabled = false;
    public static bool touchSupported = false;
    public GameObject particleSystemPrefab;
    private GameManagerScript gameManager;
    private float timer;
    private float waitTime = 0.2f;
    private Vector2 lastRemoved;

    private void Awake()
    {
        gameManager = GetComponent<GameManagerScript>();
    }

    /**
     * Returns the tile's coordinates that matches the touch input's pixel coordinates
     * 
     * returns Vector2(-1, -1) if no candidate tile is found
     */
    private Vector2 GetTilePositionFromTouchInput(Vector2 touchPos)
    {
        for (int i = 0; i < BoxScript.grid.GetLength(0); ++i)
        {
            for (int j = 0; j < BoxScript.grid.GetLength(1); ++j)
            {
                if (BoxScript.grid[i, j] != null)
                {
                    BoxScript boxObj = BoxScript.grid[i, j].gameObject.GetComponent<BoxScript>();

                    if (boxObj.IsInsideTile(touchPos, .92f))
                    {
                        return new Vector2(i, j);
                    }
                }
            }
        }

        return new Vector2(-1, -1);
    }

    private bool IsInsideGameBoard(Vector2 touchPos)
    {
        Vector2 realPos = Camera.main.ScreenToWorldPoint(touchPos);
        if (realPos.y < 5 && realPos.y > -4.5)
        {
            return true;
        }

        return false;
    }

    private void PlayParticleSystem(Vector3 pos)
    {
        //Instantiate _particleSystemPrefab as new GameObject.
        GameObject _particleSystemObj = Instantiate(particleSystemPrefab);

        // Set new Particle System GameObject as a child of desired GO.
        // Right now parent would be the same GO in which this script is attached
        // You can also make it others child by ps.transform.parent = otherGO.transform.parent;

        // After setting this, replace the position of that GameObject as where the parent is located.
        _particleSystemObj.transform.position = pos;

        ParticleSystem _particleSystem = _particleSystemObj.GetComponent<ParticleSystem>();
        _particleSystem.Play();
    }

    // Update is called once per frame
    void Update()
    {
        // if input is disabled (pop up window, etc) then skip
        if (!inputEnabled) return;

        bool touch = touchSupported && touchEnabled && Input.touchCount > 0;
        bool hasTouchHappened = false;

        Vector2 myPos;
        Vector2 myPosPixel = new Vector2();

        // touch input
        if (touch)
        {
            myPosPixel = Input.GetTouch(0).position;
            hasTouchHappened = true;
        }
        // mouse input
        else if (!touchSupported && (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0)))
        {
            myPosPixel = Input.mousePosition;
            hasTouchHappened = true;
        }
        //else
        //{
        //    // end the function
        //    return;
        //}

        //=====FEATURE: UNPRODUCTIVE JUICY PARTICLES============================
        // Only if the position is NOT the default 0,0
        //=====================================================================
        if (GameManagerScript.JUICE_UNPRODUCTIVE && hasTouchHappened)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                PlayParticleSystem(Camera.main.ScreenToWorldPoint(myPosPixel));
                timer += waitTime;
            }
        }
        else
        {
            timer = 0;
        }

        // convert pixel position to tile coordinate
        myPos = GetTilePositionFromTouchInput(myPosPixel);

        //=================Productive/Unproductive Obstruction touch=================
        if (GameManagerScript.OBSTRUCTION_PRODUCTIVE || 
            GameManagerScript.OBSTRUCTION_UNPRODUCTIVE)
        {
            if ((touch && Input.GetTouch(0).phase == TouchPhase.Began) ||
                Input.GetMouseButtonDown(0))
            {
                if (myPos.x >= 0)
                {
                    // there is no previously clicked box
                    if (BoxScript.currentSelection.Count == 0)
                    {
                        BoxScript.SelectTile(myPos);
                        BoxScript.LogAction("BNW_LetterSelected", myPos);
                        lastRemoved = new Vector2(-1, -1);
                    }
                    // add on to the current selection 
                    else if (BoxScript.IsNextTo(myPos, BoxScript.currentSelection[BoxScript.currentSelection.Count - 1]) &&
                             !BoxScript.currentSelection.Contains(myPos))
                    {
                        BoxScript.SelectTile(myPos);
                        BoxScript.LogAction("BNW_LetterSelected", myPos);
                        lastRemoved = new Vector2(-1, -1);
                    }
                    // deselect previous letters if clicking on already highlighted letters
                    else if (BoxScript.currentSelection.Contains(myPos))
                    {
                        // deselect if this is the most recently selected letter
                        if (BoxScript.currentSelection[BoxScript.currentSelection.Count - 1] == myPos)
                        {
                            lastRemoved = myPos;
                            BoxScript.RemoveLastSelection();
                        }
                        // go through backwards and remove all letters in selection until the letter that was tapped
                        else 
                        {
                            lastRemoved = new Vector2(-1, -1);
                            for (int i = BoxScript.currentSelection.Count - 1; i >= 0; --i)
                            {
                                if (BoxScript.currentSelection[i] == myPos)
                                {
                                    BoxScript.RemoveTilesPastIndex(i);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        lastRemoved = new Vector2(-1, -1);
                        // de-select what has already been selected
                        BoxScript.ClearAllSelectedTiles();
                        BoxScript.LogAction("BNW_DeselectAll");
                    }
                }
                /*
                else if (!IsInsideGameBoard(myPosPixel))
                {
                    // remove all selected letters, unless clicking inside game area
                    BoxScript.ClearAllSelectedTiles();
                    lastRemoved = new Vector2(-1, -1);
                }
                */
            }
            //=====Only for PRODUCTIVE versions of button, not unproductive=====
            else if (GameManagerScript.OBSTRUCTION_PRODUCTIVE &&
                     ((touch && Input.GetTouch(0).phase == TouchPhase.Moved) ||
                      (!touchSupported && 
                       !Input.GetMouseButtonDown(0) && 
                       !Input.GetMouseButtonUp(0) && 
                       Input.GetMouseButton(0))))
            {
                if (myPos.x >= 0)
                {
                    // selected tile and it isn't already selected)
                    if (BoxScript.currentSelection.Count > 0 &&
                       BoxScript.IsNextTo(myPos, BoxScript.currentSelection[BoxScript.currentSelection.Count - 1]) &&
                        !BoxScript.currentSelection.Contains(myPos) &&
                        myPos != lastRemoved)
                    {
                        lastRemoved = new Vector2(-1, -1);
                        BoxScript.SelectTile(myPos);
                        BoxScript.LogAction("BNW_LetterSelected", myPos);
                    }
                    // do nothing if you moved within the most recently selected tile
                    else if (BoxScript.currentSelection.Count > 0 
                             && BoxScript.currentSelection[BoxScript.currentSelection.Count - 1] == myPos)
                    {
                        // do nothing
                    }
                    else if (BoxScript.currentSelection.Contains(myPos))
                    {
                        lastRemoved = new Vector2(-1, -1);
                        // de-select the most recent tile(s) if you move back to an old one
                        for (int i = BoxScript.currentSelection.Count - 1; i >= 0; --i)
                        {
                            if (BoxScript.currentSelection[i] == myPos)
                            {
                                BoxScript.RemoveTilesPastIndex(i);
                                break;
                            }
                        }
                    }
                }
            }
        }

        //=========================Control version touch input==========================
        else
        {
            // If Control, automatically play word when lifting the finger, and cancel if canceled for all UI's
            if (((touch && Input.GetTouch(0).phase == TouchPhase.Ended) || 
                 !touchSupported && Input.GetMouseButtonUp(0))
                && BoxScript.currentSelection.Count > 2)
            {
                gameManager.PlayWord();
            }
            else if ((touch && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Canceled)
                     || ((touch && Input.GetTouch(0).phase == TouchPhase.Ended) || 
                         (!touchSupported && Input.GetMouseButtonUp(0))
                    && BoxScript.currentSelection.Count < 3))
            {
                BoxScript.ClearAllSelectedTiles();
                BoxScript.LogAction("BNW_DeselectAll");
            }

            if (myPos.x >= 0)
            {
                if ((touch && Input.GetTouch(0).phase == TouchPhase.Began) ||
                    (!touchSupported && Input.GetMouseButtonDown(0)))
                {
                    // there is no previously clicked box
                    if (BoxScript.currentSelection.Count == 0)
                    {
                        BoxScript.SelectTile(myPos);
                        BoxScript.LogAction("BNW_LetterSelected", myPos);
                    }
                    else
                    {
                        // de-select what has already been selected
                        BoxScript.ClearAllSelectedTiles();
                        BoxScript.LogAction("BNW_DeselectAll");
                    }
                }
                else if ((touch && Input.GetTouch(0).phase == TouchPhase.Moved) ||
                         (!touchSupported && Input.GetMouseButton(0)))
                {
                    // new tile and it isn't already selected)
                    if (BoxScript.currentSelection.Count > 0 && 
                        BoxScript.IsNextTo(myPos, BoxScript.currentSelection[BoxScript.currentSelection.Count - 1]) &&
                        !BoxScript.currentSelection.Contains(myPos))
                    {
                        BoxScript.SelectTile(myPos);
                        BoxScript.LogAction("BNW_LetterSelected", myPos);
                    }
                    // most recently selected tile (then do nothing, since you just selected it)
                    else if (BoxScript.currentSelection.Count > 0 && 
                             BoxScript.currentSelection[BoxScript.currentSelection.Count - 1] == myPos)
                    {
                        // do nothing
                    }
                    // you've moved back to a previous tile that was already selected
                    else if (BoxScript.currentSelection.Contains(myPos))
                    {
                        // de-select the most recent tile(s) if you move back to an old one
                        for (int i = BoxScript.currentSelection.Count - 1; i >= 0; --i)
                        {
                            if (BoxScript.currentSelection[i] == myPos)
                            {
                                BoxScript.RemoveTilesPastIndex(i);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
