# 《记忆危楼》Demo 交接文档

日期：2026-06-20
分支：`codex/import-memory-tower-demo`
远程仓库：`forest27xx/Golden-Dolphin`

## 1. 交接结论

当前分支包含完整 Unity MVP Demo 的核心工程内容：

- `Assets`
- `Packages`
- `ProjectSettings`
- 工程文档 `docs`

没有提交 Unity 本地缓存目录：

- `Library`
- `Logs`
- `Builds`
- `UserSettings`

团队成员拉取后需要让 Unity 在本地重新生成 `Library`。第一次打开会比较慢，这是正常现象。

## 2. 如何拉取并打开

### 2.1 首次克隆

```powershell
git lfs install
git clone https://github.com/forest27xx/Golden-Dolphin.git
cd Golden-Dolphin
git switch codex/import-memory-tower-demo
git lfs pull
```

然后在 Unity Hub 中选择仓库根目录：

```text
Golden-Dolphin
```

不要只选择 `Assets` 目录。

### 2.2 已经克隆过仓库

```powershell
cd <本地 Golden-Dolphin 仓库>
git fetch origin
git switch codex/import-memory-tower-demo
git pull
git lfs pull
```

### 2.3 Unity 版本

当前工程版本：

```text
Unity 6000.3.18f1
```

推荐所有成员使用同版本打开。若团队决定改回或统一到其他版本，需要由一个人负责升级/降级工程并提交完整变更，其他人不要各自用不同 Unity 版本打开同一分支。

## 3. 最短人工验收流程

打开工程后进入 Play Mode，建议按下面步骤验收：

1. 打开 `Assets/_Project/Scenes/MainMenu.unity`。
2. 点击 Play。
3. 在主菜单点击“新游戏”。
4. 进入教学关后选择“重击”或“轻敲”。
5. 点击黄色记忆块，观察 HP 下降。
6. 击破记忆块后确认碎片数量增加。
7. 胜利后点击“下一关”。
8. 在普通关尝试“替换手牌”，确认行动数减少、坍塌值增加。
9. 故意耗尽行动，确认失败面板出现。
10. 到最终关时确认普通伤害无法攻击红色核心，使用“听见回声”获得“核心裂解”后再攻击核心。

验收通过标准：

- 没有持续 Console 报错。
- 主菜单、游戏场景、结算面板都能正常操作。
- 至少教学关能被人工通关。
- 失败条件能被触发。
- UI 文字不出现明显遮挡或无法点击。

## 4. 自动化冒烟测试

Unity 菜单路径：

```text
Memory Tower / Run Smoke Test
```

Windows 命令行示例：

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.18f1\Editor\Unity.exe" `
  -batchmode -quit -nographics `
  -projectPath "<本地 Golden-Dolphin 仓库路径>" `
  -executeMethod MemoryTower.EditorTools.MemoryTowerSmokeTest.Run
