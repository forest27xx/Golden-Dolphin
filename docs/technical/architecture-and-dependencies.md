# 《记忆危楼》Demo 架构与依赖文档

日期：2026-06-20
分支：`codex/import-memory-tower-demo`
适用范围：当前 Unity MVP Demo 工程

## 1. 当前工程定位

当前分支已经不是空骨架，而是一个可打开、可运行、可自动化冒烟测试的 Unity 最小 Demo。它的目标是验证核心玩法闭环：

进入关卡 -> 抽取手牌 -> 选择卡牌 -> 点击建筑方块 -> 结算伤害、坍塌值、支撑检查和记忆碎片 -> 判断胜负 -> 进入下一关或重试。

当前版本优先保证可玩性、可交接和可继续扩展。正式商业化前仍需要替换正式美术、音频、剧情、配置管线和 Steam 发布相关系统。

## 2. Unity 与包依赖

### 2.1 Unity 版本

当前导入工程锚定版本：

```text
Unity 6000.3.18f1
```

版本来源：`ProjectSettings/ProjectVersion.txt`。

注意：仓库早期协作说明中曾写过 `2022.3.74f1 LTS`。当前分支实际工程已经是 Unity 6 系列版本，团队合并前必须统一版本，否则成员打开项目时可能出现自动升级、包版本差异或场景序列化差异。

### 2.2 主要包依赖

依赖清单来源：`Packages/manifest.json`。

| 类型 | 包 | 当前用途 |
|---|---|---|
| 渲染 | `com.unity.render-pipelines.universal` 17.3.0 | 当前工程使用 URP/2D 默认设置，作为后续正式视觉表现基础。 |
| UI | `com.unity.ugui` 2.0.0 | 当前主菜单、HUD、手牌、建筑格子、结算面板全部使用 uGUI 代码生成。 |
| 输入 | `com.unity.inputsystem` 1.19.0 | 当前 EventSystem 在开启 Input System 时使用 `InputSystemUIInputModule`。 |
| 测试 | `com.unity.test-framework` 1.6.0 | 已有 Editor 冒烟测试入口，后续应补 EditMode 单元测试。 |
| 2D 工具 | `com.unity.2d.*` 系列 | 当前资源是 Sprite 化 PNG，占位美术和后续 2D 管线会用到。 |
| IDE | Rider / Visual Studio 包 | 方便团队成员在本地编辑 C#。 |
| 可选/暂未进入玩法闭环 | AI Assistant、AI Inference、Timeline、Visual Scripting、Multiplayer Center | 当前 Demo 运行逻辑不依赖这些包。删除前需要在 Unity 中重新打开并跑测试确认。 |

### 2.3 不应提交的本地生成内容

这些目录由 Unity 或本地环境生成，不进入 Git：

```text
Library/
Temp/
Obj/
Build/
Builds/
Logs/
UserSettings/
MemoryCaptures/
```

规则已写入 `.gitignore`。图片、音频、模型等二进制资源走 Git LFS，规则已写入 `.gitattributes`。

## 3. 工程目录结构

当前主要内容集中在 `Assets/_Project`：

```text
Assets/_Project/
  Docs/                 # 早期玩法、数值、美术和自检记录
  Resources/Art/        # 当前 Demo 运行时加载的 Sprite 资源
  Scenes/               # MainMenu.unity 与 Game.unity
  Scripts/
    Building/           # 建筑格子与支撑逻辑
    Cards/              # 牌库、抽弃洗、牌效果结算
    Collapse/           # 坍塌值、阈值、失控判断
    Core/               # 配置、状态、关卡主流程
    Editor/             # 场景生成、资源配置、冒烟测试
    Rewards/            # 记忆碎片奖励
    Save/               # PlayerPrefs 存档
    UI/                 # 主菜单、游戏 UI、资源加载与按钮视图
  StoreAssets/Steam/    # 当前生成的 Steam 占位商店图
```

仓库根目录 `docs/` 用于团队协作、技术交接和外部可读文档。本文件所在的 `docs/technical/` 只记录工程级说明，不放 Unity 资源。

## 4. 运行时架构

当前 Demo 使用轻量的“纯 C# 逻辑 + MonoBehaviour 桥接 + uGUI 表现”结构。

```text
MainMenuController
  -> GameState / SaveManager
  -> SceneManager.LoadScene("Game")

LevelManager
  -> BuiltInConfigs
  -> BuildingModel
  -> DeckManager
  -> CardEffectResolver
  -> CollapseSystem
  -> RewardSystem
  -> UIManager
  -> SaveManager
```

