using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class GridPlace : MonoBehaviour
{
    private ARRaycastManager rayManager;
    private PlacementIndicator placementIndicator;


    private GameLogic gameLogic = GameLogic.GetInstance();

    public GameObject ObjectToPlace;
    public GameObject crossToPlace;
    public GameObject zeroToPlace;

    public GameObject[] ZerosOrCross = new GameObject[10];
    int iter ;

    public GameObject gameOverWindow;

    //Boolean variables
    public bool playButtonClicked = false; // Play button from start menu
    public bool GridPlaced = false;        // Grid is placed
    public bool gameOverDisplayed = false; // Game over window is displayed
    private bool isCross = true;

    // Start is called before the first frame update
    void Start()
    {
        rayManager = FindObjectOfType<ARRaycastManager>();
        placementIndicator = FindObjectOfType<PlacementIndicator>();
        iter = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameLogic.IsGameOver())
        {
            ShowGameOverWindow();
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
					GameObject obj = Instantiate(ObjectToPlace, trans.position, trans.rotation);

					//obj.transform = temp;
					gameLogic.InitGrid (trans.position, 1.3f, trans.rotation, zeroToPlace);

					//Vector3 objectSize = Vector3.Scale(transform.localScale, obj.bounds.size);
					//Vector3 objectSize = Vector3.Scale(transform.localScale, obj.mesh.bounds.size);

					GridPlaced = true;
					placementIndicator.Enable(false);
				}
            }
        }
        else
        {
            if (isCross)
            {                
                isCross = !UpdateCross();
            } else
            {
                isCross = UpdateZero();
            }
        }
    }

    private bool UpdateCross()
    {
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            rayManager.Raycast(Input.touches[0].position, hits, TrackableType.Planes);

            if (hits.Count > 0)
            {
                //GameObject obj = Instantiate(crossToPlace, hits[0].pose.position, hits[0].pose.rotation);
                ZerosOrCross[iter] = Instantiate(crossToPlace, hits[0].pose.position, hits[0].pose.rotation);
                ZerosOrCross[iter].transform.Translate (new Vector3 (0, -0.3f, -0.0f));
                ZerosOrCross[iter].transform.Rotate(-90, 0, 0);
                gameLogic.PlaceZeroOrCross (ZerosOrCross[iter], true);
                iter++;
            }

            return true;
        }

        return false;
    }

    private bool UpdateZero()
    {
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            rayManager.Raycast(Input.touches[0].position, hits, TrackableType.Planes);

            if (hits.Count > 0)
            {
                //GameObject obj = Instantiate(zeroToPlace, hits[0].pose.position, hits[0].pose.rotation);
                ZerosOrCross[iter] = Instantiate(zeroToPlace, hits[0].pose.position, hits[0].pose.rotation);
                ZerosOrCross[iter].transform.Translate (new Vector3 (0, -0.25f, -0.0f));
                ZerosOrCross[iter].transform.Rotate(-90, 0, 0);
                gameLogic.PlaceZeroOrCross (ZerosOrCross[iter], false);
                iter++;
            }

            return true;
        }

        return false;
    }
	
    // Starts the game when play button is clicked
    public void PlayButtonClicked()
    {
        playButtonClicked = true;
    }

    // Shows the game over window
    public void ShowGameOverWindow()
    {
        if(!gameOverDisplayed)
        {
            gameOverWindow.SetActive(true);
            gameOverDisplayed = true;
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

        //Hide the gameover window
        gameOverWindow.SetActive(false);
        gameOverDisplayed = false;

    }


}
