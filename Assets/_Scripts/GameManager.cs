using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Thirdweb;
using UnityEngine;
using WalletConnectSharp.Sign.Models.Engine;
using ZXing.Client.Result;

public enum GameState
{
    MainMenu,
    Moving,
    Gathering,
    Shopping,
    Trading,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public GameState CurrentGameState { get; private set; }

    public static GameManager Instance { get; private set; }
    public int StoneCount { get; private set; }
    public int HerbCount { get; private set; }
    public int WoodCount { get; private set; }
    public int TotalStoneCount { get; private set; }
    public int TotalHerbCount { get; private set; }
    public int TotalWoodCount { get; private set; }
    public Dictionary<string, BigInteger> NFTBalances { get; private set; } = new();
    public string Address { get; private set; }

    public bool Ready { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        TotalStoneCount = FindObjectsOfType<Rock>().Length;
        TotalHerbCount = FindObjectsOfType<Herb>().Length;
        TotalWoodCount = FindObjectsOfType<Tree>().Length;
    }

    private void Start()
    {
        Connect();
        CurrentGameState = GameState.MainMenu;
        UIManager.Instance.UpdateUI(GameState.MainMenu);
        CameraManager.Instance.UpdateCameraMode(GameState.MainMenu);
    }

    public void SetGameState(string gameState)
    {
        GameState state = (GameState)System.Enum.Parse(typeof(GameState), gameState);
        SetGameState(state);
    }

    internal void SetGameState(GameState gameState)
    {
        if (!Ready)
            return;

        CurrentGameState = gameState;
        UIManager.Instance.UpdateUI(gameState);
        CameraManager.Instance.UpdateCameraMode(gameState);
    }

    internal async void AddStone()
    {
        StoneCount++;
        var contract = ThirdwebManager.Instance.SDK.GetContract(GameContracts.STONE_CONTRACT);
        await contract.ERC20.Claim("1");
    }

    internal async void AddHerb()
    {
        HerbCount++;
        var contract = ThirdwebManager.Instance.SDK.GetContract(GameContracts.HERB_CONTRACT);
        await contract.ERC20.Claim("1");
    }

    internal async void AddWood()
    {
        WoodCount++;
        var contract = ThirdwebManager.Instance.SDK.GetContract(GameContracts.WOOD_CONTRACT);
        await contract.ERC20.Claim("1");
    }

    internal async Task SyncResourceBalancesWithChain()
    {
        var stone = ThirdwebManager.Instance.SDK.GetContract(GameContracts.STONE_CONTRACT);
        var herb = ThirdwebManager.Instance.SDK.GetContract(GameContracts.HERB_CONTRACT);
        var wood = ThirdwebManager.Instance.SDK.GetContract(GameContracts.WOOD_CONTRACT);
        StoneCount = int.Parse((await stone.ERC20.Balance()).value.ToEth(0, false));
        HerbCount = int.Parse((await herb.ERC20.Balance()).value.ToEth(0, false));
        WoodCount = int.Parse((await wood.ERC20.Balance()).value.ToEth(0, false));

        var nftsOwned = await ThirdwebManager.Instance.SDK.GetContract(GameContracts.NFT_Contract).ERC1155.GetOwned();
        NFTBalances = new Dictionary<string, BigInteger>();
        foreach (var nft in nftsOwned)
            NFTBalances[nft.metadata.id] = nft.quantityOwned;
    }

    private async void Connect()
    {
        Ready = false;
        var connection = new WalletConnection(provider: WalletProvider.SmartWallet, chainId: 421614, personalWallet: WalletProvider.LocalWallet);
        Address = await ThirdwebManager.Instance.SDK.wallet.Connect(connection);

        // Set erc20 allowances for shop once
        Debug.Log("Setting ERC20 allowances for Shop");
        var stone = ThirdwebManager.Instance.SDK.GetContract(GameContracts.STONE_CONTRACT);
        var herb = ThirdwebManager.Instance.SDK.GetContract(GameContracts.HERB_CONTRACT);
        var wood = ThirdwebManager.Instance.SDK.GetContract(GameContracts.WOOD_CONTRACT);
        if (BigInteger.Parse((await stone.ERC20.Allowance(GameContracts.NFT_Contract)).value) < 100)
            await stone.ERC20.SetAllowance(GameContracts.NFT_Contract, "1000000");
        if (BigInteger.Parse((await herb.ERC20.Allowance(GameContracts.NFT_Contract)).value) < 100)
            await herb.ERC20.SetAllowance(GameContracts.NFT_Contract, "1000000");
        if (BigInteger.Parse((await wood.ERC20.Allowance(GameContracts.NFT_Contract)).value) < 100)
            await wood.ERC20.SetAllowance(GameContracts.NFT_Contract, "1000000");

        // set erc20 allowance for marketplace once
        Debug.Log("Setting ERC20 allowances for Marketplace");
        if (BigInteger.Parse((await stone.ERC20.Allowance(GameContracts.MARKETPLACE_CONTRACT)).value) < 100)
            await stone.ERC20.SetAllowance(GameContracts.MARKETPLACE_CONTRACT, "1000000");
        if (BigInteger.Parse((await herb.ERC20.Allowance(GameContracts.MARKETPLACE_CONTRACT)).value) < 100)
            await herb.ERC20.SetAllowance(GameContracts.MARKETPLACE_CONTRACT, "100000");
        if (BigInteger.Parse((await wood.ERC20.Allowance(GameContracts.MARKETPLACE_CONTRACT)).value) < 100)
            await wood.ERC20.SetAllowance(GameContracts.MARKETPLACE_CONTRACT, "1000000");

        // set erc1155 allowance for marketplace once
        Debug.Log("Setting ERC1155 allowances for Marketplace");
        var nft = ThirdwebManager.Instance.SDK.GetContract(GameContracts.NFT_Contract);
        if (!bool.Parse(await nft.ERC1155.IsApprovedForAll(Address, GameContracts.MARKETPLACE_CONTRACT)))
        {
            await nft.ERC1155.SetApprovalForAll(GameContracts.MARKETPLACE_CONTRACT, true);
        }

        await SyncResourceBalancesWithChain();
        Ready = true;
    }
}
