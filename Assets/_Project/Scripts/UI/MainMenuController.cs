using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MemoryTower
{
    public sealed class MainMenuController : MonoBehaviour
    {
        private RectTransform popupPanel;
        private Text popupTitle;
        private Text popupBody;

        private void Start()
        {
            BuildMenu();
        }

        private void BuildMenu()
        {
            Canvas canvas = UiFactory.CreateCanvas("MainMenuCanvas");
            RectTransform background = UiFactory.CreateSpritePanel("Background", canvas.transform, VisualAssets.MenuBackground, new Color(0.07f, 0.08f, 0.09f, 1f), false);
            UiFactory.Stretch(background);

            Text title = UiFactory.CreateText("Title", canvas.transform, "记忆危楼", 88, TextAnchor.MiddleLeft, Color.white);
            UiFactory.SetAnchors(title.rectTransform, new Vector2(0.08f, 0.66f), new Vector2(0.92f, 0.86f), Vector2.zero, Vector2.zero);

            Text subtitle = UiFactory.CreateText("Subtitle", canvas.transform, "用手牌拆掉危楼，抢救坠落前的记忆碎片", 28, TextAnchor.MiddleLeft, new Color(0.9f, 0.82f, 0.62f, 1f));
            UiFactory.SetAnchors(subtitle.rectTransform, new Vector2(0.08f, 0.6f), new Vector2(0.92f, 0.67f), Vector2.zero, Vector2.zero);

            RectTransform menuPanel = UiFactory.CreatePanel("Menu", canvas.transform, new Color(0f, 0f, 0f, 0f));
            UiFactory.SetAnchors(menuPanel, new Vector2(0.08f, 0.16f), new Vector2(0.34f, 0.58f), Vector2.zero, Vector2.zero);
            VerticalLayoutGroup layout = menuPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 16;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            AddMenuButton(menuPanel, "新游戏", StartNewGame, true);
            AddMenuButton(menuPanel, "继续游戏", ContinueGame, SaveManager.HasSave());
            AddMenuButton(menuPanel, "设置", ShowSettings, true);
            AddMenuButton(menuPanel, "制作人员", ShowCredits, true);
            AddMenuButton(menuPanel, "退出", QuitGame, true);

            Text note = UiFactory.CreateText("Note", canvas.transform, "Minimal Version：主菜单、手牌、建筑方块、坍塌值、胜负结算。", 22, TextAnchor.MiddleLeft, new Color(0.72f, 0.76f, 0.8f, 1f));
            UiFactory.SetAnchors(note.rectTransform, new Vector2(0.08f, 0.06f), new Vector2(0.92f, 0.12f), Vector2.zero, Vector2.zero);

            BuildPopup(canvas.transform);
        }

        private void AddMenuButton(Transform parent, string label, UnityEngine.Events.UnityAction action, bool interactable)
        {
            Button button = UiFactory.CreateButton(label + "Button", parent, label, action);
            button.interactable = interactable;
            LayoutElement element = button.gameObject.AddComponent<LayoutElement>();
            element.preferredHeight = 70f;
        }

        private void BuildPopup(Transform parent)
        {
            popupPanel = UiFactory.CreateSpritePanel("Popup", parent, VisualAssets.PanelResult, new Color(0.04f, 0.05f, 0.06f, 0.98f), true);
            UiFactory.SetAnchors(popupPanel, new Vector2(0.38f, 0.2f), new Vector2(0.78f, 0.72f), Vector2.zero, Vector2.zero);
            VerticalLayoutGroup layout = popupPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(30, 30, 30, 30);
            layout.spacing = 16;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            popupTitle = UiFactory.CreateText("PopupTitle", popupPanel, "", 38, TextAnchor.MiddleCenter, Color.white);
            popupTitle.gameObject.AddComponent<LayoutElement>().preferredHeight = 70f;

            popupBody = UiFactory.CreateText("PopupBody", popupPanel, "", 24, TextAnchor.UpperLeft, new Color(0.86f, 0.9f, 0.94f, 1f));
            popupBody.gameObject.AddComponent<LayoutElement>().preferredHeight = 260f;

            Button close = UiFactory.CreateButton("ClosePopup", popupPanel, "关闭", HidePopup);
            close.gameObject.AddComponent<LayoutElement>().preferredHeight = 64f;

            popupPanel.gameObject.SetActive(false);
        }

        private void StartNewGame()
        {
            GameState.Instance.ResetProgress();
            SaveManager.Clear();
            SceneManager.LoadScene("Game");
        }

        private void ContinueGame()
        {
            SaveManager.Load(GameState.Instance);
            SceneManager.LoadScene("Game");
        }

        private void ShowSettings()
        {
            ShowPopup("设置", "占位设置面板\n\n主音量：100%\n音乐音量：100%\n音效音量：100%\n震屏：开启\n跳过已读剧情：关闭");
        }

        private void ShowCredits()
        {
            ShowPopup("制作人员", "游戏设计：易子钧、Yuri、卡门\n视效音效：陈金龙\n美术：王子怡、May\n原创音效/音乐：兰萌\nMics：郝裕如\n程序：骨架 Demo 自动生成");
        }

        private void ShowPopup(string title, string body)
        {
            popupTitle.text = title;
            popupBody.text = body;
            popupPanel.gameObject.SetActive(true);
        }

        private void HidePopup()
        {
            popupPanel.gameObject.SetActive(false);
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
