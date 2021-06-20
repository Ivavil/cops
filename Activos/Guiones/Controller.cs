using System.Collections;
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
    private List<int> repeated_tiles = new List<int>();
                    
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
         List<int> position_cop_1 = getPosition(1, true);
        List<int> position_cop_0 = getPosition(0, true);
        //List<int> position_robber = getPosition(0, false);
        List<int[]> possible_pos = getPossiblePositions(clickedTile);
        float distance = 1f;
        int x = 0;
        int y = 0;
        for (int i = 0; i < possible_pos.Count; i++)
        {
            if (distance < Mathf.Sqrt(Mathf.Pow(position_cop_1[0] - possible_pos[i][0], 2) + Mathf.Pow(position_cop_1[1] - possible_pos[i][1], 2)))
            {
                distance = Mathf.Sqrt(Mathf.Pow(position_cop_1[0] - possible_pos[i][0], 2) + Mathf.Pow(position_cop_1[1] - possible_pos[i][1], 2));
                clickedTile = convertPositionToTile(possible_pos[i]);
                x = possible_pos[i][0];
                y = possible_pos[i][1];
            }   
        }
        Debug.Log("Dsitanca ,as alejado " + distance);
        Debug.Log("Casilla mas alejada del poli1 " + clickedTile);
        Debug.Log(x + " " + y);
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
    
    public List<int> getPosition(int cop, bool who)
    {
        List<int> position = new List<int>();
        int pos_x = 0;
        int pos_y = 0;
        if (who)
        {
            if (cops[cop].GetComponent<CopMove>().currentTile >= 0 && cops[cop].GetComponent<CopMove>().currentTile <= 7)
            {
                pos_y = 0;
                pos_x = cops[cop].GetComponent<CopMove>().currentTile;
            }
            else if (cops[cop].GetComponent<CopMove>().currentTile >= 8 && cops[cop].GetComponent<CopMove>().currentTile <= 15)
            {
                pos_y = 1;
                pos_x = cops[cop].GetComponent<CopMove>().currentTile - 8;
            }
            else if (cops[cop].GetComponent<CopMove>().currentTile >= 16 && cops[cop].GetComponent<CopMove>().currentTile <= 23)
            {
                pos_y = 2;
                pos_x = cops[cop].GetComponent<CopMove>().currentTile - 16;
            }
            else if (cops[cop].GetComponent<CopMove>().currentTile >= 24 && cops[cop].GetComponent<CopMove>().currentTile <= 31)
            {
                pos_y = 3;
                pos_x = cops[cop].GetComponent<CopMove>().currentTile - 24;
            }
            else if (cops[cop].GetComponent<CopMove>().currentTile >= 32 && cops[cop].GetComponent<CopMove>().currentTile <= 39)
            {
                pos_y = 4;
                pos_x = cops[cop].GetComponent<CopMove>().currentTile - 32;
            }
            else if (cops[cop].GetComponent<CopMove>().currentTile >= 40 && cops[cop].GetComponent<CopMove>().currentTile <= 47)
            {
                pos_y = 5;
                pos_x = cops[cop].GetComponent<CopMove>().currentTile - 40;
            }
            else if (cops[cop].GetComponent<CopMove>().currentTile >= 48 && cops[cop].GetComponent<CopMove>().currentTile <= 55)
            {
                pos_y = 6;
                pos_x = cops[cop].GetComponent<CopMove>().currentTile - 48;
            }
            else if (cops[cop].GetComponent<CopMove>().currentTile >= 56 && cops[cop].GetComponent<CopMove>().currentTile <= 63)
            {
                pos_y = 7;
                pos_x = cops[cop].GetComponent<CopMove>().currentTile - 56;
            }
        }
        else
        {
            if (robber.GetComponent<RobberMove>().currentTile >= 0 && robber.GetComponent<RobberMove>().currentTile <= 7)
            {
                pos_y = 0;
                pos_x = robber.GetComponent<RobberMove>().currentTile;
            }
            else if (robber.GetComponent<RobberMove>().currentTile >= 8 && robber.GetComponent<RobberMove>().currentTile <= 15)
            {
                pos_y = 1;
                pos_x = robber.GetComponent<RobberMove>().currentTile - 8;
            }
            else if (robber.GetComponent<RobberMove>().currentTile >= 16 && robber.GetComponent<RobberMove>().currentTile <= 23)
            {
                pos_y = 2;
                pos_x = robber.GetComponent<RobberMove>().currentTile - 16;
            }
            else if (robber.GetComponent<RobberMove>().currentTile >= 24 && robber.GetComponent<RobberMove>().currentTile <= 31)
            {
                pos_y = 3;
                pos_x = robber.GetComponent<RobberMove>().currentTile - 24;
            }
            else if (robber.GetComponent<RobberMove>().currentTile >= 32 && robber.GetComponent<RobberMove>().currentTile <= 39)
            {
                pos_y = 4;
                pos_x = robber.GetComponent<RobberMove>().currentTile - 32;
            }
            else if (robber.GetComponent<RobberMove>().currentTile >= 40 && robber.GetComponent<RobberMove>().currentTile <= 47)
            {
                pos_y = 5;
                pos_x = robber.GetComponent<RobberMove>().currentTile - 40;
            }
            else if (robber.GetComponent<RobberMove>().currentTile >= 48 && robber.GetComponent<RobberMove>().currentTile <= 55)
            {
                pos_y = 6;
                pos_x = robber.GetComponent<RobberMove>().currentTile - 48;
            }
            else if (robber.GetComponent<RobberMove>().currentTile >= 56 && robber.GetComponent<RobberMove>().currentTile <= 63)
            {
                pos_y = 7;
                pos_x = robber.GetComponent<RobberMove>().currentTile - 56;
            }
        }

        position.Add(pos_x);
        position.Add(pos_y);
        return position;
    }

    public List<int[]> getPossiblePositions(int clickedTile)
    {
        List<int[]> list_positions = new List<int[]>();
        List<int> position = new List<int>();
        
        foreach (int possibleTile in tiles[clickedTile].adjacency)
        {
            int pos_x = 0;
            int pos_y = 0;
            int[] posicion = new int[2];
            if (possibleTile >= 0 && possibleTile <= 7)
            {
                pos_y = 0;
                pos_x = possibleTile;
            }
            else if (possibleTile >= 8 && possibleTile <= 15)
            {
                pos_y = 1;
                pos_x = possibleTile - 8;
            }
            else if (possibleTile >= 16 && possibleTile <= 23)
            {
                pos_y = 2;
                pos_x = possibleTile - 16;
            }
            else if (possibleTile >= 24 && possibleTile <= 31)
            {
                pos_y = 3;
                pos_x = possibleTile - 24;
            }
            else if (possibleTile >= 32 && possibleTile <= 39)
            {
                pos_y = 4;
                pos_x = possibleTile - 32;
            }
            else if (possibleTile >= 40 && possibleTile <= 47)
            {
                pos_y = 5;
                pos_x = possibleTile - 40;
            }
            else if (possibleTile >= 48 && possibleTile <= 55)
            {
                pos_y = 6;
                pos_x = possibleTile - 48;
            }
            else if (possibleTile >= 56 && possibleTile <= 63)
            {
                pos_y = 7;
                pos_x = possibleTile - 56;
            }
            posicion[0] = pos_x;
            posicion[1] = pos_y;
            list_positions.Add(posicion);
        }
        return list_positions;
    }

    public int convertPositionToTile(int[] position)
    {
        int tile = position[0] + 8 * position[1];
        Debug.Log("Antes de coinvertir" + position[0] + " " + position[1]);
        return tile;
    }

    

   

       
}
