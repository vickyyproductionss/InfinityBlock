using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuHandler : MonoBehaviour
{
    public string SceneNameToLoad;
    public void StartGame()
    {
        SceneManager.LoadScene(SceneNameToLoad);
    }
}
