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

        // our turn
        if (turn)
        {
            rival = 'b';
            player = 'w';
            rdir = -1;
        }

        // check every slot in the board
        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                // check if the piece there is ours
                if (b.board[r][c] == player || b.board[r][c] == char.ToUpper(player))
                {
                    //// add the movement direction of the row
                    int endR = r + rdir;

                    // check if we are still inside the board
                    if (endR >= 0 && endR < 8)
                    {
                        // move forwards if possible (still inside board)
                        if (c + 1 < 8)
                        {

                            // check the position where we could move
                            char dest = b.board[endR][c + 1];

                            // check if the position is empty
                            if (dest == '_')
                            {
                                // if its empty, move there
                                Move m;
                                m.startR = r;
                                m.startC = c;
                                m.endR = endR;
                                m.endC = c + 1;

                                // add the move to the move list
                                allMoves.Add(m);
                            }

                            // check if the opponent is in the position
                            else if (char.ToLower(dest) == rival)
                            {
                                // capture the opponent
                                int rcapt = endR + rdir;

                                // check if we do not jump outside the board
                                if (rcapt >= 0 && rcapt < 8)
                                {
                                    // check if after moving 2 steps forward we are still inside
                                    if (c + 2 < 8)
                                    {
                                        // check if the place to jump is empty
                                        if (b.board[rcapt][c + 2] == '_')
                                        {
                                            Move m;
                                            m.startR = r;
                                            m.startC = c;
                                            m.endR = rcapt;
                                            m.endC = c + 2;

                                            // add the jump to the move list
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

                    ///// king manouvers
                    int rendR = r - rdir;

                    // check if we are a queen
                    if (b.board[r][c] == char.ToUpper(player))
                    {
                        // check if we remain in the board after moving backwards
                        if (rendR >= 0 && rendR < 8)
                        {
                            // check if we are still inside the board
                            if (c + 1 < 8)
                            {
                                // check if the place is empty
                                char dest = b.board[rendR][c + 1];
                                if (dest == '_')
                                {
                                    Move m;
                                    m.startR = r;
                                    m.startC = c;
                                    m.endR = rendR;
                                    m.endC = c + 1;

                                    allMoves.Add(m);
                                }
                                // check if we can capture there
                                else if (char.ToLower(dest) == rival)
                                {
                                    int rcapt = rendR - rdir;
                                    if (rcapt >= 0 && rcapt < 8)
                                    {
                                        if (c + 2 < 8)
                                        {
                                            // check if the jump place is empty
                                            if (b.board[rcapt][c + 2] == '_')
                                            {
                                                Move m;
                                                m.startR = r;
                                                m.startC = c;
                                                m.endR = rcapt;
                                                m.endC = c + 2;

                                                allMoves.Add(m);
                                            }
                                        }
                                    }
                                }
                            }

                            // same as above, but to the opposite side
                            // now we check if we go down we are still on the board
                            if (c - 1 >= 0)
                            {
                                // check if place to move is empty
                                char dest = b.board[rendR][c - 1];
                                if (dest == '_')
                                {
                                    Move m;
                                    m.startR = r;
                                    m.startC = c;
                                    m.endR = rendR;
                                    m.endC = c - 1;

                                    allMoves.Add(m);
                                }

                                // check if rival is there to be captured
                                else if (char.ToLower(dest) == rival)
                                {
                                    int rcapt = rendR - rdir;
                                    if (rcapt >= 0 && rcapt < 8)
                                    {
                                        if (c - 2 >= 0)
                                        {
                                            if (b.board[rcapt][c - 2] == '_')
                                            {
                                                Move m;
                                                m.startR = r;
                                                m.startC = c;
                                                m.endR = rcapt;
                                                m.endC = c - 2;

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

        // we need to see which of these moves
        // were captures
        List<Move> captures = new List<Move>();
        foreach (var m in allMoves)
            if (Math.Abs(m.endR - m.startR) == 2)
                captures.Add(m);

        // If there are captures we return the captures
        // as captures are forced
        if (captures.Count > 0)
            return captures;

        // if there are no captures, we return the normal moves
        return allMoves;
    }

    CheckersBoard makeMove(CheckersBoard b, Move m)
    {
        // check if it is a capture aka a jump
        bool capture = Math.Abs(m.endR - m.startR) == 2;

        // move the piece
        b.board[m.endR][m.endC] = b.board[m.startR][m.startC];
        if (!b.turn)
        {
            // coronate if at last row
            if (m.endR == 7)
                b.board[m.endR][m.endC] = 'B';
        }
        else
        {
            // coronate if at first row
            if (m.endR == 0)
                b.board[m.endR][m.endC] = 'W';
        }

        // mark where we where empty
        b.board[m.startR][m.startC] = '_';


        // if we captured we leave the place we jumped over empty
        if (capture)
        {
            b.board[(m.endR + m.startR) / 2][(m.endC + m.startC) / 2] = '_';
        }

        // we return the checkerboard
        return b;
    }

    // Our evaluation Function
    int heuristicEval(CheckersBoard b)
    {
        // Main Weights for  the 'heuristic'
        int playerCrownWeight = 500;
        int pieceWeight = 300;
        int rivalCrownWeight = 500;
        int rivalWeight = 300;

        // Positional Bonuses for the heuristics
        int excellentWeight = 40;
        int veryGoodWeight = 30;
        int goodWeight = 20;

        // The score
        int totalScore = 0;

        // Player and rival information for blacks
        char player = 'b';
        char rival = 'w';
        int rdir = -1;
        int centerWeight = 0;

        // Player and rirval information for white
        if (b.turn)
        {
            player = 'w';
            rdir = 1;
            rival = 'b';
            centerWeight = 7;
        }

        // First we count all the pieces in the board
        int playerCount = 0;
        int rivalCount = 0;


        // Nested for-loops that go through every piece
        // They count how many pieces each player has
        for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
            {
                if (char.ToLower(b.board[r][c]) == player)
                    playerCount++;
                else if (char.ToLower(b.board[r][c]) == rival)
                    rivalCount++;
            }


        // Now, we found out that the AI  should play differently
        // depending on the 'stage' that the game is at
        // By default play 'normally'
        int setting = 0;

        // If it has more pieces than the rival and the rival has less than 3
        // Play aggressively
        if (playerCount > rivalCount && rivalCount < 3)
            setting = 1;

        // If the rival has more pieces than us, and we have less than 3
        // Better play safe
        else if (rivalCount > playerCount && playerCount < 3)
            setting = 2;

        // Again, we go through every cell in the board
        for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
            {
                // positioning bools
                // They tell if the current cell is at the position

                // atFront tells if its at the top 2 rows
                bool atFront = ((r - rdir * 2) >= 0) && ((r - rdir * 2) < 8);
                // atBack tells if its at the back 2 rows
                bool atBack = ((r + rdir * 2) >= 0) && ((r + rdir * 2) < 8);
                // atLeft tells if its at the leftmost 2 columns
                bool atLeft = (c - 2 >= 0);
                // atRight tells if its at the rightmost 2 columns
                bool atRight = (c + 2 < 8);


                // If the player has a queen 
                if (b.board[r][c] == char.ToUpper(player))
                {
                    totalScore += playerCrownWeight;
                }

                // If the rival has a queen
                else if (b.board[r][c] == char.ToUpper(rival))
                {
                    totalScore -= rivalCrownWeight;

                    // If we are playing aggresively and the rival has a queen
                    if (setting == 1)
                    {
                        if (atFront)
                        {
                            if (atLeft && char.ToLower(b.board[r - rdir * 2][c - 2]) == player)
                                totalScore += excellentWeight;
                            if (atRight && char.ToLower(b.board[r - rdir * 2][c + 2]) == player)
                                totalScore += excellentWeight;
                            if (char.ToLower(b.board[r - rdir * 2][c]) == player)
                                totalScore += excellentWeight;
                        }
                        if (atBack)
                        {
                            if (atLeft && b.board[r + rdir * 2][c - 2] == char.ToUpper(player))
                                totalScore += veryGoodWeight;
                            if (atRight && b.board[r + rdir * 2][c + 2] == char.ToUpper(player))
                                totalScore += veryGoodWeight;
                            if (b.board[r + rdir * 2][c] == char.ToUpper(player))
                                totalScore += veryGoodWeight;
                        }
                        if (atLeft && b.board[r][c - 2] == char.ToUpper(player))
                            totalScore += veryGoodWeight;
                        if (atRight && b.board[r][c + 2] == char.ToUpper(player))
                            totalScore += veryGoodWeight;
                    }
                }

                // If we have a normal piece there
                else if (b.board[r][c] == player)
                {
                    totalScore += pieceWeight - (centerWeight - r) * (centerWeight - r);
                }

                // If the rival has a piece there
                else if (b.board[r][c] == rival)
                {
                    totalScore -= rivalWeight + (7 - centerWeight - r) * (7 - centerWeight - r);

                    // If the rival has a piece there and we are aggresive
                    if (setting == 1)
                    {
                        if (atFront)
                        {
                            if (atLeft && char.ToLower(b.board[r - rdir * 2][c - 2]) == player)
                                totalScore += excellentWeight;
                            if (atRight && char.ToLower(b.board[r - rdir * 2][c + 2]) == player)
                                totalScore += excellentWeight;
                            if (char.ToLower(b.board[r - rdir * 2][c]) == player)
                                totalScore += excellentWeight;
                        }
                        if (atBack)
                        {
                            if (atLeft && b.board[r + rdir * 2][c - 2] == char.ToUpper(player))
                                totalScore += veryGoodWeight;
                            if (atRight && b.board[r + rdir * 2][c + 2] == char.ToUpper(player))
                                totalScore += veryGoodWeight;
                            if (b.board[r + rdir * 2][c] == char.ToUpper(player))
                                totalScore += veryGoodWeight;
                        }
                        if (atLeft && char.ToLower(b.board[r][c - 2]) == player)
                            totalScore += veryGoodWeight;
                        if (atRight && char.ToLower(b.board[r][c + 2]) == player)
                            totalScore += veryGoodWeight;
                    }
                }

                // if the piece is empty
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
                                    totalScore += goodWeight;
                                }

                                if (rival == char.ToLower(cc))
                                {
                                    totalScore -= goodWeight;
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
                                    totalScore += goodWeight;
                                }

                                if (rival == char.ToLower(cc))
                                {
                                    totalScore -= goodWeight;
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
                                    totalScore += goodWeight;
                                }

                                if (rival == char.ToLower(cc))
                                {
                                    totalScore -= goodWeight;
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
                                    totalScore += goodWeight;
                                }

                                if (rival == char.ToLower(cc))
                                {
                                    totalScore -= goodWeight;
                                }

                            }
                        }
                    }
                }

            }
        return totalScore;
    }


    // Alpha beta function
    MoveScore alphabetaMax(CheckersBoard b, MoveScore alpha, MoveScore beta, int depth)
    {
        // If our depth is at 0, time to call heuristics
        if (depth == 0)
        {

            // Generate the possible moves
            List<Move> moveList = generateMoves(b);

            // Check if we could move and if that move was a jump
            if (moveList.Count > 0 && (moveList[0].endR - moveList[0].startR) == 2)
            {
                // Note created new boards
                // make a new board with the first move
                CheckersBoard temp = makeMove(b, moveList[0]);
                CheckersBoard newBoard = new CheckersBoard();

                // copy the results of making the move into the enw Board
                newBoard.board = new List<char[]>(temp.board);
                newBoard.turn = temp.turn;

                // Store the last position as an integer last
                int last = moveList[0].endR * 10 + moveList[0].endC;

                // Loop while we did a capture, for multijumps
                bool capture = true;
                while (capture)
                {
                    List<Move> extraMoves = generateMoves(newBoard);
                    capture = false;

                    // We cannot make a multijump
                    if (extraMoves.Count == 0 || Math.Abs(extraMoves[0].endR - extraMoves[0].startR) != 2)
                        break;

                    // We can make a multijump
                    foreach (Move mm in extraMoves)
                    {
                        // Check if that jump can be made from our previous position
                        if (mm.startR * 10 + mm.startC == last)
                        {

                            newBoard = makeMove(newBoard, mm);
                            last = mm.endR * 10 + mm.endC;
                            capture = true;
                            break;
                        }
                    }
                }

                // Pass the turn to the other player
                newBoard.turn = !newBoard.turn;

                // Now call min
                return alphabetaMin(newBoard, alpha, beta, depth);
            }

            // Time to actually call the heuristic
            MoveScore ms;
            ms.m = alpha.m;
            ms.score = heuristicEval(b);
            UnityEngine.Debug.Log("Si entro");
            UnityEngine.Debug.Log("Max Heuristic = " + ms.score);
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
        // UnityEngine.Debug.Log("Depth is", depth);

        if (depth == 0)
        {
            List<Move> mvs = generateMoves(b);
            if (mvs.Count > 0 && (mvs[0].endR - mvs[0].startR) == 2)
            {
                // Note created new boards
                // make a new board with the first move
                CheckersBoard temp = makeMove(b, mvs[0]);
                CheckersBoard newBoard = new CheckersBoard();

                // copy the results of making the move into the enw Board
                newBoard.board = new List<char[]>(temp.board);
                newBoard.turn = temp.turn;

                int last = mvs[0].endR * 10 + mvs[0].endC;
                bool capture = true;
                while (capture)
                {
                    List<Move> moremvs = generateMoves(newBoard);
                    capture = false;
                    if (moremvs.Count == 0 || Math.Abs(moremvs[0].endR - moremvs[0].startR) != 2)
                        break;
                    foreach (Move mm in moremvs)
                    {
                        if (mm.startR * 10 + mm.startC == last)
                        {
                            newBoard = makeMove(newBoard, mm);
                            last = mm.endR * 10 + mm.endC;
                            capture = true;
                            break;
                        }
                    }
                }
                newBoard.turn = !newBoard.turn;
                return alphabetaMax(newBoard, alpha, beta, depth);
            }

            MoveScore ms;
            ms.m = alpha.m;
            ms.score = -heuristicEval(b);
            UnityEngine.Debug.Log("Min Heuristic = " + ms.score);
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

    Move alphabeta(CheckersBoard b, int depth)
    {

        if (!b.turn
            && b.board[0].SequenceEqual("_b_b_b_b")
            && b.board[1].SequenceEqual("b_b_b_b_")
            && b.board[2].SequenceEqual("_b_b_b_b")
            && b.board[5].SequenceEqual("w_w_w_w_")
            && b.board[6].SequenceEqual("_w_w_w_w")
            && b.board[7].SequenceEqual("w_w_w_w_"))
        {
            Move bopen;
            bopen.startR = 2;
            bopen.startC = 1;
            bopen.endR = 3;
            bopen.endC = 0;

            return bopen;
        }

        Move best = new Move();
        List<Move> moveList = generateMoves(b);
        best = moveList[0];

        int highScore = heuristicEval(b);
        UnityEngine.Debug.Log("HighScore " + highScore);

        MoveScore toTheInfinity;
        toTheInfinity.m = best;
        toTheInfinity.score = -99999999;
        MoveScore pos;
        pos.m = best;
        pos.score = 99999999;

        MoveScore ms = alphabetaMax(b, toTheInfinity, pos, depth);
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
        int depth = 25;

        Move f = legalMoves[0];
        f = alphabeta(gameBoard, depth);

        // Add the selected move current position
        Solution.Add((f.startC, f.startR));

        // Add the selected move next position
        Solution.Add((f.endC, f.endR));

        int last = f.endR * 10 + f.endC;

        UnityEngine.Debug.Log(last);

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
