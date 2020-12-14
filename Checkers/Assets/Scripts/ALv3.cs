using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System;
using UnityEngine;
using System.CodeDom.Compiler;

public class ALv3 : MonoBehaviour
{
    private static int N = 8; // Our board size
    private static int JUMP = 2; // the movements in a jump
    private static int[,] delta = // The possible movements
    {
        {-1, -1},
        {-1,  1},
        {-2, -2},
        {-2,  2}
    };
    private static char[] vals = { 'B', 'b', '_', 'w', 'W' }; // the representations
    private static int P_MAX = 7; // the depth
    private static int NOT_ZERO = 1; // no reason for this number, just doesn't have to be zero
    private const int INF = 2147483646; // Max int that can be represented
    private (int, int) NO_MULTIPLE = (-1, -1);
    private static double T_MAX = 15; // Maximun amount of time we let it run


    private SortedDictionary<int, (int, List<Move>)> mem;
    private int nbExplored;

    // The Move Struct
    // we still not sure about technicall diferences between struct and class
    public struct Move
    {
        // 
        public int row;
        public int col;
        public int dir;
        public int coeff;

        // Constructors
        public Move(int a, int b, int c, int d)
        {
            row = a;
            col = b;
            dir = c;
            coeff = d;
        }
    }

    // white > 0, black < 0, empty = 0, pawn = 1, queen =2
    public static int[,] t = new int[8, 8];


    // Tells if we are still inside the board
    bool isIn(int row, int col)
    {
        return 0 <= row && row < N && 0 <= col && col < N;
    }

    // Gives us back the new position
    void newPos(ref Move m, int pre, ref int row, ref int col)
    {
        m.coeff *= pre < 0 ? -1 : 1;
        row = m.row + delta[m.dir, 0] * m.coeff;
        col = m.col + delta[m.dir, 1] * m.coeff;
    }

    // Is the move playable?
    bool playable(Move m)
    {
        int row = 0, col = 0;
        newPos(ref m, t[m.row, m.col], ref row, ref col);

        if (!isIn(row, col))
            return false;

        if (t[row, col] != 0)
            return false;

        if (m.dir >= JUMP)
        {
            row -= delta[m.dir, 0] / 2 * m.coeff;
            col -= delta[m.dir, 1] / 2 * m.coeff;
            return t[row, col] * t[m.row, m.col] < 0;
        }

        return true;
    }

    // Get the moves
    List<Move> getMoves(int p, ref (int, int) last)
    {
        List<Move> moves = new List<Move>();
        for (int take = JUMP; take >= 0; take -= 2)
        {
            for (int row = 0; row < N; row++)
                for (int col = 0; col < N; col++)
                {
                    if (last != NO_MULTIPLE && (row, col) != last)
                        continue;
                    if (t[row, col] * p > 0)
                        for (int coeff = 1; coeff >= -1; coeff -= 4 - Math.Abs(t[row, col])) //queen: 1, -1
                            for (int dir = take; dir < take + 2; dir++) //2,3 then 0,1
                            {
                                Move move = new Move(row, col, dir, coeff);
                                if (playable(move))
                                    moves.Add(move);
                            }
                }
            if (moves.Count > 0)
                return moves;
            if (last != NO_MULTIPLE)
            {
                List<Move> temp = new List<Move>();
                return temp;
            }
        }
        return moves;
    }

    // Play the game
    int play(Move m, int pre, int taken = 0)
    {
        int row = 0;
        int col = 0;
        newPos(ref m, pre, ref row, ref col);

        t[row, col] = taken > 0 ? 0 : pre;
        if (Math.Abs(pre) == 1 && (row == 0 && t[row, col] > 0 || row == N - 1 && t[row, col] < 0))
            t[row, col] *= 2;
        t[m.row, m.col] = taken > 0 ? pre : 0;

        int r = NOT_ZERO;
        if (m.dir >= JUMP)
        {
            row -= delta[m.dir, 0] / 2 * m.coeff;
            col -= delta[m.dir, 1] / 2 * m.coeff;
            r = t[row, col];
            t[row, col] = taken;
        }
        return r;
    }

    // Get the distance between two pieces
    public int dist((int, int) a, (int, int) b)
    {
        return ((a.Item1 - b.Item1) * (a.Item1 - b.Item1)
              + (a.Item2 - b.Item2) * (a.Item2 - b.Item2));
    }


