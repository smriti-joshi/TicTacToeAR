using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;

public enum Player
{
    Empty, X, O
}

public class Cell
{

    private Player player;
    private Vector3 center;

    public Cell()
    {
        this.player = Player.Empty;
    }

    public Cell (Vector3 center)
    {
        this.player = Player.Empty;
        this.center = center;
    }

    public Vector3 Center { get => center; set => center = value; }
    public Player Player { get => player; set => player = value; }
}


public class GameLogic
{
    static GameLogic instance = new GameLogic ();

    private Cell[,] grid = new Cell[3,3];
    private Quaternion rotation { get; set; }
    public Cell[,] Grid { get => grid; set => grid = value; }

    private int round = 1;
    private bool isGameOver = false;
    private float cellWidth;
    private Player winner = Player.Empty;

    private GameLogic ()
    {

    }

    public static GameLogic GetInstance ()
    {
        return instance;
    }

    public void InitGrid (Vector3 gridCenter, float gridSize, Quaternion rotation, GameObject obj)
    {
        //obj.transform.SetPositionAndRotation (gridCenter, rotation);
        //obj.transform.RotateAround (gridCenter, Vector3.up, rotation.eulerAngles.y);

        cellWidth = gridSize / 3;        

        grid[0,0] = new Cell (new Vector3 (gridCenter.x - cellWidth, gridCenter.y, gridCenter.z - cellWidth));
        grid[0,1] = new Cell (new Vector3 (gridCenter.x            , gridCenter.y, gridCenter.z - cellWidth));
        grid[0,2] = new Cell (new Vector3 (gridCenter.x + cellWidth, gridCenter.y, gridCenter.z - cellWidth));
        grid[1,0] = new Cell (new Vector3 (gridCenter.x - cellWidth, gridCenter.y, gridCenter.z            ));
        grid[1,1] = new Cell (new Vector3 (gridCenter.x            , gridCenter.y, gridCenter.z            ));
        grid[1,2] = new Cell (new Vector3 (gridCenter.x + cellWidth, gridCenter.y, gridCenter.z            ));
        grid[2,0] = new Cell (new Vector3 (gridCenter.x - cellWidth, gridCenter.y, gridCenter.z + cellWidth));
        grid[2,1] = new Cell (new Vector3 (gridCenter.x            , gridCenter.y, gridCenter.z + cellWidth));
        grid[2,2] = new Cell (new Vector3 (gridCenter.x + cellWidth, gridCenter.y, gridCenter.z + cellWidth));

        foreach(Cell cell in grid){
            CorrectCellCenters (obj, cell, rotation);
        }
    }

    public void PlaceZeroOrCross(Vector2Int cell, bool isCross)
    {
        grid[cell.x, cell.y].Player = isCross ? Player.X : Player.O;
        round++;
        CheckIfGameOver ();
    }

