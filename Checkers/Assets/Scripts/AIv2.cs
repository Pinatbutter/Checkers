using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

// Our second Try at a Checkers AI
// Now using Iterative Deepening alpha-beta pruning
// The second time writing it seems so much simpler
// We don't use this one anymore, but it's easier to explain

public class AIv2 : MonoBehaviour
{
    // Globals
    public static int MaxUtility = 2147483647; // just the maximun int 32 value
    public static bool isPlayerBlack = true; // by default the bot plays blacks
    public static int MaxAllowedTimeInSeconds = 5; // the seconds we let the AI run
    public static int MaxDepth = 2; // how deep we want the AI to go


    public class CheckersState
    {

        // Constructors

        public CheckersState() { }

        public CheckersState(char[,] my_grid, bool my_black_move, List<(int, int)> my_moves)
        {
            grid = my_grid;
            black_move = my_black_move;
            moves = my_moves;
            black_lost = false;
        }

        // Check End Game State ( One player loosing all their pieces)
        // We check every position in the board and if there is a piece
        public bool isEndGame()
        {
            bool has_black = false;
            bool has_white = false;

            // C# foreach can go over 2d arrays, cool!
            foreach (var cell in grid)
            {
                if (cell == 'b' || cell == 'B')
                {
                    has_black = true;
                }
                else if (cell == 'w' || cell == 'W')
                {
                    has_white = true;
                }
                if (has_black && has_white)
                {
                    return false;
                }

            }
            black_lost = has_white;
            return true;
        }

        // To get the maximun, our equivalent to infinite and -infinite
        // this will be used for the minmax and pruning
        public int getTerminalUtility()
        {
            if (isPlayerBlack != black_lost)
            {
                return MaxUtility;
            }
            else
            {
                return -MaxUtility;
            }
        }

