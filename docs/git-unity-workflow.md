# 05 Git 与 Unity 协作规范

适用仓库：`forest27xx/Golden-Dolphin`  
Unity 版本：`2022.3.74f1 LTS`  
目标：多人协作时，保证每个人能稳定拉取、打开、开发、合并，不把 Unity 本地缓存和大体积构建产物塞进 Git。

## 1. 分支策略

| 分支 | 用途 | 规则 |
|---|---|---|
| `main` | 稳定可交付版本 | 不直接提交，只合并验收过的版本 |
| `dev` | 日常集成版本 | 功能分支从这里切，也合回这里 |
| `feature/<task>` | 功能开发 | 例如 `feature/card-system` |
| `fix/<bug>` | Bug 修复 | 例如 `fix/collapse-check` |
| `ui/<task>` | UI / 交互 | 例如 `ui/main-menu` |
| `art/<asset>` | 美术资源 | 例如 `art/card-placeholders` |
| `docs/<topic>` | 文档 | 例如 `docs/git-workflow` |
| `chore/<task>` | 工程配置 | 例如 `chore/lfs-rules` |

不要直接向 `main` 提交。一般流程是：从 `dev` 切任务分支，完成后开 PR 合回 `dev`。

## 2. 每个人的日常流程

每天开始前同步 `dev`：

```bash
git switch dev
git pull --ff-only origin dev
```

为任务开分支：

```bash
git switch -c feature/task-name
```

开发过程中查看改动：

```bash
git status --short
git diff
```

只添加本次任务相关文件：

```bash
git add Assets/Scripts
git add Assets/Prefabs
git add Assets/_Project
```

提交并推送：

```bash
git commit -m "feat: add task name"
git push -u origin HEAD
```

在飞书任务里同步：

```text
代码已提交：feature/player-move
等待验收：WASD 移动、不能穿墙、镜头跟随
```

## 3. 如何拉取别人代码

更新当前分支：

```bash
git fetch origin
git pull --ff-only
```

把最新 `dev` 同步到自己的任务分支：

```bash
git fetch origin
git switch feature/task-name
git rebase origin/dev
```

如果暂时不熟悉 rebase，也可以 merge：

```bash
git fetch origin
git switch feature/task-name
git merge origin/dev
```

查看队友分支：

```bash
git fetch origin
git branch -r
git switch --track origin/feature/teammate-task
```

## 4. 提交信息规范

| 前缀 | 用途 |
|---|---|
| `feat:` | 新功能 |
| `fix:` | 修复 Bug |
| `ui:` | UI / 交互 |
| `art:` | 美术资源 |
| `sound:` | 音频 |
| `level:` | 关卡 / 数值 |
| `docs:` | 文档 |
| `chore:` | 工程配置、依赖、清理 |
| `build:` | 构建相关 |

示例：

```text
feat: add collapse threshold logic
fix: prevent normal card from hitting core block
ui: add main menu buttons
art: add placeholder card illustrations
docs: update Unity git workflow
```

## 5. 需要提交的文件

Unity 项目可复现所需内容必须提交：

```text
Assets/
Packages/
ProjectSettings/
.gitignore
.gitattributes
AGENTS.md
CLAUDE.md
CONTRIBUTING.md
README.md
docs/
scripts/
```

重点说明：

| 路径 | 是否提交 | 原因 |
|---|---:|---|
| `Assets/` | 是 | 游戏代码、场景、Prefab、美术、音频都在这里 |
| `Assets/**/*.meta` | 是 | Unity 资源 GUID，漏提交会断引用 |
| `Packages/manifest.json` | 是 | 记录 Unity 包依赖 |
| `Packages/packages-lock.json` | 是 | 锁定包版本，保证队友一致 |
| `ProjectSettings/` | 是 | Unity 版本、场景列表、输入、渲染等项目设置 |
| `.gitignore` | 是 | 防止缓存和构建目录误提交 |
| `.gitattributes` | 是 | Git LFS 与文本合并规则 |
| `docs/` | 是 | 协作规范、模板、交接说明 |
| `scripts/` | 是 | 团队工具脚本 |

### `.meta` 文件规则

Unity 的 `.meta` 文件必须和资源一起提交：

```text
CardView.prefab
CardView.prefab.meta
```

不要删除 `.meta`。移动或重命名资源时，优先在 Unity Editor 内操作，让 Unity 自动维护引用。若必须在文件管理器中移动，资源和 `.meta` 必须一起移动。

## 6. 绝对不要提交的文件

这些目录或文件是本地缓存、个人设置或构建产物，不能进 Git：

```text
Library/
Temp/
Obj/
Build/
Builds/
Logs/
UserSettings/
MemoryCaptures/
.vs/
.vscode/
.idea/
*.csproj
*.sln
*.user
*.userprefs
*.pidb
*.booproj
*.svd
*.pdb
*.mdb
*.opendb
*.VC.db
sysinfo.txt
Recordings/
Captures/
```

当前 `.gitignore` 已覆盖这些路径。不要用 `git add -f` 强制添加它们。

### 为什么不能提交 `Library/`

`Library/` 是 Unity 根据 `Assets + Packages + ProjectSettings` 生成的本地缓存，常常达到数 GB。它不是源文件。队友 clone 后打开 Unity，Unity 会自动重建。

### 为什么不能提交 `Builds/`

`Builds/` 是本地打包产物，不适合进代码仓库。试玩包用以下方式分发：

- GitHub Release 附件。
- 飞书 / 网盘 / 团队文件夹。
- 后续正式走 Steamworks depot。
- 在文档中记录版本号、下载地址和 SHA256 校验值。

