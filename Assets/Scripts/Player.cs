using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;


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

    public Canvas sliderCanvas;
    public Slider gridSlider;
    public Camera arCamera;

    private ARRaycastManager rayManager;
    private PlacementIndicator placementIndicator;
    private AudioSource[] audioData;
    private Button hostPlayButton;

    private GameLogic gameLogic = GameLogic.GetInstance();
    private Network network;

    private GameObject Winner;
    private GameObject Grid;
    private GameObject[] ZerosOrCross = new GameObject[10];

    private bool playButtonClicked = false; // Play button from start menu
    private bool GridPlaced = false;        // Grid is placed
    private bool gameOverDisplayed = false; // Game over window is displayed
    private bool isPlayerOne = true;
    private bool playerOneGridPositionFinalized = false;
    private bool playerTwoGridPositionFinalized = false;

    private Mode mode;
    private Vector3[,] gridCenters = new Vector3[3, 3];
    private int iter;
    private float cellWidth;
    private Transform trans;
    private Plane finalGridPlane;


    void Start ()
    {
        rayManager = FindObjectOfType<ARRaycastManager> ();
        placementIndicator = FindObjectOfType<PlacementIndicator> ();
        audioData = GetComponents<AudioSource> ();
        iter = 0;
    }

    void Update ()
    {
        if (mode == Mode.MultiOnline && network.IsHost && !hostPlayButton.interactable && !playButtonClicked)
        {
            if (network.HasUpdate && network.MessageCode == 0)
            {
                network.GetMessage ();
                hostPlayButton.interactable = true;
            }
        }

        if (mode == Mode.MultiOnline && playerOneGridPositionFinalized && !playerTwoGridPositionFinalized)
        {
            if (network.HasUpdate && network.MessageCode == 2)
            {
                network.GetMessage ();
                playerTwoGridPositionFinalized = true;
            }
        }

        if (gameLogic.IsGameOver ())
        {
            State winner = gameLogic.WhoWon();
            ShowGameOverWindow (winner);
            return;
        }

        if (!GridPlaced)
        {
            if (placementIndicator.IsPlacementIndicatorPlaced ())
            {
                if (playButtonClicked)
                {
                    if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
                    {
                        trans = placementIndicator.transform;
                        trans.Translate (new Vector3 (0, -0.45f, -0.0f));
                        trans.Rotate (-90, 0, 0);
                        Grid = Instantiate (ObjectToPlace, trans.position, trans.rotation);

                        audioData[1].Play (0);
                        GridPlaced = true;
                        placementIndicator.Enable (false);

                        gridSlider.minValue = trans.position.y - 0.5f;
                        gridSlider.maxValue = trans.position.y + 0.5f;
                        gridSlider.value = trans.position.y;
                        sliderCanvas.gameObject.SetActive (true);

                        if (mode == Mode.MultiOnline)
                        {
                            Packet toSend = new Packet
                            {
                                id = 2,
                                message = "gridFinalized"
                            };
                            network.Send (toSend);
                        }
                    }
                }
            }
        }
        else if (playerOneGridPositionFinalized)
        {
            if (mode == Mode.MultiOnline && !playerTwoGridPositionFinalized)
                return;

            Vector2Int selectedCell = PlayRound();

            if (selectedCell.x != -1 || selectedCell.y != -1)
            {
                GameObject obj = isPlayerOne ? CrossToPlace : ZeroToPlace;
                ZerosOrCross[iter] = Instantiate (obj, gridCenters[selectedCell.x, selectedCell.y], new Quaternion ());
                ZerosOrCross[iter].transform.Rotate (-90, 0, 0);
                gameLogic.PlaceZeroOrCross (selectedCell, isPlayerOne);

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
                cell = PlayRoundOnline ();
                break;
        }

        return cell;
    }

    private Vector2Int PlayRoundLocal ()
    {
        Vector2Int cell = new Vector2Int(-1, -1); ;

        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            Ray ray = arCamera.ScreenPointToRay(Input.touches[0].position);
            float enter;

            if (finalGridPlane.Raycast (ray, out enter))
            {
                //Get the point that is clicked
                Vector3 hitPoint = ray.GetPoint(enter);
                cell = GetClosestCell (hitPoint); // compensate for the offset of the grid

                if (mode == Mode.MultiOnline)
                {
                    Packet toSend = new Packet
                    {
                        id = 1,
                        message = "update",
                        row = cell.x,
                        column = cell.y
                    };
                    network.Send (toSend);
                }
            }
        }

        return cell;
    }

    private Vector2Int PlayRoundAI ()
    {
        return gameLogic.findOptimalMove (isPlayerOne);
    }

    private Vector2Int PlayRoundOnline ()
    {
        if (network.HasUpdate && network.MessageCode == 1)
        {
            Packet message = network.GetMessage ();
            // player placed marker
            return new Vector2Int (message.row, message.column);
        }

        return new Vector2Int (-1, -1);
    }

    // Starts the game when play button is clicked
    public void PlayButtonClicked ()
    {
        playButtonClicked = true;
    }

    // Shows the game over window
    private void ShowGameOverWindow (State winner)
    {
        if (!gameOverDisplayed)
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

    public void PlayAgain ()
    {
        iter = 0;
        isPlayerOne = true;

        if (mode == Mode.MultiOnline && !network.IsHost)
            isPlayerOne = false;

        //To destroy the zeros and crosses
        for (int j = 0; j < ZerosOrCross.Length; j++)
        {
            Destroy (ZerosOrCross[j]);
        }

        //Update params in gamelogic
        gameLogic.RestartGame ();
        Winner.SetActive (false);

        //Hide the gameover window
        gameOverDisplayed = false;
    }

    public void ReturnToMenu ()
    {
        GridPlaced = false;
        Destroy (Grid);
        playButtonClicked = false;
        PlayAgain ();
        StartWindow.SetActive (true);
        playerOneGridPositionFinalized = false;
        
        if (mode == Mode.MultiOnline)
        {
            network.Disconnect ();
            playerTwoGridPositionFinalized = false;
        }
    }

    public void InitGrid (Vector3 gridCenter, float gridSize, Quaternion rotation, GameObject obj)
    {
        cellWidth = gridSize / 3;

        gridCenters[0, 0] = new Vector3 (gridCenter.x - cellWidth, gridCenter.y, gridCenter.z - cellWidth);
        gridCenters[0, 1] = new Vector3 (gridCenter.x, gridCenter.y, gridCenter.z - cellWidth);
        gridCenters[0, 2] = new Vector3 (gridCenter.x + cellWidth, gridCenter.y, gridCenter.z - cellWidth);
        gridCenters[1, 0] = new Vector3 (gridCenter.x - cellWidth, gridCenter.y, gridCenter.z);
        gridCenters[1, 1] = new Vector3 (gridCenter.x, gridCenter.y, gridCenter.z);
        gridCenters[1, 2] = new Vector3 (gridCenter.x + cellWidth, gridCenter.y, gridCenter.z);
        gridCenters[2, 0] = new Vector3 (gridCenter.x - cellWidth, gridCenter.y, gridCenter.z + cellWidth);
        gridCenters[2, 1] = new Vector3 (gridCenter.x, gridCenter.y, gridCenter.z + cellWidth);
        gridCenters[2, 2] = new Vector3 (gridCenter.x + cellWidth, gridCenter.y, gridCenter.z + cellWidth);

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
                Vector3 originalCenter = gridCenters[i, j];
                //originalCenter.y = pos.y;
                float currentDistance = Vector3.Distance(originalCenter, pos);
                if (currentDistance < minDistance && gameLogic.GetCellState (new Vector2Int (i, j)) == State.Empty)
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

    public void SetRole (string role)
    {
        if (role == "host")
        {
            hostPlayButton = GameObject.FindGameObjectWithTag ("HostPlayButton").GetComponent<Button> ();
            string ipAddress = network.StartHost ();
            Text ip = GameObject.FindGameObjectWithTag ("HostIpAddress").GetComponent<Text> ();
            ip.text = ipAddress;
            isPlayerOne = true;
        }

        if (role == "client")
        {
            isPlayerOne = false;
        }
    }

    public void ClientConnectClicked ()
    {
        InputField ip = GameObject.FindGameObjectWithTag ("ClientIpAddress").GetComponent<InputField> ();
        bool successful = network.StartClient (ip.text);
        if (successful)
        {
            network.Run ();
            GameObject.FindGameObjectWithTag ("ClientPlayButton").GetComponent<Button> ().interactable = true;
        }
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
                network = new Network ();
                break;
        }
    }

    public void ShareOnSocialMedia ()
    {
        Text ip = GameObject.FindGameObjectWithTag ("HostIpAddress").GetComponent<Text> ();
        new NativeShare ().SetText (ip.text).Share ();
    }

    public void AdjustGrid (float position)
    {
        Grid.transform.SetPositionAndRotation (new Vector3 (Grid.transform.position.x, position, Grid.transform.position.z), Grid.transform.rotation);
    }

    public void SetGridPositionFinalized (bool res)
    {
        playerOneGridPositionFinalized = res;
        InitGrid (Grid.transform.position, 1.3f, Grid.transform.rotation, ZeroToPlace);
        finalGridPlane.Set3Points (gridCenters[0, 0], gridCenters[0, 2], gridCenters[2, 1]);
        finalGridPlane.Translate (new Vector3 (0, -0.15f, 0));
    }
}
