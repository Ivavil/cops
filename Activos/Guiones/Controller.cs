﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    //GameObjects
    public GameObject board;
    public GameObject[] cops = new GameObject[2];
    public GameObject robber;
    public Text rounds;
    public Text finalMessage;
    public Button playAgainButton;

    //Otras variables
    Tile[] tiles = new Tile[Constants.NumTiles];
    private int roundCount = 0;
    private int state;
    private int clickedTile = -1;
    private int clickedCop = 0;
                    
    void Start()
    {        
        InitTiles();
        InitAdjacencyLists();
        state = Constants.Init;
    }
        
    //Rellenamos el array de casillas y posicionamos las fichas
    void InitTiles()
    {
        for (int fil = 0; fil < Constants.TilesPerRow; fil++)
        {
            GameObject rowchild = board.transform.GetChild(fil).gameObject;            

            for (int col = 0; col < Constants.TilesPerRow; col++)
            {
                GameObject tilechild = rowchild.transform.GetChild(col).gameObject;                
                tiles[fil * Constants.TilesPerRow + col] = tilechild.GetComponent<Tile>();                         
            }
        }
                
        cops[0].GetComponent<CopMove>().currentTile=Constants.InitialCop0;
        cops[1].GetComponent<CopMove>().currentTile=Constants.InitialCop1;
        robber.GetComponent<RobberMove>().currentTile=Constants.InitialRobber;           
    }

    public void InitAdjacencyLists()
    {
        //Matriz de adyacencia
        int[,] matriu = new int[Constants.NumTiles, Constants.NumTiles];

        //TODO: Inicializar matriz a 0's
        for(int i = 0; i < Constants.NumTiles; i++)
        {
            for (int j = 0; j < Constants.NumTiles; j++)
            {
                matriu[i, j] = 0;
            }
        }
        //TODO: Para cada posición, rellenar con 1's las casillas adyacentes (arriba, abajo, izquierda y derecha)
        //TODO: Rellenar la lista "adjacency" de cada casilla con los índices de sus casillas adyacentes
        for (int i = 0; i < Constants.NumTiles; i++)
        {
            if((i + 8) < 64) {
                tiles[i].adjacency.Add(i + 8);
                matriu[i, i + 8] = 1;
                
                
            }

            if ((i + 1) < 64 && i != 7 && i != 15 && i != 23 && i != 31 && i != 39 && i != 47 && i != 55 && i != 63)
            {
                tiles[i].adjacency.Add(i + 1);
                matriu[i, i + 1] = 1;
                
            }

            if ((i - 8) > -1) {
                tiles[i].adjacency.Add(i - 8);
                matriu[i, i - 8] = 1;
                
            }

            if ((i - 1) > -1 && i != 8 && i != 16 && i != 24 && i != 32 && i != 40 && i != 48 && i != 56)
            {
                tiles[i].adjacency.Add(i - 1);
                matriu[i, i - 1] = 1;
                
            }
        }
    }

    //Reseteamos cada casilla: color, padre, distancia y visitada
    public void ResetTiles()
    {        
        foreach (Tile tile in tiles)
        {
            tile.Reset();
        }
    }

    public void ClickOnCop(int cop_id)
    {
        switch (state)
        {
            case Constants.Init:
            case Constants.CopSelected:                
                clickedCop = cop_id;
                clickedTile = cops[cop_id].GetComponent<CopMove>().currentTile;
                tiles[clickedTile].current = true;

                ResetTiles();
                FindSelectableTiles(true);

                state = Constants.CopSelected;                
                break;            
        }
    }

    public void ClickOnTile(int t)
    {                     
        clickedTile = t;

        switch (state)
        {            
            case Constants.CopSelected:
                //Si es una casilla roja, nos movemos
                if (tiles[clickedTile].selectable)
                {                  
                    cops[clickedCop].GetComponent<CopMove>().MoveToTile(tiles[clickedTile]);
                    cops[clickedCop].GetComponent<CopMove>().currentTile=tiles[clickedTile].numTile;
                    tiles[clickedTile].current = true;   
                    
                    state = Constants.TileSelected;
                }                
                break;
            case Constants.TileSelected:
                state = Constants.Init;
                break;
            case Constants.RobberTurn:
                state = Constants.Init;
                break;
        }
    }

    public void FinishTurn()
    {
        switch (state)
        {            
            case Constants.TileSelected:
                ResetTiles();

                state = Constants.RobberTurn;
                RobberTurn();
                break;
            case Constants.RobberTurn:                
                ResetTiles();
                IncreaseRoundCount();
                if (roundCount <= Constants.MaxRounds)
                    state = Constants.Init;
                else
                    EndGame(false);
                break;
        }

    }

    public void RobberTurn()
    {
        clickedTile = robber.GetComponent<RobberMove>().currentTile;
        tiles[clickedTile].current = true;
        FindSelectableTiles(false);

        /*TODO: Cambia el código de abajo para hacer lo siguiente
        - Elegimos una casilla aleatoria entre las seleccionables que puede ir el caco
        - Movemos al caco a esa casilla
        - Actualizamos la variable currentTile del caco a la nueva casilla
        */
        int rand = Random.Range(0, tiles[clickedTile].adjacency.Count);
        clickedTile = tiles[clickedTile].adjacency[rand];
        tiles[clickedTile].current = true;
        robber.GetComponent<RobberMove>().currentTile = clickedTile;
        robber.GetComponent<RobberMove>().MoveToTile(tiles[robber.GetComponent<RobberMove>().currentTile]);
    }

    public void EndGame(bool end)
    {
        if(end)
            finalMessage.text = "You Win!";
        else
            finalMessage.text = "You Lose!";
        playAgainButton.interactable = true;
        state = Constants.End;
    }

    public void PlayAgain()
    {
        cops[0].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop0]);
        cops[1].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop1]);
        robber.GetComponent<RobberMove>().Restart(tiles[Constants.InitialRobber]);
                
        ResetTiles();

        playAgainButton.interactable = false;
        finalMessage.text = "";
        roundCount = 0;
        rounds.text = "Rounds: ";

        state = Constants.Restarting;
    }

    public void InitGame()
    {
        state = Constants.Init;
         
    }

    public void IncreaseRoundCount()
    {
        roundCount++;
        rounds.text = "Rounds: " + roundCount;
    }

    public void FindSelectableTiles(bool cop)
    {
                 
        int indexcurrentTile;        

        if (cop==true)
            indexcurrentTile = cops[clickedCop].GetComponent<CopMove>().currentTile;
        else
            indexcurrentTile = robber.GetComponent<RobberMove>().currentTile;

        //La ponemos rosa porque acabamos de hacer un reset
        tiles[indexcurrentTile].current = true;

        //Cola para el BFS
        Queue<Tile> nodes = new Queue<Tile>();

        //TODO: Implementar BFS. Los nodos seleccionables los ponemos como selectable=true
        //Tendrás que cambiar este código por el BFS
        if (!repeated_tiles.Contains(indexcurrentTile))
        {
            BFS(indexcurrentTile);    
        }
        for (int j = 0; j < tiles[indexcurrentTile].adjacency.Count; j++)
        {
            tiles[tiles[indexcurrentTile].adjacency[j]].selectable = true;
        }
        tiles[cops[1].GetComponent<CopMove>().currentTile].selectable = false;
        tiles[cops[0].GetComponent<CopMove>().currentTile].selectable = false;
        if (cop)
        {
            
            if(clickedCop == 1)
            {
                if (cops[0].GetComponent<CopMove>().currentTile == cops[1].GetComponent<CopMove>().currentTile + 1)
                {
                    tiles[indexcurrentTile + 2].selectable = false;
                }
                else if (cops[0].GetComponent<CopMove>().currentTile == cops[1].GetComponent<CopMove>().currentTile + 8)
                {
                    tiles[indexcurrentTile + 16].selectable = false;
                }
                else if (cops[0].GetComponent<CopMove>().currentTile == cops[1].GetComponent<CopMove>().currentTile - 1)
                {
                    tiles[indexcurrentTile - 2].selectable = false;
                }
                else if (cops[0].GetComponent<CopMove>().currentTile == cops[1].GetComponent<CopMove>().currentTile - 8)
                {
                    tiles[indexcurrentTile - 16].selectable = false;
                }
            }
            else
            {
                if (cops[1].GetComponent<CopMove>().currentTile == cops[0].GetComponent<CopMove>().currentTile + 1)
                {
                    tiles[indexcurrentTile + 2].selectable = false;
                }
                else if (cops[1].GetComponent<CopMove>().currentTile == cops[0].GetComponent<CopMove>().currentTile + 8)
                {
                    tiles[indexcurrentTile + 16].selectable = false;
                }
                else if (cops[1].GetComponent<CopMove>().currentTile == cops[0].GetComponent<CopMove>().currentTile - 1)
                {
                    tiles[indexcurrentTile - 2].selectable = false;
                }
                else if (cops[1].GetComponent<CopMove>().currentTile == cops[0].GetComponent<CopMove>().currentTile - 8)
                {
                    tiles[indexcurrentTile - 16].selectable = false;
                }
            }

        }
        
        if (!cop)
        {
            tiles[indexcurrentTile].selectable = false;
        }

    }
    
    public void BFS(int indexcurrentTile)
    {
       
        int first_size = tiles[indexcurrentTile].adjacency.Count;
        for (int j = 0; j < first_size; j++)
        {
            
            if ((tiles[indexcurrentTile].adjacency[j] + 8) < 64 && !tiles[indexcurrentTile].adjacency.Contains(tiles[indexcurrentTile].adjacency[j] + 8))
            {
                tiles[indexcurrentTile].adjacency.Add(tiles[indexcurrentTile].adjacency[j] + 8);
               
            }

            if ((tiles[indexcurrentTile].adjacency[j] + 1) < 64 && tiles[indexcurrentTile].adjacency[j] != 7 && tiles[indexcurrentTile].adjacency[j] != 15 && tiles[indexcurrentTile].adjacency[j] != 23 && tiles[indexcurrentTile].adjacency[j] != 31 && tiles[indexcurrentTile].adjacency[j] != 39 && tiles[indexcurrentTile].adjacency[j] != 47 && tiles[indexcurrentTile].adjacency[j] != 55 && tiles[indexcurrentTile].adjacency[j] != 63 && !tiles[indexcurrentTile].adjacency.Contains(tiles[indexcurrentTile].adjacency[j] + 1))
            {
                tiles[indexcurrentTile].adjacency.Add(tiles[indexcurrentTile].adjacency[j] + 1);
                
            }

            if ((tiles[indexcurrentTile].adjacency[j] - 8) > -1 && !tiles[indexcurrentTile].adjacency.Contains(tiles[indexcurrentTile].adjacency[j] - 8))
            {
                tiles[indexcurrentTile].adjacency.Add(tiles[indexcurrentTile].adjacency[j] - 8);
                
            }

            if ((tiles[indexcurrentTile].adjacency[j] - 1) > -1 && tiles[indexcurrentTile].adjacency[j] != 8 && tiles[indexcurrentTile].adjacency[j] != 16 && tiles[indexcurrentTile].adjacency[j] != 24 && tiles[indexcurrentTile].adjacency[j] != 32 && tiles[indexcurrentTile].adjacency[j] != 40 && tiles[indexcurrentTile].adjacency[j] != 48 && tiles[indexcurrentTile].adjacency[j] != 56 && !tiles[indexcurrentTile].adjacency.Contains(tiles[indexcurrentTile].adjacency[j] - 1))
            {
                tiles[indexcurrentTile].adjacency.Add(tiles[indexcurrentTile].adjacency[j] - 1);
               
            } 
        }
        tiles[indexcurrentTile].adjacency.Remove(indexcurrentTile);
        repeated_tiles.Add(indexcurrentTile);
    }
    

    

   

       
}
