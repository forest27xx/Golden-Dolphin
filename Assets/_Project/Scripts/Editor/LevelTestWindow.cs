using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MemoryTower.EditorTools
{
    public sealed class LevelTestWindow : EditorWindow
    {
        private const string GameScenePath = "Assets/_Project/Scenes/Game.unity";

        // 列宽集中管理，表头与数据行共用，保证对齐。
        private const float ColLevel = 150f;
        private const float ColGrid = 56f;
        private const float ColThreshold = 70f;
        private const float ColActions = 56f;
        private const float ColFragments = 70f;
        private const float ColCore = 50f;
        private const float ColDeck = 70f;
        private const float ColReward = 110f;
        private const float ColEnter = 72f;

        private static readonly Color ZebraColor = new Color(1f, 1f, 1f, 0.03f);
        private static readonly Color CurrentRowColor = new Color(0.3f, 0.6f, 1f, 0.12f);

        private GUIStyle currentEnterStyle;
        private GUIStyle headerCellStyle;

        [MenuItem("Memory Tower/Level Test Tools")]
        public static void Open()
        {
            LevelTestWindow window = GetWindow<LevelTestWindow>("Level Test");
            window.minSize = new Vector2(720f, 320f);
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        private void OnPlayModeChanged(PlayModeStateChange change)
        {
            Repaint();
        }

        // Play 中 LevelManager 会随每次行动改 requestedLevelIndex，需持续刷新高亮。
        private void OnInspectorUpdate()
        {
            if (EditorApplication.isPlaying)
            {
                Repaint();
            }
        }

        private void EnsureStyles()
        {
            if (currentEnterStyle == null)
            {
                currentEnterStyle = new GUIStyle(GUI.skin.button);
                currentEnterStyle.fontStyle = FontStyle.Bold;
                currentEnterStyle.normal.textColor = new Color(0.45f, 0.75f, 1f);
                currentEnterStyle.hover.textColor = new Color(0.6f, 0.85f, 1f);
            }

            if (headerCellStyle == null)
            {
                headerCellStyle = new GUIStyle(EditorStyles.miniBoldLabel);
            }
        }

        private void OnGUI()
        {
            EnsureStyles();

            EditorGUILayout.Space(4f);
            DrawLevelTable();
            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("手牌：初始 5 / 上限 7（全局，非 per-level）", EditorStyles.miniLabel);
            EditorGUILayout.Space(8f);
            DrawRuntimeControls();
        }

        private int CurrentLevelIndex()
        {
            // 只读公开字段判断"当前正在测试哪一关"，不触碰 LevelManager 私有状态。
            if (!EditorApplication.isPlaying)
            {
                return -1;
            }

            if (Object.FindFirstObjectByType<LevelManager>() == null)
            {
                return -1;
            }

            return GameState.Instance.requestedLevelIndex;
        }

        private void DrawLevelTable()
        {
            EditorGUILayout.LabelField("关卡数值总览（只读）", EditorStyles.boldLabel);

            // 表头
            Rect headerRect = EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            HeaderCell("关卡", ColLevel);
            HeaderCell("行×列", ColGrid);
            HeaderCell("坍塌阈值", ColThreshold);
            HeaderCell("行动数", ColActions);
            HeaderCell("目标碎片", ColFragments);
            HeaderCell("核心关", ColCore);
            HeaderCell("初始牌组", ColDeck);
            HeaderCell("奖励牌", ColReward);
            GUILayout.Label(GUIContent.none, GUILayout.Width(ColEnter));
            EditorGUILayout.EndHorizontal();

            int currentIndex = CurrentLevelIndex();
            List<LevelConfig> levels = ConfigLoader.Levels;
            for (int i = 0; i < levels.Count; i++)
            {
                LevelConfig level = levels[i];
                bool isCurrent = i == currentIndex;

                Rect rowRect = EditorGUILayout.BeginHorizontal();

                // 行底色：当前关蓝色高亮，其余斑马纹。
                if (Event.current.type == EventType.Repaint)
                {
                    if (isCurrent)
                    {
                        EditorGUI.DrawRect(rowRect, CurrentRowColor);
                    }
                    else if (i % 2 == 1)
                    {
                        EditorGUI.DrawRect(rowRect, ZebraColor);
                    }
                }

                string namePrefix = isCurrent ? "▶ " : "   ";
                GUILayout.Label(namePrefix + level.displayName, GUILayout.Width(ColLevel));
                GUILayout.Label(level.rows + "×" + level.columns, GUILayout.Width(ColGrid));
                GUILayout.Label(level.collapseThreshold.ToString(), GUILayout.Width(ColThreshold));
                GUILayout.Label(level.actionCount.ToString(), GUILayout.Width(ColActions));
                GUILayout.Label(level.requiredFragments.ToString(), GUILayout.Width(ColFragments));
                GUILayout.Label(level.hasFinalCore ? "✓" : "-", GUILayout.Width(ColCore));
                GUILayout.Label(
                    new GUIContent(
                        level.initialDeckCardIds.Count.ToString(),
                        string.Join(", ", level.initialDeckCardIds.ToArray())),
                    GUILayout.Width(ColDeck));
                GUILayout.Label(
                    level.rewardCardIds.Count > 0 ? string.Join(",", level.rewardCardIds.ToArray()) : "(无)",
                    GUILayout.Width(ColReward));

                string enterLabel = isCurrent ? "当前" : "进入";
                GUIStyle enterStyle = isCurrent ? currentEnterStyle : GUI.skin.button;
                if (GUILayout.Button(new GUIContent(enterLabel, isCurrent ? "正在测试这一关，点击可重新进入" : null),
                        enterStyle, GUILayout.Width(ColEnter)))
                {
                    EnterLevel(i);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void HeaderCell(string text, float width)
        {
            GUILayout.Label(text, headerCellStyle, GUILayout.Width(width));
        }

        private void DrawRuntimeControls()
        {
            EditorGUILayout.LabelField("运行时控制", EditorStyles.boldLabel);

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.LabelField("进入 Play 后可用。", EditorStyles.miniLabel);
                return;
            }

            LevelManager manager = Object.FindFirstObjectByType<LevelManager>();
            if (manager == null)
            {
                // 主菜单场景下没有 LevelManager 属正常现象，给出明确指引而非"出错"的错觉。
                EditorGUILayout.HelpBox(
                    "当前场景没有 LevelManage\n" +
                    "上方点任一关「进入」会自动切到 Game 场景并从该关开始。",
                    MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("重置本关", GUILayout.Height(26)))
            {
                manager.RetryLevel();
            }

            if (GUILayout.Button("下一关", GUILayout.Height(26)))
            {
                manager.NextLevel();
            }

            if (GUILayout.Button("返回菜单", GUILayout.Height(26)))
            {
                manager.ReturnToMenu();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void EnterLevel(int levelIndex)
        {
            if (EditorApplication.isPlaying)
            {
                LevelManager manager = Object.FindFirstObjectByType<LevelManager>();
                if (manager != null)
                {
                    manager.StartLevel(levelIndex);
                    return;
                }

                // Play 中但当前场景无 LevelManager（如主菜单）：当前版本不支持运行时热跳，
                // 不改变原有行为，仅提示用户需要先退出 Play 再选关。
                Debug.LogWarning(
                    "[LevelTest] 当前在无 LevelManager 的场景，无法在 Play 中直接跳关。" +
                    "请先退出 Play，再点「进入」即可自动切到 Game 场景并从该关开始。");
                return;
            }

            if (!System.IO.File.Exists(GameScenePath))
            {
                EditorUtility.DisplayDialog(
                    "缺少场景",
                    "找不到 " + GameScenePath + "，无法进入关卡。",
                    "好的");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            LevelTestBoot.SetTarget(levelIndex);

            if (EditorSceneManager.GetActiveScene().path != GameScenePath)
            {
                EditorSceneManager.OpenScene(GameScenePath);
            }

            EditorApplication.isPlaying = true;
        }
    }
}
