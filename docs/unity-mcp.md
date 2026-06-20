# Unity MCP Setup

官方文档：
https://docs.unity3d.com/Packages/com.unity.ai.assistant@2.0/manual/unity-mcp-overview.html

## 当前项目配置

`Packages/manifest.json` 已加入：

```json
"com.unity.ai.assistant": "2.0.0-pre.1"
```

## 本机首次启用步骤

1. 用 Unity `6000.3.18f1` 打开项目。
2. 等待 Package Manager 解析并安装 `AI Assistant` 包。
3. 打开 Unity 后，确认本机生成了 relay：

```powershell
Test-Path $env:USERPROFILE\.unity\relay\relay_win.exe
```

4. relay 存在后，再把它注册到 Codex MCP 配置。

示例命令形态：

```text
C:\Users\<user>\.unity\relay\relay_win.exe --mcp
```

## 注意

- relay 是 Unity Editor 安装/启动后生成的本机文件，不应该提交到 Git。
- `Packages/packages-lock.json` 让 Unity 解析后自动更新，不手写。
