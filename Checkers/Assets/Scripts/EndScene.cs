using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class EndScene : MonoBehaviour
{
    public Text gameOverText;
    public Winner myScript;
    // Start is called before the first frame update
    void Start()
    {

        myScript = GameObject.Find("DontDestroy").GetComponent<Winner>();

        gameOverText.text = myScript.gameOver + " Won!";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
