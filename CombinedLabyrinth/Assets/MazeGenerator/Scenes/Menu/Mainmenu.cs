using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MazeGenerator.Scenes.Menu
{
    public class Mainmenu : MonoBehaviour
    {
        [SerializeField] private GameObject menu;
        [SerializeField] private GameObject loading;
        [SerializeField] private GameObject texting;
        private void Awake()
        {
            if (WaitAndLoadScript.SceneIndex > 1)
            {
                Debug.Log("test");
                menu.SetActive(false);
                loading.SetActive(true);
                texting.SetActive(false);
            }
        }
        
        //
        //     CoinTracker.setCointCount(0);
        //     SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        //
        // }
        //
        // public void QuitGame () {
        //
        //     Application.Quit();
        //     Debug.Log("Quit");
        //
        // }
    }
}