        // Gets the successors
        public List<CheckersState> getSuccesors()
        {

            // A function to get the possible steps
            // An step is a "tuple" of two ints
            // they tell where to move the piece
            List<(int, int)> getSteps(char cell)
            {
                // Create a list of steps
                List<(int, int)> steps = new List<(int, int)>();

                // if it's not black, add white movements
                if (cell != 'b')
                {
                    steps.Add((-1, -1));
                    steps.Add((-1, 1));
                }

                // if it's not white, add black movements
                if (cell != 'w')
                {
                    steps.Add((1, -1));
                    steps.Add((1, 1));
                }

                // therefore crowns get added the 4 steps

                return steps;
            }

            // A function to generate the moves
            void generateMoves(
                char[,] board,
                int i,
                int j,
                List<CheckersState> successors)
            {
                foreach ((int stepX, int stepY) in getSteps(board[i, j]))
                {
                    int x = i + stepX;
                    int y = j + stepY;

                    // Check if the piece will remain on board
                    if (x >= 0 && x < 8 && y >= 0 && y < 8)
                    {

                        // Check if the place to move is empty
                        if (board[x, y] == '_')
                        {
                        }
                            // IMPORTANT that it is a deep copy
                            // we had so many bugs and stress because we took C# copies for granted
                            // just the = sign is a shallow copy, and our changes
                            // changed the original board
                            char[,] boardCopy = board.Clone() as char[,];

                            // "move' the piece by setting the new position with the piece it had before
                            // the old spot is marked empty with '_'
                            boardCopy[x, y] = boardCopy[i, j];
                            boardCopy[i, j] = '_';

                            // if the piece is at the top or at the bottom
                            // promote it!
                            if ((x == 7 && black_move) || (x == 0 && !black_move))
                            {
                                boardCopy[x, y] = char.ToUpper(boardCopy[x, y]);
                            }

                            // Store moves done
                            var curMoves = new List<(int, int)>();
                            curMoves.Add((i, j));
                            curMoves.Add((x, y));

                            //                  UnityEngine.Debug.Log(i + " " + j + "  " + xp + " " + yp);

                            // With this board we create a new checkers state, we pass the turn
                            // to the next player, and send the moves, we add it to our successors
                            var newState = new CheckersState(boardCopy, !black_move, curMoves);
                            successors.Add(newState);


                    }
                }
            }


            // A function to generate the jumps
            void generateJumps(
                char[,] board,
                int i,
                int j,
                List<(int, int)> moves,
                List<CheckersState> successors)
            {
                // set jump to true
                bool JumpEnd = true;

                foreach ((int stepX, int stepY) in getSteps(board[i, j]))
                {
                    int x = i + stepX;
                    int y = j + stepY;

                    // Check if with the move the piece will remain inside board
                    // Check if the place is not empty, and that the opponent is there
                    if (x >= 0 && x < 8 && y >= 0 && y < 8 && board[x, y] != '_' && char.ToLower(board[i, j]) != char.ToLower(board[x, y]))
                    {

                        int xp = x + stepX;
                        int yp = y + stepY;

                        // Check if the move is still inside board
                        // check if the place is empty (so we can jump there)
                        if (xp >= 0 && xp < 8 && yp >= 0 && yp < 8 && board[xp, yp] == '_')
                        {
                            board[xp, yp] = board[i, j];
                            char save = board[x, y];
                            board[i, j] = board[x, y] = '_';
                            char previous = board[xp, yp];

                            // Promete if needed
                            if ((xp == 7 && black_move) || (xp == 0 && !black_move))
                            {
                                board[xp, yp] = char.ToUpper(board[xp, yp]);
                            }

                            // Add the jump move
                         //   moves.Add((i, j));
                            moves.Add((xp, yp));
                            UnityEngine.Debug.Log(i + " " + j + "  " + xp + " " + yp);
                            // Generate more jumps (if multimoves don't work, we have an issue here)
                            generateJumps(board, xp, yp, moves, successors);
                            // we remove the last jump from ^
                            moves.RemoveAt(moves.Count - 1);


                            // Update boards again
                            board[i, j] = previous;
                            board[x, y] = save;
                            board[xp, yp] = '_';

                            // set jump to false
                            JumpEnd = false;

                        }

                    }

                }

                // If there was a jump
                if (JumpEnd && moves.Count > 1)
                {
                    // the boardCopy is deep copy
                    char[,] boardCopy = board.Clone() as char[,];
                    // Not so sure about this one, if we have bugs we should
                    // check this one, is a tuple value or reference ?
                    // does the constructor of List do a value or reference initialization?
                    // Also Lists do not have Clone() ... if this doesn't work it might take a while
                    // to fix it.
                    List<(int, int)> newMoves = new List<(int, int)>(moves);


                    // With this board we create a new checkers state, we pass the turn
                    // to the next player, and send the moves, we add it to our successors
                    var newState = new CheckersState(boardCopy, !black_move, newMoves);
                    successors.Add(newState);
                }


            }


            // Now the actual succesors function...
            char player = black_move ? 'b' : 'w';

            List<CheckersState> theSuccessors = new List<CheckersState>();
            List<(int, int)> theMoves = new List<(int, int)>();

            // Generate jumps
            for (int i = 0; i <= 7; i++)
            {
                for (int j = 0; j <= 7; j++)
                {
                    if (char.ToLower(grid[i, j]) == player)
                    {
                        generateJumps(grid, i, j, theMoves, theSuccessors);
                    }
                }
            }

            if (theSuccessors.Count > 0)
            {
                return theSuccessors;
            }

            // Generate moves
            for (int i = 0; i <= 7; i++)
            {
                for (int j = 0; j <= 7; j++)
                {
                    if (char.ToLower(grid[i, j]) == player)
                    {
                        generateMoves(grid, i, j, theSuccessors);
                    }
                }
            }

            return theSuccessors;
        }

        // Properties
        public char[,] grid { get; set; }
        public bool black_move;
        public List<(int, int)> moves { get; set; }
        bool black_lost;

    }

