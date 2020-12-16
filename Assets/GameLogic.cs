using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;

enum Player
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
    internal Player Player { get => player; set => player = value; }
}


public class GameLogic
{
    static GameLogic instance = new GameLogic ();

    private Cell[] grid = new Cell[9];
    private GameObject zeroToPlace;
    private Quaternion rotation { get; set; }

    private int round = 1;
    private bool isGameOver = false;
    private float cellWidth;

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

        grid[0] = new Cell (new Vector3 (gridCenter.x - cellWidth, gridCenter.y, gridCenter.z - cellWidth));
        grid[1] = new Cell (new Vector3 (gridCenter.x            , gridCenter.y, gridCenter.z - cellWidth));
        grid[2] = new Cell (new Vector3 (gridCenter.x + cellWidth, gridCenter.y, gridCenter.z - cellWidth));
        grid[3] = new Cell (new Vector3 (gridCenter.x - cellWidth, gridCenter.y, gridCenter.z            ));
        grid[4] = new Cell (new Vector3 (gridCenter.x            , gridCenter.y, gridCenter.z            ));
        grid[5] = new Cell (new Vector3 (gridCenter.x + cellWidth, gridCenter.y, gridCenter.z            ));
        grid[6] = new Cell (new Vector3 (gridCenter.x - cellWidth, gridCenter.y, gridCenter.z + cellWidth));
        grid[7] = new Cell (new Vector3 (gridCenter.x            , gridCenter.y, gridCenter.z + cellWidth));
        grid[8] = new Cell (new Vector3 (gridCenter.x + cellWidth, gridCenter.y, gridCenter.z + cellWidth));

        foreach(Cell cell in grid){
            PlacementTest (obj, cell, rotation);
        }
    }

    public void PlaceZeroOrCross(GameObject obj, bool isCross)
    {
        if (round > 9)
        {
            isGameOver = true;
            return;
        }

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

        obj.transform.SetPositionAndRotation (selectedCell.Center, obj.transform.rotation);
        selectedCell.Player = isCross ? Player.X : Player.O;

        round++;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    void PlacementTest (GameObject obj, Cell cell, Quaternion rot)
    {
        Vector3 offset = new Vector3 (0.0f, 0.25f, -0.0f);
        obj.transform.SetPositionAndRotation (cell.Center + offset, rot);
        obj.transform.RotateAround (grid[4].Center, Vector3.up, rot.eulerAngles.y);
        cell.Center = obj.transform.position;
    }

    public void RestartGame()
    {
        isGameOver = false;
        round = 1;

        foreach (Cell cell in grid)
        {
            cell.Player = Player.Empty;
        }
    }
}



