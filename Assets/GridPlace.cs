using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class GridPlace : MonoBehaviour
{
    private ARRaycastManager rayManager;
    private PlacementIndicator placementIndicator;

    private bool isCross = true;
    private GameLogic gameLogic = GameLogic.GetInstance();

    public GameObject ObjectToPlace;
    public GameObject crossToPlace;
    public GameObject zeroToPlace;

    public bool playButtonClicked = false;
    public bool done = false;

    // Start is called before the first frame update
    void Start()
    {
        rayManager = FindObjectOfType<ARRaycastManager>();
        placementIndicator = FindObjectOfType<PlacementIndicator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameLogic.IsGameOver ())
            return;

        if (!done)
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

					done = true;
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
                GameObject obj = Instantiate(crossToPlace, hits[0].pose.position, hits[0].pose.rotation);
                obj.transform.Translate (new Vector3 (0, -0.3f, -0.0f));
                obj.transform.Rotate(-90, 0, 0);
                gameLogic.PlaceZeroOrCross (obj, true);
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
                GameObject obj = Instantiate(zeroToPlace, hits[0].pose.position, hits[0].pose.rotation);
                obj.transform.Translate (new Vector3 (0, -0.25f, -0.0f));
                obj.transform.Rotate(-90, 0, 0);
                gameLogic.PlaceZeroOrCross (obj, false);
            }

            return true;
        }

        return false;
    }
	
    public void PlayButtonClicked()
    {
        playButtonClicked = true;
    }
}