    // Evaluation function, our heuristic
    public int eval(int p)
    {
        const int X = 100;
        const int Y = 100000 * X; // > 2*8*8 * 12*12 * X
        int cnt = 0;
        for (int i = 0; i < N; i++)
            for (int j = 0; j < N; j++)
                cnt += t[i, j];
        //the less pawns the better, the lest dist the better
        List<List<(int, int)>> c = new List<List<(int, int)>>(2);

        for (int i = 0; i < N; i++)
            for (int j = 0; j < N; j++)

                if (t[i, j] != 0)
                {
                    int cIndex = t[i, j];
                    cIndex = cIndex < 0 ? 0 : cIndex;
                    c[cIndex].Add((i, j));
                }

        int distSum = 0;
        foreach (var a in c[0])
            foreach (var b in c[1])
                distSum += dist(a, b);

        UnityEngine.Debug.Log("-X" + -X + "distSUm" + distSum + " y " + Y);


        
        return (-X * distSum + Y * (cnt / p));
    }

    // A pseudo hash table
    int hashT()
    {
        const int M = 222222227; // a huge prime number
        int r = 0;

        int h = 1;
        for (int i = 0; i < N; i++)
        {
            h = 1;
            for (int j = 0; j < N; j++)
            {
                h = (h * vals.Count()) % M;
                r = (r + (t[i, j] + 2) * h) % M;
            }
        }


        return r;

    }

    // Our alpha-beta function
    public (int, List<Move>) f(
        int p,
        int pr,
        (int, int) multipleMoves,
        int alpha = -INF,
        int beta = INF)
    {
        //if multiple

        // we increase the explored count
        nbExplored++;

        // If we have reached the last depth
        // the we evaluate
        List<Move> emptyMove = new List<Move>();

        if (pr >= P_MAX)
            return (eval(p), emptyMove);

        var moves = getMoves(p, ref multipleMoves);

        // If we have moves?
        if (multipleMoves != NO_MULTIPLE && !moves.Any())
        {
            var r = f(p * (-1), pr + 1, NO_MULTIPLE, -INF, INF);
            return ((-r.Item1, emptyMove));
        }

        // The best case set up
        (int, List<Move>) best = (-INF, emptyMove);


        // Go through each move
        foreach (var move in moves)
        {
            // 
            int pre = t[move.row, move.col];
            int taken = play(move, pre);
            int nextP = p;
            int nextPr = pr;
            (int, int) nextMultipleMoves = (0, 0);


            Move move_ = move;
            newPos(ref move_, p, ref nextMultipleMoves.Item1, ref nextMultipleMoves.Item2);


            if (move.dir < JUMP)
            {
                nextP *= -1;
                nextPr++;
                nextMultipleMoves = NO_MULTIPLE;
            }


            var val = f(nextP, nextPr, nextMultipleMoves, -beta, -alpha);


            if (move.dir < JUMP)
            {
                val.Item1 *= -1;
            }

            if (val.Item1 > best.Item1)
            {
                if (nextMultipleMoves == NO_MULTIPLE)
                {
                    val.Item2 = new List<Move>();
                }

                val.Item2.Insert(0, move);

                best = val;

            }
            play(move, pre, taken);
            alpha = Math.Max(alpha, val.Item1);
            if (alpha > beta)
                break;
        }
        return best;
    }



    // Start is called before the first frame update
    public void Start()
    {
        char[,] s = {
            {'_','_','_','_','_','_','_','_'},
            {'b','_','b','_','b','_','b','_'},
            {'_','_','_','b','_','_','_','_'},
            {'_','_','b','_','_','_','_','_'},
            {'_','w','_','_','_','_','_','_'},
            {'w','_','b','_','_','_','w','_'},
            {'_','w','_','_','_','_','_','b'},
            {'w','_','_','_','_','_','w','_'}
        };

        char x;
        int n;
        //cin >> x >> n;
        x = 'w';
        n = 8;

        //string s[n];

        //for (int i = 0; i < n; i++)
        // cin >> s[i];

        // Go through every cell\

        UnityEngine.Debug.Log("entro el loop antes del for");
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {

                // Representation Change from charavter to 
                for (int k = 0; k < vals.Count(); k++)
                {
                    if (s[i, j] == vals[k])
                    {
                        t[i, j] = k - 2;
                    }

                }
            }
        }
        UnityEngine.Debug.Log("Paso el loop");

        // Get the player turn
        int p = (x == 'w') ? 1 : -1;


        var timeElapsed = Stopwatch.StartNew();

        List<Move> moves;
        do
        {
            UnityEngine.Debug.Log("Paso el loop");
            nbExplored = 0;
            moves = f(p, 0, NO_MULTIPLE).Item2;
            P_MAX++;
        }
        while (timeElapsed.ElapsedTicks < T_MAX);

        UnityEngine.Debug.Log("Paso el move");


        // cout << moves.size() << endl;
        UnityEngine.Debug.Log(moves.Count());

        foreach (Move m in moves)
        {
            UnityEngine.Debug.Log("pos = " + m.row + ' ' + m.col);
        }

        int row = 0;
        int col = 0;

        Move temp = moves[moves.Count() - 1];
        newPos(ref temp, p, ref row, ref col);


        UnityEngine.Debug.Log("zpos = " + row + col);

    }

}
