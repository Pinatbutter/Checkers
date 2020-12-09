using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Board : MonoBehaviour
{

    public Pieces[,] piece = new Pieces[8, 8];
    public Vector3 offSet = new Vector3(-4.0f, 0, -4.0f);
    public Vector3 otherOffSet = new Vector3(0.5f, 0, 0.5f);

    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;

    public bool isWhite;
    private bool isWhiteTurn;

    private Pieces selectedPiece;
    private List<Pieces> forcedPieces;
    private bool hasKilled;
    private Vector2 mouseOver;
    private Vector2 startDrag;
    private Vector2 endDrag;
    public GameObject MinMax;
    private AI AIScript;


    private void Start()
    {
        AIScript = MinMax.GetComponent<AI>();
        isWhiteTurn = true;
        forcedPieces = new List<Pieces>();
        GeneratePieces();
    }

    private void Update()
    {
        UpdateMouseOver();

        if ((isWhite) ? isWhiteTurn : !isWhiteTurn)
        {

            int x = (int)mouseOver.x;
            int y = (int)mouseOver.y;

            if (selectedPiece != null)
                UpdatePieceDrag(selectedPiece);

            if (Input.GetMouseButtonDown(0))
                SelectPiece(x, y);

            if (Input.GetMouseButtonUp(0))
                TryMove((int)startDrag.x, (int)startDrag.y, x, y);
        }
    }
    private void UpdateMouseOver()
    {
        if (!Camera.main)
        {
            UnityEngine.Debug.Log("Unable to find main camera");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            mouseOver.x = (int)(hit.point.x - offSet.x);
            mouseOver.y = (int)(hit.point.z - offSet.z);

        }
        else
        {
            mouseOver.x = -1;
            mouseOver.y = -1;
        }
    }
    private void SelectPiece(int x, int y)
    {
        if (x < 0 || x >= 8 || y < 0 || y >= 8)
            return;

        Pieces p = piece[x, y];
        if (p != null && p.isWhite == isWhite && isWhite == true)
        {
            if (forcedPieces.Count == 0)
            {
                selectedPiece = p;
                startDrag = mouseOver;
            }
            else
            {
                if (forcedPieces.Find(fp => fp == p) == null)
                    return;
                selectedPiece = p;
                startDrag = mouseOver;

            }
        }
    }

    private void UpdatePieceDrag(Pieces p)
    {
        if (!Camera.main)
        {
            UnityEngine.Debug.Log("Unable to find main camera");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            p.transform.position = hit.point + Vector3.up;
        }


    }

    private void TryMove(int x1, int y1, int x2, int y2)
    {
        startDrag = new Vector2(x1, y1);
        endDrag = new Vector2(x2, y2);
        selectedPiece = piece[x1, y1];
        forcedPieces = ScanForPossibleMove();

        if (x2 < 0 || x2 >= 8 || y2 < 0 || y2 >= 8)
        {
            if (selectedPiece != null)
            {
                PlacePiece(selectedPiece, x1, y1);
            }
            selectedPiece = null;
            startDrag = Vector2.zero;
            return;
        }

        if (selectedPiece != null)
        {
            if (endDrag == startDrag)
            {
                PlacePiece(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                return;
            }

            if (selectedPiece.ValidMove(piece, x1, y1, x2, y2))
            {
                if (Mathf.Abs(x2 - x1) == 2)
                {
                    Pieces p = piece[(x1 + x2) / 2, (y1 + y2) / 2];
                    if (p != null)
                    {
                        piece[(x1 + x2) / 2, (y1 + y2) / 2] = null;
                        Destroy(p.gameObject);
                        hasKilled = true;
                    }
                }

                if (forcedPieces.Count != 0 && !hasKilled)
                {
                    PlacePiece(selectedPiece, x1, y1);
                    startDrag = Vector2.zero;
                    selectedPiece = null;
                    return;
                }

                piece[x2, y2] = selectedPiece;
                piece[x1, y1] = null;
                PlacePiece(selectedPiece, x2, y2);

                EndTurn();
            }
            else
            {
                PlacePiece(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                return;
            }
        }


    }

    private void EndTurn()
    {
        int x = (int)endDrag.x;
        int y = (int)endDrag.y;

        //Promotions
        if (selectedPiece != null)
        {
            if (selectedPiece.isWhite && !selectedPiece.isKing && y == 7)
            {
                selectedPiece.isKing = true;
                selectedPiece.transform.Rotate(Vector3.right * 180);
            }
            else if (!selectedPiece.isWhite && !selectedPiece.isKing && y == 0)
            {
                selectedPiece.isKing = true;
                selectedPiece.transform.Rotate(Vector3.right * 180);
            }
        }


        selectedPiece = null;
        startDrag = Vector2.zero;

        if (ScanForPossibleMove(selectedPiece, x, y).Count != 0 && hasKilled)
            return;

        isWhiteTurn = !isWhiteTurn;
        isWhite = !isWhite;
        hasKilled = false;
        CheckVictory();
        //TransformBoard();

        // AI.CheckersBoard tempBoard = TransformBoard();
        if (!isWhiteTurn)
        {
            List<(int, int)> Solution = AIScript.MiniMax('b', TransformBoard());

            moveBlack(Solution);
        }
    }


    // Transforms the Checkersboard from a notation that makes it
    // easy to interact with on unity to an 8x8 matrix
    private AI.CheckersBoard TransformBoard()
    {
        AI.CheckersBoard myBoard = new AI.CheckersBoard();

        List<char[]> tempList = new List<char[]>();

        for (int i = 7; i >= 0; i--)
        {
            char[] temp = { '_', '_', '_', '_', '_', '_', '_', '_' };

            for (int j = 0; j < 8; j++)
            {

                char thePiece = '_';

                if (piece[j, i] != null)
                {
                    Pieces curPiece = piece[j, i];

                    if (curPiece.isWhite && curPiece.isKing)
                    {
                        thePiece = 'W';
                    }
                    else if (curPiece.isWhite)
                    {
                        thePiece = 'w';
                    }
                    else if (curPiece.isKing)
                    {
                        thePiece = 'B';
                    }
                    else
                    {
                        thePiece = 'b';
                    }
                }

                temp[j] = thePiece;
            }

            //foreach( char c in temp)
            //{
            //     UnityEngine.Debug.Log(c);
            //}


            tempList.Add(temp);
        }

        myBoard.board = tempList;

        return myBoard;
    }

    private void moveBlack(List<(int, int)> Solution)
    {

        UnityEngine.Debug.Log("Moving Black");
        int moveToX, moveToY, px, py;

        px = Solution[0].Item1;
        py = Solution[0].Item2;

        py = Math.Abs(7 - py);

        moveToX = Solution[1].Item1;
        moveToY = Solution[1].Item2;
        moveToY = Math.Abs(7 - moveToY);
/*
         foreach ((int xPos, int yPos) in Solution)
        {
            UnityEngine.Debug.Log(xPos + " " + (7- yPos));
        }
*/
        TryMove(px, py, moveToX, moveToY);

    }


    private void CheckVictory()
    {
        var ps = FindObjectsOfType<Pieces>();
        bool hasWhite = false, hasBlack = false;
        for (int i = 0; i < ps.Length; i++)
        {
            if (ps[i].isWhite)
                hasWhite = true;
            else
                hasBlack = true;
        }
        if (!hasWhite)
            Victory(false);
        if (!hasBlack)
            Victory(true);
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

    private List<Pieces> ScanForPossibleMove(Pieces p, int x, int y)
    {
        forcedPieces = new List<Pieces>();

        if (piece[x, y].IsForceToMove(piece, x, y))
            forcedPieces.Add(piece[x, y]);

        return forcedPieces;

    }
    private List<Pieces> ScanForPossibleMove()
    {
        forcedPieces = new List<Pieces>();

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (piece[i, j] != null && piece[i, j].isWhite == isWhiteTurn)
                    if (piece[i, j].IsForceToMove(piece, i, j))
                        forcedPieces.Add(piece[i, j]);
            }
        }
        return forcedPieces;
    }

    private void GeneratePieces()
    {
        //white pieces
        for (int y = 0; y < 3; y++)
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
