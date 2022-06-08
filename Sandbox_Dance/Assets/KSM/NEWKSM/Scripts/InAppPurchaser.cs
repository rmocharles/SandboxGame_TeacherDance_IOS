using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using BackEnd;
using UnityEngine.Purchasing;
using UnityEngine.SceneManagement;

public class InAppPurchaser : MonoBehaviour, IStoreListener
{
    public static InAppPurchaser instance;

    private static IStoreController storeController;
    private static IExtensionProvider extensionProvider;

    public const string PACKAGE = "com.sandbox_game.dance.package";
    public const string COIN_2000 = "com.sandbox_game.dance.coin2000";
    public const string COIN_5000 = "com.sandbox_game.dance.coin5000";

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        InitializePurchasing();
    }

    bool IsInitialized()
    {
        return (storeController != null && extensionProvider != null);
    }
    
    void InitializePurchasing()
    {
        if (IsInitialized()) return;


        var module = StandardPurchasingModule.Instance();

        ConfigurationBuilder builder = ConfigurationBuilder.Instance(module);

        builder.AddProduct(PACKAGE, ProductType.NonConsumable);
        builder.AddProduct(COIN_2000, ProductType.Consumable);
        builder.AddProduct(COIN_5000, ProductType.Consumable);


        UnityPurchasing.Initialize(this, builder);
        Debug.Log("##### InitializePurchasing : Initialize");
    }

    void BuyProductID(string productId)
    {
        if (SceneManager.GetActiveScene().name == "2. Lobby")
        {
            LobbyUI.GetInstance().loadingObject.SetActive(true);
        }
        else
            GameManager.GetInstance().loadingObject.SetActive(true);
        // If Purchasing has been initialized ...
        if (IsInitialized())
        {
            

            // ... look up the Product reference with the general product identifier and the Purchasing 
            // system's products collection.
            Product product = storeController.products.WithID(productId);

            // If the look up found a product for this device's store and that product is ready to be sold ... 
            if (product != null && product.availableToPurchase)
            {
                Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                // ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
                // asynchronously.

                

                storeController.InitiatePurchase(product);
            }
            // Otherwise ...
            else
            {

                if (SceneManager.GetActiveScene().name == "2. Lobby")
                {
                    LobbyUI.GetInstance().loadingObject.SetActive(false);
                }
                else
                    GameManager.GetInstance().loadingObject.SetActive(false);
                // ... report the product look-up failure situation  
                Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
            }
        }
        // Otherwise ...
        else
        {
            if (SceneManager.GetActiveScene().name == "2. Lobby")
            {
                LobbyUI.GetInstance().errorObject.SetActive(true);
                LobbyUI.GetInstance().loadingObject.SetActive(false);

                LobbyUI.GetInstance().errorObject.GetComponentInChildren<Text>().text = PlayerPrefs.GetInt("LangIndex") == 0 ? "결제 실패" : "Purchase failed";
            }
            else
            {
                GameManager.GetInstance().errorObject.SetActive(true);
                GameManager.GetInstance().loadingObject.SetActive(false);

                GameManager.GetInstance().errorObject.GetComponentInChildren<Text>().text = PlayerPrefs.GetInt("LangIndex") == 0 ? "결제 실패" : "Purchase failed";
            }
            // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
            // retrying initiailization.
            Debug.Log("BuyProductID FAIL. Not initialized.");
        }
    }

    public void OnInitialized(IStoreController _sc, IExtensionProvider _ep)
    {
        storeController = _sc;
        extensionProvider = _ep;
    }

    public void OnInitializeFailed(InitializationFailureReason reason)
    {

    }

    // ====================================================================================================
    #region 영수증 검증
    

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        /*
        뒤끝 영수증 검증 처리
        */
        Debug.LogError("Receipt");
        if (SceneManager.GetActiveScene().name == "2. Lobby")
        {
            LobbyUI.GetInstance().loadingObject.SetActive(false);
        }
        else
            GameManager.GetInstance().loadingObject.SetActive(false);

        Debug.Log(args.purchasedProduct.availableToPurchase);
        // 뒤끝 영수증 검증 처리    
        BackendReturnObject validation = null;
#if UNITY_ANDROID || UNITY_EDITOR
        validation = Backend.Receipt.IsValidateGooglePurchase(args.purchasedProduct.receipt, "receiptDescriptionGoogle");
#elif UNITY_IOS
        validation = Backend.Receipt.IsValidateApplePurchase(args.purchasedProduct.receipt, "receiptDescriptionApple");
