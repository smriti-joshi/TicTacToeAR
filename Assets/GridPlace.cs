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
            if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
            {
                Transform trans = placementIndicator.transform;
                trans.Rotate(-90, 0, 0);
                GameObject obj = Instantiate(ObjectToPlace, trans.position, trans.rotation);
                
                //Vector3 objectSize = Vector3.Scale(transform.localScale, obj.bounds.size);

                done = true;
                placementIndicator.Enable(false);
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
                obj.transform.Rotate(-90, 0, 0);
            }

            gameLogic.PlaceZeroOrCross (hits[0].pose.position, true);
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
                obj.transform.Rotate(-90, 0, 0);
            }

            gameLogic.PlaceZeroOrCross (hits[0].pose.position, false);
            return true;
        }

        return false;
    }
}