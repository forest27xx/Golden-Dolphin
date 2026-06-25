# 关卡测试工具（Level Test Tools）— 设计规格

- 日期：2026-06-24
- 任务：程序任务分配 S1 · 任务 #5「关卡数值编辑器 / 测试工具」
- 分支：`feature/level-test-tools`
- 状态：设计已确认，待转入实现计划

## 1. 目标与范围

做一个**纯编辑器工具**，方便调试时「查看关卡数值 + 快速试不同关」。

第一版交付（已与用户锁定范围，YAGNI）：

- **数值只读总览**：一张表列出全部 5 关的设计数值。
- **一键选关进 Play**：点某关 → 干净地从该关开始游戏。
- **运行时重置 / 跳关**：Play 中调用 `LevelManager` 已有公开方法。

**明确不做**（本版排除）：

- 不做数值编辑/落盘（"调整数值"用"选关 + 重置试不同关"覆盖）。
- 不做游戏内运行时调试覆盖层（全部功能放编辑器窗口）。
- 不做"半解锁"等进度变体开关（选关一律重置进度 + 全卡解锁）。

## 2. 硬约束（来自用户，不可违反）

1. **不侵入**：除一处启动钩子外，**不准碰任何游戏源码**；不改场景、不改 Prefab、不改现有脚本签名或逻辑。
2. **唯一允许触碰的游戏源码**：`LevelManager` 启动读取处，且必须用 `#if UNITY_EDITOR` 包裹，保证**正式发布版编译时完全不存在、零残留**。
3. **独立**：任务自包含，不依赖其他 S1 任务，可直接开干。
4. **不靠嘴说"已验证"**：编译通过与人工验收必须在 Unity 里真实跑过（MCP 驱动 + `read_console` 确认无 `error CS`）才能声称完成（AGENTS.md 硬规则）。

## 3. 架构：三个单元与程序集归属

> 项目无任何 `.asmdef`，程序集归属由**文件夹位置**隐式决定：`Editor/` 文件夹下 → `Assembly-CSharp-Editor`（编辑器程序集，不进构建）；其余 → `Assembly-CSharp`（运行时程序集）。下表"程序集"列即据此。

| 单元 | 程序集 | `#if UNITY_EDITOR` | 引用方向（合法性） |
|---|---|---|---|
| `LevelManager` 启动钩子 | 运行时 | 仅调用行包裹 | 同程序集内部调用 `LevelTestBoot` ✅ |
| `LevelTestBoot` | **运行时** | **整个类**包裹 | 类内直接用 `UnityEditor.SessionState`（`#if` 块内用 UnityEditor API，标准用法）✅ |
| `LevelTestWindow`（EditorWindow） | Editor | 不需要（Editor 程序集本就不进构建） | Editor → 运行时，调 `LevelTestBoot.SetTarget` ✅ |

### 关键设计决策：`LevelTestBoot` 放在运行时程序集，而非 Editor 程序集

`#if UNITY_EDITOR` 是预处理指令，只控制"哪些代码参与编译"，**不能突破程序集隔离**。若把 `LevelTestBoot` 放进 Editor 程序集，运行时的 `LevelManager` 将无法引用它（Editor 程序集对运行时不可见），即使套 `#if` 也编译失败。

因此 `LevelTestBoot` 放在**运行时程序集**（与 `LevelManager` 同 asmdef），整个类用 `#if UNITY_EDITOR ... #endif` 包住，类内直接调用 `UnityEditor.SessionState`。这样：

- `LevelManager → LevelTestBoot`：同程序集内部调用，合法。
- `LevelTestBoot` 类内用 `UnityEditor` API：在 `#if UNITY_EDITOR` 块内，编辑器下可见、构建时整段消失，是 Unity 官方标准做法。
- `LevelTestWindow（Editor）→ LevelTestBoot（运行时）`：Editor 引用运行时，方向天然合法。
- **彻底消除"运行时引用 Editor"这一非法方向。**

