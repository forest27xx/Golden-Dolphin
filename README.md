# Golden Dolphin Game Jam 2026

5 人协作的 Unity 单机游戏项目仓库。这个仓库先服务三件事：

1. 统一 Unity 项目结构和 Git 规则。
2. 让每个人在正式开题前跑通 clone、分支、提交、推送、合并。
3. 把每日可交付、验收标准和开发日志沉淀到飞书。

## 当前状态

- 比赛周期：2026-06-16 11:00 公布主题，2026-07-06 23:59 截止提交。
- 推荐 Unity：`2022.3.74f1 LTS`，如果主机上已有统一版本，以团队最终确认版本为准。
- 本仓库当前是 Unity 项目骨架；首次在开发主机打开后，由 Unity 生成 `Library/` 等本地目录，不要提交这些目录。
- 这台 Mac 没有安装 Unity Hub，后续实际 Unity 开发放到主机完成。

## 项目结构

```text
golden-dolphin-game-jam-2026/
  Assets/
    _Project/      # 项目说明、共享配置说明
    _Sandbox/      # 每个人的测试脚本/测试场景
    Art/           # 美术资源
    Audio/         # 音效和音乐
    Prefabs/       # 预制体
    Scenes/        # 场景
    Scripts/       # C# 脚本
  Packages/        # Unity Package Manager 配置
  ProjectSettings/ # Unity 版本锚点
  docs/            # 飞书可复制文档、Git/Unity 协作规范
  scripts/         # 本地辅助脚本
```

## 分支规则

- `main`：稳定可运行版本，只合并验收过的内容。
- `dev`：日常集成分支，大家的 feature/fix 先合入这里。
- `feature/<task-name>`：个人功能分支。
- `fix/<bug-name>`：Bug 修复分支。

示例：

```bash
git switch dev
git pull
git switch -c feature/player-move
```

## 首次克隆后

```bash
git lfs install
git switch dev
```

如果在这台 Mac 上临时处理资源，可使用本仓库的本地 LFS 工具：

```bash
./scripts/bootstrap-local-lfs.sh
./.tools/bin/git-lfs version
```

## 立即要跑通

1. 在主机安装统一 Unity 版本和目标平台打包模块。
2. 所有人 clone 仓库。
3. 每个人创建 `feature/test-姓名拼音` 分支。
4. 每个人在 `Assets/_Sandbox/姓名拼音/` 下提交一个测试 README 或脚本。
5. 推送分支，合入 `dev`。
6. 工具/集成程序在主机上打开 Unity 项目并打一个空项目测试包。

详细清单见 [docs/preflight-checklist.md](docs/preflight-checklist.md)。
