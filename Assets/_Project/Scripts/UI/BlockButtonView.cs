using UnityEngine;
using UnityEngine.UI;

namespace MemoryTower
{
    public sealed class BlockButtonView : MonoBehaviour
    {
        private BlockModel block;
        private LevelManager levelManager;
        private Button button;
        private Image image;
        private Text label;
        private Text typeBadge;
        private Image memoryIcon;
        private Image warningIcon;

        public void Initialize(BlockModel blockModel, LevelManager manager)
        {
            block = blockModel;
            levelManager = manager;
            button = GetComponent<Button>();
            image = GetComponent<Image>();
            label = GetComponentInChildren<Text>();
            EnsureBadges();

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);
            Refresh();
        }

        public void Refresh()
        {
            if (block == null)
            {
                return;
            }

            button.interactable = block.IsSelectable;
            Sprite sprite = VisualAssets.BlockSprite(block);
            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Simple;
                image.color = block.IsCollapsed ? new Color(1f, 1f, 1f, 0.55f) : Color.white;
            }
            else
            {
                image.color = ColorForBlock(block);
            }

            label.text = block.IsCollapsed ? "" : "HP " + block.hp + "/" + block.maxHp;

            if (typeBadge != null)
            {
                typeBadge.text = TypeBadgeText(block);
                typeBadge.gameObject.SetActive(!block.IsCollapsed && !string.IsNullOrEmpty(typeBadge.text));
            }

            if (memoryIcon != null)
            {
                memoryIcon.gameObject.SetActive(!block.IsCollapsed && (block.isMemoryBlock || block.isCoreBlock));
            }

            if (warningIcon != null)
            {
                warningIcon.gameObject.SetActive(!block.IsCollapsed && block.state == BlockState.Unstable);
            }
        }

        private void HandleClick()
        {
            if (levelManager != null && block != null)
            {
                levelManager.HandleBlockClicked(block);
            }
        }

        private Color ColorForBlock(BlockModel model)
        {
            if (model.IsCollapsed)
            {
                return new Color(0.07f, 0.07f, 0.07f, 0.35f);
            }

            if (model.state == BlockState.Unstable)
            {
                return new Color(0.95f, 0.52f, 0.18f, 1f);
            }

            if (model.isCoreBlock)
            {
                return new Color(0.8f, 0.16f, 0.12f, 1f);
            }

            if (model.isMemoryBlock)
            {
                return new Color(0.95f, 0.78f, 0.18f, 1f);
            }

            if (model.isSupportBlock)
            {
                return new Color(0.2f, 0.38f, 0.58f, 1f);
            }

            if (model.state == BlockState.Damaged)
            {
                return new Color(0.42f, 0.44f, 0.47f, 1f);
            }

            return new Color(0.6f, 0.62f, 0.65f, 1f);
        }

        private void EnsureBadges()
        {
            if (typeBadge == null)
            {
                typeBadge = UiFactory.CreateText("TypeBadge", transform, "", 20, TextAnchor.MiddleCenter, Color.white);
                RectTransform rect = typeBadge.rectTransform;
                rect.anchorMin = new Vector2(0.05f, 0.68f);
                rect.anchorMax = new Vector2(0.28f, 0.96f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                typeBadge.raycastTarget = false;
            }

            if (memoryIcon == null)
            {
                GameObject iconObject = new GameObject("MemoryIcon");
                iconObject.transform.SetParent(transform, false);
                memoryIcon = iconObject.AddComponent<Image>();
                memoryIcon.sprite = VisualAssets.LoadSprite(VisualAssets.Fragment);
                memoryIcon.raycastTarget = false;
                RectTransform rect = memoryIcon.rectTransform;
                rect.anchorMin = new Vector2(0.72f, 0.62f);
                rect.anchorMax = new Vector2(0.96f, 0.96f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            if (warningIcon == null)
            {
                GameObject warningObject = new GameObject("WarningIcon");
                warningObject.transform.SetParent(transform, false);
                warningIcon = warningObject.AddComponent<Image>();
                warningIcon.sprite = VisualAssets.LoadSprite("Art/VFX/collapse_warning");
                warningIcon.raycastTarget = false;
                RectTransform rect = warningIcon.rectTransform;
                rect.anchorMin = new Vector2(0.36f, 0.58f);
                rect.anchorMax = new Vector2(0.64f, 0.94f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
        }

        private string TypeBadgeText(BlockModel model)
        {
            if (model.isCoreBlock)
            {
                return "\u6838";
            }

            if (model.isMemoryBlock)
            {
                return "\u5fc6";
            }

            if (model.isSupportBlock)
            {
                return "\u6881";
            }

            return "";
        }
    }
}
