# 关卡测试工具（Level Test Tools）Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 做一个纯编辑器工具，提供 5 关数值只读总览 + 一键选关进 Play（干净状态）+ Play 中重置/跳关，零运行时足迹、发布版零残留。

**Architecture:** 三个单元——`LevelTestBoot`（运行时程序集，整类 `#if UNITY_EDITOR`，用 `SessionState` 跨域重载传一个目标关索引）；`LevelManager.Start()` 插入一段 `#if UNITY_EDITOR` 调用（唯一触碰的游戏源码）；`LevelTestWindow`（Editor 程序集 EditorWindow，渲染只读表 + 选关 + 运行时控制）。另加一个菜单驱动自检方法验证 `LevelTestBoot` 的一次性消费逻辑。

**Tech Stack:** Unity 6000.3.18f1，C#，Unity 默认程序集（无 asmdef），`UnityEditor.SessionState` / `EditorWindow` / `EditorSceneManager`。

## Global Constraints

- Unity 版本锚点 `6000.3.18f1`，不升级。
- **唯一允许触碰的游戏源码**：`LevelManager.Start()`，且改动必须用 `#if UNITY_EDITOR` 包裹，发布版零残留。其余游戏源码（`GameState`、`BuiltInConfigs`、`GameTypes` 等）一律不改签名、不改逻辑。
- 程序集归属由文件夹位置决定：`Editor/` 下 → `Assembly-CSharp-Editor`（不进构建）；其余 → `Assembly-CSharp`（运行时）。
- 不引入任何 `.asmdef`、不引入测试工程；自动化验证沿用现有 `MemoryTowerSmokeTest` 的菜单驱动 + `throw`/`Debug.Log` 风格。
- 运行时类命名空间 `MemoryTower`；编辑器类命名空间 `MemoryTower.EditorTools`；菜单前缀 `Memory Tower/`。
- C# 命名：类型 PascalCase、字段 camelCase，跟随周围风格。
- **不声称"已验证"除非真的在 Unity 里编译/跑过**（MCP 驱动 + `read_console` 确认无 `error CS`）。
- 提交用 work 身份（worktree 已配置），提交前缀按 `.gitmessage`：`feat:` / `docs:` / `chore:`。

---

## File Structure

- Create: `Assets/_Project/Scripts/Core/LevelTestBoot.cs` — 运行时程序集，整类 `#if UNITY_EDITOR`。用 `SessionState` 存/取/清一个目标关索引；对外暴露 `SetTarget(int)`（供 EditorWindow 调）与 `TryApplyTestTarget()`（供 LevelManager 启动钩子调，内含重置进度 + 全卡解锁 + 设索引）。
- Modify: `Assets/_Project/Scripts/Core/LevelManager.cs:29-33` — `Start()` 内 `EnsureInitialized()` 与 `StartLevel(...)` 之间插入一段 `#if UNITY_EDITOR` 调用。
- Create: `Assets/_Project/Scripts/Editor/LevelTestWindow.cs` — Editor 程序集 EditorWindow，菜单 `Memory Tower/Level Test Tools`，渲染只读数值表（行末「进入」按钮）+ Play 时运行时控制区。
- Create: `Assets/_Project/Scripts/Editor/LevelBootSelfTest.cs` — Editor 程序集，菜单 `Memory Tower/Self-Test Level Boot`，菜单驱动断言 `LevelTestBoot` 一次性消费逻辑。

每个任务完成后 `.cs` 必须连同其 `.meta` 一起提交（Unity 会在导入时生成 `.meta`；通过 MCP `refresh_unity` 或编辑器导入触发后再提交）。

---

### Task 1: LevelTestBoot（运行时传值 + 启动应用逻辑）

**Files:**
- Create: `Assets/_Project/Scripts/Core/LevelTestBoot.cs`

**Interfaces:**
- Consumes: `UnityEditor.SessionState`（仅 `#if UNITY_EDITOR` 块内）；`MemoryTower.GameState`、`MemoryTower.BuiltInConfigs`（同运行时程序集，公开 API）。
- Produces:
  - `public static class LevelTestBoot`（命名空间 `MemoryTower`，整类 `#if UNITY_EDITOR`）
  - `public static void SetTarget(int levelIndex)` — 写 `SessionState` 键 `"MemoryTower.LevelTest.TargetIndex"`。
  - `public static bool TryConsumeTarget(out int levelIndex)` — 读键；存在则取值、`EraseInt` 清除、返回 `true`；不存在返回 `false` 且 `levelIndex = -1`。一次性消费。
  - `public static void TryApplyTestTarget()` — 调 `TryConsumeTarget`；命中则 `GameState.Instance.ResetProgress()` + 遍历 `BuiltInConfigs.Cards` 全解锁 + 设 `requestedLevelIndex`；未命中直接返回。