#endif

            if (validation.IsSuccess())
            {
                if (String.Equals(args.purchasedProduct.definition.id, COIN_2000, StringComparison.Ordinal))
                {
                    Debug.LogError("3000!!");
                    if (SceneManager.GetActiveScene().name == "2. Lobby")
                    {
                        BackendServerManager.GetInstance().SaveGold(LobbyUI.GetInstance().gold + 2000);
                    }
                    else
                    {
                        BackendServerManager.GetInstance().SaveGold(GameManager.GetInstance().gold + 2000);
                    }
                }
                else if (String.Equals(args.purchasedProduct.definition.id, COIN_5000, StringComparison.Ordinal))
                {
                    Debug.LogError("5000!!");

                    if (SceneManager.GetActiveScene().name == "2. Lobby")
                    {
                        BackendServerManager.GetInstance().SaveGold(LobbyUI.GetInstance().gold + 5000);
                    }
                    else
                    {
                        BackendServerManager.GetInstance().SaveGold(GameManager.GetInstance().gold + 5000);
                    }
                }
                else if (String.Equals(args.purchasedProduct.definition.id, PACKAGE, StringComparison.Ordinal))
                {
                    PlayerPrefs.SetInt("PACKAGE", 1);
                    Debug.LogError("PACKAGE Purchase : " + PlayerPrefs.GetInt("PACKAGE"));


                    if (SceneManager.GetActiveScene().name == "2. Lobby")
                    {
                        BackendServerManager.GetInstance().SaveGold(LobbyUI.GetInstance().gold + 3000);
                    }
                    else
                    {
                        BackendServerManager.GetInstance().SaveGold(GameManager.GetInstance().gold + 3000);
                    }
                    BackendServerManager.GetInstance().SaveItem(5, 5, 5);
                }

                if (SceneManager.GetActiveScene().name == "2. Lobby")
                {
                    LobbyUI.GetInstance().errorObject.GetComponentInChildren<Text>().text = PlayerPrefs.GetInt("LangIndex") == 0 ? "구매 성공" : "Purchase success";
                    LobbyUI.GetInstance().errorObject.SetActive(true);

                    SoundManager.Instance.PlayEffect(4);
                }
                else if (SceneManager.GetActiveScene().name == "3. Game")
                {
                    GameManager.GetInstance().errorObject.GetComponentInChildren<Text>().text = PlayerPrefs.GetInt("LangIndex") == 0 ? "구매 성공" : "Purchase success";
                    GameManager.GetInstance().errorObject.SetActive(true);

                    SoundManager.Instance.PlayEffect(13);
                }
                LobbyUI.GetInstance().errorObject.GetComponentInChildren<Text>().text = PlayerPrefs.GetInt("LangIndex") == 0 ? BackendServerManager.GetInstance().langaugeSheet[20].kor : BackendServerManager.GetInstance().langaugeSheet[20].eng;
            }

            else
            {
                //if (SceneManager.GetActiveScene().name == "2. Lobby")
                //{
                //    LobbyUI.GetInstance().errorObject.SetActive(true);
                //    LobbyUI.GetInstance().errorObject.GetComponentInChildren<Text>().text = PlayerPrefs.GetInt("LangIndex") == 0 ? "결제 실패" : "Purchase failed";
                //}
                //else
                //{
                //    GameManager.GetInstance().errorObject.SetActive(true);
                //    GameManager.GetInstance().errorObject.GetComponentInChildren<Text>().text = PlayerPrefs.GetInt("LangIndex") == 0 ? "결제 실패" : "Purchase failed";
                //}
                    
            }

         
        

        // Return a flag indicating whether this product has completely been received, or if the application needs 
        // to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
        // saving purchased products to the cloud, and when that save is delayed. 
        return PurchaseProcessingResult.Complete;
    }
    #endregion

    // ====================================================================================================	

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        if (SceneManager.GetActiveScene().name == "2. Lobby")
        {
            LobbyUI.GetInstance().loadingObject.SetActive(false);
        }
        else
            GameManager.GetInstance().loadingObject.SetActive(false);
    }

    // ==================================================
    public void BuyItem(int num)
    {
        switch (num)
        {
            case 0:
                Debug.Log("패키지 누름");
                BuyProductID(PACKAGE);
                break;
            case 1:
                BuyProductID(COIN_2000);
                break;
            case 2:
                BuyProductID(COIN_5000);
                break;
        }
    }
}