```

期望结果：

```text
Memory Tower smoke test passed.
```

这个测试会验证资源加载、教学关、失败逻辑、全关卡脚本通关和最终关弱提示流程。它不能替代人工体验验收。

## 5. 常用开发入口

### 5.1 想改玩法数值

当前入口：

```text
Assets/_Project/Scripts/Core/BuiltInConfigs.cs
```

可以调整：

- 卡牌伤害
- 卡牌坍塌值
- 卡牌目标类型
- 关卡行列
- 关卡坍塌阈值
- 关卡行动数
- 关卡碎片目标
- 初始牌组
- 关卡奖励牌

注意：当前是硬编码配置。多人协作时不要多人同时改这个文件。下一轮应优先迁移到 ScriptableObject 或 JSON。

### 5.2 想改卡牌效果

入口：

```text
Assets/_Project/Scripts/Cards/CardEffectResolver.cs
```

当前卡牌特殊效果都在这里：

- `stabilize` 降低坍塌值
- `inspect_crack` 下一张破坏牌 +1 伤害
- `sealed_echo` 下一次记忆/核心坍塌额外 +1 碎片
- `chain_shock` 影响目标和左右相邻方块
- `cut_support` 打承重块时强制支撑检查

### 5.3 想改建筑/坍塌逻辑

入口：

```text
Assets/_Project/Scripts/Building/BuildingModel.cs
Assets/_Project/Scripts/Collapse/CollapseSystem.cs
```

常见改动：

- 方块 HP
- 记忆块生成位置
- 核心块位置
- 支撑判断规则
- 失控阈值
- 建筑整体坍塌判定

### 5.4 想改 UI

入口：

```text
Assets/_Project/Scripts/UI/UIManager.cs
Assets/_Project/Scripts/UI/MainMenuController.cs
Assets/_Project/Scripts/UI/BlockButtonView.cs
Assets/_Project/Scripts/UI/UiFactory.cs
```

当前 UI 是代码生成，改布局需要改 C#。如果要进入正式视觉迭代，建议先把主菜单、手牌、方块、结算面板拆成 Prefab，再由脚本绑定数据。

### 5.5 想换美术资源

当前资源目录：

```text
Assets/_Project/Resources/Art/UI/
Assets/_Project/Resources/Art/Building/
Assets/_Project/Resources/Art/Cards/
Assets/_Project/Resources/Art/Cards/AgentIllustrations/
Assets/_Project/Resources/Art/VFX/
Assets/_Project/StoreAssets/Steam/
```

换图时注意：

- 文件名尽量保持英文。
- PNG 会走 Git LFS。
- `.meta` 必须和图片一起提交。
- 替换后在 Unity 菜单中执行 `Memory Tower / Configure Visual Assets`，确保导入类型是 Sprite。

## 6. 当前分工建议

### 程序

优先事项：

1. 统一 Unity 版本。
2. 把 `BuiltInConfigs` 迁移到 ScriptableObject。
3. 给 `DeckManager`、`BuildingModel`、`CardEffectResolver`、`CollapseSystem` 补 EditMode 测试。
4. 加 `AudioManager`。
5. 把 UI 从全代码生成逐步迁到 Prefab。

### 策划

优先事项：

1. 人工试玩 5 个关卡，记录每关是否太简单或太随机。
2. 明确最终关“核心裂解”的教学提示是否足够清楚。
3. 制作卡牌升级、关卡节奏和失败反馈的下一版规则。
4. 确认是否继续使用“记忆碎片”作为唯一胜利资源。

### 美术/UI

优先事项：

1. 确定正式视觉方向。
2. 替换主菜单背景、方块、卡牌、按钮和结算面板。
3. 制作卡牌详情/悬停面板设计。
4. 替换 Steam 商店图为正式 Key Art，而不是继续使用占位图。

### 音频

当前没有实装音频。建议最小音频包：

- 按钮点击
- 选牌
- 出牌
- 方块受击
- 方块坍塌
- 获得碎片
- 胜利
- 失败
- 主菜单环境底噪或短循环音乐

## 7. 提交与合并注意事项

提交前检查：

```powershell
git status -sb
git lfs status
```

不要提交：

```text
Library/
Logs/
Builds/
UserSettings/
*.csproj
*.sln
```

必须一起提交：

- 新增或修改的 Unity 资源
- 对应 `.meta`
- 相关配置或文档

多人协作注意：

- 不要两个人同时改同一个 `.unity` 场景。
- 不要两个人同时改同一个大 Prefab。
- 改 `BuiltInConfigs.cs` 前先在群里同步，因为它现在承载全部数值。
- 每次改完玩法后跑一次冒烟测试，至少人工过教学关。

## 8. 已知问题

1. 当前 Unity 版本与旧协作文档不一致。
2. UI 是 MVP，占位感仍明显。
3. 没有音效、动画和屏幕反馈。
4. 设置面板只是占位。
5. 存档是 PlayerPrefs，正式 Steam 版需要升级。
6. 当前 Steam 商店图是占位包装，不建议直接用于正式提审。
7. 自动化测试没有覆盖大量随机种子和打包后运行。
8. 中文字体使用内置字体，正式版要替换为可商用字体。

## 9. 交接后第一天建议任务

1. 由团队负责人确认 Unity 版本并更新 `README.md`、`AGENTS.md` 和相关流程文档。
2. 一名程序负责配置迁移，不同时做其他大改。
3. 一名策划负责 5 关人工试玩记录和数值反馈。
4. 一名 UI/美术负责确定主菜单与卡牌详情的正式样式。
5. 一名音频负责做最小音效清单和占位音效接入方案。
6. 合并前跑 `MemoryTowerSmokeTest`，并由至少一名成员在 Unity Play Mode 中人工验收。
