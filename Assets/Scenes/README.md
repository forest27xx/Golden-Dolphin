# Scenes

建议场景拆分：

```text
MainMenu.unity
Level_Prototype.unity
Combat_Test.unity
UI_Test.unity
Build_Test.unity
```

规则：

- 同一时间尽量只让一个人改一个 Scene。
- 测试场景先放在 `_Sandbox`，稳定后再迁移。
- 每次合并前确认场景能打开、能运行、无持续 Console 报错。
