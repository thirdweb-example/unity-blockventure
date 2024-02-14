using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Thirdweb;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Thirdweb.Redcode.Awaiting;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _mainMenuPanel;

    [SerializeField]
    private GameObject _movingPanel;

    [SerializeField]
    private GameObject _gatheringPanel;

    [SerializeField]
    private GameObject _shoppingPanel;

    [SerializeField]
    private GameObject _tradingPanel;

    [SerializeField]
    private GameObject _gameOverPanel;

    [SerializeField]
    private TMP_Text _stoneText;

    [SerializeField]
    private TMP_Text _herbText;

    [SerializeField]
    private TMP_Text _woodText;

    [SerializeField]
    private TMP_Text _totalItemsOwnedText;

    [SerializeField]
    private TMP_Text _shopLog;

    [SerializeField]
    private Button _playButton;

    [SerializeField]
    private TMP_Text _marketplaceLog;

    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LateUpdate()
    {
        if (GameManager.Instance.CurrentGameState == GameState.MainMenu)
        {
            _playButton.interactable = GameManager.Instance.Ready;
            _playButton.GetComponentInChildren<TMP_Text>().text = GameManager.Instance.Ready ? "Play" : "Loading";
        }

        _stoneText.text = "Stone: " + GameManager.Instance.StoneCount + "/" + GameManager.Instance.TotalStoneCount;
        _herbText.text = "Herbs: " + GameManager.Instance.HerbCount + "/" + GameManager.Instance.TotalHerbCount;
        _woodText.text = "Wood: " + GameManager.Instance.WoodCount + "/" + GameManager.Instance.TotalWoodCount;

        if (GameManager.Instance.CurrentGameState == GameState.Shopping)
        {
            BigInteger totalItemsOwned = 0;
            var balances = GameManager.Instance.NFTBalances;
            foreach (var nft in balances)
                totalItemsOwned += nft.Value;
            _totalItemsOwnedText.text = "Total Items Owned: " + totalItemsOwned;
        }
    }

    public void UpdateUI(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.MainMenu:
                _mainMenuPanel.SetActive(true);
                _movingPanel.SetActive(false);
                _gatheringPanel.SetActive(false);
                _shoppingPanel.SetActive(false);
                _tradingPanel.SetActive(false);
                _gameOverPanel.SetActive(false);
                break;
            case GameState.Moving:
                _mainMenuPanel.SetActive(false);
                _movingPanel.SetActive(true);
                _gatheringPanel.SetActive(false);
                _shoppingPanel.SetActive(false);
                _tradingPanel.SetActive(false);
                _gameOverPanel.SetActive(false);
                break;
            case GameState.Gathering:
                _gatheringPanel.SetActive(true);
                break;
            case GameState.Shopping:
                _shoppingPanel.SetActive(true);
                var shopItems = FindObjectsOfType<ShopItem>();
                foreach (var item in shopItems)
                    item.Initialize();
                break;
            case GameState.Trading:
                _tradingPanel.SetActive(true);
                var marketplaceItems = FindObjectsOfType<MarketplaceItem>();
                foreach (var item in marketplaceItems)
                    item.Initialize();
                break;
            case GameState.GameOver:
                _mainMenuPanel.SetActive(false);
                _movingPanel.SetActive(false);
                _gatheringPanel.SetActive(false);
                _shoppingPanel.SetActive(false);
                _tradingPanel.SetActive(false);
                _gameOverPanel.SetActive(true);
                break;
        }
    }

    internal void SetShopLog(string v)
    {
        _shopLog.text = v;
    }

    internal void SetMarketplaceLog(string v)
    {
        _marketplaceLog.text = v;
    }
}
