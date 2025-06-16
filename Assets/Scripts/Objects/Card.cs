using UnityEngine;

public class Card : MonoBehaviour
{
    [SerializeField] private string cardName;
    [SerializeField] private string cardBody;
    [SerializeField] private Sprite cardImage;
    [SerializeField] private float movementSpeed;
    [SerializeField] private int attack;

    public string CardName => cardName;
    public string CardBody => cardBody;
    public Sprite CardImage => cardImage;
    public float MovementSpeed => movementSpeed;
    public int Attack => attack;

    public void Initialize(CardSO cardData)
    {
        cardName = cardData.CardName;
        cardBody = cardData.CardBody;
        cardImage = cardData.CardImage;
        movementSpeed = cardData.MovementSpeed;
        attack = cardData.Attack;
    }
} 