### 4.1 核心模块职责

| 模块 | 文件 | 职责 |
|---|---|---|
| `GameTypes` | `Assets/_Project/Scripts/Core/GameTypes.cs` | 定义卡牌、目标类型、方块状态、关卡结果、配置数据和方块数据模型。 |
| `BuiltInConfigs` | `Assets/_Project/Scripts/Core/BuiltInConfigs.cs` | 当前内置 8 张卡牌和 5 个关卡配置。后续应迁移到 ScriptableObject 或 JSON。 |
| `GameState` | `Assets/_Project/Scripts/Core/GameState.cs` | 维护局外进度：当前关卡、累计碎片、已通关关卡、已解锁卡牌。 |
| `LevelManager` | `Assets/_Project/Scripts/Core/LevelManager.cs` | 当前游戏主流程入口：开关卡、处理点击、出牌、换手、弱提示、胜负判定、刷新 UI。 |
| `DeckManager` | `Assets/_Project/Scripts/Cards/DeckManager.cs` | 管理抽牌堆、弃牌堆、消耗牌堆、手牌、洗牌、补牌、换手。 |
| `CardEffectResolver` | `Assets/_Project/Scripts/Cards/CardEffectResolver.cs` | 结算卡牌效果：伤害、坍塌值变化、下一张破坏加成、记忆奖励加成、支撑检查触发。 |
| `BuildingModel` | `Assets/_Project/Scripts/Building/BuildingModel.cs` | 生成关卡建筑格子、处理 HP、坍塌、支撑关系、不稳定状态、核心块判定。 |
| `CollapseSystem` | `Assets/_Project/Scripts/Collapse/CollapseSystem.cs` | 管理坍塌值、检查阈值、失控阈值和百分比展示。 |
| `RewardSystem` | `Assets/_Project/Scripts/Rewards/RewardSystem.cs` | 统计本关获得的记忆碎片，处理下一次记忆/核心坍塌额外奖励。 |
| `SaveManager` | `Assets/_Project/Scripts/Save/SaveManager.cs` | 使用 PlayerPrefs 保存和读取最小进度。 |
| `UIManager` | `Assets/_Project/Scripts/UI/UIManager.cs` | 代码生成游戏界面，包括 HUD、建筑网格、手牌区、结算面板和弱提示按钮。 |
| `MainMenuController` | `Assets/_Project/Scripts/UI/MainMenuController.cs` | 代码生成主菜单，提供新游戏、继续游戏、设置占位、制作人员、退出。 |
| `MemoryTowerSmokeTest` | `Assets/_Project/Scripts/Editor/MemoryTowerSmokeTest.cs` | Editor 自动化冒烟测试，验证资源加载、教学关、失败、全关卡胜利、最终关弱提示流程。 |
| `MemoryTowerProjectBuilder` | `Assets/_Project/Scripts/Editor/MemoryTowerProjectBuilder.cs` | 生成/重建 MainMenu 与 Game 场景，并配置 Sprite 导入参数。 |

### 4.2 数据流

1. 主菜单点击“新游戏”时，`GameState.ResetProgress()` 清空进度，`SaveManager.Clear()` 清除本地存档。
2. 进入 `Game` 场景后，`LevelManager.StartLevel()` 读取 `BuiltInConfigs.Levels`。
3. `BuildingModel.Generate()` 根据关卡行列、核心标记和碎片目标生成方块。
4. `DeckManager.Initialize()` 根据关卡初始牌组和 `GameState.unlockedCardIds` 生成抽牌堆。
5. 玩家选牌、选块后，`LevelManager.PlayCard()` 调用 `CardEffectResolver.Resolve()`。
6. 牌效果改变方块 HP、坍塌值、奖励状态，并可能触发 `BuildingModel.RunSupportCheck()`。
7. `RewardSystem.CollectFragments()` 从新坍塌的记忆块或核心块中计算碎片。
8. `LevelManager.EvaluateOutcome()` 判断胜利或失败。
9. 胜利时 `GameState.MarkLevelComplete()` 解锁奖励牌并累计碎片，随后 `SaveManager.Save()` 写入 PlayerPrefs。

## 5. 配置架构

当前配置写在 `BuiltInConfigs.cs` 中，优点是 Demo 阶段稳定、便于快速测试；缺点是数值策划无法在 Unity Inspector 或外部表格中直接改。

当前配置类型：

