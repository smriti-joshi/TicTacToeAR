using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


enum Player
{
    Empty, X, O
}

public class Cell
{
    Player player;
    Vector3 center;

    public Cell()
    {
        this.player = Player.Empty;
    }
}


public class GameLogic
{
    static GameLogic instance = new GameLogic ();

    public Cell[] grid = new Cell[9];
    private int round = 0;
    private bool isGameOver = false;

    public int a;

    private GameLogic ()
    {

    }

    public static GameLogic GetInstance ()
    {
        return instance;
    }

    public void InitGrid (Vector3 gridCenter, Vector3 gridSize)
    {

    }

    public void PlaceZeroOrCross(Vector3 pos, bool isCross)
    {
        if (round >= 9)
        {
            isGameOver = true;
            return;
        }

        round++;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }
}