- [ ] **Step 1: 写 LevelTestBoot.cs**

`Assets/_Project/Scripts/Core/LevelTestBoot.cs`：

```csharp
#if UNITY_EDITOR
using UnityEditor;

namespace MemoryTower
{
    /// <summary>
    /// 编辑器专用：跨 domain reload 把"测试要进入的关卡索引"传给 LevelManager 启动钩子。
    /// 整类被 #if UNITY_EDITOR 包裹，正式构建中完全不存在。
    /// </summary>
    public static class LevelTestBoot
    {
        private const string TargetKey = "MemoryTower.LevelTest.TargetIndex";

        public static void SetTarget(int levelIndex)
        {
            SessionState.SetInt(TargetKey, levelIndex);
        }

        public static bool TryConsumeTarget(out int levelIndex)
        {
            const int missing = int.MinValue;
            int stored = SessionState.GetInt(TargetKey, missing);
            if (stored == missing)
            {
                levelIndex = -1;
                return false;
            }

            SessionState.EraseInt(TargetKey);
            levelIndex = stored;
            return true;
        }

        public static void TryApplyTestTarget()
        {
            int targetIndex;
            if (!TryConsumeTarget(out targetIndex))
            {
                return;
            }

            GameState state = GameState.Instance;
            state.ResetProgress();
            foreach (CardConfig card in BuiltInConfigs.Cards)
            {
                state.unlockedCardIds.Add(card.id);
            }

            state.requestedLevelIndex = targetIndex;
        }
    }
}
#endif
```

- [ ] **Step 2: 触发 Unity 导入并确认编译无错**

通过 MCP `refresh_unity`，再轮询 `mcpforunity://editor/state` 直到 `is_compiling == false`，然后：
Run（MCP）：`read_console(types=["error"], count=20, include_stacktrace=True)`
Expected: 无 `error CS`（无新增编译错误）。

- [ ] **Step 3: 提交**

```bash
cd /Users/meng/Dev/Games/Unity/Golden-Dolphin-level-tools
git add Assets/_Project/Scripts/Core/LevelTestBoot.cs Assets/_Project/Scripts/Core/LevelTestBoot.cs.meta
git commit -m "feat: add LevelTestBoot for editor-only level jump (task #5)"
```

---

### Task 2: LevelManager 启动钩子（唯一触碰的游戏源码）

**Files:**
- Modify: `Assets/_Project/Scripts/Core/LevelManager.cs:29-33`

**Interfaces:**
- Consumes: `MemoryTower.LevelTestBoot.TryApplyTestTarget()`（Task 1，同运行时程序集）。
- Produces: 无新公开 API；仅在 `Start()` 中条件调用。

- [ ] **Step 1: 修改 LevelManager.Start()**

把（第 29-33 行）：

```csharp
        private void Start()
        {
            EnsureInitialized();
            StartLevel(GameState.Instance.requestedLevelIndex);
        }
```

改为（原两行一字不动，仅插入 `#if` 块）：

```csharp
        private void Start()
        {
            EnsureInitialized();
#if UNITY_EDITOR
            LevelTestBoot.TryApplyTestTarget();
#endif
            StartLevel(GameState.Instance.requestedLevelIndex);
        }
```

- [ ] **Step 2: 触发导入并确认编译无错**

MCP `refresh_unity` → 轮询 `is_compiling == false` →
Run（MCP）：`read_console(types=["error"], count=20, include_stacktrace=True)`
Expected: 无 `error CS`。特别确认无"`LevelTestBoot` 不存在/不可达"类错误（验证同程序集引用合法、`#if` 边界对齐）。

- [ ] **Step 3: 提交**

```bash
cd /Users/meng/Dev/Games/Unity/Golden-Dolphin-level-tools
git add Assets/_Project/Scripts/Core/LevelManager.cs
git commit -m "feat: add editor-only level test boot hook in LevelManager.Start (task #5)"
```

---

### Task 3: LevelBootSelfTest（一次性消费的菜单驱动自检）

**Files:**
- Create: `Assets/_Project/Scripts/Editor/LevelBootSelfTest.cs`

**Interfaces:**
- Consumes: `MemoryTower.LevelTestBoot.SetTarget` / `TryConsumeTarget`（Task 1；Editor → 运行时方向调用，合法）。
- Produces: 菜单 `Memory Tower/Self-Test Level Boot`；成功 `Debug.Log("Level boot self-test passed.")`，失败 `throw`。

