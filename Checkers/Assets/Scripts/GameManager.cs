using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckVictory();
    }
    private void CheckVictory()
    {
        var ps = FindObjectsOfType<Pieces>();
        bool hasWhite = false;
        bool hasBlack = false;
        for (int i = 0; i < ps.Length; i++)
        {
            if (ps[i].isWhite)
                hasWhite = true;
            else
                hasBlack = true;
        }
        if (!hasWhite)
        {
            UnityEngine.Debug.Log(ps.Length);
            Victory(false);
        }
        if (!hasBlack)
        {
            UnityEngine.Debug.Log("Black Won");
            Victory(true);

        }
        
    }


    private void Victory(bool isWhite)
    {
        if (isWhite)
        {
            UnityEngine.Debug.Log("Black Won");
        }
        else
            UnityEngine.Debug.Log("Wite Won");
    }
}