**发布版零残留**：`LevelTestBoot` 整个类被 `#if` 包住 → 构建时整个类不存在；`LevelManager` 里的调用行也被 `#if` 包住 → 构建时那行不存在，不会引用一个不存在的类。两处 `#if` 边界严格对齐（调用点与被调类同生共灭）。

### 文件落点

- `Assets/_Project/Scripts/Core/LevelTestBoot.cs` —— 运行时程序集，全类 `#if UNITY_EDITOR`
- `Assets/_Project/Scripts/Editor/LevelTestWindow.cs` —— Editor 程序集
- `Assets/_Project/Scripts/Core/LevelManager.cs` —— 仅插入一段 `#if UNITY_EDITOR` 调用

## 4. 数据流：一键选关进 Play

```
1. [EditorWindow] 用户点某关末尾的「进入」按钮
2. [LevelTestBoot.SetTarget(N)]
        → SessionState.SetInt("MemoryTower.LevelTest.TargetIndex", N)
        （SessionState 跨 domain reload、跨 Play 切换存活，编辑器关闭才清）
3. [EditorWindow] 若活动场景不是 Game.unity：
        - 先 EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()
        - 校验 Assets/_Project/Scenes/Game.unity 存在（File.Exists），缺失则 DisplayDialog 报错并中止
        - OpenScene(Game.unity)
4. [EditorWindow] EditorApplication.isPlaying = true   // 一键进 Play
5. —— Unity domain reload，GameState 单例被清空（符合预期）——
6. [LevelManager.Start()]：
       EnsureInitialized();
   #if UNITY_EDITOR
       LevelTestBoot.TryApplyTestTarget();   // 见下
   #endif
       StartLevel(GameState.Instance.requestedLevelIndex);  // 原有逻辑，一字不动
7. [LevelTestBoot.TryApplyTestTarget()]：
        - 从 SessionState 读目标索引；没有 → 直接 return（正常 Play 走原路径）
        - 有 → EraseInt 立即清除（一次性消费）
        - GameState.Instance.ResetProgress()          // 干净进度
        - foreach card in BuiltInConfigs.Cards: state.unlockedCardIds.Add(card.id)  // 全卡解锁
        - GameState.Instance.requestedLevelIndex = idx
```

### `LevelManager` 改动（唯一触碰的游戏源码）

现状：
```csharp
private void Start()
{
    EnsureInitialized();
    StartLevel(GameState.Instance.requestedLevelIndex);
}
```

改为（原有两行一字不动，仅插入 `#if` 块）：
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

「重置 + 全解锁 + 设索引」这套"测试专用脏活"全部收进 `LevelTestBoot.TryApplyTestTarget()`，不摊在 `LevelManager` 里——使 `LevelManager` 改动面最小、最易 review，游戏逻辑不被污染。

### 全卡解锁实现

`GameState.unlockedCardIds` 是公开 `HashSet<string>`，`BuiltInConfigs.Cards` 可枚举全部卡 id。`LevelTestBoot` 直接 `foreach` 添加，**不需要给 `GameState` 加任何方法**。

## 5. EditorWindow 布局

菜单入口：`Memory Tower / Level Test Tools`（与现有 `Memory Tower / Run Smoke Test` 同组）。

窗口分三块，从上到下：

### ① 数值总览表（只读，始终显示）

读 `BuiltInConfigs.Levels`，每关一行，**行末带「进入」按钮**（选关入口合进表格行）。列：

```
关卡            行×列  坍塌阈值 行动数 目标碎片 核心关 初始牌组(张) 奖励牌      [进入]
教学关：危楼入口  2×3     8       5     1       -      8           cut_support  [进入]
普通关1：基础拆解 3×4     10      6     2       -      10          chain_shock  [进入]
普通关2：坍塌管理 4×4     12      8     3       -      11          sealed_echo  [进入]
普通关3：结构规划 5×5     14      9     4       -      12          (无)         [进入]
最终关：记忆核心  5×5     18      8     1       ✓      11          (无)         [进入]
```

