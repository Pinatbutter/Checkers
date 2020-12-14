using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Winner : MonoBehaviour
{
    public string gameOver;

    public void setWinner(string winner)
    {
        gameOver = winner;
        DontDestroyOnLoad(transform.gameObject);

        SceneManager.LoadScene("End");
    }
}
