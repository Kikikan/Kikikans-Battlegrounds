using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TileManager : MonoBehaviour {

    private bool alreadyClicked = false;
    public SpriteRenderer srenderer;

    private GameManager manager;
    private MultiplayerManager mManager;
    private InventoryManager iManager;

    public Sprite X;
    public Sprite O;

    // Use this for initialization
    void Start () {
        manager = FindObjectOfType<GameManager>();
        if (SceneManager.GetActiveScene().buildIndex == 0)
            enabled = false;
        mManager = FindObjectOfType<MultiplayerManager>();
        iManager = FindObjectOfType<InventoryManager>();
        if (SceneManager.GetActiveScene().buildIndex != 0)
        manager.generatedBoard[(int)transform.position.x, (int)transform.position.y] = true;
    }
    
    // Update is called once per frame
    void Update () {
    }

    private void OnMouseDown()
    {
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            int x = (int)transform.position.x;
            int y = (int)transform.position.y;
            if (!alreadyClicked && manager.GameOngoing && !manager.ui.pausePanel.activeInHierarchy && (NextToUsedTile() || !manager.firstPlaced) && !mManager.isMultiplayer)
            {
                if (!manager.firstPlaced)
                    manager.firstPlaced = true;
                if (manager.isX)
                    srenderer.sprite = X;
                else
                    srenderer.sprite = O;
                manager.board[x, y] = manager.isX;
                Debug.Log(manager.board[x, y]);
                manager.GenerateTiles((int)transform.position.x, (int)transform.position.y);
                gameObject.transform.GetChild(0).gameObject.SetActive(true);
                manager.tiles.Add(gameObject);
                manager.Last = gameObject.GetComponent<SpriteRenderer>();
                if (manager.CheckNewWin((int)transform.position.x, (int)transform.position.y))
                    manager.GameOngoing = false;
                else
                    manager.ui.SetStatus(!manager.isX);
                manager.isX = !manager.isX;
                alreadyClicked = true;
            }
            else if (mManager.isMultiplayer && mManager.YourTurn && manager.GameOngoing)
            {
                if (manager.firstPlaced && NextToUsedTile() && manager.board[x,y] == null)
                {
                    mManager.connection.Send("send", x, y, mManager.myId, iManager.currentFigureId);
                }
                else if (!manager.firstPlaced)
                {
                    manager.firstPlaced = true;
                    mManager.connection.Send("send", x, y, mManager.myId, iManager.currentFigureId);
                }
            }
        }
    }

    private bool NextToUsedTile()
    {
        int x = (int)transform.position.x;
        int y = (int)transform.position.y;
        if (x - 1 > 0 && y - 1 > 0 && x + 1 < 100 && y + 1 < 100)
        if (manager.board[x - 1, y] == null && manager.board[x - 1, y - 1] == null && manager.board[x, y - 1] == null && manager.board[x + 1, y] == null && manager.board[x + 1, y + 1] == null && manager.board[x, y + 1] == null && manager.board[x - 1, y + 1] == null && manager.board[x + 1, y - 1] == null)
        {
            return false;
        }
        return true;
    }
}
