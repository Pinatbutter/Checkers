from copy import deepcopy
from time import time
import sys

# Global Constants
Infinity = 2147483647  # maximum integer value
isBlack = True  # if the player is black
TimeLimit = 2  # maximum allowed time
DepthLimit = 200  # maximum depth


# Checkers State Representation class:
# Storing the pieces in an 8x8 representation.
class CheckersState:

    # Constructor
    def __init__(self, grid, black_move, moves):
        self.grid = grid  # the grid with the board
        self.black_turn = black_move  # if its black's turn
        self.moves = moves  # the number of jumps in the turn
        self.did_black_lose = False

    # This checks if one of the players has lost all of their pieces.
    def is_end_game(self):
        black_found, white_found = False, False

        # we go through every cell and check if it has pieces
        for row in grid:
            for cell in row:
                if cell == 'b' or cell == 'B':
                    black_found = True
                elif cell == 'w' or cell == 'W':
                    white_found = True
                if black_found and white_found:
                    return False

        # if we only found one piece then, we need to see who won
        self.did_black_lose = white_found
        return True

    # This gives us the relative infinite
    def relative_infinity(self):
        return Infinity if isBlack != self.did_black_lose else -Infinity

    # We generate the possible next boards
    def get_children(self):

        # We get the possible steps
        def get_steps(cell):

            # if its white it can move above and left/ right
            white_steps = [(-1, -1), (-1, 1)]
            # if its black it can move below and left/right
            black_steps = [(1, -1), (1, 1)]

            # we create an array with the steps
            steps = []
            if cell != 'b':
                steps.extend(white_steps)
            if cell != 'w':
                steps.extend(black_steps)

            return steps

        # We generate the possible moves
        def get_moves(board, i, j, successors):

            # for every possible step done by the current piece
            for step in get_steps(board[i][j]):

                x = i + step[0]
                y = j + step[1]

                if is_inside(x, y) and board[x][y] == '_':

                    # deepCopy important
                    boardCopy = deepcopy(board)

                    boardCopy[x][y] = boardCopy[i][j]
                    boardCopy[i][j] = '_'

                    # Promote the pawns at their last rows
                    if can_promote(x, self.black_turn):
                        boardCopy[x][y] = boardCopy[x][y].upper()

                    # Add the new Checkers State to the successors list
                    # Pass the turn, and the moves too.
                    successors.append(CheckersState(boardCopy, not self.black_turn, [(i, j), (x, y)]))

        # We generate the jumps
        def get_jumps(board, i, j, moves, successors):
            jump_end = True

            # for every step possible with the current piece
            for step in get_steps(board[i][j]):
                x, y = i + step[0], j + step[1]

                # are we still inside, and the piece there is not ours?
                if is_inside(x, y) and board[x][y] != '_' and board[i][j].lower() != board[x][y].lower():

                    # the let's jump there
                    xp = x + step[0]
                    yp = y + step[1]

                    # after the jump are we still inside and the cell is free?
                    if is_inside(xp, yp) and board[xp][yp] == '_':

                        # then jump! leave our cell, and the cell where we jump over
                        # empty, move our cell to the place where we jumped
                        board[xp][yp], save = board[i][j], board[x][y]
                        board[i][j] = board[x][y] = '_'
                        previous = board[xp][yp]

                        # Promote the pawns if possible
                        if can_promote(xp, self.black_turn):
                            board[xp][yp] = board[xp][yp].upper()

                        # now add the moves
                        moves.append((xp, yp))

                        # IMPORTANT
                        # generate more jumps as it could be a double jump
                        get_jumps(board, xp, yp, moves, successors)

                        # we still need to save the board so pop it
                        moves.pop()

                        board[i][j], board[x][y], board[xp][yp] = previous, save, '_'
                        jump_end = False

            # Add it to the list if possible
            if jump_end and len(moves) > 1:
                successors.append(CheckersState(deepcopy(board), not self.black_turn, deepcopy(moves)))

        # pass the turn
        player = 'b' if self.black_turn else 'w'

        # the successors array
        successors = []

        # generate jumps
        # look for every cell if we are there, generate jumps
        # if we can jump oc we return the jumps as its a rule
        for i in range(8):
            for j in range(8):
                if self.grid[i][j].lower() == player:
                    get_jumps(self.grid, i, j, [(i, j)], successors)
        if len(successors) > 0:
            return successors

        # generate moves
        # look for every cell, if we are there, generate moves
        for i in range(8):
            for j in range(8):
                if self.grid[i][j].lower() == player:
                    get_moves(self.grid, i, j, successors)

        # we return the moves
        return successors


# minor helper functions
def is_inside(x, y):
    return 0 <= x < 8 and 0 <= y < 8


# Can a pawn be promoted?
def can_promote(turn, x):
    return (x == 7 and turn) or (x == 0 and not turn)



