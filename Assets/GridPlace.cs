using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


public enum Mode
{
    Single, MultiLocal, MultiOnline
}


public class GridPlace : MonoBehaviour
{
    private ARRaycastManager rayManager;
    private PlacementIndicator placementIndicator;
    AudioSource[] audioData;

    private GameLogic gameLogic = GameLogic.GetInstance();

    public GameObject ObjectToPlace;
    public GameObject crossToPlace;
    public GameObject zeroToPlace;
    public Mode mode;

    public GameObject CrossWinnerScreen;
    public GameObject ZeroWinnerScreen;
    public GameObject GameOverScreen;
    public GameObject Winner;

    private GameObject Grid;

    //Zeros and Crosses Object
    public GameObject[] ZerosOrCross = new GameObject[10];
    int iter ;

    public GameObject startWindow;
    public GameObject Camera;

    //Boolean variables
    public bool playButtonClicked = false; // Play button from start menu
    public bool GridPlaced = false;        // Grid is placed
    public bool gameOverDisplayed = false; // Game over window is displayed
    private bool isPlayerOne = true;

    // Start is called before the first frame update
    void Start()
    {
        rayManager = FindObjectOfType<ARRaycastManager>();
        placementIndicator = FindObjectOfType<PlacementIndicator>();
        audioData = GetComponents<AudioSource>();
        iter = 0;
        mode = Mode.MultiLocal;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameLogic.IsGameOver())
        {
            Player winner = gameLogic.WhoWon();
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
                    gameLogic.InitGrid (trans.position, 1.3f, trans.rotation, zeroToPlace);
                    audioData[1].Play(0);

					GridPlaced = true;
					placementIndicator.Enable(false);
				}
            }
        }
        else
        {
            if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
            {
                Vector2Int selectedCell = PlayRound();

                if (selectedCell.x != -1 || selectedCell.y != -1)
                {
                    GameObject obj = isPlayerOne ? crossToPlace : zeroToPlace;
                    ZerosOrCross[iter] = Instantiate (obj, gameLogic.Grid[selectedCell.x, selectedCell.y].Center, new Quaternion ());
                    ZerosOrCross[iter].transform.Rotate (-90, 0, 0);
                    gameLogic.PlaceZeroOrCross (selectedCell, isPlayerOne);

                    audioData[0].Play (0);
                    iter++;
                    isPlayerOne = !isPlayerOne;
                }

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
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        rayManager.Raycast(Input.touches[0].position, hits, TrackableType.Planes);

        if (hits.Count > 0)
            return gameLogic.GetClosestCell (hits[0].pose.position - new Vector3(0, 0.2f, 0)); // compensate for the offset of the grid

        return new Vector2Int(-1, -1);
    }

    private Vector2Int PlayRoundAI ()
    {
        return new Vector2Int (-1, -1);
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
    public void ShowGameOverWindow(Player winner)
    {
        if(!gameOverDisplayed)
        {
            
            if (winner == Player.X)
            {
                CrossWinnerScreen.SetActive(true);
                Winner = CrossWinnerScreen;
            }
            else if (winner == Player.O)
            {
                ZeroWinnerScreen.SetActive(true);
                Winner = ZeroWinnerScreen;
            }
            else if(winner == Player.Empty)
            {
                GameOverScreen.SetActive(true);
                Winner = GameOverScreen;
            }
            gameOverDisplayed = true;

            // Camera.SetActive(false);
        }        
    }

    public void PlayAgain()
    {
        iter = 0;

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
        //Camera.SetActive(true);
    }

    public void ReturnToMenu()
    {
        GridPlaced = false;
        Destroy(Grid);

        playButtonClicked = false;
        PlayAgain();
        startWindow.SetActive(true);
       // Camera.SetActive(false);
    }

}
