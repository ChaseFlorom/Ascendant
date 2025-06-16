using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class CardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private Image cardImage;
    [SerializeField] private TextMeshProUGUI movementSpeedText;

    [Header("Hover Effect")]
    [SerializeField] private float hoverYOffset = 40f;
    [SerializeField] private float hoverScale = 1.15f;
    [SerializeField] private float hoverAnimSpeed = 10f;

    private Card cardData;
    private RectTransform rectTransform;
    private Vector3 originalScale;
    private bool isHovered = false;

    private Vector2 targetAnchoredPos;
    private Vector3 targetScale;

    public bool IsHovered => isHovered;
    public float HoverYOffset => hoverYOffset;
    public float HoverScale => hoverScale;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
        targetAnchoredPos = rectTransform.anchoredPosition;
        targetScale = rectTransform.localScale;
    }

    public void Initialize(Card card)
    {
        cardData = card;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (cardData == null) return;

        if (titleText != null) titleText.text = cardData.CardName;
        if (bodyText != null) bodyText.text = cardData.CardBody;
        if (cardImage != null) cardImage.sprite = cardData.CardImage;
        if (movementSpeedText != null) movementSpeedText.text = cardData.MovementSpeed.ToString();
    }

    public void SetHighlighted(bool highlighted)
    {
        // TODO: Implement card highlighting
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    public void SetTargetPosition(Vector2 anchoredPos)
    {
        targetAnchoredPos = anchoredPos;
    }

    public void SetTargetScale(Vector3 scale)
    {
        targetScale = scale;
    }

    private void Update()
    {
        // Animate position and scale towards targets
        rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetAnchoredPos, Time.deltaTime * hoverAnimSpeed);
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.deltaTime * hoverAnimSpeed);
    }
} 