- [ ] **Step 1: 写 LevelBootSelfTest.cs**

`Assets/_Project/Scripts/Editor/LevelBootSelfTest.cs`：

```csharp
using UnityEditor;
using UnityEngine;

namespace MemoryTower.EditorTools
{
    public static class LevelBootSelfTest
    {
        [MenuItem("Memory Tower/Self-Test Level Boot")]
        public static void Run()
        {
            // 设值后消费应取回该值
            LevelTestBoot.SetTarget(3);
            int consumed;
            if (!LevelTestBoot.TryConsumeTarget(out consumed) || consumed != 3)
            {
                throw new System.Exception("Self-test failed: consume did not return the value that was set.");
            }

            // 一次性消费：紧接着再消费应为空
            int again;
            if (LevelTestBoot.TryConsumeTarget(out again))
            {
                throw new System.Exception("Self-test failed: target was not cleared after first consume.");
            }

            Debug.Log("Level boot self-test passed.");
        }
    }
}
```

- [ ] **Step 2: 触发导入并确认编译无错**

MCP `refresh_unity` → 轮询 `is_compiling == false` →
Run（MCP）：`read_console(types=["error"], count=20)`
Expected: 无 `error CS`。

- [ ] **Step 3: 跑自检并确认通过**

Run（MCP）：`execute_menu_item(menu_path="Memory Tower/Self-Test Level Boot")`，随后 `read_console(types=["log","error"], count=10)`
Expected: 出现 `Level boot self-test passed.`，无 exception。

- [ ] **Step 4: 提交**

```bash
cd /Users/meng/Dev/Games/Unity/Golden-Dolphin-level-tools
git add Assets/_Project/Scripts/Editor/LevelBootSelfTest.cs Assets/_Project/Scripts/Editor/LevelBootSelfTest.cs.meta
git commit -m "test: add menu-driven self-test for LevelTestBoot consume (task #5)"
```

---

### Task 4: LevelTestWindow（只读数值表 + 选关 + 运行时控制）

**Files:**
- Create: `Assets/_Project/Scripts/Editor/LevelTestWindow.cs`

**Interfaces:**
- Consumes: `MemoryTower.BuiltInConfigs.Levels`（只读渲染）；`MemoryTower.LevelTestBoot.SetTarget`（Task 1）；`MemoryTower.LevelManager` 的公开方法 `RetryLevel()` / `NextLevel()` / `ReturnToMenu()`（Play 时通过 `Object.FindFirstObjectByType<LevelManager>()` 获取）。
- Produces: 菜单 `Memory Tower/Level Test Tools` 打开窗口。

注：`LevelConfig` 公开字段（来自 `GameTypes.cs`）：`id, displayName, rows, columns, collapseThreshold, actionCount, requiredFragments, initialDeckCardIds(List<string>), rewardCardIds(List<string>), hasFinalCore`。全局手牌常量 `LevelManager.InitialHandSize=5 / HandLimit=7` 为 `private const`，**不可反射读取作为运行时依赖**——窗口里直接以字面量 `5 / 7` 显示并注明"全局"。

- [ ] **Step 1: 写 LevelTestWindow.cs**

`Assets/_Project/Scripts/Editor/LevelTestWindow.cs`：

```csharp
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
```

- [ ] **Step 2: 触发导入并确认编译无错**

MCP `refresh_unity` → 轮询 `is_compiling == false` →
Run（MCP）：`read_console(types=["error"], count=20, include_stacktrace=True)`
Expected: 无 `error CS`。

- [ ] **Step 3: 打开窗口确认渲染无报错**

Run（MCP）：`execute_menu_item(menu_path="Memory Tower/Level Test Tools")`，随后 `read_console(types=["error","warning"], count=10)`
Expected: 窗口打开，无 exception/报错。

- [ ] **Step 4: 提交**

```bash
cd /Users/meng/Dev/Games/Unity/Golden-Dolphin-level-tools
git add Assets/_Project/Scripts/Editor/LevelTestWindow.cs Assets/_Project/Scripts/Editor/LevelTestWindow.cs.meta
git commit -m "feat: add LevelTestWindow with level overview, jump, runtime controls (task #5)"
```

---

### Task 5: 端到端人工 / MCP 验收

**Files:** 无（仅验证）。

- [ ] **Step 1: 选关进 Play（干净状态）**

