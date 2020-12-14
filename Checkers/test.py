from copy import deepcopy
from time import time
import sys

# Global Constants
MaxUtility = 1e9
IsPlayerBlack = True
MaxAllowedTimeInSeconds = 0.5
MaxDepth = 250


# Checkers State Representation class:
# Storing the pieces in an 8x8 representation.
class CheckersState:

    # object
    def __init__(self, grid, black_move, moves):
        self.grid = grid  # the grid with the board
        self.blackToMove = black_move  # if its black's turn
        self.moves = moves  # the number of jumps in the turn
        self.did_black_lose = False

    # This checks if one of the players has lost all of their pieces.
    def is_end_game(self):
        black_found, white_found = False, False
        for row in grid:
            for cell in row:
                if cell == 'b' or cell == 'B':
                    black_found = True
                elif cell == 'w' or cell == 'W':
                    white_found = True
                if black_found and white_found:
                    return False
        self.did_black_lose = white_found
        return True

    def get_terminal_utility(self):
        return MaxUtility if IsPlayerBlack != self.did_black_lose else -MaxUtility

    def get_successors(self):
        def get_steps(cell):
            white_steps = [(-1, -1), (-1, 1)]
            black_steps = [(1, -1), (1, 1)]
            steps = []
            if cell != 'b':
                steps.extend(white_steps)
            if cell != 'w':
                steps.extend(black_steps)
            return steps

        def generate_moves(board, i, j, successors):

            for step in get_steps(board[i][j]):

                x = i + step[0]
                y = j + step[1]

                if x >= 0 and x < 8 and y >= 0 and y < 8 and board[x][y] == '_':

                    # deepCopy important
                    boardCopy = deepcopy(board)

                    boardCopy[x][y] = boardCopy[i][j]
                    boardCopy[i][j] = '_'

                    # A pawn is promoted when it reaches the last row
                    if (x == 7 and self.blackToMove) or (x == 0 and not self.blackToMove):
                        boardCopy[x][y] = boardCopy[x][y].upper()
                    successors.append(CheckersState(boardCopy, not self.blackToMove, [(i, j), (x, y)]))

        def generate_jumps(board, i, j, moves, successors):
            jump_end = True

            for step in get_steps(board[i][j]):
                x, y = i + step[0], j + step[1]


                if x >= 0 and x < 8 and y >= 0 and y < 8 and board[x][y] != '_' and board[i][j].lower() != board[x][
                    y].lower():
                    xp, yp = x + step[0], y + step[1]


                    if xp >= 0 and xp < 8 and yp >= 0 and yp < 8 and board[xp][yp] == '_':
                        board[xp][yp], save = board[i][j], board[x][y]
                        board[i][j] = board[x][y] = '_'
                        previous = board[xp][yp]
                        # A pawn is promoted when it reaches the last row
                        if (xp == 7 and self.blackToMove) or (xp == 0 and not self.blackToMove):
                            board[xp][yp] = board[xp][yp].upper()

                        moves.append((xp, yp))
                        generate_jumps(board, xp, yp, moves, successors)
                        moves.pop()

                        board[i][j], board[x][y], board[xp][yp] = previous, save, '_'
                        jump_end = False
            if jump_end and len(moves) > 1:
                successors.append(CheckersState(deepcopy(board), not self.blackToMove, deepcopy(moves)))

        player = 'b' if self.blackToMove else 'w'
        successors = []

        # generate jumps
        for i in range(8):
            for j in range(8):
                if self.grid[i][j].lower() == player:
                    generate_jumps(self.grid, i, j, [(i, j)], successors)
        if len(successors) > 0:
            return successors

        # generate moves
        for i in range(8):
            for j in range(8):
                if self.grid[i][j].lower() == player:
                    generate_moves(self.grid, i, j, successors)
        return successors


# saber si una dama esta segura
# Todo

# saber si una reina esta segura (pegada al borde)
# Todo


# La Heuristic / Evaluation Function, con esto vamo a jugar
def evaluation_function(state):

    # weights:
    plain_weight = 1.0
    king_weight = 1.4


    black, white = 0, 0

    for row in state.grid:
        for cell in row:
            if cell == 'b':
                black += plain_weight
            elif cell == 'B':
                black += king_weight
            elif cell == 'w':
                white += plain_weight
            elif cell == 'W':
                white += king_weight

    return black - white if IsPlayerBlack else white - black


def iterative_deepening_alpha_beta(state, evaluationFunc):
    startTime = time()

    def alpha_beta_search(state, alpha, beta, depth):
        def max_value(state, alpha, beta, depth):
            val = -MaxUtility
            for successor in state.get_successors():
                val = max(val, alpha_beta_search(successor, alpha, beta, depth))
                if val >= beta: return val
                alpha = max(alpha, val)
            return val

        def min_value(state, alpha, beta, depth):
            val = MaxUtility
            for successor in state.get_successors():
                val = min(val, alpha_beta_search(successor, alpha, beta, depth - 1))
                if val <= alpha:
                    return val
                beta = min(beta, val)
            return val

        if state.is_end_game():
            return state.get_terminal_utility()

        if depth <= 0 or time() - startTime > MaxAllowedTimeInSeconds:
            return evaluationFunc(state)
        return max_value(state, alpha, beta, depth) if state.blackToMove == IsPlayerBlack else min_value(state, alpha,
                                                                                                       beta, depth)

    bestMove = None
    for depth in range(1, MaxDepth):
        if time() - startTime > MaxAllowedTimeInSeconds: break
        val = -MaxUtility
        for successor in state.get_successors():
            score = alpha_beta_search(successor, -MaxUtility, MaxUtility, depth)
            if score > val:
                val, bestMove = score, successor.moves
    return bestMove


if __name__ == '__main__':
    player = sys.argv[1]

    boardSize = 8
    grid = []
    #for i in range(boardSize):
     #   grid.append(input())

    for i in range(2, 10):
        grid.append(sys.argv[i])


    #for row in grid:
    #    print(row)

    # print(grid)

    IsPlayerBlack = player[0] == 'b'
    state = CheckersState([list(row.rstrip()) for row in grid], IsPlayerBlack, [])

    # print(state.grid)

    # print(state)

    move = iterative_deepening_alpha_beta(state, evaluation_function)
    print(len(move) - 1)
    for step in move:
        print(step[0], step[1])
