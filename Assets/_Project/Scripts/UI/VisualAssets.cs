using UnityEngine;

namespace MemoryTower
{
    public static class VisualAssets
    {
        public const string MenuBackground = "Art/UI/menu_background";
        public const string GameBackground = "Art/UI/game_background";
        public const string PanelDark = "Art/UI/panel_dark";
        public const string PanelResult = "Art/UI/panel_result";
        public const string HudStrip = "Art/UI/hud_strip";
        public const string HandTray = "Art/UI/hand_tray";
        public const string ButtonIdle = "Art/UI/button_idle";
        public const string ButtonHover = "Art/UI/button_hover";
        public const string ButtonPressed = "Art/UI/button_pressed";
        public const string CardBasic = "Art/Cards/card_basic";
        public const string CardFunction = "Art/Cards/card_function";
        public const string CardOneShot = "Art/Cards/card_oneshot";
        public const string CardSelected = "Art/Cards/card_selected";
        public const string Fragment = "Art/VFX/memory_fragment";

        public static Sprite LoadSprite(string resourcePath)
        {
            return Resources.Load<Sprite>(resourcePath);
        }

        public static Sprite CardSprite(CardConfig card, bool selected)
        {
            if (card != null)
            {
                Sprite agentCard = LoadSprite("Art/Cards/AgentIllustrations/" + card.id);
                if (agentCard != null)
                {
                    return agentCard;
                }

                Sprite namedCard = LoadSprite("Art/Cards/card_" + card.id);
                if (namedCard != null)
                {
                    return namedCard;
                }
            }

            if (selected)
            {
                return LoadSprite(CardSelected);
            }

            if (card.cardType == CardType.OneShot)
            {
                return LoadSprite(CardOneShot);
            }

            if (card.cardType == CardType.Function)
            {
                return LoadSprite(CardFunction);
            }

            return LoadSprite(CardBasic);
        }

        public static Sprite BlockSprite(BlockModel block)
        {
            if (block == null)
            {
                return null;
            }

            if (block.IsCollapsed)
            {
                return LoadSprite("Art/Building/block_collapsed");
            }

            if (block.state == BlockState.Unstable)
            {
                return LoadSprite("Art/Building/block_unstable");
            }

            if (block.isCoreBlock)
            {
                return LoadSprite("Art/Building/block_core");
            }

            if (block.isMemoryBlock)
            {
                return LoadSprite("Art/Building/block_memory");
            }

            if (block.isSupportBlock)
            {
                return LoadSprite("Art/Building/block_support");
            }

            if (block.state == BlockState.Damaged)
            {
                return LoadSprite("Art/Building/block_damaged");
            }

            return LoadSprite("Art/Building/block_normal");
        }
    }
}
