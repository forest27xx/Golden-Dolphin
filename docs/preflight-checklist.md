# 赛前环境跑通清单

主题公布前必须跑一遍完整流程。

## 每个人都要完成

| 检查项 | 通过标准 | 状态 |
|---|---|---|
| Unity Hub | 主机已安装 | 待确认 |
| Unity 版本 | 与团队版本一致 | 待确认 |
| Windows Build Support | 已安装 | 待确认 |
| Git | `git --version` 可用 | 待确认 |
| Git LFS | `git lfs version` 可用 | 待确认 |
| Clone 仓库 | 能拉取项目 | 待确认 |
| 测试分支 | 能 push `feature/test-姓名` | 待确认 |
| Unity 打开 | Console 无持续报错 | 待确认 |
| 合并测试 | 至少一次 feature 合入 dev | 待确认 |
| 打包测试 | 工具/集成程序能打空包 | 待确认 |

## 测试任务

每个人在自己的分支做一件小事：

```text
Assets/_Sandbox/<name>/README.md
```

内容写：

```text
姓名：
设备：
Unity 版本：
Git 分支：
是否能打开项目：
遇到的问题：
```

## 主机工具程序要完成

- 打开 Unity 项目。
- 确认 Visible Meta Files。
- 确认 Force Text Serialization。
- 创建 `Build_Test.unity`。
- 打出一个空项目测试包。
- 让另一台电脑运行一次。
