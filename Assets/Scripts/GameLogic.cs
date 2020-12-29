using UnityEngine;
using System;

public enum State
{
    Empty, X, O
}

public class GameLogic
{
    static GameLogic instance = new GameLogic ();

    private State[,] gridStates = new State[3,3];
    private int round = 1;
    private bool isGameOver = false;
    private State winner = State.Empty;

    private GameLogic ()
    {
    }

    public static GameLogic GetInstance ()
    {
        return instance;
    }

    public void PlaceZeroOrCross (Vector2Int cell, bool isCross)
    {
        gridStates[cell.x, cell.y] = isCross ? State.X : State.O;
        round++;
        CheckIfGameOver ();
    }

    public State GetCellState (Vector2Int cell)
    {
        return gridStates[cell.x, cell.y];
    }


    public bool IsGameOver ()
    {
        return isGameOver;
    }

    public State WhoWon ()
    {
        Debug.Assert (isGameOver);
        return winner;
    }

    public void RestartGame ()
    {
        isGameOver = false;
        round = 1;
        winner = State.Empty;
        gridStates = new State[3, 3];
    }

    private void CheckIfGameOver ()
    {
        // horizontally and vertically
        for (int i = 0; i < 3 && winner == State.Empty; i++)
        {
            if (gridStates[i, 0] != State.Empty && gridStates[i, 0] == gridStates[i, 1] && gridStates[i, 1] == gridStates[i, 2])
            {
                isGameOver = true;
                winner = gridStates[i, 0];
            }

            if (gridStates[0, i] != State.Empty && gridStates[0, i] == gridStates[1, i] && gridStates[1, i] == gridStates[2, i])
            {
                isGameOver = true;
                winner = gridStates[0, i];
            }
        }

        //diagonally
        if (gridStates[0, 0] != State.Empty && gridStates[0, 0] == gridStates[1, 1] && gridStates[1, 1] == gridStates[2, 2])
        {
            isGameOver = true;
            winner = gridStates[0, 0];
        }

        if (gridStates[0, 2] != State.Empty && gridStates[0, 2] == gridStates[1, 1] && gridStates[1, 1] == gridStates[2, 0])
        {
            isGameOver = true;
            winner = gridStates[0, 2];
        }

        if (round > 9)
            isGameOver = true;
    }


    // methods implementing AI
    public Vector2Int findOptimalMove (bool aiPlaysX)
    {
        State[,] tempGrid = gridStates;
        int bestValue = -1000;
        int rowValue = 0;
        int colValue = 0;
        State aiPlayer = aiPlaysX ? State.X : State.O;

        // check the gridStates and find the best next move
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (tempGrid[i, j] == State.Empty)
                {
                    // we make a guess and AI decided to choose this 
                    tempGrid[i, j] = aiPlayer;

                    // calculate the cost of this step using the minimax
                    int costMove = minimax(tempGrid, 0, false, aiPlaysX);

                    //remove this step
                    tempGrid[i, j] = State.Empty;

                    // if this is best, then update this new score
                    if (costMove > bestValue)
                    {
                        rowValue = i;
                        colValue = j;
                        bestValue = costMove;
                    }
                }
            }
        }

        return new Vector2Int (rowValue, colValue);
    }

    //minimax algorithm
    private int minimax (State[,] tempGrid, int depth, bool isMax, bool aiPlaysX)
    {
        int score = totalscorecheck(tempGrid, aiPlaysX);

        State aiPlayer = aiPlaysX ? State.X : State.O;
        State humanPlayer = aiPlaysX ? State.O : State.X;

        // maximizer won the game  
        if (score == 10)
            return score - depth;

        // minimizer won the game   
        if (score == -10)
            return score + depth;

        // no moves left and no winner 
        if (!HasEmptyCell (tempGrid))
            return 0;

        // maximizer's move 
        if (isMax)
        {
            int best = -1000;

            // check all cells
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (tempGrid[i, j] == State.Empty)
                    {
                        // guess the step
                        tempGrid[i, j] = aiPlayer;

                        // minimax recursively to compute maximum value 
                        best = Math.Max (best, minimax (tempGrid, depth + 1, false, aiPlaysX));

                        // back this step
                        tempGrid[i, j] = State.Empty;
                    }
                }
            }
            return best;
        }

        // minimizer's move 
        else
        {
            int best = 1000;

            // check the tempGrid
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (tempGrid[i, j] == State.Empty)
                    {
                        // Make a guess
                        tempGrid[i, j] = humanPlayer;

                        // minimax recursively to compute minimum value 
                        best = Math.Min (best, minimax (tempGrid, depth + 1, true, aiPlaysX));

                        // back this step
                        tempGrid[i, j] = State.Empty;
                    }
                }
            }
            return best;
        }
    }

    private int totalscorecheck (State[,] tempGrid, bool aiPlaysX)
    {
        State aiPlayer = aiPlaysX ? State.X : State.O;
        State humanPlayer = aiPlaysX ? State.O : State.X;

        // Rows - X or O win
        for (int row = 0; row < 3; row++)
        {
            if (tempGrid[row, 0] == tempGrid[row, 1] && tempGrid[row, 1] == tempGrid[row, 2])
            {
                if (tempGrid[row, 0] == aiPlayer)
                    return +10;
                else if (tempGrid[row, 0] == humanPlayer)
                    return -10;
            }
        }

        // Columns - X or O win
        for (int col = 0; col < 3; col++)
        {
            if (tempGrid[0, col] == tempGrid[1, col] && tempGrid[1, col] == tempGrid[2, col])
            {
                if (tempGrid[0, col] == aiPlayer)
                    return +10;

                else if (tempGrid[0, col] == humanPlayer)
                    return -10;
            }
        }

        // Diagonals - X or O win
        if (tempGrid[0, 0] == tempGrid[1, 1] && tempGrid[1, 1] == tempGrid[2, 2])
        {
            if (tempGrid[0, 0] == aiPlayer)
                return +10;
            else if (tempGrid[0, 0] == humanPlayer)
                return -10;
        }

        if (tempGrid[0, 2] == tempGrid[1, 1] && tempGrid[1, 1] == tempGrid[2, 0])
        {
            if (tempGrid[0, 2] == aiPlayer)
                return +10;
            else if (tempGrid[0, 2] == humanPlayer)
                return -10;
        }

        // Elif 
        return 0;
    }

    private bool HasEmptyCell (State[,] gridStates)
    {
        foreach (State cellState in gridStates)
        {
            if (cellState == State.Empty)
                return true;
        }

        return false;
    }
}



