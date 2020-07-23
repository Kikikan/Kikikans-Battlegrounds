using PlayerIOClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    private ShopManager shopManager;
    private MultiplayerManager mManager;

    private List<Button> itemButtonList = new List<Button>();

    public Sprite MissingContent;

    [Header("Original Sprites")]
    public Sprite originalX;
    public Sprite originalO;
    public Sprite scifiX;
    public Sprite scifiO;
    public Sprite neonX;
    public Sprite neonO;
    public Sprite abstractX;
    public Sprite abstractO;
    public Sprite illuminatiW;
    public Sprite illuminatiWO;
    public Sprite spaceShip;
    public Sprite earth;

    [Header("Alternative Sprites")]
    public Sprite _originalX;
    public Sprite _originalO;
    public Sprite _scifiX;
    public Sprite _scifiO;
    public Sprite _neonX;
    public Sprite _neonO;
    public Sprite _abstractX;
    public Sprite _abstractO;
    public Sprite _illuminatiW;
    public Sprite _illuminatiWO;
    public Sprite _spaceShip;
    public Sprite _earth;



    private int offset = -66;

    public int currentFigureId;

    public List<Sprite> SpriteList = new List<Sprite>();
    public List<Sprite> AlternativeSpriteList = new List<Sprite>();

    private void Awake()
    {

        var collection = FindObjectsOfType<InventoryManager>();
        if (collection.Length > 1)
            Destroy(this.gameObject);
        DontDestroyOnLoad(gameObject);

    }
    // Use this for initialization
    void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            shopManager = FindObjectOfType<ShopManager>();
            InitializeSpriteList();
            mManager = FindObjectOfType<MultiplayerManager>();
        }
    }

    private void OnLevelWasLoaded(int level)
    {
        if (level == 0)
        {
            shopManager = FindObjectOfType<ShopManager>();
            itemButtonList.Clear();
            offset = -66;
        }
    }

    void InitializeSpriteList()
    {
        SpriteList.Add(originalX);
        SpriteList.Add(originalO);
        SpriteList.Add(scifiX);
        SpriteList.Add(scifiO);
        SpriteList.Add(neonX);
        SpriteList.Add(neonO);
        SpriteList.Add(abstractX);
        SpriteList.Add(abstractO);
        SpriteList.Add(illuminatiW);
        SpriteList.Add(illuminatiWO);
        SpriteList.Add(spaceShip);
        SpriteList.Add(earth);

        AlternativeSpriteList.Add(_originalX);
        AlternativeSpriteList.Add(_originalO);
        AlternativeSpriteList.Add(_scifiX);
        AlternativeSpriteList.Add(_scifiO);
        AlternativeSpriteList.Add(_neonX);
        AlternativeSpriteList.Add(_neonO);
        AlternativeSpriteList.Add(_abstractX);
        AlternativeSpriteList.Add(_abstractO);
        AlternativeSpriteList.Add(_illuminatiW);
        AlternativeSpriteList.Add(_illuminatiWO);
        AlternativeSpriteList.Add(_spaceShip);
        AlternativeSpriteList.Add(_earth);
    }

    public void InitializeInventory()
    {
        offset = -66;
        foreach (var button in itemButtonList)
        {
            Destroy(button.gameObject);
            itemButtonList.Remove(button);
        }
        mManager.client.BigDB.LoadMyPlayerObject((DatabaseObject playerObj) =>
        {
            currentFigureId = (int)playerObj["currentFId"];
            AddInventoryButton(1);
            mManager.client.PayVault.Refresh(() =>
            {
                if (mManager.client.PayVault.Has("scifix"))
                    AddInventoryButton(2);
                if (mManager.client.PayVault.Has("scifio"))
                    AddInventoryButton(3);
                if (mManager.client.PayVault.Has("neonx"))
                    AddInventoryButton(4);
                if (mManager.client.PayVault.Has("neono"))
                    AddInventoryButton(5);
                if (mManager.client.PayVault.Has("abstractx"))
                    AddInventoryButton(6);
                if (mManager.client.PayVault.Has("abstracto"))
                    AddInventoryButton(7);
                if (mManager.client.PayVault.Has("illuminatiw"))
                    AddInventoryButton(8);
                if (mManager.client.PayVault.Has("illuminatiwo"))
                    AddInventoryButton(9);
                if (mManager.client.PayVault.Has("spaceship"))
                    AddInventoryButton(10);
                if (mManager.client.PayVault.Has("earth"))
                    AddInventoryButton(11);
            });
        });
    }

    void AddInventoryButton(int id)
    {
        var newButton = Instantiate(shopManager.itemButtonPrefab, shopManager.invContent.transform);
        itemButtonList.Add(newButton.GetComponent<Button>());
        newButton.GetComponent<Button>().onClick.AddListener(() => { ShowItem(id); });
        newButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(shopManager.lastInvButton.GetComponent<RectTransform>().anchoredPosition.x, shopManager.lastInvButton.GetComponent<RectTransform>().anchoredPosition.y + offset);
        //newButton.transform.position = new Vector2(lastButton.transform.position.x, lastButton.transform.position.y + offset);
        newButton.GetComponentInChildren<Text>().text = shopManager.ShopList[id].Name;
        offset -= 66;
    }

    public void ShowItem(int id)
    {
        shopManager.itemImage.sprite = SpriteList[id];
        shopManager.itemNameText.text = shopManager.ShopList[id].Name;
        shopManager.itemDescriptionText.text = shopManager.ShopList[id].Description;
        if (id == currentFigureId)
        {
            shopManager.equipButton.GetComponentInChildren<Text>().text = "equipped";
            shopManager.equipButton.interactable = false;
        }
        else
        {
            shopManager.equipButton.onClick.RemoveAllListeners();
            shopManager.equipButton.onClick.AddListener(() =>
            {
                EquipItem(id);
            });
            shopManager.equipButton.GetComponentInChildren<Text>().text = "equip";
            shopManager.equipButton.interactable = true;
        }

        if (!shopManager.itemImage.gameObject.activeInHierarchy)
            shopManager.itemImage.gameObject.SetActive(true);
        if (!shopManager.itemNameText.gameObject.activeInHierarchy)
            shopManager.itemNameText.gameObject.SetActive(true);
        if (!shopManager.itemDescriptionText.gameObject.activeInHierarchy)
            shopManager.itemDescriptionText.gameObject.SetActive(true);
        if (!shopManager.equipButton.gameObject.activeInHierarchy)
            shopManager.equipButton.gameObject.SetActive(true);
    }

    public void EquipItem(int id)
    {
        currentFigureId = id;
        shopManager.equipButton.interactable = false;
        shopManager.equipButton.GetComponentInChildren<Text>().text = "equipped";
        mManager.client.BigDB.LoadMyPlayerObject((DatabaseObject playerObj) =>
        {
            playerObj.Set("currentFId", id);
            playerObj.Save();
        });
    }
}