MCP 步骤：
1. `execute_menu_item("Memory Tower/Level Test Tools")` 打开窗口。
2. 在窗口里点「普通关2」行的「进入」（或经由 `LevelTestBoot.SetTarget(2)` + 打开 Game.unity + 进 Play 复现）。
3. 进 Play 后 `read_console(types=["error"], count=20)` 确认无 `error CS` / 无 exception。
4. 确认游戏从 `level_02` 开始（`manage_scene get_active` 为 Game.unity；HUD/建筑符合该关行列 4×4）。

Expected: 干净从第 3 关（index 2）开始，进度已重置 + 全卡解锁。

- [ ] **Step 2: 一次性消费验证**

退出 Play，再正常按 Play（不经窗口）。
Expected: 从 `requestedLevelIndex` 的默认值（教学关 index 0）开始，**不被上次选关劫持** —— 证明一次性消费生效。

- [ ] **Step 3: 运行时控制验证**

Play 中，在窗口点「下一关」「重置本关」。
Expected: 关卡相应切换 / 重置；`read_console` 无报错。

- [ ] **Step 4: 记录验收结论**

在本计划文件末尾追加一段「验收记录」，写明：改了哪些文件、在 Unity 里验证了什么、预期效果、已知风险（按 AGENTS.md 要求）。提交：

```bash
cd /Users/meng/Dev/Games/Unity/Golden-Dolphin-level-tools
git add docs/superpowers/plans/2026-06-25-level-test-tools.md
git commit -m "docs: record level test tools acceptance (task #5)"
```

---

## 备注

- `LevelTestBoot` 与 `LevelManager` 钩子的 `#if UNITY_EDITOR` 边界必须严格对齐（调用点与被调类同生共灭）；Task 1/2 的编译检查即在验证这一点。
- 本任务**不**把 AGENTS.md 重新纳入版本控制（它已被 main 的 `.gitignore` 忽略，仅作本地参考）。
- 本任务**不**向 `main` 推送任何 AGENTS.md 修复——那是 Trace（任务 #1）的职责范围。

---

## 验收记录（2026-06-25，MCP 实跑）

实例：Golden-Dolphin-level-tools @ Unity 6000.3.18f1。全程 0 error CS / 0 exception。

**改动文件**（feature/level-test-tools，commits 61b9d1e..9f6fe91）：
- `Assets/_Project/Scripts/Core/LevelTestBoot.cs`（新建，整类 `#if UNITY_EDITOR`）
- `Assets/_Project/Scripts/Core/LevelManager.cs`（Start() 插入一段 `#if UNITY_EDITOR` 调用，唯一触碰的游戏源码）
- `Assets/_Project/Scripts/Editor/LevelBootSelfTest.cs`（新建，菜单自检）
- `Assets/_Project/Scripts/Editor/LevelTestWindow.cs`（新建，EditorWindow）

**验证结果**：
1. 选关进 Play（干净状态）：SetTarget(2) → 打开 Game.unity → 进 Play。结果 `currentLevel.id=level_02, rows=4, cols=4`；`unlockedCards=8/8`（全解锁）；`requestedLevelIndex=2`；SessionState 消费后 = 空（一次性消费已 Erase）。✅
2. 一次性消费：退 Play，不设目标再进 Play → `currentLevel.id=tutorial`（index 0），未被上次 level_02 劫持。✅
3. 运行时控制：Play 中 `NextLevel()` tutorial→level_01；`RetryLevel()` 停在 level_01（重开本关）。`ReturnToMenu()` 为简单 LoadScene("MainMenu")，方法已由窗口绑定，未实际触发以免扰动。✅
4. 菜单自检 `Memory Tower/Self-Test Level Boot` 在 Task 3 实跑输出 `Level boot self-test passed.`。✅
5. 窗口 `Memory Tower/Level Test Tools` 在 Task 4 实跑成功打开（720×552），无 IMGUI 报错。✅

**已知风险/局限**：
- `TryConsumeTarget` 用 `int.MinValue` 作哨兵；关卡索引恒为 0-4 小正整数，无碰撞风险（Minor，规格既定设计）。
- `EnterLevel` 与三个运行时按钮的验证通过等价方式完成：`SetTarget` + 进 Play 验 boot 链路；三方法直接调用验运行时行为。未模拟 IMGUI 鼠标点击本身（MCP 不支持点 IMGUI 按钮），但所调用的方法与窗口按钮绑定的方法一致（Task 4 审查确认绑定正确）。
- 发布版零残留：`LevelTestBoot` 整类 + `LevelManager` 调用行均 `#if UNITY_EDITOR` 包裹；编译期无 error 间接佐证边界对齐（构建期不存在不可达引用）。未实际出 Windows 构建验证（超出本机验收范围）。
