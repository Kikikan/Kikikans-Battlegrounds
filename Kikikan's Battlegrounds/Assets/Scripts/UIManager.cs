using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.EventSystems;
using System;

public class UIManager : MonoBehaviour
{



    [Header("Main Menu")]
    [SerializeField]
    private Animator headerAnimator;
    [SerializeField]
    private Animator creditsAnimator;
    [SerializeField]
    private Animator offMPAnimator;
    [SerializeField]
    private Animator onMPAnimator;
    [SerializeField]
    private Animator optionsAnimator;
    [SerializeField]
    private Animator exitAnimator;
    [SerializeField]
    private Animator creditsPanelAnimator;
    [SerializeField]
    private Animator multiplayerPanelAnimator;
    [SerializeField]
    private Animator customRoomAnimator;
    [SerializeField]
    private Animator shopAnimator;

    public GameObject errorPanel;

    public InputField UsernameInput;
    public InputField PasswordInput;
    public Button LoginButton;


    public Button ShopButton;
    public Text CoinText;

    [SerializeField]
    public Button CasualButton;
    [SerializeField]
    private Button RankedButton;
    [SerializeField]
    private Button CustomButton;
    [SerializeField]
    private Button LogoutButton;

    public GameObject QueuePanel;
    public Text QueueStatus;
    public Button QueueCancel;
    public Animator QueueAnimator;
    public Text QueueTimer;
    private float queueTime;
    public bool queueTimerOn = false;

    public GameObject RoomPrefab;
    public GameObject RoomList;

    public List<GameObject> roomButtons = new List<GameObject>();

    public InputField RoomNameInput;
    public Button CreateButton;
    public Button RefreshButton;

    public GameObject MultiplayerPanel;

    [SerializeField]
    private Sprite X;
    [SerializeField]
    private Sprite O;

    [SerializeField]
    private ShopManager shopManager;


    [Header("Game")]
    public Text StatusText;

    public GameObject pausePanel;


    public Text GgText;
    [SerializeField]
    private Button GgButton;
    private bool canGg = true;
    public bool CanGG
    {
        get { return canGg; }
        set
        {
            GgButton.interactable = value;

        }
    }

    public Button RematchButton;

    private MultiplayerManager mManager;

