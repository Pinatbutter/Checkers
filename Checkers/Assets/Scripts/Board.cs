using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public Pieces[,] piece = new Pieces[8, 8];
    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;
    private Vector3 offSet = new Vector3(-4.0f, 0, -4.0f);
    private Vector3 otherOffSet = new Vector3(0.5f, 0, 0.5f);


    private void Start()
    {
        GeneratePieces();
    }

    private void GeneratePieces()
    {
        //white pieces
        for (int y = 0; y < 3; y++)
        {
            bool oddRow = (y % 2 == 0);
            for (int x =0; x<8; x += 2)
            {
                if (oddRow)
                    CreatePiece(x, y);
                else
                    CreatePiece(x + 1, y);
            }
        }

        //black pieces
        for (int y = 7; y > 4; y--)
        {
            bool oddRow = (y % 2 == 0);
            for (int x = 0; x < 8; x += 2)
            {
                if (oddRow)
                    CreatePiece(x, y);
                else
                    CreatePiece(x + 1, y);
            }
        }


    }
    private void CreatePiece(int x, int y)
    {
        GameObject go;
        if (y > 3)
            go = Instantiate(blackPiecePrefab) as GameObject;
        else
            go = Instantiate(whitePiecePrefab) as GameObject;
        go.transform.SetParent(transform);
        Pieces p = go.GetComponent<Pieces>();
        piece[x, y] = p;
        PlacePiece(p, x, y);
    }

    private void PlacePiece(Pieces p, int x, int y)
    {
        p.transform.position = (Vector3.right * x) + (Vector3.forward * y) + offSet + otherOffSet;
    }


}
