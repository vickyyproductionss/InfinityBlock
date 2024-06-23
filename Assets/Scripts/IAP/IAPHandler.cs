using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;

public class IAPHandler : MonoBehaviour, IStoreListener, IDetailedStoreListener
{
    private static IStoreController storeController;
    private static IExtensionProvider extensionProvider;
    [SerializeField] private List<string> consumableProducts;
    [SerializeField] private List<string> subscriptionProducts;

    // Initialize the IAP Service
    void Start()
    {
        if (storeController == null)
        {
            InitializePurchasing();
        }
    }

    // Initialize purchasing system
    public void InitializePurchasing()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance(AppStore.GooglePlay));

        // Add products here
        foreach (var product in consumableProducts)
        {
            builder.AddProduct(product,ProductType.Consumable);
        }
        foreach (var subscription in subscriptionProducts)
        {
            builder.AddProduct(subscription,ProductType.Subscription);
        }

        UnityPurchasing.Initialize(this, builder);
    }

    // Handle product purchase request
    public void BuyProduct(string productId)
    {
        if (storeController != null)
        {
            Product product = storeController.products.WithID(productId);
            if (product != null && product.availableToPurchase)
            {
                storeController.InitiatePurchase(product);
            }
            else
            {
                Debug.Log("BuyProduct: FAIL. Not purchasing product, either is not found or is not available for purchase.");
            }
        }
        else
        {
            Debug.Log("BuyProduct: FAIL. Not initialized.");
        }
    }

    // Called when Unity IAP system is initialized
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        extensionProvider = extensions;
    }

    // Called when Unity IAP initialization fails
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
    }

    // Called when a purchase is completed
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        Debug.Log("ProcessPurchase: PASS. Product: " + args.purchasedProduct.definition.id);

        // Add logic to process the purchased product
        switch (args.purchasedProduct.definition.id)
        {
            case "consumable_product_id":
                Debug.Log("Consumable product purchased.");
                // Handle consumable product purchase
                break;

            case "subscription_product_id":
                Debug.Log("Subscription product purchased.");
                // Handle subscription product purchase
                break;

            default:
                Debug.Log("ProcessPurchase: FAIL. Unrecognized product: " + args.purchasedProduct.definition.id);
                break;
        }

        return PurchaseProcessingResult.Complete;
    }

    // Called when a purchase fails
    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.Log("Initialisation Failed!!!");
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {

        Debug.Log($"OnPurchaseFailed: FAIL. Product: '{product}', PurchaseFailureReason: {failureReason}");
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.Log($"OnPurchaseFailed: FAIL. Product: '{product}', PurchaseFailureReason: {failureDescription}");
    }
}
public enum ProductIds
{
    Consumable_100Gems,
    Subscription_Premium
}