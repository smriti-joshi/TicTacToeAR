using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChange : MonoBehaviour
{
    // Start is called before the first frame update
    public Animator transition;
    public SceneChange panel;
    public float transitionTime = 5f;

    //maybe make a function to disable button, argument in function
    public void SampleScene()
    {
        StartCoroutine(LoadSampleScene("Assets/Scenes/SampleScene"));
    }

    IEnumerator LoadSampleScene(string scenePath)
    {

        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(scenePath);
    }

    /*public void DeleteSampleScene()
    {
        SceneManager.UnloadSceneAsync("Assets/Scenes/SampleScene");
    }*/
}