    // Use this for initialization
    void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0) // Main Menu
        {
            for (int x = 0; x < board.GetLength(0); x++)
            {
                for (int y = 0; y < board.GetLength(1); y++)
                {
                    board[x, y] = false;
                }
            }
            StartCoroutine(BackgroundPlay());
        }
        mManager = FindObjectOfType<MultiplayerManager>();

        if (mManager.client != null && SceneManager.GetActiveScene().buildIndex == 0)
        {
            OnLogin();
            mManager.RefreshRooms();
        }
        if (SceneManager.GetActiveScene().buildIndex == 1 && mManager.connection != null)
        {
            if (mManager.connection.Connected)
                StatusText.text = "Waiting for your opponent";
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && pausePanel.activeInHierarchy)
            pausePanel.SetActive(false);
        else if (Input.GetKeyDown(KeyCode.Escape))
            pausePanel.SetActive(true);

        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (EventSystem.current.currentSelectedGameObject != null)
                {
                    Selectable selectable = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
                    if (selectable != null)
                        selectable.Select();
                }
            }
            if (queueTimerOn)
            {
                queueTime += Time.deltaTime;
                QueueTimer.text = Math.Floor(queueTime / 60) + ":" + (Math.Floor(queueTime % 60) < 10 ? "0" : "") + Math.Floor(queueTime % 60);
            }
        }

    }

    public void SetStatus(bool isX = false)
    {
        if (!mManager.isMultiplayer)
        {
            if (isX)
                StatusText.text = "It's X's turn!";
            else
                StatusText.text = "It's O's turn!";
        }
        else
        {
            if (mManager.YourTurn)
                StatusText.text = "It's your turn!";
            else
                StatusText.text = "It's " + mManager.enemyUsername + "'s turn!";
        }
    }

    public void SetWin(bool isX)
    {
        if (!mManager.isMultiplayer)
        {
            if (isX)
                StatusText.text = "X won!";
            else
                StatusText.text = "O won!";
        }
        else
        {
            if (isX)
                StatusText.text = "You won!";
            else
                StatusText.text = mManager.enemyUsername + " won!";
        }
    }

    #region Main Menu
    private bool[,] board = new bool[28, 17];

    private bool creditsSelected = false;
    public bool CreditsSelected
    {
        get { return creditsSelected; }
        set
        {
            creditsSelected = value;
            if (creditsSelected)
            {
                creditsAnimator.SetBool("ThisClicked", true);
                offMPAnimator.SetBool("ElseClicked", true);
                onMPAnimator.SetBool("ElseClicked", true);
                optionsAnimator.SetBool("ElseClicked", true);
                creditsPanelAnimator.SetBool("ThisClicked", true);
                exitAnimator.SetBool("ElseClicked", true);
                multiplayerPanelAnimator.SetBool("ThisClicked", false);
                headerAnimator.SetBool("ElseClicked", true);
                shopAnimator.SetBool("ElseClicked", true);
            }
            else
            {
                creditsAnimator.SetBool("ThisClicked", false);
                offMPAnimator.SetBool("ElseClicked", false);
                onMPAnimator.SetBool("ElseClicked", false);
                optionsAnimator.SetBool("ElseClicked", false);
                creditsPanelAnimator.SetBool("ThisClicked", false);
                exitAnimator.SetBool("ElseClicked", false);
                multiplayerPanelAnimator.SetBool("ThisClicked", false);
                headerAnimator.SetBool("ElseClicked", false);
                shopAnimator.SetBool("ElseClicked", false);
            }

        }
    }

    public void OfflineMultiplayer()
    {
        SceneManager.LoadScene(1);
    }

    private bool onMPSelected = false;
    public bool OnMPSelected
    {
        get { return onMPSelected; }
        set
        {
            onMPSelected = value;
            if (onMPSelected)
            {
                creditsAnimator.SetBool("ElseClicked", true);
                offMPAnimator.SetBool("ElseClicked", true);
                onMPAnimator.SetBool("ThisClicked", true);
                optionsAnimator.SetBool("ElseClicked", true);
                creditsPanelAnimator.SetBool("ThisClicked", false);
                exitAnimator.SetBool("ElseClicked", true);
                multiplayerPanelAnimator.SetBool("ThisClicked", true);
                headerAnimator.SetBool("ElseClicked", true);
                shopAnimator.SetBool("ElseClicked", true);
            }
            else
            {
                creditsAnimator.SetBool("ElseClicked", false);
                offMPAnimator.SetBool("ElseClicked", false);
                onMPAnimator.SetBool("ThisClicked", false);
                optionsAnimator.SetBool("ElseClicked", false);
                creditsPanelAnimator.SetBool("ThisClicked", false);
                exitAnimator.SetBool("ElseClicked", false);
                multiplayerPanelAnimator.SetBool("ThisClicked", false);
                headerAnimator.SetBool("ElseClicked", false);
                shopAnimator.SetBool("ElseClicked", false);
            }
        }
    }

    private bool customRoomSelected = false;

    private bool shopSelected = false;
    public bool ShopSelected
    {
        get { return shopSelected; }
        set
        {
            shopSelected = value;
            if (shopSelected)
            {
                creditsAnimator.SetBool("ElseClicked", true);
                creditsPanelAnimator.SetBool("ThisClicked", false);
                offMPAnimator.SetBool("ElseClicked", true);
                onMPAnimator.SetBool("ThisClicked", true);
                optionsAnimator.SetBool("ElseClicked", true);
                exitAnimator.SetBool("ElseClicked", true);
                multiplayerPanelAnimator.SetBool("ThisClicked", false);
                headerAnimator.SetBool("ElseClicked", true);
                shopAnimator.SetBool("ThisClicked", true);
            }
            else
            {
                creditsAnimator.SetBool("ElseClicked", false);
                creditsPanelAnimator.SetBool("ThisClicked", false);
                offMPAnimator.SetBool("ElseClicked", false);
                onMPAnimator.SetBool("ThisClicked", false);
                optionsAnimator.SetBool("ElseClicked", false);
                exitAnimator.SetBool("ElseClicked", false);
                multiplayerPanelAnimator.SetBool("ThisClicked", false);
                headerAnimator.SetBool("ElseClicked", false);
                shopAnimator.SetBool("ThisClicked", false);
            }
        }
    }

    public void CreditsClick()
    {
        CreditsSelected = !CreditsSelected;
    }

    public void OnMPClick()
    {
        OnMPSelected = !OnMPSelected;
    }

    public void CustomRoomClick()
    {
        customRoomAnimator.SetBool("ThisSelected", !customRoomSelected);
        customRoomSelected = !customRoomSelected;
        mManager.RefreshRooms();
    }

    public void CasualClick()
    {
        mManager.CasualClick();
    }

    public void QueueTimerReset(bool start = false, bool overwriteWith0 = false)
    {
        queueTime = 0;
        if (start)
        {
            queueTimerOn = true;
        }
        else
        {
            queueTimerOn = false;
        }
        if (overwriteWith0)
            QueueTimer.text = "0:00";
    }

    public void CancelQueueClick()
    {
        mManager.CancelQueue();
    }

    public void LogoutClick()
    {
        mManager.Logout();
    }

    public void ShopClick()
    {
        ShopSelected = !ShopSelected;
        if (ShopSelected)
            shopManager.UpdatePlayerCoin();
    }

    public void Exit()
    {
        Application.Quit();
    }

    private bool LeftTile()
    {
        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
            {
                if (board[x, y] == false)
                    return true;
            }
        }
        return false;
    }
    private bool isX = true;
    IEnumerator BackgroundPlay()
    {
        while (LeftTile())
        {
            int x = UnityEngine.Random.Range(0, board.GetLength(0));
            int y = UnityEngine.Random.Range(0, board.GetLength(1));
            foreach (Transform child in GameObject.Find("Background").transform)
            {
                if (child.position.x == x && child.position.y == y && !child.GetChild(0).gameObject.activeInHierarchy)
                {
                    child.GetChild(0).gameObject.SetActive(true);
                    if (isX)
                        child.GetChild(0).GetComponent<SpriteRenderer>().sprite = X;
                    else
                        child.GetChild(0).GetComponent<SpriteRenderer>().sprite = O;
                    board[x, y] = true;
                    isX = !isX;
                }
            }
            yield return new WaitForSeconds(0.25f);
        }
    }

    public void LoginClick()
    {
        mManager.LoginClick();
    }

    public void CreateClick()
    {
        mManager.CreateClick();
    }

    public void RefreshClick()
    {
        mManager.RefreshRooms();
    }

    internal void OnLogin()
    {
        CreateButton.interactable = true;
        RefreshButton.interactable = true;
        LoginButton.gameObject.SetActive(false);
        UsernameInput.gameObject.SetActive(false);
        PasswordInput.gameObject.SetActive(false);
        ShopButton.interactable = true;
        LogoutButton.gameObject.SetActive(true);

        CasualButton.gameObject.SetActive(true);
        RankedButton.gameObject.SetActive(true);
        CustomButton.gameObject.SetActive(true);

        shopManager.ConnectToShop();
    }

    public void InRoomOrQueue(bool value, bool queue)
    {
        if (value)
        {
            CreateButton.interactable = false;
            CasualButton.interactable = false;
            RankedButton.interactable = false;
            if (!queue)
                CreateButton.gameObject.transform.GetChild(0).gameObject.GetComponent<Text>().text = "waiting...";

            foreach (Transform trans in RoomList.transform)
            {
                trans.gameObject.GetComponent<Button>().interactable = false;
            }
        }
        else
        {
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                CreateButton.interactable = true;
                CasualButton.interactable = true;
                RankedButton.interactable = false;
                CreateButton.gameObject.transform.GetChild(0).gameObject.GetComponent<Text>().text = "create";
                QueuePanel.SetActive(false);

                foreach (Transform trans in RoomList.transform)
                {
                    trans.gameObject.GetComponent<Button>().interactable = true;
                }
            }
        }
    }

    public void Popup(string title, string desc, UnityEngine.Events.UnityAction eventA = null)
    {
        errorPanel.transform.Find("Error Button").transform.Find("Title").GetComponent<Text>().text = title;
        errorPanel.transform.Find("Error Button").transform.Find("Description").GetComponent<Text>().text = desc;
        errorPanel.transform.Find("Error Button").GetComponent<Button>().onClick.RemoveAllListeners();
        if (eventA == null)
            errorPanel.transform.Find("Error Button").GetComponent<Button>().onClick.AddListener(() =>
            {
                errorPanel.SetActive(false);
            });
        else
            errorPanel.transform.Find("Error Button").GetComponent<Button>().onClick.AddListener(eventA);
        errorPanel.SetActive(true);
    }

    private void ClosePopup()
    {
        errorPanel.SetActive(false);
    }

    #endregion

    #region Pause Menu
    public void Continue()
    {
        pausePanel.SetActive(false);
    }

    public void MainMenu()
    {
        if (mManager.isMultiplayer)
            mManager.connection.Disconnect();
        SceneManager.LoadScene(0);
    }
    #endregion

    #region Multiplayer Menu

    public void RequestRematch()
    {
        mManager.connection.Send("rematch");
        GameObject.Find("RematchButton").GetComponent<Button>().interactable = false;
    }

    public void Leave()
    {
        mManager.connection.Disconnect();
        SceneManager.LoadScene(0);
    }

    public void GG(Button button)
    {
        if (mManager.connection.Connected)
            mManager.connection.Send("gg");
        button.interactable = false;
    }

    #endregion

}
