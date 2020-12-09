using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChange : MonoBehaviour
{
    // Start is called before the first frame update

    public SceneChange panel;

  //maybe make a function to disable button, argument in function
    public void SampleScene()
    {
        SceneManager.LoadScene("Assets/Scenes/SampleScene");
        //panel.gameObject.SetActive(false);
        //Canvas.gameObject.SetActive(false);


        //Application.LoadLevel("Scenes/SampleScene");
    }
}
