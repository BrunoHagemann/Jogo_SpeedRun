using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class MenuButtonHoverAnimator : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
    {
        [SerializeField] private TMP_Text m_Label;
        [SerializeField, Range(1f, 1.1f)] private float m_HoverScale = 1.045f;
        [SerializeField, Range(0f, 1f)] private float m_GlowOpacity = 0.38f;
        [SerializeField, Min(0.1f)] private float m_GlowDistance = 1.5f;
        [SerializeField, Min(0.01f)] private float m_TransitionDuration = 0.18f;

        private Vector3 m_OriginalScale;
        private Outline m_Glow;
        private Tween m_ScaleTween;
        private Tween m_GlowTween;
        private bool m_PointerInside;
        private static Texture2D s_HoverCursor;

        private void Awake()
        {
            if (m_Label == null)
                m_Label = GetComponent<TMP_Text>();

            if (m_Label == null)
            {
                Debug.LogWarning("MenuButtonHoverAnimator must be on a TMP text object.", this);
                enabled = false;
                return;
            }

            Button button = GetComponentInParent<Button>();
            if (button != null)
            {
                button.transition = Selectable.Transition.None;
                Image background = button.GetComponent<Image>();
                if (background != null)
                {
                    Color backgroundColor = background.color;
                    backgroundColor.a = 0f;
                    background.color = backgroundColor;
                    background.raycastTarget = true;
                }
            }

            m_OriginalScale = m_Label.transform.localScale;
            m_Label.color = Color.white;
            m_Label.raycastTarget = true;
            m_Glow = m_Label.GetComponent<Outline>();
            if (m_Glow == null)
                m_Glow = m_Label.gameObject.AddComponent<Outline>();

            m_Glow.effectColor = new Color(1f, 1f, 1f, 0f);
            m_Glow.effectDistance = new Vector2(m_GlowDistance, m_GlowDistance);
            m_Glow.useGraphicAlpha = true;
            m_Glow.enabled = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            UpdateHoverState(IsPointerOverGlyph(eventData));
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            UpdateHoverState(IsPointerOverGlyph(eventData));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UpdateHoverState(false);
        }

        private bool IsPointerOverGlyph(PointerEventData eventData)
        {
            return TMP_TextUtilities.FindIntersectingCharacter(
                m_Label, eventData.position, eventData.enterEventCamera, true) >= 0;
        }

        private void UpdateHoverState(bool isHovered)
        {
            if (m_PointerInside == isHovered)
                return;

            m_PointerInside = isHovered;
            Cursor.SetCursor(isHovered ? GetHoverCursor() : null, new Vector2(8f, 3f), CursorMode.Auto);

            m_ScaleTween?.Kill();
            m_ScaleTween = m_Label.transform
                .DOScale(isHovered ? m_OriginalScale * m_HoverScale : m_OriginalScale, m_TransitionDuration)
                .SetEase(isHovered ? Ease.OutSine : Ease.OutCubic)
                .SetUpdate(true);

            m_GlowTween?.Kill();
            if (isHovered)
                m_Glow.enabled = true;

            Color targetColor = new Color(1f, 1f, 1f, isHovered ? m_GlowOpacity : 0f);
            m_GlowTween = DOTween.To(
                    () => m_Glow.effectColor,
                    color => m_Glow.effectColor = color,
                    targetColor,
                    m_TransitionDuration)
                .SetEase(Ease.OutSine)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (!m_PointerInside)
                        m_Glow.enabled = false;
                });
        }

        private static Texture2D GetHoverCursor()
        {
            if (s_HoverCursor != null)
                return s_HoverCursor;

            const int size = 24;
            bool[,] hand = new bool[size, size];
            Fill(hand, 7, 8, 10, 21);
            Fill(hand, 10, 8, 13, 16);
            Fill(hand, 13, 8, 16, 15);
            Fill(hand, 16, 8, 19, 13);
            Fill(hand, 5, 4, 17, 10);
            Fill(hand, 2, 6, 7, 10);
            Fill(hand, 7, 1, 15, 5);

            s_HoverCursor = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Menu Hover Hand Cursor",
                filterMode = FilterMode.Point,
                hideFlags = HideFlags.HideAndDontSave
            };

            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                if (!hand[x, y])
                    continue;

                bool edge = false;
                for (int offsetY = -1; offsetY <= 1 && !edge; offsetY++)
                for (int offsetX = -1; offsetX <= 1; offsetX++)
                {
                    int neighborX = x + offsetX;
                    int neighborY = y + offsetY;
                    if (neighborX < 0 || neighborX >= size || neighborY < 0 || neighborY >= size
                        || !hand[neighborX, neighborY])
                    {
                        edge = true;
                        break;
                    }
                }

                pixels[y * size + x] = edge
                    ? new Color32(18, 22, 29, 255)
                    : new Color32(255, 255, 255, 255);
            }

            s_HoverCursor.SetPixels32(pixels);
            s_HoverCursor.Apply(false, true);
            return s_HoverCursor;
        }

        private static void Fill(bool[,] pixels, int left, int bottom, int right, int top)
        {
            for (int x = left; x < right; x++)
            for (int y = bottom; y < top; y++)
                pixels[x, y] = true;
        }

        private void OnDisable()
        {
            m_ScaleTween?.Kill();
            m_GlowTween?.Kill();

            if (m_Label != null)
                m_Label.transform.localScale = m_OriginalScale;

            if (m_Glow != null)
            {
                m_Glow.effectColor = new Color(1f, 1f, 1f, 0f);
                m_Glow.enabled = false;
            }

            m_PointerInside = false;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}
