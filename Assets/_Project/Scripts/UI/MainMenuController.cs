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
        private RectTransform settingsPanel;
        private Slider masterSlider;
        private Slider bgmSlider;
        private Slider sfxSlider;
        private Button muteButton;
        private Text muteButtonText;

        private void Start()
        {
            BuildMenu();
            AudioEvents.RequestBgm("MainMenu");
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
            BuildSettingsPanel(canvas.transform);
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
            AudioEvents.RequestBgm("Game");
            GameState.Instance.ResetProgress();
            SaveManager.Clear();
            SceneManager.LoadScene("Game");
        }

        private void ContinueGame()
        {
            AudioEvents.RequestBgm("Game");
            SaveManager.Load(GameState.Instance);
            SceneManager.LoadScene("Game");
        }

        private void ShowSettings()
        {
            ShowSettingsPanel();
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
            if (popupPanel != null)
            {
                popupPanel.gameObject.SetActive(false);
            }

            if (settingsPanel != null)
            {
                settingsPanel.gameObject.SetActive(false);
            }
        }

        private void BuildSettingsPanel(Transform parent)
        {
            settingsPanel = UiFactory.CreateSpritePanel("SettingsPanel", parent, VisualAssets.PanelResult, new Color(0.04f, 0.05f, 0.06f, 0.98f), true);
            UiFactory.SetAnchors(settingsPanel, new Vector2(0.32f, 0.18f), new Vector2(0.68f, 0.82f), Vector2.zero, Vector2.zero);
            VerticalLayoutGroup layout = settingsPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(30, 30, 30, 30);
            layout.spacing = 18;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            Text title = UiFactory.CreateText("SettingsTitle", settingsPanel, "设置", 38, TextAnchor.MiddleCenter, Color.white);
            title.gameObject.AddComponent<LayoutElement>().preferredHeight = 60f;

            AudioManager audioManager = AudioManager.Instance;

            masterSlider = CreateVolumeSlider(settingsPanel, "主音量", audioManager.MasterVolume, audioManager.SetMasterVolume);
            bgmSlider = CreateVolumeSlider(settingsPanel, "音乐音量", audioManager.BgmVolume, audioManager.SetBgmVolume);
            sfxSlider = CreateVolumeSlider(settingsPanel, "音效音量", audioManager.SfxVolume, audioManager.SetSfxVolume);

            muteButton = UiFactory.CreateButton("MuteButton", settingsPanel, audioManager.IsMuted ? "取消静音" : "静音", audioManager.ToggleMute);
            muteButton.gameObject.AddComponent<LayoutElement>().preferredHeight = 56f;
            muteButtonText = muteButton.GetComponentInChildren<Text>();

            Button close = UiFactory.CreateButton("CloseSettings", settingsPanel, "关闭", HidePopup);
            close.gameObject.AddComponent<LayoutElement>().preferredHeight = 56f;

            settingsPanel.gameObject.SetActive(false);
        }

        private Slider CreateVolumeSlider(Transform parent, string label, float initialValue, UnityEngine.Events.UnityAction<float> onValueChanged)
        {
            GameObject row = new GameObject(label + "Row");
            row.transform.SetParent(parent, false);
            VerticalLayoutGroup rowLayout = row.AddComponent<VerticalLayoutGroup>();
            rowLayout.spacing = 6;
            rowLayout.childControlWidth = true;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = false;
            rowLayout.childAlignment = TextAnchor.UpperLeft;

            Text labelText = UiFactory.CreateText(label + "Label", row.transform, label, 24, TextAnchor.MiddleLeft, Color.white);
            labelText.gameObject.AddComponent<LayoutElement>().preferredHeight = 32f;

            GameObject sliderObj = new GameObject(label + "Slider");
            sliderObj.transform.SetParent(row.transform, false);
            RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.sizeDelta = new Vector2(0, 40f);

            Image trackImage = sliderObj.AddComponent<Image>();
            trackImage.color = new Color(0.15f, 0.16f, 0.18f, 1f);
            trackImage.raycastTarget = true;

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = initialValue;
            slider.onValueChanged.AddListener(onValueChanged);
            slider.targetGraphic = trackImage;

            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(sliderObj.transform, false);
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            Image fillImage = fillObj.AddComponent<Image>();
            fillImage.color = new Color(0.9f, 0.76f, 0.28f, 0.9f);
            slider.fillRect = fillRect;

            GameObject handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(sliderObj.transform, false);
            RectTransform handleRect = handleObj.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20f, 20f);
            Image handleImage = handleObj.AddComponent<Image>();
            handleImage.color = Color.white;
            handleImage.raycastTarget = true;
            slider.handleRect = handleRect;
            slider.direction = Slider.Direction.LeftToRight;

            return slider;
        }

        private void ShowSettingsPanel()
        {
            if (settingsPanel == null)
            {
                return;
            }

            if (popupPanel != null)
            {
                popupPanel.gameObject.SetActive(false);
            }

            AudioManager audioManager = AudioManager.Instance;
            if (masterSlider != null)
            {
                masterSlider.value = audioManager.MasterVolume;
            }

            if (bgmSlider != null)
            {
                bgmSlider.value = audioManager.BgmVolume;
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = audioManager.SfxVolume;
            }

            if (muteButtonText != null)
            {
                muteButtonText.text = audioManager.IsMuted ? "取消静音" : "静音";
            }

            settingsPanel.gameObject.SetActive(true);
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
