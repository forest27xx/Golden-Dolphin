# 《记忆危楼》骨架 Demo

## 如何运行

### Unity 内运行

1. 用 Unity 6.3 LTS `6000.3.18f1` 打开 `C:\Users\krona\demo`。
2. 打开 `Assets/_Project/Scenes/MainMenu.unity`。
3. 点击 Play。
4. 在主菜单点击“新游戏”，进入教学关。

### Windows 构建包

已生成最小 Windows Demo：

`C:\Users\krona\demo\Builds\MemoryTowerDemo\MemoryTowerDemo.exe`

旁边的 `MemoryTowerDemo_Data`、`UnityPlayer.dll`、`MonoBleedingEdge` 等文件夹/文件需要和 exe 保持在同一目录。

## 已实现

- 主菜单：新游戏、继续游戏、设置、制作人员、退出。
- 游戏场景：HUD、建筑方块、手牌区、操作按钮、弱提示按钮预留。
- 卡牌：轻敲、重击、稳住、观察裂缝、支点切除、连锁震动、尘封回声、核心裂解。
- 关卡：tutorial、level_01、level_02、level_03、final 五套配置，复用同一游戏场景。
- 建筑规则：HP、承重块、记忆块、核心块、不稳定状态、支撑检查、坍塌。
- 胜负：普通关收集碎片并破坏关键目标胜利；行动耗尽或失控失败；最终关核心块坍塌胜利。
- 存档：通关后保存总碎片、已通关关卡、已解锁卡牌、继续关卡。
- 美术：已接入主菜单/游戏背景、按钮三态、HUD、手牌托盘、方块状态图、8 张 Agent 卡牌插画和 Steam 包装草图。
- 交接：规则、数值、美术资源、Steam 草图和 Agent 自检记录保存在 `Assets/_Project/Docs`。

## 当前限制

- 当前美术仍是 MVP 占位资源，不是正式 Steam 提审级 Key Art 或最终商店素材。
- 没有真实物理坍塌、动画和音效。
- 卡牌与关卡配置暂时写在 `BuiltInConfigs.cs`，后续可迁移到 ScriptableObject 或 JSON。
- 记忆碎片只由记忆块/核心块坍塌产生，普通块 20% 掉落暂未做。
- 中文字体暂用 Unity 默认字体，正式发布前需要替换为可商用中文字体。
