# 《记忆危楼》MVP Demo 目标与风格

日期：2026-06-20  
适用版本：Minimal Version Demo  
工程：`C:\Users\krona\demo`

## Demo 目标

本版本的目标不是完成正式商业版本，而是让玩家在 3 到 5 分钟内理解并体验核心循环：

1. 选择手牌。
2. 点击危楼方块。
3. 方块 HP 下降或坍塌。
4. 坍塌值与行动次数变化。
5. 记忆块坍塌后获得记忆碎片。
6. 达成目标胜利，行动耗尽或失控失败。

当前关卡范围为 5 关：教学关、3 个普通关、最终核心关。自动测试已经用固定策略跑通过完整 Demo，但当前内容仍定位为“最小可试玩切片”，不是正式 Steam 商业版本。

## 受众与平台假设

- 目标平台：PC / Steam。
- 目标玩家：喜欢卡牌策略、轻解谜、结构拆除反馈的玩家。
- 本 Demo 的第一优先级：可读、可玩、有气质。
- 本 Demo 的第二优先级：方便后续策划、美术、程序继续替换资源与扩展规则。

## 设计哲学

当前采用「废墟档案馆」方向：

- 冷色废墟：表现危楼、结构压力和破败空间。
- 暖色记忆：用黄色/琥珀色表现碎片、奖励和希望。
- 纸牌结构：建筑块像被抽出的牌和断裂楼板，不做真实物理，但保留“拆牌塔”的心理反馈。
- 低装饰密度：不追求华丽，而是让玩家能快速读懂方块类型、手牌效果和胜负状态。

## 视觉约束

- 主色：深灰、冷蓝灰、石板色。
- 强调色：琥珀黄、记忆金、危险橙、核心红。
- 禁止：赛博霓虹、过亮紫蓝渐变、纯文字白板界面。
- 字体：当前先用 Unity 内置字体，后续 Steam Demo 建议替换为可商用中文字体。
- 图像：所有当前资源为程序生成的占位美术，保存在 `Assets/_Project/Resources/Art`。
- 卡牌：当前优先使用  Agent 生成的 8 张竖版卡牌插画，保存在 `Assets/_Project/Resources/Art/Cards/AgentIllustrations`。
- Steam 包装：已生成官方尺寸草图，保存在 `Assets/_Project/StoreAssets/Steam`，正式提审前仍需替换为最终 Key Art 与宣传素材。

## Unity 实现依据

- uGUI `Image` 需要使用 Sprite 作为 Source Image，因此 PNG 资源统一导入为 Sprite。
- Canvas 使用 `Scale With Screen Size`，参考分辨率为 1920x1080。
- 按钮、面板使用带边框的 Sprite，避免缩放后失去边界感。

参考：

- Unity uGUI Image: https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/script-Image.html
- Unity Canvas Scaler: https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/script-CanvasScaler.html
- Unity Sprite Import Settings: https://docs.unity3d.com/Manual/texture-type-sprite.html