计算校验值：

```powershell
Get-FileHash "Builds\\Windows\\Game.exe" -Algorithm SHA256
```

## 7. 大文件与 Git LFS

本仓库已在 `.gitattributes` 中配置常见图片、音频、模型文件走 Git LFS：

```text
*.psd *.psb *.png *.jpg *.jpeg *.gif *.tga
*.wav *.mp3 *.ogg
*.fbx *.blend
*.mp4 *.mov
```

每台开发机器首次 clone 后执行：

```bash
git lfs install
git lfs pull
```

查看 LFS 跟踪规则：

```bash
git lfs track
```

如果机器没有管理员权限，或者团队没有足够 LFS 额度，先不要提交大资源；由工具/集成程序统一导入，或放到外部云盘。

外部资源建议在 `docs/EXTERNAL_ASSETS.md` 记录：

| 资源 | 外部地址 | 放置路径 | 版本 | SHA256 | 负责人 |
|---|---|---|---|---|---|
| 原始卡牌 PSD | 网盘链接 | 不进仓库 | v0.1 | hash | 美术 |
| 宣传视频 | 网盘链接 | 不进仓库 | v0.1 | hash | 发行 |

## 8. Unity 开发注意事项

### 项目设置

Unity 中保持：

```text
Edit > Project Settings > Editor
Version Control Mode = Visible Meta Files
Asset Serialization Mode = Force Text
```

这样场景、Prefab、ScriptableObject 更容易被 Git 追踪和合并。

### Scene / Prefab 协作

Unity 场景和 Prefab 虽然是文本，但冲突仍然难处理：

- 同一天只安排一个人改同一个 Scene。
- 同一天只安排一个人改同一个核心 Prefab。
- Scene 保持最薄，逻辑和装配尽量放 Prefab / ScriptableObject / 配置。
- UI 或可复用对象做成 Prefab，减少 Scene 改动。
- 动 Scene / Prefab 前先在群里说一声，避免互相覆盖。

### 资源命名

资源路径使用英文，不带空格和特殊符号：

```text
Assets/Art/Cards/card_strike.png
Assets/Prefabs/UI/CardView.prefab
Assets/Scripts/Gameplay/CardEffectResolver.cs
```

不推荐：

```text
Assets/美术/卡牌 01.png
Assets/My Card@Final!.png
```

## 9. 提交前检查

每次提交前执行：

```bash
git status --short
git status --ignored
```

确认没有 `Library/`、`Builds/`、`Logs/`、`UserSettings/` 被加入暂存区。

检查超过 50MB 的非忽略文件：

```powershell
Get-ChildItem -Recurse -File |
Where-Object {
    $_.Length -gt 50MB -and
    $_.FullName -notmatch '\\Library\\|\\Builds\\|\\Logs\\|\\Temp\\|\\UserSettings\\'
} |
Select-Object FullName,@{Name="MB";Expression={[math]::Round($_.Length/1MB,2)}}
```

## 10. 如果误提交了不该提交的内容

只是 staged，还没 commit：

```bash
git restore --staged Library
git restore --staged Builds
git restore --staged Logs
```

已经 commit，但还没 push：

```bash
git rm -r --cached Library
git rm -r --cached Builds
git rm -r --cached Logs
git commit -m "chore: remove generated Unity folders"
```

已经 push 到远端：不要每个人各自乱改。通知负责人统一处理，必要时清理历史，并要求所有人重新同步仓库。

## 11. 初次 clone 与本地启动

```bash
git clone https://github.com/forest27xx/Golden-Dolphin.git
cd Golden-Dolphin
git switch dev
git lfs install
git lfs pull
```

然后用 Unity Hub 选择 Unity `2022.3.74f1 LTS` 打开仓库根目录。第一次打开会生成 `Library/`，不要提交。

## 12. PR 合并前检查清单

合并前至少确认：

1. 从 `dev` 切分支开发，没有直接提交到 `main`。
2. Unity 能打开工程。
3. Console 没有持续报错。
4. 任务验收标准通过。
5. 改动范围和 PR 说明一致，没有顺手改无关文件。
6. 没有提交 `Library/`、`Builds/`、`Logs/`、`UserSettings/`。
7. 新增资源带 `.meta`。
8. 修改 `Packages/manifest.json` 时，同时确认 `Packages/packages-lock.json`。
9. 大资源走 Git LFS 或外部资源清单。
10. Scene / Prefab 改动已确认没有并行冲突。
11. 至少一名队友 review 后再合并。
12. AI 生成代码必须由人在 Unity 里跑过再合。

## 13. 常用命令速查

状态与历史：

```bash
git status
git status --short
git log --oneline --graph --decorate --all
git diff
git diff --staged
```

分支：

```bash
git branch
git switch dev
git switch -c feature/new-task
git branch -d feature/finished-task
```

拉取与推送：

```bash
git fetch origin
git pull --ff-only origin dev
git push -u origin HEAD
git push
```

暂存与提交：

```bash
git add Assets/Scripts
git add Assets/Prefabs
git add Packages ProjectSettings
git commit -m "feat: add gameplay card effect"
```

查看忽略规则：

```bash
git status --ignored
git check-ignore -v Library
git check-ignore -v Builds
git check-ignore -v Logs
```

清理被忽略的本地生成文件，先预览：

```bash
git clean -ndX
```

确认后才执行：

```bash
git clean -fdX
```

执行前关闭 Unity，并确认没有需要保留的本地构建包或日志。
