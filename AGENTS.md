# AGENTS.md

## 项目协作偏好

- 默认用中文沟通，回复保持简洁，优先给出结论和关键改动。
- 不要在每个小操作后都运行测试或全仓验证，避免浪费时间和 token。
- 只有在改动会影响运行逻辑、Unity 配置、包依赖、构建流程、Git/LFS 规则，或用户明确要求时，才做对应验证。
- 文档、说明文字、README、模板内容等低风险改动，通常只做精确搜索或 diff 复查即可，不需要跑测试。
- 验证要按风险选择最小必要范围：能用 `rg`/`git diff` 确认的，不升级到构建或完整测试。
- 如果某个验证成本较高，先说明收益和成本，再等用户确认。
- 每当改动已经到适合 `git add` + `git commit` 的节点，在回复结尾附上建议执行的命令。

## Unity 项目约定

- 团队统一 Unity 版本：`6000.3.18f1`。
- 不提交 `Library/`、`Temp/`、`Logs/`、`UserSettings/` 等 Unity 本地生成目录。
- Unity 资源相关改动优先保持 Visible Meta Files 和 Force Text Serialization 友好。
- 修改 Scene、Prefab、ProjectSettings 前要更谨慎，尽量说明影响范围。
