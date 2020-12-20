using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


public enum Mode
{
    Single, MultiLocal, MultiOnline
}


public class Player : MonoBehaviour
{
    public GameObject ObjectToPlace;
    public GameObject CrossToPlace;
    public GameObject ZeroToPlace;

    public GameObject CrossWinnerScreen;
    public GameObject ZeroWinnerScreen;
    public GameObject GameOverScreen;
    public GameObject StartWindow;

    private ARRaycastManager rayManager;
    private PlacementIndicator placementIndicator;
    private AudioSource[] audioData;

    private GameLogic gameLogic = GameLogic.GetInstance();

    private GameObject Winner;
    private GameObject Grid;
    private GameObject[] ZerosOrCross = new GameObject[10];

    //Boolean variables
    private bool playButtonClicked = false; // Play button from start menu
    private bool GridPlaced = false;        // Grid is placed
    private bool gameOverDisplayed = false; // Game over window is displayed
    private bool isPlayerOne = true;

    private Mode mode;
    private Vector3[,] gridCenters = new Vector3[3, 3];
    private int iter;
    private float cellWidth;


    // Start is called before the first frame update
    void Start ()
    {
        rayManager = FindObjectOfType<ARRaycastManager>();
        placementIndicator = FindObjectOfType<PlacementIndicator>();
        audioData = GetComponents<AudioSource>();
        iter = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameLogic.IsGameOver())
        {
            State winner = gameLogic.WhoWon();
            ShowGameOverWindow(winner);
            return;
        }