# The Alpha Beta Function
def iterative_deepening_alpha_beta(state, evaluationFunc):
    # Start a clock
    timer = time()

    # Our Alpha Beta Search
    def alpha_beta_search(state, alpha, beta, depth):

        # Maximizing Player
        def max_value(state, alpha, beta, depth):
            value = -Infinity
            for child in state.get_children():
                value = max(value, alpha_beta_search(child, alpha, beta, depth))
                if value >= beta:
                    return value  # Beta cutoff
                alpha = max(alpha, value)
            return value

        # Minimizing Adversary
        def min_value(state, alpha, beta, depth):
            value = Infinity
            for child in state.get_children():
                value = min(value, alpha_beta_search(child, alpha, beta, depth - 1))
                if value <= alpha:
                    return value  # Alpha cutoff
                beta = min(beta, value)
            return value

        # check if the game is done
        if state.is_end_game():
            return state.relative_infinity()

        # evaluate due to depth and time constraints
        if depth <= 0 or time() - timer > TimeLimit:
            return evaluationFunc(state)

        # if its our turn max, otherwise min
        if state.black_turn == isBlack:
            return max_value(state, alpha, beta, depth)
        else:
            return min_value(state, alpha, beta, depth)

    # best move
    bestMove = None

    # For every depth
    for depth in range(1, DepthLimit):
        # Stop due to time constraints
        if time() - timer > TimeLimit:
            break

        # setup -infinity
        val = -Infinity
        for child in state.get_children():
            score = alpha_beta_search(child, -Infinity, Infinity, depth)

            # keep the best score and its moves
            if score > val:
                val = score
                bestMove = child.moves

    return bestMove


# La Heuristic / Evaluation Function, con esto vamos a jugar
def evaluation_function(state):
    # weights:
    plain_weight = 100
    king_weight = 150

    # NOTE
    # these bonuses made our AI play worse than before
    # but logically they should be good, therefore
    # we increased they ar not as significant compared to
    # the amount of pieces that one has

    # The number of pieces just seems way more important
    # than their position

    # bonus weights:
    back_weight = 4
    middle_weight = 2.5
    center_weight = 0.5
    risky_weight = 3

    # Position Helpers:

    # for black the back rows are at the top
    def is_top(y):
        return y == 0 or y == 1

    # for white the back rows are at the bottom
    def is_bot(y):
        return y == 6 or y == 7

    # pieces in the center and active field
    def is_center(x, y):
        return (y == 3 or y == 4) and (x == 3 or x == 4)

    # pieces in the middle two columns
    def is_middle(x):
        return x == 3 or x == 4

    # Positional Bonuses helper functions
    def black_bonus(x, y):
        bonus = 0
        if is_top(y):
            bonus += back_weight
        if is_middle(x):
            bonus += middle_weight
        if is_center(x, y):
            bonus += center_weight
        return bonus

    def white_bonus(x, y):
        bonus = 0
        if is_bot(y):
            bonus += back_weight
        if is_middle(x):
            bonus += middle_weight
        if is_center(x, y):
            bonus += center_weight
        return bonus

    # Check if opponent could take the piece
    def white_risk_penalty(x, y):
        bonus = 0
        pY = y - 1
        pX = x - 1
        if is_inside(x,y):
            if state.grid[y][x].lower() == 'b':
                bonus -= risky_weight
        pX = x + 1
        if is_inside(x, y):
            if state.grid[y][x].lower() == 'b':
                bonus -= risky_weight
        return bonus

    def black_risk_penalty(x, y):
        bonus = 0
        pY = y + 1
        pX = x - 1
        if is_inside(x,y):
            if state.grid[y][x].lower() == 'w':
                bonus -= risky_weight
        pX = x + 1
        if is_inside(x, y):
            if state.grid[y][x].lower() == 'w':
                bonus -= risky_weight
        return bonus

    # Counters
    black = 0
    white = 0

    for y in range(8):
        for x in range(8):
            if state.grid[y][x] == 'b':
                black += plain_weight
                black += black_bonus(x, y)
                black += black_risk_penalty(x, y)
            elif state.grid[y][x] == 'B':
                black += king_weight
                black += black_bonus(x, y)
                black += black_risk_penalty(x, y)
            elif state.grid[y][x] == 'w':
                white += plain_weight
                white += white_bonus(x, y)
                white += white_risk_penalty(x, y)
            elif state.grid[y][x] == 'W':
                white += king_weight
                white += white_bonus(x, y)
                white += white_risk_penalty(x, y)

    return black - white if isBlack else white - black


# Main Function
if __name__ == '__main__':

    # Input from command line
    grid = []
    for i in range(2, 10):
        grid.append(sys.argv[i])

    # Process the date
    isBlack = sys.argv[1] == 'b'
    state = CheckersState([list(row.rstrip()) for row in grid], isBlack, [])
    move = iterative_deepening_alpha_beta(state, evaluation_function)

    # Output, best moves
    print(len(move) - 1)
    for step in move:
        print(step[0], step[1])
