using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Wrestleverse/Card")]
public class CardSO : ScriptableObject
{
    [SerializeField] private string cardName;
    [SerializeField] [TextArea(3, 10)] private string cardBody;
    [SerializeField] private Sprite cardImage;
    [SerializeField] private float movementSpeed;
    [SerializeField] private int attack;

    public string CardName => cardName;
    public string CardBody => cardBody;
    public Sprite CardImage => cardImage;
    public float MovementSpeed => movementSpeed;
    public int Attack => attack;
} 