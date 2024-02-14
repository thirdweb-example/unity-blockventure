using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Thirdweb;
using System.Numerics;
using Thirdweb.Redcode.Awaiting;

public class MarketplaceItem : MonoBehaviour
{
    public string tokenId = "0";
    public Button buttonListStone;
    public Button buttonListHerb;
    public Button buttonListWood;
    public Button buttonBuyStone;
    public Button buttonBuyHerb;
    public Button buttonBuyWood;
    public TMP_Text nameText;
    public TMP_Text ownedText;
    public Image image;

    public async void Initialize()
    {
        UIManager.Instance.SetMarketplaceLog("Loading...");

        buttonListStone.onClick.RemoveAllListeners();
        buttonListHerb.onClick.RemoveAllListeners();
        buttonListWood.onClick.RemoveAllListeners();
        buttonBuyStone.onClick.RemoveAllListeners();
        buttonBuyHerb.onClick.RemoveAllListeners();
        buttonBuyWood.onClick.RemoveAllListeners();

        buttonListStone.interactable = false;
        buttonListHerb.interactable = false;
        buttonListWood.interactable = false;
        buttonBuyStone.interactable = false;
        buttonBuyHerb.interactable = false;
        buttonBuyWood.interactable = false;

        var nftContract = ThirdwebManager.Instance.SDK.GetContract(GameContracts.NFT_Contract);
        var nft = await nftContract.ERC1155.Get(tokenId);
        nameText.text = nft.metadata.name;
        image.sprite = await ThirdwebManager.Instance.SDK.storage.DownloadImage(nft.metadata.image);

        var balance = await nftContract.ERC1155.Balance(tokenId);
        ownedText.text = "Owned: " + balance;
        if (balance != "0")
        {
            buttonListStone.onClick.AddListener(() => ListItem(GameContracts.STONE_CONTRACT));
            buttonListHerb.onClick.AddListener(() => ListItem(GameContracts.HERB_CONTRACT));
            buttonListWood.onClick.AddListener(() => ListItem(GameContracts.WOOD_CONTRACT));
        }

        var marketplaceContract = ThirdwebManager.Instance.SDK.GetContract(GameContracts.MARKETPLACE_CONTRACT);
        var allValidListings = await marketplaceContract.marketplace.directListings.GetAllValid();
        int playerStoneListings = 0;
        int playerHerbListings = 0;
        int playerWoodListings = 0;
        int globalStoneListings = 0;
        int globalHerbListings = 0;
        int globalWoodListings = 0;
        foreach (var listing in allValidListings)
        {
            if (listing.assetContractAddress == GameContracts.NFT_Contract && listing.tokenId == tokenId)
            {
                if (listing.currencyContractAddress == GameContracts.STONE_CONTRACT)
                {
                    if (listing.creatorAddress == GameManager.Instance.Address)
                    {
                        playerStoneListings++;
                        continue;
                    }
                    globalStoneListings++;
                    buttonBuyStone.interactable = true;
                    buttonBuyStone.onClick.RemoveAllListeners();
                    buttonBuyStone.onClick.AddListener(() => BuyItem(listing.id));
                }
                else if (listing.currencyContractAddress == GameContracts.HERB_CONTRACT)
                {
                    if (listing.creatorAddress == GameManager.Instance.Address)
                    {
                        playerHerbListings++;
                        continue;
                    }
                    globalHerbListings++;
                    buttonBuyHerb.interactable = true;
                    buttonBuyHerb.onClick.RemoveAllListeners();
                    buttonBuyHerb.onClick.AddListener(() => BuyItem(listing.id));
                }
                else if (listing.currencyContractAddress == GameContracts.WOOD_CONTRACT)
                {
                    if (listing.creatorAddress == GameManager.Instance.Address)
                    {
                        playerWoodListings++;
                        continue;
                    }
                    globalWoodListings++;
                    buttonBuyWood.interactable = true;
                    buttonBuyWood.onClick.RemoveAllListeners();
                    buttonBuyWood.onClick.AddListener(() => BuyItem(listing.id));
                }
            }
        }

        var nftBalance = int.Parse(balance);
        buttonListStone.GetComponentInChildren<TMP_Text>().text = "List for 1 Stone (" + (nftBalance - playerStoneListings) + ")";
        buttonListStone.interactable = nftBalance - playerStoneListings > 0;
        buttonListHerb.GetComponentInChildren<TMP_Text>().text = "List for 1 Herb (" + (nftBalance - playerHerbListings) + ")";
        buttonListHerb.interactable = nftBalance - playerHerbListings > 0;
        buttonListWood.GetComponentInChildren<TMP_Text>().text = "List for 1 Wood (" + (nftBalance - playerWoodListings) + ")";
        buttonListWood.interactable = nftBalance - playerWoodListings > 0;

        buttonBuyStone.GetComponentInChildren<TMP_Text>().text = "Buy for 1 Stone (" + globalStoneListings + ")";
        buttonBuyHerb.GetComponentInChildren<TMP_Text>().text = "Buy for 1 Herb (" + globalHerbListings + ")";
        buttonBuyWood.GetComponentInChildren<TMP_Text>().text = "Buy for 1 Wood (" + globalWoodListings + ")";

        UIManager.Instance.SetMarketplaceLog("");
    }

    private async void ListItem(string currencyAddress)
    {
        try
        {
            UIManager.Instance.SetMarketplaceLog("Listing item...");
            var contract = ThirdwebManager.Instance.SDK.GetContract(GameContracts.MARKETPLACE_CONTRACT);
            var result = await contract.marketplace.directListings.CreateListing(
                new CreateListingInput()
                {
                    assetContractAddress = GameContracts.NFT_Contract,
                    tokenId = tokenId,
                    currencyContractAddress = currencyAddress,
                    pricePerToken = "1",
                    quantity = "1"
                }
            );
            UIManager.Instance.SetMarketplaceLog("Item listed!");
            await GameManager.Instance.SyncResourceBalancesWithChain();
            Initialize();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            UIManager.Instance.SetMarketplaceLog("Error listing item: " + e.Message);
        }
    }

    private async void BuyItem(string listingId)
    {
        try
        {
            UIManager.Instance.SetMarketplaceLog("Buying item...");
            var contract = ThirdwebManager.Instance.SDK.GetContract(GameContracts.MARKETPLACE_CONTRACT);
            var result = await contract.marketplace.directListings.BuyFromListing(listingId, "1", GameManager.Instance.Address);
            UIManager.Instance.SetMarketplaceLog("Item bought!");
            await GameManager.Instance.SyncResourceBalancesWithChain();
            Initialize();
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            UIManager.Instance.SetMarketplaceLog("Error buying item: " + e.Message);
        }
    }
}
