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

    public bool PlaceZeroOrCross(GameObject obj, bool isCross)
    {
        float minDistance = 1000.0f;
        Cell selectedCell = new Cell();
        foreach (Cell cell in grid)
        {
            float currentDistance = Vector3.Distance(cell.Center, obj.transform.position);
            if (currentDistance < minDistance && cell.Player == Player.Empty)
            {
                minDistance = currentDistance;
                selectedCell = cell;
            }
        }

        if (minDistance > cellWidth / 2)
        {
            GameObject.Destroy (obj);
            return false;
        }

        obj.transform.SetPositionAndRotation (selectedCell.Center, obj.transform.rotation);
        selectedCell.Player = isCross ? Player.X : Player.O;

        round++;

        CheckIfGameOver ();

        return true;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public Player WhoWon()
    {
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
}



