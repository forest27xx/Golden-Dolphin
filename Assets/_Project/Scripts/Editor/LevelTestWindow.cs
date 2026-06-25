using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MemoryTower.EditorTools
{
    public sealed class LevelTestWindow : EditorWindow
    {
        private const string GameScenePath = "Assets/_Project/Scenes/Game.unity";

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

        private void OnGUI()
        {
            DrawLevelTable();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("手牌：初始 5 / 上限 7（全局，非 per-level）", EditorStyles.miniLabel);
            EditorGUILayout.Space();
            DrawRuntimeControls();
        }

        private void DrawLevelTable()
        {
            EditorGUILayout.LabelField("关卡数值总览（只读）", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label("关卡", GUILayout.Width(150));
            GUILayout.Label("行×列", GUILayout.Width(60));
            GUILayout.Label("坍塌阈值", GUILayout.Width(70));
            GUILayout.Label("行动数", GUILayout.Width(55));
            GUILayout.Label("目标碎片", GUILayout.Width(70));
            GUILayout.Label("核心关", GUILayout.Width(50));
            GUILayout.Label("初始牌组", GUILayout.Width(70));
            GUILayout.Label("奖励牌", GUILayout.Width(110));
            GUILayout.Label("", GUILayout.Width(70));
            EditorGUILayout.EndHorizontal();

            List<LevelConfig> levels = BuiltInConfigs.Levels;
            for (int i = 0; i < levels.Count; i++)
            {
                LevelConfig level = levels[i];
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(level.displayName, GUILayout.Width(150));
                GUILayout.Label(level.rows + "×" + level.columns, GUILayout.Width(60));
                GUILayout.Label(level.collapseThreshold.ToString(), GUILayout.Width(70));
                GUILayout.Label(level.actionCount.ToString(), GUILayout.Width(55));
                GUILayout.Label(level.requiredFragments.ToString(), GUILayout.Width(70));
                GUILayout.Label(level.hasFinalCore ? "✓" : "-", GUILayout.Width(50));
                GUILayout.Label(
                    new GUIContent(
                        level.initialDeckCardIds.Count.ToString(),
                        string.Join(", ", level.initialDeckCardIds.ToArray())),
                    GUILayout.Width(70));
                GUILayout.Label(
                    level.rewardCardIds.Count > 0 ? string.Join(",", level.rewardCardIds.ToArray()) : "(无)",
                    GUILayout.Width(110));
                if (GUILayout.Button("进入", GUILayout.Width(70)))
                {
                    EnterLevel(i);
                }

                EditorGUILayout.EndHorizontal();
            }
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
                EditorGUILayout.HelpBox("当前场景无 LevelManager。", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("重置本关", GUILayout.Height(24)))
            {
                manager.RetryLevel();
            }

            if (GUILayout.Button("下一关", GUILayout.Height(24)))
            {
                manager.NextLevel();
            }

            if (GUILayout.Button("返回菜单", GUILayout.Height(24)))
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
