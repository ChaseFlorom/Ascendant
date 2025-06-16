using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data container for a person's stats & info. 
/// Generated once and stored as an asset.
/// </summary>
[CreateAssetMenu(fileName = "New Person", menuName = "Person/New Person")]
public class Person_SO : ScriptableObject
{
    [Header("Personal Info")]
    public string firstName;
    public string lastName;
    public string alias;
    public int gender;

    [Header("Attributes (1�100)")]
    public int brawn;
    public int speed;
    public int aerial;
    public int looks;
    public int stamina;
    public int mic;



    [Header("Card Deck")]
    public List<CardSO> cardDeck = new List<CardSO>();

    private bool initialized = false;

    private void OnEnable()
    {
        // Only randomize stats once
        if (!initialized)
        {
            brawn = Random.Range(1, 101);
            speed = Random.Range(1, 101);
            aerial = Random.Range(1, 101);
            looks = Random.Range(1, 101);
            mic = Random.Range(1, 101);
            stamina = Random.Range(1, 101);

            initialized = true;
        }
    }
}