    public Vector2Int GetClosestCell (Vector3 pos)
    {
        float minDistance = 1000.0f;
        Vector2Int selectedCell = new Vector2Int(-1, -1);
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                float currentDistance = Vector3.Distance(grid[i,j].Center, pos);
                if (currentDistance < minDistance && grid[i, j].Player == Player.Empty)
                {
                    minDistance = currentDistance;
                    selectedCell = new Vector2Int (i, j);
                }
            }
        }
        
        if (minDistance > cellWidth / 2)
            return new Vector2Int (-1, -1);

        return selectedCell;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public Player WhoWon()
    {
        Debug.Assert (isGameOver);
        return winner;
    }

    public void RestartGame()
    {
        isGameOver = false;
        round = 1;
        winner = Player.Empty;

        foreach (Cell cell in grid)
        {
            cell.Player = Player.Empty;
        }
    }


    private void CorrectCellCenters (GameObject obj, Cell cell, Quaternion rot)
    {
        Vector3 offset = new Vector3 (0.0f, 0.25f, -0.0f);
        obj.transform.SetPositionAndRotation (cell.Center + offset, rot);
        obj.transform.RotateAround (grid[1,1].Center, Vector3.up, rot.eulerAngles.y);
        cell.Center = obj.transform.position;
    }

    private void CheckIfGameOver()
    {
        if (round > 9)
        {
            isGameOver = true;
            winner = Player.Empty;
            return;
        }

        // horizontally and vertically
        for (int i = 0; i < 3 && winner == Player.Empty; i++)
        {
            if (grid[i,0].Player != Player.Empty && grid[i,0].Player == grid[i,1].Player && grid[i,1].Player == grid[i,2].Player)
            {
                isGameOver = true;
                winner = grid[i, 0].Player;
            }

            if (grid[0,i].Player != Player.Empty && grid[0,i].Player == grid[1,i].Player && grid[1,i].Player == grid[2,i].Player)
            {
                isGameOver = true;
                winner = grid[0,i].Player;
            }
        }

        //diagonally
        if (grid[0, 0].Player != Player.Empty && grid[0, 0].Player == grid[1, 1].Player && grid[1, 1].Player == grid[2, 2].Player)
        {
            isGameOver = true;
            winner = grid[0, 0].Player;
        }

        if (grid[0, 2].Player != Player.Empty && grid[0, 2].Player == grid[1, 1].Player && grid[1, 1].Player == grid[2, 0].Player)
        {
            isGameOver = true;
            winner = grid[0, 2].Player;
        }
    }


    // methods implementing AI
    public Vector2Int findOptimalMove ()
    {
        Cell[,] tempGrid = grid;
        int bestValue = -1000;
        int rowValue = 0;
        int colValue = 0;

        // check the grid and find the best next move
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (tempGrid[i, j].Player == Player.Empty)
                {
                    // we make a guess and AI decided to choose this 
                    tempGrid[i, j].Player = Player.X; // make a move AI is only X???

                     // calculate the cost of this step using the minimax
                    int costMove = minimax(tempGrid, 0, false);

                    //remove this step
                    tempGrid[i, j].Player = Player.Empty;

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
    private int minimax (Cell[,] tempGrid,   //I am not sure what type tempGrid is
                   int depth, Boolean isMax)
    {
        int score = totalscorecheck(tempGrid);

        // maximizer won the game  
        if (score == 10)
            return score;

        // minimizer won the game   
        if (score == -10)
            return score;

        // no moves left and no winner 
        // ? how to check
        if(HasEmptyCell(tempGrid)) // maybe like this
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
                    if (tempGrid[i, j].Player == Player.Empty)
                    {
                        // guess the step
                        tempGrid[i, j].Player = Player.X;

                        // minimax recursively to compute maximum value 
                        best = Math.Max (best, minimax (tempGrid, depth + 1, !isMax));

                        // back this step
                        tempGrid[i, j].Player = Player.Empty;
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
                    if (tempGrid[i, j].Player == Player.Empty)
                    {
                        // Make a guess
                        tempGrid[i, j].Player = Player.O;

                        // minimax recursively to compute minimum value 
                        best = Math.Min (best, minimax (tempGrid, depth + 1, !isMax));

                        // back this step
                        tempGrid[i, j].Player = Player.Empty;
                    }
                }
            }
            return best;
        }
    }

    private int totalscorecheck (Cell[,] tempGrid)
    {
        // Rows - X or O win
        for (int row = 0; row < 3; row++)
        {
            if (tempGrid[row, 0].Player == tempGrid[row, 1].Player && tempGrid[row, 1].Player == tempGrid[row, 2].Player)
            {
                if (tempGrid[row, 0].Player == Player.X)
                    return +10;
                else if (tempGrid[row, 0].Player == Player.O)
                    return -10;
            }
        }

        // Columns - X or O win
        for (int col = 0; col < 3; col++)
        {
            if (tempGrid[0, col].Player == tempGrid[1, col].Player && tempGrid[1, col].Player == tempGrid[2, col].Player)
            {
                if (tempGrid[0, col].Player == Player.X)
                    return +10;

                else if (tempGrid[0, col].Player == Player.O)
                    return -10;
            }
        }

        // Diagonals - X or O win
        if (tempGrid[0, 0].Player == tempGrid[1, 1].Player && tempGrid[1, 1].Player == tempGrid[2, 2].Player)
        {
            if (tempGrid[0, 0].Player == Player.X)
                return +10;
            else if (tempGrid[0, 0].Player == Player.O)
                return -10;
        }

        if (tempGrid[0, 2].Player == tempGrid[1, 1].Player && tempGrid[1, 1].Player == tempGrid[2, 0].Player)
        {
            if (tempGrid[0, 2].Player == Player.X)
                return +10;
            else if (tempGrid[0, 2].Player == Player.O)
                return -10;
        }

        // Elif 
        return 0;
    }

    private bool HasEmptyCell (Cell[,] grid)
    {
        foreach (Cell cell in grid)
        {
            if (cell.Player == Player.Empty)
                return true;
        }

        return false;
    }
}