| 配置 | 类 | 当前位置 | 后续建议 |
|---|---|---|---|
| 卡牌 | `CardConfig` | `BuiltInConfigs.Cards` | 迁移到 `CardConfig` ScriptableObject 或 JSON。 |
| 关卡 | `LevelConfig` | `BuiltInConfigs.Levels` | 迁移到 `LevelConfig` ScriptableObject，支持关卡编辑和批量校验。 |
| 方块 | `BlockModel` 运行时生成 | `BuildingModel.Generate()` | 补 `BlockConfig`，明确普通块、承重块、记忆块、核心块的 HP、权重和视觉资源。 |

配置迁移时应保持逻辑层不依赖 Unity 场景对象。建议先保留 `BuiltInConfigs` 作为 fallback，再让 `LevelManager` 优先读取 ScriptableObject。

## 6. 场景与 UI 架构

当前 Build Settings 包含两个场景：

```text
Assets/_Project/Scenes/MainMenu.unity
Assets/_Project/Scenes/Game.unity
```

场景很薄，只放 Camera、EventSystem 和根节点脚本。实际 UI 在运行时由脚本生成：

- 主菜单：`MainMenuController.BuildMenu()`
- 游戏界面：`UIManager.BuildGameUi()`
- 建筑按钮：`UIManager.RenderBuilding()`
- 手牌卡片：`UIManager.RenderHand()`
- 方块显示：`BlockButtonView.Refresh()`

这样做的好处是减少多人同时改场景造成的冲突；风险是美术和 UI 同学难以直接在 Inspector 里调整布局。后续正式版本建议逐步迁移到 Prefab + 少量脚本绑定，而不是继续把全部 UI 写死在 C#。

## 7. 资源加载架构

当前运行时资源通过 `Resources.Load<Sprite>()` 加载，入口在：

```text
Assets/_Project/Scripts/UI/VisualAssets.cs
```

资源路径约定：

```text
Assets/_Project/Resources/Art/UI/
Assets/_Project/Resources/Art/Building/
Assets/_Project/Resources/Art/Cards/
Assets/_Project/Resources/Art/Cards/AgentIllustrations/
Assets/_Project/Resources/Art/VFX/
```

当前优先加载 Agent 生成的卡牌插画：

```text
Resources/Art/Cards/AgentIllustrations/{card_id}.png
```

如果找不到对应插画，会回退到通用卡牌底图或颜色块。

正式版本建议减少 `Resources` 使用，改为 Addressables 或明确的 ScriptableObject 引用，避免后期资源体积和加载路径失控。

## 8. 存档架构

当前存档是最小实现：

```text
PlayerPrefs key: MemoryTower.Save
saveVersion: 0.1-demo
```

保存内容：

- 当前请求关卡下标
- 累计记忆碎片
- 已完成关卡 id
- 已解锁卡牌 id

当前没有实现多个存档槽、Steam Cloud、存档迁移、设置持久化和失败局中断恢复。正式版本需要升级为 JSON 文件或平台存档系统，并保留版本迁移逻辑。

## 9. 自动化验证入口

Unity 菜单：

```text
Memory Tower / Run Smoke Test
```

命令行示例：

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.18f1\Editor\Unity.exe" `
  -batchmode -quit -nographics `
  -projectPath "C:\Users\krona\Golden-Dolphin-pr1" `
  -executeMethod MemoryTower.EditorTools.MemoryTowerSmokeTest.Run
```

当前冒烟测试覆盖：

- 视觉 Sprite 资源可加载
- 教学关可击破记忆块并获得碎片
- 坍塌值会变化
- 连续换手耗尽行动后会失败
- 5 个 Demo 关卡可以用脚本策略通关
- 最终关普通伤害不能打核心
- 最终关可以通过“听见回声”获得“核心裂解”并击破核心

## 10. 当前架构风险与后续优先级

P0：合并前必须确认团队 Unity 版本。当前工程是 `6000.3.18f1`，旧文档中的 `2022.3.74f1` 已不匹配。

P1：配置仍然硬编码在 `BuiltInConfigs.cs`。后续一旦进入数值频繁调整阶段，应迁到 ScriptableObject 或 JSON。

P1：UI 全部代码生成，适合 MVP，但不适合长期美术迭代。建议尽快拆成 Prefab。

P1：没有正式 AudioManager，音效和音乐尚未接入。

P1：当前测试是 Editor 冒烟测试，还没有独立 asmdef 的 EditMode 单元测试。

P2：资源加载依赖 `Resources`，后期需要迁移到更可控的资源引用方式。

P2：存档使用 PlayerPrefs，只适合 Demo，不适合正式 Steam 版本。
