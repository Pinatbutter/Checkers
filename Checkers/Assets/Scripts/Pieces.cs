using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class Pieces : MonoBehaviour
{
    public bool isWhite;
    public bool isKing;

    public bool IsForceToMove(Pieces[,] board, int x, int y)
    {
        if (isWhite || isKing)
        {
            //top left
            if (x >= 2 && y <= 5)
            {
                Pieces p = board[x - 1, y + 1];

                if (p != null && p.isWhite != isWhite)
                {
                    if (board[x - 2, y + 2] == null)
                    {
                        return true;
                    }
                }
            }

            //top right
            if (x <= 5 && y <= 5)
            {
                Pieces p = board[x + 1, y + 1];

                if (p != null && p.isWhite != isWhite)
                {
                    if (board[x + 2, y + 2] == null)
                    {
                        return true;
                    }
                }
            }
        }
        if(!isWhite || isKing)
        {

            //bot left
            if (x >= 2 && y >= 2)
            {
                Pieces p = board[x - 1, y - 1];

                if (p != null && p.isWhite != isWhite)
                {
                    if (board[x - 2, y - 2] == null)
                    {
                        return true;
                    }
                }
            }

            //bot right
            if (x <= 5 && y >= 2)
            {
                Pieces p = board[x + 1, y - 1];

                if (p != null && p.isWhite != isWhite)
                {
                    if (board[x + 2, y - 2] == null)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public bool ValidMove(Pieces[,] board, int x1, int y1, int x2, int y2)
    {
        // if moving on top of another piece
        if (board[x2, y2] != null)
            return false;

        int deltaMove = Mathf.Abs(x1 - x2);
        int deltaMoveY = (y2 - y1);
        if (isWhite || isKing)
        {
            if (deltaMove == 1)
            {
                if (deltaMoveY == 1)
                    return true;
            }
            else if (deltaMove == 2)
            {
                if (deltaMoveY == 2)
                {
                    Pieces p = board[(x1 + x2) / 2, (y1 + y2) / 2];
                    if (p != null && p.isWhite != isWhite)
                        return true;
                }
            }
        }
        if (!isWhite || isKing)
        {
            if (deltaMove == 1)
            {
                if (deltaMoveY == -1)
                    return true;
            }
            else if (deltaMove == 2)
            {
                if (deltaMoveY == -2)
                {
                    Pieces p = board[(x1 + x2) / 2, (y1 + y2) / 2];
                    if (p != null && p.isWhite != isWhite)
                        return true;
                }
            }
        }
        return false;
    }

}