- 初始牌组只显示张数，tooltip 展示完整 id 列表。
- 表下方单独一行标注全局手牌：「手牌：初始 5 / 上限 7（全局，非 per-level）」。

### ② 运行时控制（仅 Play 时激活，否则灰掉并提示）

Play 且 `FindFirstObjectByType<LevelManager>()` 非空时激活，调用其已有公开方法：

```
当前控制（Play 中）：
[ 重置本关 RetryLevel ] [ 下一关 NextLevel ] [ 返回菜单 ReturnToMenu ]
```

- 非 Play：显示灰字「进入 Play 后可用」。
- Play 但无 `LevelManager`：按钮灰掉，提示「当前场景无 LevelManager」。

### 状态切换

窗口用 `EditorApplication.isPlaying` 判断当前状态决定 ② 是否激活，并订阅 `EditorApplication.playModeStateChanged`（或用 `OnInspectorUpdate`）自动刷新。

## 6. 错误处理

- **场景路径找不到**：进 Play 前 `File.Exists` 校验 `Game.unity`，缺失则 `EditorUtility.DisplayDialog` 报错并中止，不盲目进 Play。
- **Play 时无 `LevelManager`**：② 区按钮检测到 null 时灰掉并提示。
- **未保存场景改动**：切场景前走 `SaveCurrentModifiedScenesIfUserWantsTo()`，避免吞改动。
- **`SessionState` 残留**：靠"读完即 `EraseInt`"一次性消费保证不污染下次正常 Play；异常时 `TryApplyTestTarget` 没拿到值即走原逻辑，不卡死。

## 7. 测试策略

项目现状：**无任何 `.asmdef`、无测试目录**，全部脚本走 Unity 默认程序集（运行时 → `Assembly-CSharp`，`Editor/` 文件夹 → `Assembly-CSharp-Editor`）。现有 `MemoryTowerSmokeTest` 不是 NUnit `[Test]`，而是菜单驱动方法，靠 `throw` + `Debug.Log` 断言。为保持本任务独立、不引入项目第一个 asmdef / 测试工程，**沿用 smoke test 风格**。

- **菜单驱动自检（跟随 smoke test 风格）**：唯一可自动化验证的纯逻辑是 `LevelTestBoot` 的 set/consume。在 Editor 程序集加一个菜单方法 `Memory Tower / Self-Test Level Boot`，断言：设值后消费应得到该值，紧接着再消费应为空（一次性消费）；失败 `throw`，成功 `Debug.Log("Level boot self-test passed.")`。零新基础设施，不引入 asmdef。
- **人工 / MCP 验收**：`EditorWindow` 渲染与"进 Play"无法纯单测。用 MCP 驱动编辑器：打开窗口 → 点选关 → 确认进 Play 且从正确关卡开始 → 确认 ② 区按钮在 Play 时可用；全程 `read_console` 确认无 `error CS` / 无报错。
- **声称完成的前提**：编译通过与人工验收均在 Unity 里真实跑过（AGENTS.md 硬规则），否则不声称"已验证"。

## 8. 验收标准

1. 菜单 `Memory Tower / Level Test Tools` 能打开窗口。
2. 数值总览表正确显示 5 关全部数值（与 `BuiltInConfigs.Levels` 一致）。
3. 点任一关「进入」→ 自动（必要时切到 Game.unity）进 Play，并**干净地**从该关开始（进度已重置、全卡解锁、索引正确）。
4. 一次性消费生效：选关进 Play 一次后，再正常按 Play 不会被劫持，走原启动逻辑。
5. Play 中 ② 区「重置本关 / 下一关 / 返回菜单」可用且行为正确。
6. 运行时构建与编辑器编译均无 `error CS`。
7. `LevelManager` 改动仅为一段 `#if UNITY_EDITOR` 调用；`LevelTestBoot` 整类 `#if UNITY_EDITOR` 包裹；二者 `#if` 边界对齐，发布版零残留。
