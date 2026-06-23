# 《记忆危楼》Steam 视觉包装与上层参考

日期：2026-06-20  
状态：MVP 草案，用于后续正式美术、商店页与交付规范继续迭代。

## 上层资料提炼

来源目录：`C:\Users\krona\Documents\File\File_MD\vault\tech\game`

- 21 天学生项目不应按完整 Steam 商业游戏体量排期，应优先做 10-15 分钟、高完成度、有开头和结尾、视觉统一的可试玩 Demo。
- “最佳学生奖”方向更看重第一眼完成感、风格统一、主题和机制绑定，而不是大量内容堆砌。MVP 目标是接近参考作品第一印象的 60%-70%，内容体量控制在参考作品的 10%-25%。
- 可借鉴《玛卡之歌》的情绪清晰、童话气质和原创世界观统一，但不复制它的大量手绘场景体量。
- 可借鉴《危光》的少量资产 + 光影/VFX/后期形成强观感。对《记忆危楼》来说，对应策略是“少量卡牌和危楼模块 + 明确光点/裂纹反馈”。
- 学生获奖作品共性：强概念优先、机制与主题绑定、小切口叙事、完成度比体量重要、风格化胜过精细堆量。

## 当前视觉定位

《记忆危楼》当前不是横版平台光影游戏，而是“卡牌拆解危楼”的策略小品。MVP 版本采用以下视觉原则：

| 上层原则 | Demo 落地 |
|---|---|
| 一个强视觉符号 | 纸牌式危楼、裂纹、记忆光点 |
| 少量资产形成统一观感 | 一套 UI、8 张卡图、6 种方块状态 |
| 光影/VFX 补足美术体量 | 冷色废墟 + 暖金记忆碎片 |
| 机制即主题 | 打牌不是攻击敌人，而是在有限行动内拆出记忆危楼 |
| 完成度优先 | 主菜单、关卡、胜负、文档、Steam 包装草图齐备 |

## Steam 包装草案

生成目录：`Assets/_Project/StoreAssets/Steam`

| 文件 | 用途 |
|---|---|
| `steam_header_capsule_920x430.png` | Steam Header Capsule 草案 |
| `steam_small_capsule_462x174.png` | Steam Small Capsule 草案 |
| `steam_main_capsule_1232x706.png` | Steam Main Capsule 草案 |
| `steam_vertical_capsule_748x896.png` | Steam Vertical Capsule 草案 |
| `steam_page_background_1438x810.png` | Steam Page Background 草案 |
| `steam_library_capsule_600x900.png` | Steam Library Capsule 草案 |
| `steam_library_header_920x430.png` | Steam Library Header 草案 |
| `steam_library_hero_3840x1240.png` | Steam Library Hero 草案 |
| `steam_logo_transparent_1280x720.png` | 透明 Logo 草案 |

注意：当前资源仅用于 MVP 视觉方向与商店页预览，不代表最终 Steam 提审素材。正式提审前仍需再次核对 Steamworks 后台的最新规范、安全区、动图/截图数量和本地化要求。

## 官方规范记录

本轮用于设计的公开官方资料：

- Steamworks Graphical Assets Overview: https://partner.steamgames.com/doc/store/assets
  - Header Capsule: 920x430
  - Small Capsule: 462x174
  - Main Capsule: 1232x706
  - Vertical Capsule: 748x896
  - Screenshot: minimum 1920x1080, 16:9
  - Page Background: 1438x810
- Steamworks Library Assets: https://partner.steamgames.com/doc/store/assets/libraryassets
  - Library Capsule: 600x900
  - Library Header: 920x430
  - Library Hero: 3840x1240
  - Library Logo: 1280px wide and/or 720px tall, transparent PNG

## 卡牌图片策略

8 张卡牌均已生成独立卡图：

| 卡牌 | 图形符号 |
|---|---|
| 轻敲 | 细裂纹与轻触圆环 |
| 重击 | 闪电式冲击裂纹 |
| 稳住 | 横梁与竖向支撑 |
| 观察裂缝 | 放大镜与裂纹 |
| 支点切除 | 被切断的承重梁 |
| 连锁震动 | 三块方块与扩散波纹 |
| 封存回声 | 记忆碎片与回环 |
| 核心裂解 | 红色核心破裂 |

原则：

- 卡图不依赖中文文字，避免在小尺寸 UI 中失真。
- UI 文本负责说明数值，卡图负责建立记忆点。
- 基础牌、功能牌、一次性牌通过边框与主色区分。

## 当前差距

- 现有图片是程序化生成的占位美术，不是最终手绘或 3D 资产。
- 缺少真实 Steam 页面截图、正式商店文案、宣传 GIF 和可提交构建包。
- 后续建议优先补一张高质量 Key Art，再替换当前 Steam 草案中的塔楼主体。
