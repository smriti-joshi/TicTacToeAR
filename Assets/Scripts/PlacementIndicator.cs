using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


public class PlacementIndicator : MonoBehaviour
{
    private ARRaycastManager rayManager;
    private GameObject visual;
    private bool active = false;
    private bool placementIndicatorPlaced = false;

    private void Start ()
    {
        rayManager = FindObjectOfType<ARRaycastManager> ();
        visual = transform.GetChild (0).gameObject;

        visual.SetActive (false);
    }

    private void Update ()
    {
        if (visual.activeInHierarchy && !active)
        {
            visual.SetActive (false);
            transform.Rotate (90, 0, 0);
        }

        if (!active)
            return;

        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        rayManager.Raycast (new Vector2 (Screen.width / 2, Screen.height / 2), hits, TrackableType.Planes);

        if (hits.Count > 0)
        {
            transform.position = hits[0].pose.position;
            transform.rotation = hits[0].pose.rotation;

            if (!visual.activeInHierarchy && active)
            {
                visual.SetActive (true);
                placementIndicatorPlaced = true;
            }
        }
    }

    public void Enable (bool state)
    {
        active = state;
    }

    public bool IsPlacementIndicatorPlaced ()
    {
        return placementIndicatorPlaced;
    }
}
