using UnityEngine;
using System.Collections.Generic;

public class MatchEngine : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private Person_SO playerPersonSO;
    [SerializeField] private Hand playerHand;

    [Header("Card Prefab")]
    [SerializeField] private GameObject cardPrefab;

    private const int STARTING_HAND_SIZE = 3;
    private Person playerPerson;

    private void Start()
    {
        if (playerPersonSO == null || playerHand == null || cardPrefab == null)
        {
            Debug.LogError("MatchEngine: Required references are missing!");
            return;
        }

        // Create the player person instance
        playerPerson = new Person(playerPersonSO);
        
        // Shuffle the deck before drawing
        playerPerson.ShuffleDeck();
        
        DrawStartingHand();
    }

    private void DrawStartingHand()
    {
        if (playerPerson.cardDeck.Count == 0)
        {
            Debug.LogWarning("Player has no cards in their deck!");
            return;
        }

        int cardsToDraw = Mathf.Min(STARTING_HAND_SIZE, playerPerson.cardDeck.Count);
        
        for (int i = 0; i < cardsToDraw; i++)
        {
            DrawCard(playerPerson);
        }
    }

    private void DrawCard(Person person)
    {
        if (person.cardDeck.Count == 0) return;

        // Get the card data
        Card cardData = person.cardDeck[0];
        person.cardDeck.RemoveAt(0);

        // Create the card view
        GameObject cardObj = Instantiate(cardPrefab);
        CardView cardView = cardObj.GetComponent<CardView>();
        
        if (cardView == null)
        {
            Debug.LogError("Card prefab is missing CardView component!");
            Destroy(cardObj);
            return;
        }

        // Initialize and add to hand
        cardView.Initialize(cardData);
        playerHand.AddCard(cardView);
    }
} 