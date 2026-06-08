# 协作规范

## 每天只追一个可交付结果

每个成员每天优先完成 1 个主任务，最多 1 个副任务。任务必须写清：

- 今日目标
- 验收标准
- 最小可交付版本
- 影响资源路径
- 对应 Git 分支或提交

## 提交信息

使用下面的前缀：

- `feat:` 新功能
- `fix:` 修 Bug
- `ui:` UI 调整
- `art:` 美术资源导入
- `sound:` 音效接入
- `docs:` 文档修改
- `chore:` 项目配置/杂项

示例：

```text
feat: add prototype player movement
fix: resolve camera jitter in prototype scene
docs: update preflight checklist
```

## 合并前检查

合并到 `dev` 前至少确认：

- Unity 能打开项目。
- Console 没有持续报错。
- 对应任务验收标准通过。
- 没有提交 `Library/`、`Temp/`、`Build/`、`UserSettings/`。
- 没有把无关大文件直接塞进 Git 历史。
- 飞书任务状态改为 `待验收` 或 `已完成`。

## Unity 冲突规避

- 不要两个人同时改同一个 Scene。
- 原型、战斗、UI、关卡测试场景分开。
- 改动前在飞书任务里写清楚今天会动哪些 Scene、Prefab、脚本。
- 不会解冲突时先截图发群，不要硬覆盖。

## AI 代码规则

- AI 生成代码必须人工检查。
- AI 修改前限定文件范围。
- 每次只让 AI 做一个小目标。
- 合并前必须在 Unity 里跑过。
- 报错要贴完整 Console 信息，不要只写“报错了”。
