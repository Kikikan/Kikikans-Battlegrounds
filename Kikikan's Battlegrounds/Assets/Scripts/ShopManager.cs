using PlayerIOClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ShopManager : MonoBehaviour
{


    private MultiplayerManager mManager;
    private InventoryManager iManager;
    private UIManager ui;
    internal List<ShopItem> ShopList = new List<ShopItem>();
    private List<string> bundleList = new List<string>();

    private int offset = -66;

    [Header("Shop")]
    [SerializeField]
    private Button bundleButtonPrefab;
    [SerializeField]
    private Button lastButton;
    [SerializeField]
    private GameObject shopContent;

    [SerializeField]
    private Animator shopItemPanelAnimator;
    [SerializeField]
    private Animator shopNavAnimator;
    [SerializeField]
    private Animator invPanelAnimator;
    private bool shopItemPanelSelected;
    public bool ShopItemPanelSelected
    {
        get { return shopItemPanelSelected; }
        set
        {
            shopItemPanelSelected = value;
            shopItemPanelAnimator.SetBool("ThisClicked", value);
            shopNavAnimator.SetBool("ShopSelected", value);

        }
    }

    public Text BundleName;

    public GameObject ItemPrefab;
    public Transform BundleContent;
    private List<GameObject> itemPrefabs = new List<GameObject>();
    private bool purchaseInProgress = false;

    [Header("Inventory")]
    [SerializeField]
    internal GameObject invContent;
    [SerializeField]
    internal Image itemImage;
    [SerializeField]
    internal Text itemNameText;
    [SerializeField]
    internal Text itemDescriptionText;
    [SerializeField]
    internal Button equipButton;
    [SerializeField]
    internal GameObject itemButtonPrefab;
    [SerializeField]
    internal Button lastInvButton;

    public void ShowTraditionalX()
    {
        iManager.ShowItem(0);
    }

    // Use this for initialization
    void Start()
    {
        mManager = FindObjectOfType<MultiplayerManager>();
        iManager = FindObjectOfType<InventoryManager>();
        ui = FindObjectOfType<UIManager>();
    }

    public void ConnectToShop()
    {
        ShopList.Add(new ShopItem("Traditional X", 0, 0, "The ordinary X figure.", 0, 0));
        ShopList.Add(new ShopItem("Traditional O", 1, 0, "The good old O figure.", 0, 0));
        mManager.client.BigDB.Load("Game", "bundle", (DatabaseObject bundleObj) =>
        {
            int count = (int)bundleObj["count"];
            var itemNameList = new List<string>();
            bundleList.Add("Traditional");
            for (int i = 1; i < 1 + count; i++)
            {
                var bundleInfo = bundleObj.GetArray(i.ToString());
                bundleList.Add(bundleInfo.GetString(1));
                int countMembers = bundleInfo.GetInt(2);
                for (int j = 3; j < 3 + countMembers; j++)
                {
                    itemNameList.Add(bundleInfo.GetString(j));
                }
            }
            if (count > 1)
            {
                for (int i = 2; i < 1 + count; i++)
                {
                    AddShopButton(i);
                }
            }
            var itemNameArray = itemNameList.ToArray();
            mManager.client.BigDB.LoadKeys("PayVaultItems", itemNameArray, (DatabaseObject[] objs) =>
            {

                foreach (var obj in objs)
                {
                    ShopList.Add(new ShopItem(obj.GetString("name"), obj.GetInt("id"), obj.GetInt("bundleid"), obj.GetString("description"), obj.GetInt("PriceCoins"), obj.GetInt("saleprice")));
                }
                iManager.InitializeInventory();
            });
        });
    }

    public void UpdatePlayerCoin()
    {
        mManager.client.PayVault.Refresh(() =>
        {
            ui.CoinText.text = mManager.client.PayVault.Coins.ToString();
        });
    }

    void AddShopButton(int id)
    {
        var newButton = Instantiate(bundleButtonPrefab);
        newButton.transform.SetParent(shopContent.transform, false);
        newButton.GetComponent<Button>().onClick.AddListener(() => { ShowBundle(id); });
        newButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(lastButton.GetComponent<RectTransform>().anchoredPosition.x, lastButton.GetComponent<RectTransform>().anchoredPosition.y + offset);
        //newButton.transform.position = new Vector2(lastButton.transform.position.x, lastButton.transform.position.y + offset);
        newButton.GetComponentInChildren<Text>().text = bundleList[id];
        offset -= 66;
    }

    public void ShowBundle(int id)
    {
        foreach (var itemShowcase in itemPrefabs)
        {
            Destroy(itemShowcase);
        }
        itemPrefabs.Clear();

        var shopItems = ShopList.Where(x => x.BundleId == id).ToList();
        for (int i = 0; i < shopItems.Count; i++)
        {
            var itemShowcase = Instantiate(ItemPrefab);
            itemShowcase.transform.SetParent(BundleContent, false);
            itemPrefabs.Add(itemShowcase);
        }

        BundleName.text = bundleList[id];
        if (iManager.SpriteList.Count > shopItems[shopItems.Count - 1].Id)
        {
            for (int i = 0; i < shopItems.Count; i++)
            {
                itemPrefabs[i].GetComponent<ItemPrefabRef>().ItemSprite.sprite = iManager.SpriteList[shopItems[i].Id];
            }
        }
        else
        {
            ui.Popup("update plox", "Update your client pretty please.");
        }
        for (int i = 0; i < shopItems.Count; i++)
        {
            var itemPrefabRef = itemPrefabs[i].GetComponent<ItemPrefabRef>();
            var copyOfId = shopItems[i].Id;
            if (purchaseInProgress)
                itemPrefabRef.BuyButton.interactable = false;
            itemPrefabRef.ItemNameText.text = shopItems[i].Name;
            itemPrefabRef.ItemDescriptionText.text = shopItems[i].Description;
            itemPrefabRef.ItemPriceText.text = shopItems[i].Price.ToString();
            itemPrefabRef.BuyButton.onClick.RemoveAllListeners();
            itemPrefabRef.BuyButton.onClick.AddListener(() => { BuyFigure(copyOfId); });
        }
    }

    public void BuyFigure(int id)
    {
        foreach (var item in itemPrefabs)
        {
            item.GetComponent<ItemPrefabRef>().BuyButton.interactable = false;
        }
        purchaseInProgress = true;
        mManager.client.Multiplayer.CreateJoinRoom("Shop", "shop", true, null, null, (Connection con) =>
        {
            con.OnMessage += ShopMsgHandler;
            con.Send("buy", id);
        });
    }

    private void ShopMsgHandler(object sender, Message m)
    {
        purchaseInProgress = false;
        foreach (var item in itemPrefabs)
        {
            item.GetComponent<ItemPrefabRef>().BuyButton.interactable = true;
        }
        switch (m.Type)
        {
            case "unknownerror":
                ui.Popup("ERROR", "An unknown error happened on our side. Sorry for the inconvenience, please try again.");
                break;

            case "notenoughcoins":
                ui.Popup("You're poor", "You don't have enough coins for that item.");
                break;

            case "successfulpurchase":
                ui.Popup("Damn, fancypants", "You've successfully bought this item!", () =>
                {
                    ui.errorPanel.SetActive(false);
                    UpdatePlayerCoin();
                    iManager.InitializeInventory();
                });
                break;
            case "alreadyhave":
                ui.Popup("C'mon man,", "You already have this item. Stop making an idiot out of yourself, it's embarassing.");
                break;
        }
    }



    public void ShopNavButtonPressed()
    {
        ShopItemPanelSelected = true;
        invPanelAnimator.SetBool("InvSelected", false);
        StartCoroutine(InventoryFadeOut());
    }
    public void InvNavButtonPressed()
    {
        ShopItemPanelSelected = false;
        invPanelAnimator.gameObject.SetActive(true);
        invPanelAnimator.SetBool("InvSelected", true);
    }

    IEnumerator InventoryFadeOut()
    {
        yield return new WaitForSeconds(0.5f);
        invPanelAnimator.gameObject.SetActive(false);
    }
}

public class ShopItem
{
    public string Name;
    public int Id;
    public int BundleId;
    public string Description;
    public int Price;
    public int SalePrice;

    public ShopItem(string _name, int _id, int _bundleId, string _desc, int _price, int _salePrice)
    {
        Name = _name;
        Id = _id;
        BundleId = _bundleId;
        Description = _desc;
        Price = _price;
        SalePrice = _salePrice;
    }
}