using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerIOClient;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MultiplayerManager : MonoBehaviour
{

    private const string clientVersion = "0.2";
    private const string compatibleServerVersion = "0.2.3";

    public string EloDiff = "200";

    public int currentSpriteId = -1;

    private bool isDebug = false;

    private GameManager manager;
    private UIManager ui;
    private InventoryManager iManager;

    public bool isMultiplayer = false;
    public bool isCasual = false;
    private bool casualFound = false;


    internal Client client;
    internal Connection connection;

    internal string username;
    internal int myId;
    internal string enemyUsername;

    private bool yourTurn = false;

    internal bool YourTurn
    {
        get { return yourTurn; }
        set
        {
            yourTurn = value;
            manager.ui.SetStatus();
        }
    }

    private void OnApplicationQuit()
    {
        if (connection != null)
            if (connection.Connected)
                connection.Disconnect();
        if (client != null)
            client.Logout();
    }

    void Awake()
    {
        var collection = FindObjectsOfType<MultiplayerManager>();
        if (collection.Length > 1)
            Destroy(this);
        else
            DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        ui = FindObjectOfType<UIManager>();
        iManager = FindObjectOfType<InventoryManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoginClick()
    {
        if (!ui.UsernameInput.text.Contains(" ") && !ui.PasswordInput.text.Contains(" "))
            PlayerIO.Authenticate(
                "kbg-23zerasmuki13gqunxenca",
                "public",
                new Dictionary<string, string>() {
                { "username", ui.UsernameInput.text },
                { "password", ui.PasswordInput.text }
                },
                null,
                (Client _client) =>
                {
                    client = _client;
                    username = ui.UsernameInput.text;
                    UpdatePlayerObj();
                    if (isDebug)
                        client.Multiplayer.DevelopmentServer = new ServerEndpoint("192.168.0.111", 8184);
                    RefreshRooms();
                    ui.OnLogin();
                    LatestVersion();
                },
                (PlayerIOError error) =>
                {
                    if (error.ErrorCode == ErrorCode.UnknownUser)
                    {
                        PlayerIO.Authenticate(
                            "kbg-23zerasmuki13gqunxenca",
                            "public",
                            new Dictionary<string, string>() {
                            {"register", "true" },
                            { "username", ui.UsernameInput.text },
                            { "password", ui.PasswordInput.text }
                            },
                            null,
                            (Client _client) =>
                            {
                                client = _client;
                                username = ui.UsernameInput.text;
                                UpdatePlayerObj();
                                if (isDebug)
                                    client.Multiplayer.DevelopmentServer = new ServerEndpoint("192.168.0.111", 8184);
                                RefreshRooms();
                                ui.OnLogin();
                            },
                            (PlayerIOError _error) =>
                            {
                                Debug.LogError(_error.ToString());
                            });
                    }
                    else if (error.ErrorCode == ErrorCode.InvalidPassword)
                    {
                        ui.Popup("Just a typo", "The password you entered is invalid.");
                    }
                    else if (error.ErrorCode == ErrorCode.InternalError)
                        ui.Popup("What?", "We're not sure what happened, try again please.");
                });
    }

    private void UpdatePlayerObj()
    {
        client.BigDB.LoadMyPlayerObject((DatabaseObject playerObj) =>
        {
  
            if (playerObj.Contains("coin"))
            {
                client.Multiplayer.CreateJoinRoom("Shop", "shop", true, null, new Dictionary<string, string>() { { "userName", username } }, (Connection con) =>
                {
                    con.Send("update");
                });
            }
            if (!playerObj.Contains("currentFId"))
            {
                playerObj.Set("currentFId", 0);
                playerObj.Save();
            }
        });
    }

    private void LatestVersion()
    {
        client.BigDB.Load("Game", "versions", (DatabaseObject obj) =>
        {

            if (compatibleServerVersion != (string)obj["serverVersion"])
                ui.Popup("ERROR", "Your client is outdated! Please update if you want to go online.\nDisconnected from server.", () => { Logout(); });
            else if (clientVersion != (string)obj["latestClientVersion"])
                ui.Popup("WARNING", "Your client is outdated! You can play online, but you might encounter bugs and missing content.\nMay the Force be with you.");
        });
    }

    public void Logout()
    {
        if (connection != null)
        {
            if (connection.Connected)
                connection.Disconnect();
            connection = null;
        }
        client.Logout();
        client = null;
        SceneManager.LoadScene(0);
    }

    public void CasualClick()
    {
        ui.CasualButton.interactable = false;
        ui.QueueTimer.gameObject.SetActive(true);
        ui.QueueCancel.interactable = false;
        ui.QueueAnimator.speed = 1;
        ui.QueueCancel.GetComponentInChildren<Text>().text = "cancel";
        ui.QueuePanel.SetActive(true);
        ui.QueueTimerReset(true);
        ui.queueTimerOn = true;
        ui.QueueStatus.text = "Connecting to the Matchmaking Servers";
        client.Multiplayer.CreateJoinRoom(
            "casualMatchmaking",
            "casualMatchmaking",
            true,
            null,
            new Dictionary<string, string>() { { "userName", username } },
            (Connection con) =>
            {
                isCasual = true;
                ui.QueueStatus.text = "Searching for an honorable opponent";
                ui.QueueCancel.interactable = true;
                connection = con;
                connection.OnMessage += MessageHandler;
                connection.OnDisconnect += OnCasualDisconnect;
                ui.InRoomOrQueue(true, true);
            },
            (PlayerIOError e) =>
            {
                ui.QueueStatus.text = e.Message;
                ui.QueueCancel.GetComponentInChildren<Text>().text = "back";
                ui.QueueAnimator.speed = 0;
                ui.QueueTimerReset();
            }
            );
    }

    public void RefreshRooms()
    {
        foreach (var gameObj in ui.roomButtons)
        {
            Destroy(gameObj);
        }
        ui.roomButtons.Clear();
        client.Multiplayer.ListRooms(
            "custom",
            new Dictionary<string, string>() { { "game", "true" } },
            0,
            0,
            (RoomInfo[] rooms) =>
            {
                int offset = 0;
                foreach (var room in rooms)
                {
                    var button = Instantiate(ui.RoomPrefab, ui.RoomList.transform, false);
                    button.GetComponentInChildren<Text>().text = room.Id;
                    button.GetComponent<RectTransform>().anchoredPosition = new Vector2(button.GetComponent<RectTransform>().anchoredPosition.x, button.GetComponent<RectTransform>().anchoredPosition.y + offset);
                    offset -= 120;
                    if (connection != null)
                    {
                        if (connection.Connected)
                            button.GetComponent<Button>().interactable = false;
                    }
                    button.GetComponent<Button>().onClick.AddListener(() => { RoomClick(room.Id); });
                    ui.roomButtons.Add(button);

                }
            },
            (PlayerIOError error) =>
            {
                Debug.LogError(error.ToString());
            });
    }

    public void CreateClick()
    {
        ui.QueuePanel.SetActive(true);
        ui.QueueTimer.gameObject.SetActive(false);
        ui.QueueCancel.interactable = false;
        ui.QueueStatus.text = "Connecting to the servers";
        ui.QueueCancel.GetComponentInChildren<Text>().text = "cancel";
        ui.QueueAnimator.speed = 1;
        client.Multiplayer.CreateJoinRoom(
            ui.RoomNameInput.text,
            "custom",
            true,
            new Dictionary<string, string>() { { "game", "true" } },
            new Dictionary<string, string>() { { "userName", username } },
            (Connection con) =>
            {
                ui.QueueStatus.text = "Waiting for someone to join";
                ui.QueueCancel.interactable = true;
                isCasual = false;
                connection = con;
                connection.OnMessage += MessageHandler;
                connection.OnDisconnect += OnDisconnect;
                ui.InRoomOrQueue(true, false);
            });
    }

    public void CancelQueue()
    {
        casualFound = true;
        connection.Disconnect();
        ui.InRoomOrQueue(false, false);
        ui.QueueTimerReset(false, true);
    }

    void RoomClick(string roomID)
    {
        client.Multiplayer.JoinRoom(
            roomID,
            new Dictionary<string, string>() { { "userName", username } },
            (Connection con) =>
            {
                isCasual = false;
                connection = con;
                connection.OnMessage += MessageHandler;
                connection.OnDisconnect += OnDisconnect;
            });
    }

    int tempFigureId = -1;
    bool sameId = false;
    bool altIdNext = false;
    int count = 0;
    private bool UpdateBoard(int x, int y, int figureId)
    {
        for (int i = 0; i < manager.Map.transform.childCount; i++)
        {
            var child = manager.Map.transform.GetChild(i);
            if (child.transform.position.x == x && child.transform.position.y == y)
            {
                count++;
                manager.tiles.Add(child.gameObject);
                child.transform.GetChild(0).gameObject.SetActive(true);
                if (tempFigureId == -1)
                {
                    tempFigureId = figureId;
                }                   
                if (count == 2 && figureId == tempFigureId)
                    sameId = true;
                if (!sameId)
                {
                    if (iManager.SpriteList.Count > figureId)
                        child.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = iManager.SpriteList[figureId];
                    else
                        child.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = iManager.MissingContent;
                }
                else if (iManager.SpriteList.Count > figureId && !altIdNext)
                    child.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = iManager.SpriteList[figureId];
                else if (iManager.SpriteList.Count > figureId && altIdNext)
                    child.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = iManager.AlternativeSpriteList[figureId];
                else
                    child.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = iManager.MissingContent;
                altIdNext = !altIdNext;
                manager.Last = child.GetComponent<SpriteRenderer>();

                return true;
            }
        }
        return false;
    }

    private void MessageHandler(object sender, Message m)
    {
        Debug.Log(m.Type);

        switch (m.Type)
        {
            #region in-game messages
            case "left":
                if (manager.GameOngoing)
                {
                    ui.StatusText.text = m.GetString(0) + " has left. You won!";
                    manager.GameOngoing = false;
                    ui.CanGG = false;
                }
                else
                {
                    ui.CanGG = false;
                }

                break;

            case "init":
                myId = m.GetInt(0);
                break;


            case "begin":
                isMultiplayer = true;
                SceneManager.LoadScene(1);
                connection.Send("ready");
                break;

            case "send":

                int x = m.GetInt(0);
                int y = m.GetInt(1);
                int id = m.GetInt(2);
                int figureId = m.GetInt(3);

                manager.board[x, y] = (id == myId);
                manager.GenerateTiles(x, y);
                YourTurn = (id != myId);
                if (id == myId)
                {
                    if (!UpdateBoard(x, y, figureId))
                    {
                        Debug.LogError("UpdateBoard failed");
                    }
                }
                else
                {
                    if (!UpdateBoard(x, y, figureId))
                    {
                        Debug.LogError("UpdateBoard failed");
                    }
                }

                /*if(manager.CheckNewWin(x, y))
                {
                    connection.Send("win", x, y);
                    manager.GameOngoing = false;
                    manager.ui.SetWin(true);
                }*/


                break;

            case "first":
                YourTurn = true;
                manager.startingTime = Time.time;
                break;

            case "enemy":
                enemyUsername = m.GetString(0);
                break;

            case "second":
                YourTurn = false;
                manager.startingTime = Time.time;
                break;

            case "win":
                manager.GameOngoing = false;
                manager.CheckNewWin(m.GetInt(0), m.GetInt(1), true);
                manager.ui.SetWin(true);
                break;

            case "lose":
                manager.GameOngoing = false;
                manager.CheckNewWin(m.GetInt(0), m.GetInt(1));
                manager.ui.SetWin(false);
                break;

            case "coinvalue":
                uint coins = m.GetUInt(0);
                uint currentCoins = m.GetUInt(1);
                GameObject.Find("CoinText").GetComponent<Text>().text = " " + currentCoins + "\n+" + coins + "\n____________\n " + (currentCoins + coins) + " coins";
                GameObject.Find("MultiplayerPanel").SetActive(true);
                break;

            case "gg":
                ui.GgText.text = enemyUsername + ": GG!";
                ui.GgText.gameObject.SetActive(true);
                break;

            case "rematchconfirmed":
                SceneManager.LoadScene(1);
                connection.Send("ready");
                break;
            #endregion

            case "casualMatchFound":
                casualFound = true;
                ui.QueueCancel.interactable = false;
                ui.QueueTimerReset();
                enemyUsername = m.GetString(1);
                ui.QueueStatus.text = "Match found!\n" + username + " vs " + enemyUsername;
                ui.QueueAnimator.speed = 0;
                client.Multiplayer.CreateJoinRoom(m.GetString(0), "casual", true, null, new Dictionary<string, string>() { { "userName", username } }, (Connection con) =>
                 {
                     isCasual = true;
                     connection = con;
                     connection.OnMessage += MessageHandler;
                 });
                break;

            case "groupdisallowedjoin":
                if (ui.QueuePanel.activeInHierarchy)
                {
                    ui.QueueTimerReset();
                    ui.QueueAnimator.speed = 0;
                    ui.QueueStatus.text = "You can't join this room. Try again later.";
                    ui.QueueCancel.GetComponentInChildren<Text>().text = "back";
                }
                else
                {
                    ui.Popup("ERROR", "You can't join this room.");
                }
                break;
        }
    }

    private void OnDisconnect(object sender, string message)
    {
        isMultiplayer = false;
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            ui.InRoomOrQueue(false, false);
            ui.QueuePanel.SetActive(false);
        }
        
    }

    private void OnCasualDisconnect(object sender, string message)
    {
        if (!casualFound)
        {
            ui.QueueAnimator.speed = 0;
            ui.QueueCancel.GetComponentInChildren<Text>().text = "back";
            ui.QueueStatus.text = "Disconnected from matchmaking:\n" + message;
            ui.QueueTimerReset();
        }
        casualFound = false;
    }

    internal void AddManager(GameManager _manager)
    {
        manager = _manager;
    }

    private void OnLevelWasLoaded(int level)
    {
        ui = FindObjectOfType<UIManager>();
        if (level == 1)
        {
            if (isCasual)
                ui.RematchButton.gameObject.SetActive(false);
        }
        if (level == 0)
        {
            tempFigureId = -1;
            sameId = false;
            altIdNext = false;
            count = 0;
            
        }
    }
}