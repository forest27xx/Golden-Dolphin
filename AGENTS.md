# AGENTS.md

写给 AI 编码 agent 的工作说明。人类协作规范见 `README.md` 和 `CONTRIBUTING.md`，冲突时以 `CONTRIBUTING.md` 为准。

## 项目

- 多人协作参加 game jam 的 **Unity 单机小游戏**。
- **可交付优先：先做能验收的最小版本，不要一上来搭大架构。** 一次只实现一个明确目标。
- 节点：6/22 玩法原型能跑 → 6/29 有菜单/关卡/基础 UI → 7/3 功能冻结只修 Bug 和打包 → 7/6 提交。

## 游戏：塔楼牌（Turrot）

卡牌策略 + 拆建筑解谜。完整设计见 GDD（暂存于仓库外，待确认后再 check in），**动玩法/数值/系统前先读对应章节**。

单局核心循环：抽牌 → 出牌/弃牌/替换 → 结算牌效果改建筑 → 累计**坍塌值**(0–100%) → 达阈值触发坍塌判定 → 落地方块生成**记忆碎片** → 胜负判定。

三个互相解耦的系统（不要把它们的逻辑混在一起）：

- **卡牌**：基础牌（扑克花色+数字）、功能牌（拆建筑解锁）。抽/弃/洗/替换 + 牌效果结算。
- **建筑**：方块网格（`row/column/hp/supportValue/isKeyBlock` 等），下层破坏影响上层的支撑关系。
- **坍塌**：坍塌值累计、阈值、失控(150%)、连锁坍塌、碎片生成。

## 架构（来自 GDD 10.7 / 10.8，agent 必须遵守）

- 按 GDD 既定职责分层，**不要自己另起命名或合并系统**：`GameState`(局外进度/存档) · `LevelManager`(加载关卡配置、判胜负) · `DeckManager`(抽弃洗替) · `CardEffectResolver`(执行牌效果) · `BuildingModel`(方块数据/支撑) · `CollapseSystem`(坍塌值/失控/连锁) · `RewardSystem`(碎片/功能牌) · `UIManager` · `AudioManager`。
- **数据驱动是硬要求**：卡牌/关卡/方块走 `CardConfig` / `LevelConfig` / `BlockConfig`（JSON 或 ScriptableObject）。伤害、坍塌值、阈值、出牌次数、手牌数等数值**一律读配置，绝不硬编码进逻辑**——数值策划要能改。
- **游戏逻辑写成可测的纯 C#**，MonoBehaviour 只做表现与桥接。坍塌/支撑用**格子逻辑 + 动画表现**，不引入物理引擎（除非用户明确要求）。
- **不做超出 MVP 的东西**：GDD 里标 `P2`/`P4` 的（如每次出牌即存档、继续游戏）先不实现；功能牌总数控制在 5 张内。要扩范围先问用户。

## 环境

- **Unity `2022.3.74f1 LTS`**（锚点 `ProjectSettings/ProjectVersion.txt`，不要随意升级）。目标平台 Windows。
- 依赖见 `Packages/manifest.json`。加包先确认必要性。

## 仓库结构

```text
Assets/
  _Project/   # 项目级共享配置（ScriptableObject、输入表、数值表）
  _Sandbox/   # 个人实验区 Assets/_Sandbox/<name>/，AI 待验证代码先放这
  Scripts/    # 正式 C# 脚本，按 Core/Player/Gameplay/UI/Tools 拆分
  Scenes/  Prefabs/  Art/  Audio/
docs/         # 协作规范与模板
```

- 新脚本放 `Assets/Scripts/<模块>/`；实验/待验证代码先放 `_Sandbox/<name>/`，稳定后再迁移。
- 不要把个人实验文件放进 `_Project/`。

## 分支与提交

- `main` 只合验收过的稳定版本，**永远不要直接提交到 `main`**。从 `dev` 切：`git switch dev && git pull && git switch -c feature/<task>`（Bug 用 `fix/<bug>`）。
- 提交前缀（见 `.gitmessage`）：`feat:` `fix:` `ui:` `art:` `sound:` `docs:` `chore:`。

## Unity 红线

- **绝不提交生成目录**：`Library/` `Temp/` `Obj/` `Build(s)/` `Logs/` `UserSettings/` `MemoryCaptures/`（`.gitignore` 已覆盖，别绕过）。
- `.meta` 必须和对应资源一起提交，不要留孤儿 `.meta`。
- 大图/音频/模型走 **Git LFS**（`.gitattributes` 已配置），不要把大二进制塞进 Git。
- 资源路径用**英文**，不带空格和特殊符号。
- **Scene 保持最薄，逻辑和装配放 Prefab**：可复用对象做成 Prefab，在 Prefab 上挂脚本/配引用，Scene 只摆放实例。这样既能并行开发，也减少 Scene 合并冲突。
- **同一时间不要两个人改同一个 Scene 或 Prefab**——最大的合并冲突来源。

## 代码与 AI 硬规则

- C# 命名 PascalCase 类型 / camelCase 字段，跟随周围风格；改动尽量缩小文件范围，便于人工 review。
- **引用用序列化字段（`[SerializeField]`）或事件传入**，不要满地 `GameObject.Find` / 到处加单例；启动时缓存好引用。
- **不在 `Update` 或热循环里 `Find` / `GetComponent` / `new` 大对象**——开销缓存到 `Awake`/`Start`。
- UI 用项目已有的 **uGUI + TextMeshPro**，音频走 `AudioManager`；不要为小功能引入新框架/插件。
- **纯逻辑（坍塌/支撑/牌效果等）写 EditMode 单测**，放独立 asmdef 的测试目录（`com.unity.test-framework` 已在 `Packages/manifest.json`）。
- **AI 写的代码不能直接合主分支**，必须由人在 Unity 里跑过再合。
- **每次只做一个小目标**，限定允许修改的文件，不要顺手改无关文件。
- 没在 Unity 里真正跑过就**不要声称"已验证通过"**；改完明确说明：改了哪些文件、需要在 Unity 里验收什么、预期效果、已知风险。
- 分析报错要带**完整 Console 信息**，不要凭"报错了"猜。

## 文档

- 玩法/任务/打卡/日志模板在 `docs/`。任务状态在飞书跟踪（外部），本仓库不维护 issue 队列。
