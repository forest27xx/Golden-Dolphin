# 远端仓库设置

推荐创建一个私有 GitHub 仓库，等提交前再按比赛要求决定是否公开。

建议仓库名：

```text
golden-dolphin-game-jam-2026
```

## 如果使用 GitHub 网页

1. 打开 GitHub，新建 repository。
2. Owner 选 `forest27xx` 或团队组织。
3. Repository name 填 `golden-dolphin-game-jam-2026`。
4. Visibility 先选 Private。
5. 不要勾选初始化 README、.gitignore、license。
6. 创建后，在本地执行：

```bash
git remote add origin git@github.com:forest27xx/golden-dolphin-game-jam-2026.git
git push -u origin main
git push -u origin dev
```

## 如果使用 GitHub CLI

```bash
gh repo create forest27xx/golden-dolphin-game-jam-2026 --private --source . --remote origin --push
git push -u origin dev
```

## 协作者

创建远端后，把 4 名队友加为 collaborator。建议权限：

| 成员 | 权限 |
|---|---|
| 工具/集成程序 | Admin 或 Maintain |
| 核心程序 | Write |
| 关卡/内容 | Write |
| 美术/音效 | Write |
| jun | Maintain 或 Write |

如果启用分支保护：

- `main` 禁止直接 push。
- 合并到 `main` 至少需要 1 人 review。
- `dev` 可以允许工具/集成程序直接合并，但仍需先跑 Unity。
