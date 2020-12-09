using System.Collections;
using System.Collections.Generic;
using System.Windows;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Diagnostics;
using System.Linq;

public class AI : MonoBehaviour
{

    // Piece class
    public struct Piece
    {
        public int r;
        public int c;
        public bool color;
    }

    // Move class
    public struct Move
    {
        public int startR;
        public int startC;
        public int endR;
        public int endC;
    }

    // CheckersBoard class
    public struct CheckersBoard
    {
        public List<char[]> board;
        public bool turn;
    }

    // The Score of the move
    public struct MoveScore
    {
        public Move m;
        public int score;
    }

    // Generate Checkers moves
    private List<Move> generateMoves(CheckersBoard b)
    {
        List<Move> allMoves = new List<Move>();

        bool turn = b.turn;
        char player = 'b';
        char rival = 'w';
        int rdir = 1;

        if (turn)
        {
            rival = 'b';
            player = 'w';
            rdir = -1;
        }

        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                if (b.board[r][c] == player || b.board[r][c] == char.ToUpper(player))
                {
                    int endR = r + rdir;
                    if (endR >= 0 && endR < 8)
                    {   // move forwards
                        if (c + 1 < 8)
                        {
                            char dest = b.board[endR][c + 1];
                            if (dest == '_')
                            {
                                Move m;
                                m.startR = r; m.startC = c; m.endR = endR; m.endC = c + 1;
                                allMoves.Add(m);
                            }
                            else if (char.ToLower(dest) == rival)
                            {   // Capture
                                int rcapt = endR + rdir;
                                if (rcapt >= 0 && rcapt < 8)
                                {
                                    if (c + 2 < 8)
                                    {
                                        if (b.board[rcapt][c + 2] == '_')
                                        {
                                            Move m;
                                            m.startR = r; m.startC = c; m.endR = rcapt; m.endC = c + 2;
                                            allMoves.Add(m);
                                        }
                                    }
                                }
                            }
                        }
                        if (c - 1 >= 0)
                        {
                            char dest = b.board[endR][c - 1];
                            if (dest == '_')
                            {
                                Move m;
                                m.startR = r; m.startC = c; m.endR = endR; m.endC = c - 1;
                                allMoves.Add(m);
                            }
                            else if (char.ToLower(dest) == rival) //capture
                            {
                                int rcapt = endR + rdir;
                                if (rcapt >= 0 && rcapt < 8)
                                {
                                    if (c - 2 >= 0)
                                    {
                                        if (b.board[rcapt][c - 2] == '_')
                                        {
                                            Move m;
                                            m.startR = r; m.startC = c; m.endR = rcapt; m.endC = c - 2;
                                            allMoves.Add(m);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    int rendR = r - rdir;
                    if (b.board[r][c] == char.ToUpper(player))
                    {
                        if (rendR >= 0 && rendR < 8) //move backwards if king
                        {
                            if (c + 1 < 8)
                            {
                                char dest = b.board[rendR][c + 1];
                                if (dest == '_')
                                {
                                    Move m;
                                    m.startR = r; m.startC = c; m.endR = rendR; m.endC = c + 1;
                                    allMoves.Add(m);
                                }
                                else if (char.ToLower(dest) == rival) //capture
                                {
                                    int rcapt = rendR - rdir;
                                    if (rcapt >= 0 && rcapt < 8)
                                    {
                                        if (c + 2 < 8)
                                        {
                                            if (b.board[rcapt][c + 2] == '_')
                                            {
                                                Move m;
                                                m.startR = r; m.startC = c; m.endR = rcapt; m.endC = c + 2;
                                                allMoves.Add(m);
                                            }
                                        }
                                    }
                                }
                            }
                            if (c - 1 >= 0)
                            {
                                char dest = b.board[rendR][c - 1];
                                if (dest == '_')
                                {
                                    Move m;
                                    m.startR = r; m.startC = c; m.endR = rendR; m.endC = c - 1;
                                    allMoves.Add(m);
                                }
                                else if (char.ToLower(dest) == rival) //capture
                                {
                                    int rcapt = rendR - rdir;
                                    if (rcapt >= 0 && rcapt < 8)
                                    {
                                        if (c - 2 >= 0)
                                        {
                                            if (b.board[rcapt][c - 2] == '_')
                                            {
                                                Move m;
                                                m.startR = r; m.startC = c; m.endR = rcapt; m.endC = c - 2;
                                                allMoves.Add(m);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        List<Move> captures = new List<Move>();
        foreach (var m in allMoves)
            if (Math.Abs(m.endR - m.startR) == 2)
                captures.Add(m);

        if (captures.Count > 0)
            return captures;
        return allMoves;
    }

    CheckersBoard makeMove(CheckersBoard b, Move m)
    {
        bool capture = Math.Abs(m.endR - m.startR) == 2;
        b.board[m.endR][m.endC] = b.board[m.startR][m.startC];
        if (!b.turn)
        {
            if (m.endR == 7)
                b.board[m.endR][m.endC] = 'B';
        }
        else
        {
            if (m.endR == 0)
                b.board[m.endR][m.endC] = 'W';
        }
        b.board[m.startR][m.startC] = '_';

        if (capture)
        {
            b.board[(m.endR + m.startR) / 2][(m.endC + m.startC) / 2] = '_';
        }
        return b;
    }

    int staticEval(CheckersBoard b)
    {
        int score = 0;
        char player = 'b';
        char rival = 'w';
        int rdir = -1;
        int centerWeight = 0;
        int playerCrownWeight = 500;
        int os = 300;
        int rivalCrownWeight = 500;
        int ts = 300;
        if (b.turn)
        {
            player = 'w';
            rdir = 1;
            rival = 'b';
            centerWeight = 7;
        }

        int playerCount = 0;
        int rivalCount = 0;
        for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
            {
                if (char.ToLower(b.board[r][c]) == player)
                    playerCount++;
                else if (char.ToLower(b.board[r][c]) == rival)
                    rivalCount++;
            }

        int setting = 0;
        if (playerCount > rivalCount && rivalCount < 3)
            setting = 1;
        else if (rivalCount > playerCount && playerCount < 3)
            setting = 2;

        for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
            {
                bool front2 = ((r - rdir * 2) >= 0) && ((r - rdir * 2) < 8);
                bool behind2 = ((r + rdir * 2) >= 0) && ((r + rdir * 2) < 8);
                bool left2 = (c - 2 >= 0);
                bool right2 = (c + 2 < 8);

                if (b.board[r][c] == char.ToUpper(player))
                {
                    score += playerCrownWeight;
                }
                else if (b.board[r][c] == char.ToUpper(rival))
                {
                    score -= rivalCrownWeight;
                    if (setting == 1)
                    {
                        if (front2)
                        {
                            if (left2 && char.ToLower(b.board[r - rdir * 2][c - 2]) == player)
                                score += 40;
                            if (right2 && char.ToLower(b.board[r - rdir * 2][c + 2]) == player)
                                score += 40;
                            if (char.ToLower(b.board[r - rdir * 2][c]) == player)
                                score += 40;
                        }
                        if (behind2)
                        {
                            if (left2 && b.board[r + rdir * 2][c - 2] == char.ToUpper(player))
                                score += 30;
                            if (right2 && b.board[r + rdir * 2][c + 2] == char.ToUpper(player))
                                score += 30;
                            if (b.board[r + rdir * 2][c] == char.ToUpper(player))
                                score += 30;
                        }
                        if (left2 && b.board[r][c - 2] == char.ToUpper(player))
                            score += 30;
                        if (right2 && b.board[r][c + 2] == char.ToUpper(player))
                            score += 30;
                    }
                }
                else if (b.board[r][c] == player)
                {
                    if (!b.turn && c == 0)
                        score++;
                    score += os - (centerWeight - r) * (centerWeight - r);
                }
                else if (b.board[r][c] == rival)
                {
                    score -= ts + (7 - centerWeight - r) * (7 - centerWeight - r);
                    if (setting == 1)
                    {
                        if (front2)
                        {
                            if (left2 && char.ToLower(b.board[r - rdir * 2][c - 2]) == player)
                                score += 40;
                            if (right2 && char.ToLower(b.board[r - rdir * 2][c + 2]) == player)
                                score += 40;
                            if (char.ToLower(b.board[r - rdir * 2][c]) == player)
                                score += 40;
                        }
                        if (behind2)
                        {
                            if (left2 && b.board[r + rdir * 2][c - 2] == char.ToUpper(player))
                                score += 30;
                            if (right2 && b.board[r + rdir * 2][c + 2] == char.ToUpper(player))
                                score += 30;
                            if (b.board[r + rdir * 2][c] == char.ToUpper(player))
                                score += 30;
                        }
                        if (left2 && char.ToLower(b.board[r][c - 2]) == player)
                            score += 30;
                        if (right2 && char.ToLower(b.board[r][c + 2]) == player)
                            score += 30;
                    }
                }
                else //'_'
                {
                    if (r - 1 >= 0)
                    {
                        if (c - 1 >= 0)
                        {
                            char cc = b.board[r - 1][c - 1];

                            if (cc == 'b' || cc == 'B' || cc == 'W')
                            {
                                if (player == char.ToLower(cc))
                                {
                                    score += 20;
                                }

                                if (rival == char.ToLower(cc))
                                {
                                    score -= 20;
                                }

                            }
                        }
                        if (c + 1 < 8)
                        {

                            char cc = b.board[r - 1][c + 1];

                            if (cc == 'b' || cc == 'B' || cc == 'W')
                            {
                                if (player == char.ToLower(cc))
                                {
                                    score += 20;
                                }

                                if (rival == char.ToLower(cc))
                                {
                                    score -= 20;
                                }
                            }

                        }
                    }
                    if (r + 1 < 8)
                    {
                        if (c - 1 >= 0)
                        {
                            char cc = b.board[r + 1][c - 1];

                            if (cc == 'w' || cc == 'W' || cc == 'B')
                            {
                                if (player == char.ToLower(cc))
                                {
                                    score += 20;
                                }

                                if (rival == char.ToLower(cc))
                                {
                                    score -= 20;
                                }
                            }

                        }
                        if (c + 1 < 8)
                        {
                            char cc = b.board[r + 1][c + 1];

                            if (cc == 'w' || cc == 'W' || cc == 'B')
                            {
                                if (player == char.ToLower(cc))
                                {
                                    score += 20;
                                }

                                if (rival == char.ToLower(cc))
                                {
                                    score -= 20;
                                }

                            }
                        }
                    }
                }

            }
        return score;
    }

    MoveScore alphabetaMax(CheckersBoard b, MoveScore alpha, MoveScore beta, int depth)
    {
        if (depth == 0)
        {
            List<Move> moveList = generateMoves(b);
            if (moveList.Count > 0 && (moveList[0].endR - moveList[0].startR) == 2)
            {
                CheckersBoard temp = makeMove(b, moveList[0]);
                int last = moveList[0].endR * 10 + moveList[0].endC;
                bool capture = true;
                while (capture)
                {
                    List<Move> moremvs = generateMoves(temp);
                    capture = false;
                    if (moremvs.Count == 0 || Math.Abs(moremvs[0].endR - moremvs[0].startR) != 2)
                        break;
                    foreach (Move mm in moremvs)
                    {
                        if (mm.startR * 10 + mm.startC == last)
                        {
                            temp = makeMove(temp, mm);
                            last = mm.endR * 10 + mm.endC;
                            capture = true;
                            break;
                        }
                    }
                }
                temp.turn = !temp.turn;
                return alphabetaMin(temp, alpha, beta, 0);
            }

            MoveScore ms;
            ms.m = alpha.m;
            ms.score = staticEval(b);
            return ms;
        }

        int score;
        List<Move> mvs = generateMoves(b);
        foreach (Move m in mvs)
        {
            CheckersBoard temp = b;
            temp = makeMove(temp, m);
            int last = m.endR * 10 + m.endC;
            bool capture = Math.Abs(m.endR - m.startR) == 2;
            while (capture)
            {
                List<Move> moremvs = generateMoves(temp);
                capture = false;
                if (moremvs.Count == 0 || Math.Abs(moremvs[0].endR - moremvs[0].startR) != 2)
                    break;
                foreach (Move mm in moremvs)
                {
                    if (mm.startR * 10 + mm.startC == last)
                    {
                        temp = makeMove(temp, mm);
                        last = mm.endR * 10 + mm.endC;
                        // Debug.Log("Max Double hop at " + (mm.endR*10 + mm.endC));
                        capture = true;
                        break;
                    }
                }
            }

            temp.turn = !temp.turn;
            MoveScore ms = alphabetaMin(temp, alpha, beta, depth - 1);
            ms.m = m;
            score = ms.score;

            if (score >= beta.score)
                return beta; //fail hard beta-cutoff
            if (score > alpha.score)
                alpha = ms;
        }
        return alpha;
    }

    MoveScore alphabetaMin(CheckersBoard b, MoveScore alpha, MoveScore beta, int depth)
    {
        // UnityEngine.Debug.Log("Depth is");

        if (depth == 0)
        {
            List<Move> mvs = generateMoves(b);
            if (mvs.Count > 0 && (mvs[0].endR - mvs[0].startR) == 2)
            {
                CheckersBoard temp = makeMove(b, mvs[0]);
                int last = mvs[0].endR * 10 + mvs[0].endC;
                bool capture = true;
                while (capture)
                {
                    List<Move> moremvs = generateMoves(temp);
                    capture = false;
                    if (moremvs.Count == 0 || Math.Abs(moremvs[0].endR - moremvs[0].startR) != 2)
                        break;
                    foreach (Move mm in moremvs)
                    {
                        if (mm.startR * 10 + mm.startC == last)
                        {
                            temp = makeMove(temp, mm);
                            last = mm.endR * 10 + mm.endC;
                            capture = true;
                            break;
                        }
                    }
                }
                temp.turn = !temp.turn;
                return alphabetaMax(temp, alpha, beta, 0);
            }

            MoveScore ms;
            ms.m = alpha.m;
            ms.score = -staticEval(b);
            return ms;
        }

        int score;
        foreach (Move m in generateMoves(b))
        {
            CheckersBoard temp = b;
            temp = makeMove(temp, m);
            int last = m.endR * 10 + m.endC;
            bool capture = Math.Abs(m.endR - m.startR) == 2;
            while (capture)
            {
                List<Move> moremvs = generateMoves(temp);
                capture = false;
                if (moremvs.Count == 0 || Math.Abs(moremvs[0].endR - moremvs[0].startR) != 2)
                    break;
                foreach (Move mm in moremvs)
                {
                    if (mm.startR * 10 + mm.startC == last)
                    {
                        temp = makeMove(temp, mm);
                        last = mm.endR * 10 + mm.endC;
                        //Debug.Log("Min Double hop at " ++ (mm.endR*10 + mm.endC));
                        capture = true;
                        break;
                    }
                }
            }

            temp.turn = !temp.turn;
            MoveScore ms = alphabetaMax(temp, alpha, beta, depth - 1);
            ms.m = m;
            score = ms.score;
            if (score <= alpha.score)
            {
                return alpha; //fail hard alpha-cutoff
            }
            if (score < beta.score)
                beta = ms;
        }
        return beta;
    }

    Move evaluate(CheckersBoard b, int depth)
    {
        //char[] black1 = { '_', 'b', '_', 'b', '_', 'b', '_', 'b'};

        if (!b.turn && b.board[0].SequenceEqual("_b_b_b_b") && b.board[1].SequenceEqual("b_b_b_b_") && b.board[2].SequenceEqual("_b_b_b_b")
            && b.board[5].SequenceEqual("w_w_w_w_") && b.board[6].SequenceEqual("_w_w_w_w") && b.board[7].SequenceEqual("w_w_w_w_"))
        {
            Move bopen;
            bopen.startR = 2; bopen.startC = 1; bopen.endR = 3; bopen.endC = 0;
            return bopen;
        }

        Move best;
        List<Move> moveList = generateMoves(b);
        best = moveList[0];

        int highScore = staticEval(b);

        MoveScore neg;
        neg.m = best;
        neg.score = -99999999;
        MoveScore pos;
        pos.m = best;
        pos.score = 99999999;

        MoveScore ms = alphabetaMax(b, neg, pos, depth);
        highScore = ms.score;
        best = ms.m;
        //UnityEngine.Debug.Log("High: " + highScore);
        return best;
    }

    public List<(int, int)> MiniMax(char turn, CheckersBoard gameBoard)
    {

        // CheckersBoard gameBoard;
        // char turn;

        gameBoard.turn = (turn == 'w');

        List<Move> legalMoves = generateMoves(gameBoard);

        List<(int, int)> Solution = new List<(int, int)>();

        int nmoves = 1;
        int depth = 75;

        Move f = legalMoves[0];
        f = evaluate(gameBoard, depth);

        // Add the selected move current position
        Solution.Add((f.startC, f.startR));
        // Add the selected move next position
        Solution.Add((f.endC, f.endR));

        int last = f.endR * 10 + f.endC;

        bool capture = Math.Abs(f.endR - f.startR) == 2;
        while (capture)
        {
            UnityEngine.Debug.Log("Did a capture");
            capture = false;
            gameBoard = makeMove(gameBoard, f);
            legalMoves = generateMoves(gameBoard);
            foreach (Move m in legalMoves)
            {
                UnityEngine.Debug.Log("A possible move");
                if (Math.Abs(m.endR - m.startR) == 2 && (m.startR * 10 + m.startC) == last)
                {
                    f = m;
                    // adds the next jump positions
                    Solution.Add((f.endC, f.endR));
                    UnityEngine.Debug.Log("could have more moves 3");

                    last = f.endR * 10 + f.endC;
                    nmoves++;
                    capture = true;
                    break;
                }
            }
        }

        //UnityEngine.Debug.Log(nmoves);

        foreach ((int xPos, int yPos) in Solution)
        {
            UnityEngine.Debug.Log(xPos + " " + yPos);
        }

        return Solution;


    }

}
