using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Thirdweb;
using System.Numerics;

public class ShopItem : MonoBehaviour
{
    public string tokenId = "0";
    public int amountToClaim = 1;
    public Button button;
    public TMP_Text nameText;
    public TMP_Text costText;
    public Image image;

    public async void Initialize()
    {
        button.onClick.RemoveAllListeners();

        var contract = ThirdwebManager.Instance.SDK.GetContract(GameContracts.NFT_Contract);
        var nft = await contract.ERC1155.Get(tokenId);
        nameText.text = nft.metadata.name;
        ClaimConditions costs = await contract.ERC1155.claimConditions.GetActive(tokenId);

        costText.text = "";
        if (costs.currencyAddress == GameContracts.STONE_CONTRACT)
            costText.text += costs.currencyMetadata.value.ToEth(0, false) + " Stone";
        else if (costs.currencyAddress == GameContracts.HERB_CONTRACT)
            costText.text += costs.currencyMetadata.value.ToEth(0, false) + " Herbs";
        else if (costs.currencyAddress == GameContracts.WOOD_CONTRACT)
            costText.text += costs.currencyMetadata.value.ToEth(0, false) + " Wood";
        else
            costText.text += costs.currencyMetadata.value.ToEth(0, false) + " " + costs.currencyMetadata.symbol;

        button.onClick.AddListener(ClaimItem);

        image.sprite = await ThirdwebManager.Instance.SDK.storage.DownloadImage(nft.metadata.image);
    }

    private async void ClaimItem()
    {
        try
        {
            UIManager.Instance.SetShopLog("Claiming item...");
            var contract = ThirdwebManager.Instance.SDK.GetContract(GameContracts.NFT_Contract);
            var result = await contract.ERC1155.Claim(tokenId, amountToClaim);
            var balance = await contract.ERC1155.Balance(tokenId);
            await GameManager.Instance.SyncResourceBalancesWithChain();
            UIManager.Instance.SetShopLog("Item claimed!");
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            UIManager.Instance.SetShopLog("Error claiming item: " + e.Message);
        }
    }
}
