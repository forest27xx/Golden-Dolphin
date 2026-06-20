using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryTower
{
    public sealed class UIManager : MonoBehaviour
    {
        private readonly List<BlockButtonView> blockViews = new List<BlockButtonView>();

        private Canvas canvas;
        private RectTransform buildingPanel;
        private RectTransform handPanel;
        private RectTransform resultPanel;
        private Text levelText;
        private Text collapseText;
        private Text actionText;
        private Text fragmentText;
        private Text pileText;
        private Text instructionText;
        private Text resultTitleText;
        private Text resultMessageText;
        private Button redrawButton;
        private Button weakHintButton;
        private Button resultNextButton;
        private Button resultRetryButton;

        public void BuildGameUi(LevelManager levelManager)
        {
            if (canvas != null)
            {
                return;
            }

            canvas = UiFactory.CreateCanvas("GameCanvas");

            RectTransform background = UiFactory.CreateSpritePanel("Background", canvas.transform, VisualAssets.GameBackground, new Color(0.08f, 0.09f, 0.1f, 1f), false);
            UiFactory.Stretch(background);

            RectTransform topBar = UiFactory.CreateSpritePanel("HUD", canvas.transform, VisualAssets.HudStrip, new Color(0.04f, 0.05f, 0.06f, 0.96f), true);
            UiFactory.SetAnchors(topBar, new Vector2(0f, 0.9f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            HorizontalLayoutGroup topLayout = topBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            topLayout.padding = new RectOffset(22, 22, 10, 10);
            topLayout.spacing = 18;
            topLayout.childForceExpandHeight = true;
            topLayout.childControlHeight = true;

            levelText = CreateHudText(topBar, "关卡");
            collapseText = CreateHudText(topBar, "坍塌");
            actionText = CreateHudText(topBar, "行动");
            fragmentText = CreateHudText(topBar, "碎片");
            pileText = CreateHudText(topBar, "牌堆");

            buildingPanel = UiFactory.CreateSpritePanel("BuildingGrid", canvas.transform, VisualAssets.PanelDark, new Color(0.12f, 0.13f, 0.14f, 0.95f), true);
            UiFactory.SetAnchors(buildingPanel, new Vector2(0.04f, 0.22f), new Vector2(0.68f, 0.86f), Vector2.zero, Vector2.zero);

            RectTransform sidePanel = UiFactory.CreateSpritePanel("SidePanel", canvas.transform, VisualAssets.PanelDark, new Color(0.1f, 0.12f, 0.14f, 0.98f), true);
            UiFactory.SetAnchors(sidePanel, new Vector2(0.7f, 0.22f), new Vector2(0.96f, 0.86f), Vector2.zero, Vector2.zero);
            VerticalLayoutGroup sideLayout = sidePanel.gameObject.AddComponent<VerticalLayoutGroup>();
            sideLayout.padding = new RectOffset(18, 18, 18, 18);
            sideLayout.spacing = 14;
            sideLayout.childControlWidth = true;
            sideLayout.childForceExpandWidth = true;
            sideLayout.childForceExpandHeight = false;

            instructionText = UiFactory.CreateText("Instruction", sidePanel, "", 28, TextAnchor.UpperLeft, Color.white);
            LayoutElement instructionLayout = instructionText.gameObject.AddComponent<LayoutElement>();
            instructionLayout.preferredHeight = 190;

            redrawButton = UiFactory.CreateButton("RedrawButton", sidePanel, "替换手牌", levelManager.HandleRedrawHand);
            AddLayout(redrawButton.gameObject, 0f, 64f);

            weakHintButton = UiFactory.CreateButton("WeakHintButton", sidePanel, "听见回声", levelManager.HandleWeakHint);
            AddLayout(weakHintButton.gameObject, 0f, 64f);

            Button retryButton = UiFactory.CreateButton("RetryButton", sidePanel, "重试本关", levelManager.RetryLevel);
            AddLayout(retryButton.gameObject, 0f, 64f);

            Button menuButton = UiFactory.CreateButton("MenuButton", sidePanel, "回到主菜单", levelManager.ReturnToMenu);
            AddLayout(menuButton.gameObject, 0f, 64f);

            Text legend = UiFactory.CreateText("Legend", sidePanel, "颜色：灰=普通  蓝=承重  黄=记忆  红=核心  橙=不稳定", 20, TextAnchor.UpperLeft, new Color(0.82f, 0.86f, 0.9f, 1f));
            AddLayout(legend.gameObject, 0f, 92f);

            handPanel = UiFactory.CreateSpritePanel("HandPanel", canvas.transform, VisualAssets.HandTray, new Color(0.04f, 0.05f, 0.06f, 0.96f), true);
            UiFactory.SetAnchors(handPanel, new Vector2(0f, 0f), new Vector2(1f, 0.19f), Vector2.zero, Vector2.zero);
            HorizontalLayoutGroup handLayout = handPanel.gameObject.AddComponent<HorizontalLayoutGroup>();
            handLayout.padding = new RectOffset(22, 22, 18, 18);
            handLayout.spacing = 14;
            handLayout.childForceExpandWidth = false;
            handLayout.childControlWidth = false;
            handLayout.childControlHeight = true;

            BuildResultPanel(levelManager);
            SetWeakHintVisible(false);
        }

        public void RenderBuilding(BuildingModel building, LevelManager levelManager)
        {
            ClearChildren(buildingPanel);
            blockViews.Clear();

            GridLayoutGroup grid = buildingPanel.gameObject.GetComponent<GridLayoutGroup>();
            if (grid == null)
            {
                grid = buildingPanel.gameObject.AddComponent<GridLayoutGroup>();
            }

            grid.padding = new RectOffset(18, 18, 18, 18);
            grid.spacing = new Vector2(8f, 8f);
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = building.Columns;

            float cellWidth = Mathf.Clamp(760f / Mathf.Max(1, building.Columns), 88f, 150f);
            float cellHeight = Mathf.Clamp(500f / Mathf.Max(1, building.Rows), 76f, 118f);
            grid.cellSize = new Vector2(cellWidth, cellHeight);

            for (int row = building.Rows - 1; row >= 0; row--)
            {
                for (int column = 0; column < building.Columns; column++)
                {
                    BlockModel block = building.GetBlock(row, column);
                    Button button = UiFactory.CreateButton("Block_" + row + "_" + column, buildingPanel, "", null);
                    Text text = button.GetComponentInChildren<Text>();
                    text.fontSize = 22;
                    text.color = Color.black;
                    BlockButtonView view = button.gameObject.AddComponent<BlockButtonView>();
                    view.Initialize(block, levelManager);
                    blockViews.Add(view);
                }
            }
        }

        public void RenderHand(DeckManager deckManager, LevelManager levelManager, int selectedHandIndex)
        {
            ClearChildren(handPanel);

            for (int i = 0; i < deckManager.Hand.Count; i++)
            {
                int capturedIndex = i;
                CardConfig card = deckManager.Hand[i];
                CreateCardView(card, capturedIndex, i == selectedHandIndex, handPanel, levelManager);
            }

            if (deckManager.Hand.Count == 0)
            {
                Text empty = UiFactory.CreateText("EmptyHand", handPanel, "没有手牌", 28, TextAnchor.MiddleCenter, Color.white);
                AddLayout(empty.gameObject, 240f, 120f);
            }
        }

        public void UpdateHud(LevelConfig level, CollapseSystem collapse, int actionsRemaining, int fragmentsThisLevel, DeckManager deck, int totalFragments)
        {
            if (level == null)
            {
                return;
            }

            levelText.text = level.displayName;
            collapseText.text = "坍塌 " + collapse.Value + " / " + collapse.Threshold + " (" + collapse.Percent() + "%)";
            actionText.text = "行动 " + actionsRemaining;
            fragmentText.text = "碎片 " + fragmentsThisLevel + " / " + level.requiredFragments + "  总计 " + totalFragments;
            pileText.text = "抽牌 " + deck.DrawPileCount + "  弃牌 " + deck.DiscardPileCount;
        }

        public void SetInstruction(string instruction)
        {
            instructionText.text = instruction;
        }

        public void SetWeakHintVisible(bool visible)
        {
            if (weakHintButton != null)
            {
                weakHintButton.gameObject.SetActive(visible);
            }
        }

        public void RefreshBlockViews()
        {
            foreach (BlockButtonView view in blockViews)
            {
                view.Refresh();
            }
        }

        public void ShowResult(bool victory, string message, bool hasNextLevel, LevelManager levelManager)
        {
            resultPanel.gameObject.SetActive(true);
            resultTitleText.text = victory ? "胜利" : "失败";
            resultMessageText.text = message;
            resultNextButton.gameObject.SetActive(victory && hasNextLevel);
            resultRetryButton.gameObject.SetActive(!victory);
        }

        public void HideResult()
        {
            if (resultPanel != null)
            {
                resultPanel.gameObject.SetActive(false);
            }
        }

        private void BuildResultPanel(LevelManager levelManager)
        {
            resultPanel = UiFactory.CreateSpritePanel("ResultPanel", canvas.transform, VisualAssets.PanelResult, new Color(0.04f, 0.05f, 0.06f, 0.97f), true);
            UiFactory.SetAnchors(resultPanel, new Vector2(0.29f, 0.28f), new Vector2(0.71f, 0.74f), Vector2.zero, Vector2.zero);
            VerticalLayoutGroup layout = resultPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(34, 34, 34, 34);
            layout.spacing = 18;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            resultTitleText = UiFactory.CreateText("ResultTitle", resultPanel, "", 46, TextAnchor.MiddleCenter, Color.white);
            AddLayout(resultTitleText.gameObject, 0f, 72f);

            resultMessageText = UiFactory.CreateText("ResultMessage", resultPanel, "", 26, TextAnchor.MiddleCenter, new Color(0.86f, 0.9f, 0.94f, 1f));
            AddLayout(resultMessageText.gameObject, 0f, 120f);

            resultNextButton = UiFactory.CreateButton("NextLevelButton", resultPanel, "下一关", levelManager.NextLevel);
            AddLayout(resultNextButton.gameObject, 0f, 66f);

            resultRetryButton = UiFactory.CreateButton("RetryResultButton", resultPanel, "重试", levelManager.RetryLevel);
            AddLayout(resultRetryButton.gameObject, 0f, 66f);

            Button menuButton = UiFactory.CreateButton("MenuResultButton", resultPanel, "回到主菜单", levelManager.ReturnToMenu);
            AddLayout(menuButton.gameObject, 0f, 66f);

            resultPanel.gameObject.SetActive(false);
        }

        private Text CreateHudText(Transform parent, string placeholder)
        {
            Text text = UiFactory.CreateText(placeholder, parent, placeholder, 24, TextAnchor.MiddleLeft, Color.white);
            LayoutElement layout = text.gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = 340f;
            return text;
        }

        private void CreateCardView(CardConfig card, int handIndex, bool selected, Transform parent, LevelManager levelManager)
        {
            GameObject cardObject = new GameObject("Card_" + card.id + "_" + handIndex);
            cardObject.transform.SetParent(parent, false);
            Image image = cardObject.AddComponent<Image>();
            Sprite cardSprite = VisualAssets.CardSprite(card, selected);
            if (cardSprite != null)
            {
                image.sprite = cardSprite;
                image.type = Image.Type.Simple;
                image.preserveAspect = true;
                image.color = Color.white;
            }
            else
            {
                image.color = CardColor(card, selected);
            }

            Button button = cardObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(delegate { levelManager.HandleCardClicked(handIndex); });
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
            colors.selectedColor = new Color(1f, 0.92f, 0.68f, 1f);
            button.colors = colors;

            AddLayout(cardObject, 128f, 168f);

            RectTransform cardRect = cardObject.GetComponent<RectTransform>();
            if (selected)
            {
                cardRect.anchoredPosition += new Vector2(0f, 8f);
                Outline outline = cardObject.AddComponent<Outline>();
                outline.effectColor = new Color(1f, 0.76f, 0.24f, 0.9f);
                outline.effectDistance = new Vector2(4f, -4f);
            }

            Text title = UiFactory.CreateText("Title", cardObject.transform, card.displayName, 19, TextAnchor.MiddleCenter, Color.white);
            UiFactory.SetAnchors(title.rectTransform, new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.96f), Vector2.zero, Vector2.zero);
            title.raycastTarget = false;

            Text symbol = UiFactory.CreateText("Symbol", cardObject.transform, CardSymbol(card), 30, TextAnchor.MiddleCenter, new Color(0.95f, 0.82f, 0.48f, 0.88f));
            UiFactory.SetAnchors(symbol.rectTransform, new Vector2(0.16f, 0.42f), new Vector2(0.84f, 0.78f), Vector2.zero, Vector2.zero);
            symbol.raycastTarget = false;

            Text desc = UiFactory.CreateText("Description", cardObject.transform, ShortDescription(card), 13, TextAnchor.UpperCenter, new Color(0.86f, 0.9f, 0.94f, 1f));
            UiFactory.SetAnchors(desc.rectTransform, new Vector2(0.12f, 0.22f), new Vector2(0.88f, 0.45f), Vector2.zero, Vector2.zero);
            desc.raycastTarget = false;

            Text damage = UiFactory.CreateText("DamageBadge", cardObject.transform, "伤 " + card.damage, 13, TextAnchor.MiddleCenter, Color.white);
            UiFactory.SetAnchors(damage.rectTransform, new Vector2(0.1f, 0.04f), new Vector2(0.45f, 0.2f), Vector2.zero, Vector2.zero);
            damage.raycastTarget = false;

            string delta = card.collapseDelta > 0 ? "+" + card.collapseDelta : card.collapseDelta.ToString();
            Text collapse = UiFactory.CreateText("CollapseBadge", cardObject.transform, "坍 " + delta, 13, TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0.56f, 1f));
            UiFactory.SetAnchors(collapse.rectTransform, new Vector2(0.55f, 0.04f), new Vector2(0.9f, 0.2f), Vector2.zero, Vector2.zero);
            collapse.raycastTarget = false;
        }

        private string CardSymbol(CardConfig card)
        {
            switch (card.id)
            {
                case "tap":
                    return "裂";
                case "strike":
                    return "破";
                case "stabilize":
                    return "稳";
                case "inspect_crack":
                    return "察";
                case "cut_support":
                    return "梁";
                case "chain_shock":
                    return "震";
                case "sealed_echo":
                    return "忆";
                case "core_fracture":
                    return "核";
                default:
                    return "牌";
            }
        }

        private string ShortDescription(CardConfig card)
        {
            switch (card.id)
            {
                case "tap":
                    return "低风险破坏";
                case "strike":
                    return "直接击碎弱块";
                case "stabilize":
                    return "压低坍塌风险";
                case "inspect_crack":
                    return "下张破坏 +1";
                case "cut_support":
                    return "打梁柱触发检查";
                case "chain_shock":
                    return "目标与左右同层";
                case "sealed_echo":
                    return "下次记忆额外碎片";
                case "core_fracture":
                    return "只能命中核心";
                default:
                    return card.description;
            }
        }

        private Color CardColor(CardConfig card, bool selected)
        {
            if (selected)
            {
                return new Color(0.2f, 0.48f, 0.76f, 1f);
            }

            if (card.cardType == CardType.OneShot)
            {
                return new Color(0.62f, 0.18f, 0.16f, 1f);
            }

            if (card.cardType == CardType.Function)
            {
                return new Color(0.28f, 0.35f, 0.48f, 1f);
            }

            return new Color(0.18f, 0.22f, 0.27f, 1f);
        }

        private void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                GameObject child = parent.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
        }

        private void AddLayout(GameObject target, float preferredWidth, float preferredHeight)
        {
            LayoutElement element = target.GetComponent<LayoutElement>();
            if (element == null)
            {
                element = target.AddComponent<LayoutElement>();
            }

            if (preferredWidth > 0f)
            {
                element.preferredWidth = preferredWidth;
            }

            if (preferredHeight > 0f)
            {
                element.preferredHeight = preferredHeight;
            }
        }
    }
}