    // The Heuristic / Evaluation Function
    static public int evaluationFunction(CheckersState state)
    {
        // Weights
        int plainWeight = 10;
        int kingWeight = 14;

        int black = 0;
        int white = 0;

        // Go through every cell and check the piece
        foreach (var cell in state.grid)
        {
            if (cell == 'b')
            {
                black += plainWeight;
            }
            else if (cell == 'B')
            {
                black += kingWeight;
            }
            else if (cell == 'w')
            {
                white += plainWeight;
            }
            else if (cell == 'W')
            {
                white += kingWeight;
            }
        }

        if (isPlayerBlack)
        {
            return black - white;
        }
        else
        {
            return white - black;
        }
    }

    // Iterative Deepening Alpha Beta Search
    static public List<(int, int)> iterativeDeepeningAlphaBeta(CheckersState state)
    {
        // Alpha Beta search
        int alphaBetaSearch(CheckersState abState, int alpha, int beta, int depth)
        {
            // Max Function
            int MaxValue(CheckersState mState, int mAlpha, int mBeta, int mDepth)
            {
                int val = -MaxUtility;
                foreach (var successor in mState.getSuccesors())
                {
                    val = Math.Max(val, alphaBetaSearch(successor, mAlpha, mBeta, mDepth));

                    if (val >= mBeta)
                    {
                        return val;
                    }

                    mAlpha = Math.Max(mAlpha, val);

                }

                return val;
            }

            // Min Function
            int MinValue(CheckersState mState, int mAlpha, int mBeta, int mDepth)
            {
                int val = MaxUtility;
                foreach (var succesor in mState.getSuccesors())
                {
                    val = Math.Min(val, alphaBetaSearch(succesor, mAlpha, mBeta, mDepth - 1));
                    if (val <= mAlpha)
                    {
                        return val;
                    }
                    mBeta = Math.Min(mBeta, val);

                }

                return val;
            }

            // End Game check
            if (abState.isEndGame())
            {
                return abState.getTerminalUtility();
            }

            // At the deepest depth use the evaluation function instead of going deeper.
            if (depth <= 0)
            {
                return evaluationFunction(abState);
            }

            // if not at the bottom, then keep going down, hasta el subsuelo
            if (state.black_move == isPlayerBlack)
            {
                return MaxValue(abState, alpha, beta, depth);
            }
            else
            {
                return MinValue(abState, alpha, beta, depth);
            }

        }

        // iterative deepening
        List<(int, int)> bestMove = new List<(int, int)>();

        for (int depth = 1; depth <= MaxDepth; depth++)
        {
            int val = -MaxUtility;
            foreach (var succesor in state.getSuccesors())
            {
                int score = alphaBetaSearch(succesor, -MaxUtility, MaxUtility, depth);
                if (score > val)
                {
                    val = score;
                    bestMove = succesor.moves;
                }

            }
        }

        return bestMove;

    }


    // Start is called before the first frame update
    void Start()
    {
        char[,] testBoard = {
            {'_','_','_','_','_','_','_','_'},
            {'b','_','b','_','b','_','b','_'},
            {'_','_','_','b','_','_','_','_'},
            {'_','_','b','_','_','_','_','_'},
            {'_','w','_','_','_','_','_','_'},
            {'w','_','b','_','_','_','w','_'},
            {'_','w','_','_','_','_','_','b'},
            {'w','_','_','_','_','_','w','_'}
        };
        char testTurn = 'w';

        isPlayerBlack = testTurn == 'b';

        List<(int, int)> moves = new List<(int, int)>();
        CheckersState state = new CheckersState(testBoard, isPlayerBlack, moves);

        moves = iterativeDeepeningAlphaBeta(state);

        UnityEngine.Debug.Log(moves.Count - 1);



        foreach ((int x, int y) in moves)
        {
            UnityEngine.Debug.Log("pos = " + x + ' ' + y);
        }

    }


}