        if (!GridPlaced)
        {
            if(playButtonClicked)
            {                
				if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
				{
					Transform trans = placementIndicator.transform;
					trans.Translate (new Vector3 (0, -0.45f, -0.0f));
					trans.Rotate(-90, 0, 0);
                    Grid = Instantiate(ObjectToPlace, trans.position, trans.rotation);
                    InitGrid (trans.position, 1.3f, trans.rotation, ZeroToPlace);
                    audioData[1].Play(0);

					GridPlaced = true;
					placementIndicator.Enable(false);
				}
            }
        }
        else
        {            
            Vector2Int selectedCell = PlayRound();

            if (selectedCell.x != -1 || selectedCell.y != -1)
            {
                GameObject obj = !isPlayerOne ? CrossToPlace : ZeroToPlace;
                ZerosOrCross[iter] = Instantiate (obj, gridCenters[selectedCell.x, selectedCell.y], new Quaternion ());
                ZerosOrCross[iter].transform.Rotate (-90, 0, 0);
                gameLogic.PlaceZeroOrCross (selectedCell, !isPlayerOne);

                audioData[0].Play (0);
                iter++;
                isPlayerOne = !isPlayerOne;
            }                       
        }
    }

    private Vector2Int PlayRound ()
    {
        if (isPlayerOne)
            return PlayRoundLocal ();

        // else player two
        Vector2Int cell = new Vector2Int(-1, -1);

        switch (mode)
        {
            case Mode.Single:
                cell = PlayRoundAI ();
                break;
            case Mode.MultiLocal:
                cell = PlayRoundLocal ();
                break;
            case Mode.MultiOnline:
                cell = PlayRoundOnline();
                break;
        }

        return cell;
    }

    private Vector2Int PlayRoundLocal ()
    {
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            rayManager.Raycast (Input.touches[0].position, hits, TrackableType.Planes);

            if (hits.Count > 0)
                return GetClosestCell (hits[0].pose.position - new Vector3 (0, 0.2f, 0)); // compensate for the offset of the grid
        }
        return new Vector2Int(-1, -1);
    }

    private Vector2Int PlayRoundAI ()
    {
        return gameLogic.findOptimalMove ();
    }

    private Vector2Int PlayRoundOnline ()
    {
        return new Vector2Int (-1, -1);
    }

    // Starts the game when play button is clicked
    public void PlayButtonClicked()
    {
        playButtonClicked = true;
    }

    // Shows the game over window
    private void ShowGameOverWindow(State winner)
    {
        if(!gameOverDisplayed)
        {
            switch (winner)
            {
                case State.X:
                    CrossWinnerScreen.SetActive (true);
                    Winner = CrossWinnerScreen;
                    break;
                case State.O:
                    ZeroWinnerScreen.SetActive (true);
                    Winner = ZeroWinnerScreen;
                    break;
                case State.Empty:
                    GameOverScreen.SetActive (true);
                    Winner = GameOverScreen;
                    break;
            }

            gameOverDisplayed = true;
        }
    }

    public void PlayAgain()
    {
        iter = 0;
        isPlayerOne = true;

        //To destroy the zeros and crosses
        for (int j = 0; j < ZerosOrCross.Length; j++)
        {
            Destroy(ZerosOrCross[j]);
        }

        //Update params in gamelogic
        gameLogic.RestartGame();
        Winner.SetActive(false);

        //Hide the gameover window
        gameOverDisplayed = false;
    }

    public void ReturnToMenu()
    {
        GridPlaced = false;
        Destroy(Grid);

        playButtonClicked = false;
        PlayAgain();
        StartWindow.SetActive(true);
    }

    public void InitGrid (Vector3 gridCenter, float gridSize, Quaternion rotation, GameObject obj)
    {
        cellWidth = gridSize / 3;        

        gridCenters[0,0] = new Vector3 (gridCenter.x - cellWidth, gridCenter.y, gridCenter.z - cellWidth);
        gridCenters[0,1] = new Vector3 (gridCenter.x            , gridCenter.y, gridCenter.z - cellWidth);
        gridCenters[0,2] = new Vector3 (gridCenter.x + cellWidth, gridCenter.y, gridCenter.z - cellWidth);
        gridCenters[1,0] = new Vector3 (gridCenter.x - cellWidth, gridCenter.y, gridCenter.z            );
        gridCenters[1,1] = new Vector3 (gridCenter.x            , gridCenter.y, gridCenter.z            );
        gridCenters[1,2] = new Vector3 (gridCenter.x + cellWidth, gridCenter.y, gridCenter.z            );
        gridCenters[2,0] = new Vector3 (gridCenter.x - cellWidth, gridCenter.y, gridCenter.z + cellWidth);
        gridCenters[2,1] = new Vector3 (gridCenter.x            , gridCenter.y, gridCenter.z + cellWidth);
        gridCenters[2,2] = new Vector3 (gridCenter.x + cellWidth, gridCenter.y, gridCenter.z + cellWidth);

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                gridCenters[i, j] = CorrectCellCenters (obj, gridCenters[i, j], rotation);
            }
        }
    }

    private Vector3 CorrectCellCenters (GameObject obj, in Vector3 cellCenter, Quaternion rot)
    {
        Vector3 offset = new Vector3 (0.0f, 0.25f, -0.0f);
        obj.transform.SetPositionAndRotation (cellCenter + offset, rot);
        obj.transform.RotateAround (gridCenters[1, 1], Vector3.up, rot.eulerAngles.y);
        return obj.transform.position;
    }

    public Vector2Int GetClosestCell (Vector3 pos)
    {
        float minDistance = 1000.0f;
        Vector2Int selectedCell = new Vector2Int(-1, -1);
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                float currentDistance = Vector3.Distance(gridCenters[i,j], pos);
                if (currentDistance < minDistance && gameLogic.GetCellState(new Vector2Int(i, j)) == State.Empty)
                {
                    minDistance = currentDistance;
                    selectedCell = new Vector2Int (i, j);
                }
            }
        }

        if (minDistance > cellWidth / 1.5)
            return new Vector2Int (-1, -1);

        return selectedCell;
    }

    public void SetMode (string mode_name)
    {
        switch (mode_name)
        {
            case "Single":
                mode = Mode.Single;
                break;
            case "MultiLocal":
                mode = Mode.MultiLocal;
                break;
            case "MultiOnline":
                mode = Mode.MultiOnline;
                break;
        }
    }

}
    