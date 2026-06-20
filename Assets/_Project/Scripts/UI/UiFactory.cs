using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MemoryTower
{
    public static class UiFactory
    {
        private static Font cachedFont;

        public static Font DefaultFont
        {
            get
            {
                if (cachedFont == null)
                {
                    cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (cachedFont == null)
                    {
                        cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                    }
                }

                return cachedFont;
            }
        }

        public static Canvas CreateCanvas(string name)
        {
            GameObject canvasObject = new GameObject(name);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<GraphicRaycaster>();

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        public static RectTransform CreatePanel(string name, Transform parent, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            Image image = panel.AddComponent<Image>();
            image.color = color;
            return panel.GetComponent<RectTransform>();
        }

        public static RectTransform CreateSpritePanel(string name, Transform parent, string spritePath, Color fallbackColor, bool sliced)
        {
            RectTransform rectTransform = CreatePanel(name, parent, fallbackColor);
            ApplySprite(rectTransform.GetComponent<Image>(), spritePath, sliced);
            return rectTransform;
        }

        public static Text CreateText(string name, Transform parent, string content, int fontSize, TextAnchor alignment, Color color)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            Text text = textObject.AddComponent<Text>();
            text.font = DefaultFont;
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        public static Button CreateButton(string name, Transform parent, string label, UnityAction onClick)
        {
            GameObject buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.18f, 0.22f, 0.27f, 0.96f);
            ApplySprite(image, VisualAssets.ButtonIdle, true);

            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState
            {
                highlightedSprite = VisualAssets.LoadSprite(VisualAssets.ButtonHover),
                pressedSprite = VisualAssets.LoadSprite(VisualAssets.ButtonPressed),
                selectedSprite = VisualAssets.LoadSprite(VisualAssets.ButtonHover),
                disabledSprite = VisualAssets.LoadSprite(VisualAssets.ButtonPressed)
            };
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            Text text = CreateText("Label", buttonObject.transform, label, 26, TextAnchor.MiddleCenter, Color.white);
            Stretch(text.rectTransform);
            text.raycastTarget = false;

            return button;
        }

        public static void ApplySprite(Image image, string spritePath, bool sliced)
        {
            if (image == null || string.IsNullOrEmpty(spritePath))
            {
                return;
            }

            Sprite sprite = VisualAssets.LoadSprite(spritePath);
            if (sprite == null)
            {
                return;
            }

            image.sprite = sprite;
            image.color = Color.white;
            image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        }

        public static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        public static void SetAnchors(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }

        public static void SetSize(RectTransform rectTransform, float width, float height)
        {
            rectTransform.sizeDelta = new Vector2(width, height);
        }
    }
}
