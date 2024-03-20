using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoinCollection : MonoBehaviour
{
    private int coinCount;
    public TextMeshProUGUI coinText;
    public AudioClip coinCollectSound;
    private AudioSource audioSource;
    public GameOverScreen GameOverScreen;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Coin"))
        {
            coinCount++;
            CoinTracker.setCointCount(coinCount);
            coinText.text = "Coins: " + coinCount;
            Destroy(collider.gameObject);


            if (coinCollectSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(coinCollectSound);
            }
        }

        if (collider.gameObject.CompareTag("Spike"))
        {
            if (coinCount > 0)
            {
                coinCount--;
                CoinTracker.setCointCount(coinCount);
                coinText.text = "Coins: " + coinCount;
                Debug.Log("HIT");


            } 
            else
            {
                //GameOverScreen.Setup();
            }
        }
    }
}

public static class CoinTracker
{
    private static int coinCount = 0;

    public static int getCoinCount()
    {
        return coinCount;
    }

    public static void setCointCount(int setCoins)
    {
        coinCount = setCoins;
    }

    public static void incrementCoinCount()
    {
        coinCount++;
    }

    public static void decrementCoinCount()
    {
        coinCount--;
    }
}
