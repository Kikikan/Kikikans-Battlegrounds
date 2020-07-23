using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    internal UIManager ui;
    private MultiplayerManager mManager;

    public GameObject TilePrefab;
    public GameObject Map;

    public Sprite border;
    public Sprite borderWin;
    public Sprite borderLose;
    public Sprite borderLast;

    private SpriteRenderer last;
    internal SpriteRenderer Last { get { return last; } set
        {
            if (last != null)
                last.sprite = border;
            last = value;
            last.sprite = borderLast;
        }
    }


    internal bool firstPlaced = false;

    internal float startingTime = 0;
    internal float matchTime = 0;

    private bool gameOngoing = true;

    internal bool GameOngoing { get { return gameOngoing; } set
        {
            if (!mManager.isMultiplayer)
            {
                ui.SetWin(isX);
                gameOngoing = value;
            }
            else
            {
                gameOngoing = value;
                matchTime = Time.time - startingTime;
                mManager.connection.Send("coin", matchTime);
                if (!gameOngoing)
                ui.MultiplayerPanel.SetActive(true);
            }

        }
    }

    private int startingX;
    private int startingY;

    public bool?[,] board = new bool?[1000, 1000];
    public bool[,] generatedBoard = new bool[1000, 1000];
    public List<GameObject> tiles = new List<GameObject>();

    public bool isX = true;

    // Use this for initialization
    void Start () {
        ui = FindObjectOfType<UIManager>();
        mManager = FindObjectOfType<MultiplayerManager>();
        mManager.AddManager(this);
        //GenerateTiles();
    }

    void Update()
    {
        if (mManager != null)
            if (Input.GetKeyDown(KeyCode.R) && !mManager.isMultiplayer)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    public bool CheckNewWin(int x, int y, bool won = false)
    {

        List<GameObject> winnerTiles = new List<GameObject>();
        if (!mManager.isMultiplayer)
        {
            for (int i = -4; i < 5; i++)
            {
                if (x + i >= 0)
                    if (board[x + i, y] == isX)
                    {
                        winnerTiles = tiles.Where(foo => foo.transform.position.x >= x + i && foo.transform.position.x < x + i + 5 && foo.transform.position.y == y).ToList();
                        for (int j = 0; j < 5; j++)
                        {
                            if (board[x + i + j, y] == isX)
                            {
                                if (j == 4)
                                {
                                    foreach (var item in winnerTiles)
                                    {
                                        item.GetComponent<SpriteRenderer>().sprite = borderWin;
                                    }
                                    return true;
                                }
                            }
                            else
                            {
                                winnerTiles = null;
                                break;
                            }
                        }
                    }
                winnerTiles = null;
            }
            winnerTiles = null;

            for (int i = -4; i < 5; i++)
            {
                if (y + i >= 0)
                    if (board[x, y + i] == isX)
                    {
                        winnerTiles = tiles.Where(foo => foo.transform.position.y >= y + i && foo.transform.position.y < y + i + 5 && foo.transform.position.x == x).ToList();
                        for (int j = 0; j < 5; j++)
                        {
                            if (board[x, y + i + j] == isX)
                            {
                                if (j == 4)
                                {
                                    foreach (var item in winnerTiles)
                                    {
                                        item.GetComponent<SpriteRenderer>().sprite = borderWin;
                                    }
                                    return true;
                                }
                            }
                            else
                            {
                                winnerTiles = null;
                                break;
                            }
                        }
                    }
                winnerTiles = null;
            }
            winnerTiles = null;

            for (int i = -4; i < 5; i++)
            {
                if (y + i >= 0 && x + i >= 0)
                    if (board[x + i, y + i] == isX)
                    {
                        winnerTiles = tiles.Where(foo => (foo.transform.position.x == x + i && foo.transform.position.y == y + i) || (foo.transform.position.x == x + i + 1 && foo.transform.position.y == y + i + 1) || (foo.transform.position.x == x + i + 2 && foo.transform.position.y == y + i + 2) || (foo.transform.position.x == x + i + 3 && foo.transform.position.y == y + i + 3) || (foo.transform.position.x == x + i + 4 && foo.transform.position.y == y + i + 4)).ToList();
                        for (int j = 0; j < 5; j++)
                        {
                            if (board[x + i + j, y + i + j] == isX)
                            {
                                if (j == 4)
                                {
                                    foreach (var item in winnerTiles)
                                    {
                                        item.GetComponent<SpriteRenderer>().sprite = borderWin;
                                    }
                                    return true;
                                }
                            }
                            else
                            {
                                winnerTiles = null;
                                break;
                            }
                        }
                    }
                winnerTiles = null;
            }

            for (int i = -4; i < 5; i++)
            {
                if (y - i >= 0 && x + i >= 0)
                    if (board[x + i, y - i] == isX)
                    {
                        winnerTiles = tiles.Where(foo => (foo.transform.position.x == x + i && foo.transform.position.y == y - i) || (foo.transform.position.x == x + i + 1 && foo.transform.position.y == y - i - 1) || (foo.transform.position.x == x + i + 2 && foo.transform.position.y == y - i - 2) || (foo.transform.position.x == x + i + 3 && foo.transform.position.y == y - i - 3) || (foo.transform.position.x == x + i + 4 && foo.transform.position.y == y - i - 4)).ToList();
                        for (int j = 0; j < 5; j++)
                        {
                            if (board[x + i + j, y - i - j] == isX)
                            {
                                if (j == 4)
                                {
                                    foreach (var item in winnerTiles)
                                    {
                                        item.GetComponent<SpriteRenderer>().sprite = borderWin;
                                    }
                                    return true;
                                }
                            }
                            else
                            {
                                winnerTiles = null;
                                break;
                            }
                        }
                    }
                winnerTiles = null;

            }
                return false;
            
        }
        else
        {
            for (int i = -4; i < 5; i++)
            {
                if (x + i >= 0)
                    if (board[x + i, y] == won)
                    {
                        winnerTiles = tiles.Where(foo => foo.transform.position.x >= x + i && foo.transform.position.x < x + i + 5 && foo.transform.position.y == y).ToList();
                        for (int j = 0; j < 5; j++)
                        {
                            if (board[x + i + j, y] == won)
                            {
                                if (j == 4)
                                {
                                    foreach (var item in winnerTiles)
                                    {
                                        if (won)
                                            item.GetComponent<SpriteRenderer>().sprite = borderWin;
                                        else
                                            item.GetComponent<SpriteRenderer>().sprite = borderLose;
                                    }
                                    return true;
                                }
                            }
                            else
                            {
                                winnerTiles = null;
                                break;
                            }
                        }
                    }
                winnerTiles = null;
            }
            winnerTiles = null;

            for (int i = -4; i < 5; i++)
            {
                if (y + i >= 0)
                    if (board[x, y + i] == won)
                    {
                        winnerTiles = tiles.Where(foo => foo.transform.position.y >= y + i && foo.transform.position.y < y + i + 5 && foo.transform.position.x == x).ToList();
                        for (int j = 0; j < 5; j++)
                        {
                            if (board[x, y + i + j] == won)
                            {
                                if (j == 4)
                                {
                                    foreach (var item in winnerTiles)
                                    {
                                        if (won)
                                            item.GetComponent<SpriteRenderer>().sprite = borderWin;
                                        else
                                            item.GetComponent<SpriteRenderer>().sprite = borderLose;
                                    }
                                    return true;
                                }
                            }
                            else
                            {
                                winnerTiles = null;
                                break;
                            }
                        }
                    }
                winnerTiles = null;
            }
            winnerTiles = null;

            for (int i = -4; i < 5; i++)
            {
                if (y + i >= 0 && x + i >= 0)
                    if (board[x + i, y + i] == won)
                    {
                        winnerTiles = tiles.Where(foo => (foo.transform.position.x == x + i && foo.transform.position.y == y + i) || (foo.transform.position.x == x + i + 1 && foo.transform.position.y == y + i + 1) || (foo.transform.position.x == x + i + 2 && foo.transform.position.y == y + i + 2) || (foo.transform.position.x == x + i + 3 && foo.transform.position.y == y + i + 3) || (foo.transform.position.x == x + i + 4 && foo.transform.position.y == y + i + 4)).ToList();
                        for (int j = 0; j < 5; j++)
                        {
                            if (board[x + i + j, y + i + j] == won)
                            {
                                if (j == 4)
                                {
                                    foreach (var item in winnerTiles)
                                    {
                                        if (won)
                                            item.GetComponent<SpriteRenderer>().sprite = borderWin;
                                        else
                                            item.GetComponent<SpriteRenderer>().sprite = borderLose;
                                    }
                                    return true;
                                }
                            }
                            else
                            {
                                winnerTiles = null;
                                break;
                            }
                        }
                    }
                winnerTiles = null;
            }

            for (int i = -4; i < 5; i++)
            {
                if (y - i >= 0 && x + i >= 0)
                    if (board[x + i, y - i] == won)
                    {
                        winnerTiles = tiles.Where(foo => (foo.transform.position.x == x + i && foo.transform.position.y == y - i) || (foo.transform.position.x == x + i + 1 && foo.transform.position.y == y - i - 1) || (foo.transform.position.x == x + i + 2 && foo.transform.position.y == y - i - 2) || (foo.transform.position.x == x + i + 3 && foo.transform.position.y == y - i - 3) || (foo.transform.position.x == x + i + 4 && foo.transform.position.y == y - i - 4)).ToList();
                        for (int j = 0; j < 5; j++)
                        {
                            if (board[x + i + j, y - i - j] == won)
                            {
                                if (j == 4)
                                {
                                    foreach (var item in winnerTiles)
                                    {
                                        if (won)
                                            item.GetComponent<SpriteRenderer>().sprite = borderWin;
                                        else
                                            item.GetComponent<SpriteRenderer>().sprite = borderLose;
                                    }
                                    return true;
                                }
                            }
                            else
                            {
                                winnerTiles = null;
                                break;
                            }
                        }
                    }
                winnerTiles = null;

            }
            return false;
        }
    }

    public void GenerateTiles(int x, int y)
    {
        if (!generatedBoard[x - 1, y])
        {
            var newTile = Instantiate(TilePrefab);
            newTile.transform.SetParent(Map.transform);
            newTile.transform.position = new Vector2(x - 1, y);
            generatedBoard[x - 1, y] = true;
        }
        if (!generatedBoard[x - 1, y - 1])
        {
            var newTile = Instantiate(TilePrefab);
            newTile.transform.SetParent(Map.transform);
            newTile.transform.position = new Vector2(x - 1, y - 1);
            generatedBoard[x - 1, y - 1] = true;
        }
        if (!generatedBoard[x - 1, y + 1])
        {
            var newTile = Instantiate(TilePrefab);
            newTile.transform.SetParent(Map.transform);
            newTile.transform.position = new Vector2(x - 1, y + 1);
            generatedBoard[x - 1, y + 1] = true;
        }
        if (!generatedBoard[x, y - 1])
        {
            var newTile = Instantiate(TilePrefab);
            newTile.transform.SetParent(Map.transform);
            newTile.transform.position = new Vector2(x, y - 1);
            generatedBoard[x, y - 1] = true;
        }
        if (!generatedBoard[x, y + 1])
        {
            var newTile = Instantiate(TilePrefab);
            newTile.transform.SetParent(Map.transform);
            newTile.transform.position = new Vector2(x, y + 1);
            generatedBoard[x, y + 1] = true;
        }
        if (!generatedBoard[x + 1, y + 1])
        {
            var newTile = Instantiate(TilePrefab);
            newTile.transform.SetParent(Map.transform);
            newTile.transform.position = new Vector2(x + 1, y + 1);
            generatedBoard[x + 1, y + 1] = true;
        }
        if (!generatedBoard[x + 1, y])
        {
            var newTile = Instantiate(TilePrefab);
            newTile.transform.SetParent(Map.transform);
            newTile.transform.position = new Vector2(x + 1, y);
            generatedBoard[x + 1, y] = true;
        }
        if (!generatedBoard[x + 1, y - 1])
        {
            var newTile = Instantiate(TilePrefab);
            newTile.transform.SetParent(Map.transform);
            newTile.transform.position = new Vector2(x + 1, y - 1);
            generatedBoard[x + 1, y - 1] = true;
        }
    }



    
}
