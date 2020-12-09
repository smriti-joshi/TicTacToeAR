using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlacementIndicator : MonoBehaviour
{
    private ARRaycastManager rayManager;
    private GameObject visual;
    private bool enabled = false;

    private void Start()
    {
        rayManager = FindObjectOfType<ARRaycastManager>();
        visual = transform.GetChild(0).gameObject;

        visual.SetActive(false);
    }

    private void Update()
    {
        if (visual.activeInHierarchy && !enabled)
        {
            visual.SetActive(false);
            transform.Rotate(90, 0, 0);
        }

        if (!enabled)
            return;

        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        rayManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits, TrackableType.Planes);

        if(hits.Count>0)
        {
            transform.position = hits[0].pose.position;
            transform.rotation = hits[0].pose.rotation;

            if (!visual.activeInHierarchy && enabled)
                visual.SetActive(true);
        }
    }

    public void Enable(bool state)
    {
        enabled = state;
    }

}
