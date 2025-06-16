using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(RectTransform))]
public class Hand : MonoBehaviour
{
    [Header("Layout Settings")]
    [SerializeField] private float cardSpacing = 100f;
    [SerializeField] private float cardScale = 1f;
    [SerializeField] private float cardRotation = 0f;
    [SerializeField] private float cardYOffset = 0f;
    [SerializeField] private float firstCardXOffset = 0f;

    private List<CardView> cardsInHand = new List<CardView>();
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void AddCard(CardView card)
    {
        card.transform.SetParent(transform, false);
        cardsInHand.Add(card);
        UpdateCardPositions();
    }

    public void RemoveCard(CardView card)
    {
        if (cardsInHand.Remove(card))
        {
            UpdateCardPositions();
        }
    }

    private void UpdateCardPositions()
    {
        float totalWidth = (cardsInHand.Count - 1) * cardSpacing;
        float startX = firstCardXOffset;

        for (int i = 0; i < cardsInHand.Count; i++)
        {
            CardView cardView = cardsInHand[i];
            RectTransform cardRect = cardView.GetComponent<RectTransform>();
            float yOffset = cardYOffset;
            if (cardView.IsHovered)
            {
                yOffset += cardView.HoverYOffset;
            }
            Vector2 targetPos = new Vector2(startX + (i * cardSpacing), yOffset);
            cardView.SetTargetPosition(targetPos);

            // Set target scale: use hoverScale if hovered, otherwise cardScale
            Vector3 targetScale = Vector3.one * cardScale;
            if (cardView.IsHovered)
            {
                targetScale *= cardView.HoverScale;
            }
            cardView.SetTargetScale(targetScale);

            cardRect.localRotation = Quaternion.Euler(0f, 0f, cardRotation);
        }
    }

    public void Clear()
    {
        foreach (CardView card in cardsInHand)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }
        cardsInHand.Clear();
    }

    private void Update()
    {
        // Continuously update positions to reflect hover state
        UpdateCardPositions();
    }
} 