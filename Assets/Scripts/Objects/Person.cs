using UnityEngine;
using System;
using NUnit.Framework;
using System.Collections.Generic;

/// <summary>
/// A runtime "wrestler" object built from a Person_SO.
/// This is what we'll actually place in factions and matches.
/// </summary>
public class Person
{
    // Personal Info
    public string firstName;
    public string lastName;
    public string alias;
    public int gender;

    // Attributes
    public int brawn;
    public int speed;
    public int aerial;
    public int looks;
    public int mic;
    public float stamina;
    public float energy = 95;
    public Dictionary<string, float> Damage { get; private set; } = new Dictionary<string, float>()
    {
        { "Head", 0 },
        { "Torso", 0 },
        { "Arms", 0 },
        { "Legs", 0 }
    };

    public float Fatigue { get; private set; } = 0; // Builds over time

    // Card Deck
    public List<Card> cardDeck = new List<Card>();

    // Unique ID so two "Bob Jones" won't collide
    public Guid uniqueID;

    public Person(Person_SO so)
    {
        // Copy data from the ScriptableObject into this Person
        firstName = so.firstName;
        lastName = so.lastName;
        alias = so.alias;
        gender = so.gender;
        stamina = so.stamina;
        brawn = so.brawn;
        speed = so.speed;
        aerial = so.aerial;
        looks = so.looks;
        mic = so.mic;

        // Convert CardSOs to Cards
        foreach (CardSO cardSO in so.cardDeck)
        {
            GameObject cardObj = new GameObject(cardSO.CardName);
            Card card = cardObj.AddComponent<Card>();
            card.Initialize(cardSO);
            cardDeck.Add(card);
        }

        // Generate a unique ID to differentiate two wrestlers with the same name
        uniqueID = Guid.NewGuid();
    }

    public void ShuffleDeck()
    {
        // Fisher-Yates shuffle
        for (int i = cardDeck.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            Card temp = cardDeck[i];
            cardDeck[i] = cardDeck[j];
            cardDeck[j] = temp;
        }
    }
}
