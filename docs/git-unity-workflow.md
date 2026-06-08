# 05 Git 与 Unity 协作规范

## 轻量分支策略

| 分支 | 用途 | 规则 |
|---|---|---|
| main | 稳定可运行版本 | 只合并验收过的内容 |
| dev | 日常集成版本 | 程序组和工具/集成程序维护 |
| feature/xxx | 功能开发 | 对应负责人创建 |
| fix/xxx | Bug 修复 | 对应负责人创建 |

## 每个人每天的 Git 流程

```bash
git switch dev
git pull
git switch -c feature/task-name
git status
git add <files>
git commit -m "feat: add task name"
git push origin feature/task-name
```

在飞书任务里同步：

```text
代码已提交：feature/player-move
等待验收：WASD 移动、不能穿墙、镜头跟随
```

## Unity 资源规则

- `Library/`、`Temp/`、`Obj/`、`Build/`、`Builds/`、`Logs/`、`UserSettings/` 不提交。
- `.meta` 文件要提交。
- 大图、音频、模型走 Git LFS。
- 英文路径优先，不用空格和特殊符号。
- 同一时间尽量不要两个人改同一个 Scene 或 Prefab。

## Git LFS

本仓库已在 `.gitattributes` 中配置常见图片、音频、模型文件走 Git LFS。

每台开发机器首次 clone 后执行：

```bash
git lfs install
git lfs pull
```

如果机器没有管理员权限，至少先不要提交大资源；由工具/集成程序统一导入。

## 合并检查

1. Unity 能打开项目。
2. Console 没有持续报错。
3. 任务验收标准通过。
4. 没有误提交生成目录。
5. 飞书任务更新到 `待验收` 或 `已完成`。
