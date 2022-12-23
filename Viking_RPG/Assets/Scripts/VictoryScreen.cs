using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryScreen : MonoBehaviour
{
    public string sceneToLoad;

    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.instance != null)
        {
            Destroy(GameManager.instance.gameObject);
            GameManager.instance = null;
        }

        if (PlayerController.instance != null)
        {
            Destroy(PlayerController.instance.gameObject);
            PlayerController.instance = null;
        }

        SaveManager.instance.activeSave.hasBegun = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
