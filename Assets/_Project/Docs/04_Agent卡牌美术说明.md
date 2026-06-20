# 《记忆危楼》Agent 卡牌美术说明

日期：2026-06-20  
资源目录：`Assets/_Project/Resources/Art/Cards/AgentIllustrations`  
生成脚本：`Assets/_Project/Tools/generate_agent_card_art.py`

## 目标

这批资源用于 MVP Demo 的卡牌插画，占位层级高于纯 UI 卡底。目标是让手牌区域第一眼更像一个可试玩的 Steam 独立游戏 Demo，而不是一组文字按钮。

统一方向：

- 暗色废墟档案馆：深灰、黑石、档案划痕、灰尘颗粒。
- 纸牌与断裂楼板：每张卡都有卡框、暗面板和中心结构破裂物。
- 暖金记忆光：用于轻敲、观察、尘封回声等偏记忆信息的动作。
- 核心红：只给高风险、核心破坏类动作使用，避免整套卡变成杂乱霓虹。
- 低文字依赖：卡面只保留抽象视觉符号，具体规则由 UI 文本说明。

## 资源清单

| 卡牌 ID | 文件 | 视觉意图 | 主色 | 后续正式美术替换建议 |
|---|---|---|---|---|
| `tap` | `tap.png` | 轻敲楼板，中间是细裂纹和暖金触点，表达试探性影响。 | 深灰、暖金、石板灰 | 可替换为手指、测震针或细小金色脉冲；裂纹保持轻微，避免像重击。 |
| `strike` | `strike.png` | 大裂纹冲击，橙金楔形力量穿过楼板，表达直接破坏。 | 暗灰、危险橙、黑裂纹 | 可替换成坠落锤影、冲击碎石或强烈结构断裂；保留高对比中心爆点。 |
| `stabilize` | `stabilize.png` | 支撑梁和绷带压住裂缝，表达临时加固。 | 冷青灰、旧绷带米色、柔和金点 | 可做成木梁、钢梁、档案封条混合结构；看起来应是“稳住了”，不是治疗魔法。 |
| `inspect_crack` | `inspect_crack.png` | 放大镜覆盖裂缝，表达观察和判断结构弱点。 | 冷灰、玻璃灰、暖金镜框 | 可强化镜片折射、标注线和微小碎屑；不要加入大量文字标注。 |
| `cut_support` | `cut_support.png` | 两根支撑梁被切开，红色切线穿过结构，表达切除支点。 | 冷蓝灰、危险橙、核心红 | 可替换为断梁截面、切割粉尘和坠落预兆；红色只作危险切线。 |
| `chain_shock` | `chain_shock.png` | 多个结构节点被震动波串联，表达连锁震动。 | 冷蓝灰、低饱和蓝、少量金线 | 正式图可加入连续楼层、波纹传播和次级裂纹；重点是传导，不是单点爆炸。 |
| `sealed_echo` | `sealed_echo.png` | 金色记忆回环包住星形碎片，表达尘封记忆被唤回。 | 暗紫灰、记忆金、旧纸色 | 可替换为封存档案、金色回声环或记忆碎片；保持温暖、神秘、非科幻霓虹。 |
| `core_fracture` | `core_fracture.png` | 红色核心被裂解，中心红核与黑裂纹突出高风险终局感。 | 深红、核心红、少量暖金 | 正式图可做成危楼核心、红色结晶或承重核心破裂；它应是整套中最危险的一张。 |

## Unity 接入

- 运行时优先路径：`Resources.Load<Sprite>("Art/Cards/AgentIllustrations/{card_id}")`。
- 已在 `VisualAssets.CardSprite` 中接入；如果找不到 Agent 插画，会回退到 `Art/Cards/card_{card_id}`，再回退到通用卡底。
- `MemoryTowerProjectBuilder.ConfigureVisualAssets()` 会把 `Assets/_Project/Resources/Art` 下的图片导入为 `Sprite (2D and UI)`。
- 后续正式美术保持 `512x768` 竖卡比例并替换同名 PNG，即可减少代码改动。

## 生成方式

```powershell
python Assets/_Project/Tools/generate_agent_card_art.py
```

脚本会重新生成 8 张 PNG：

- `tap.png`
- `strike.png`
- `stabilize.png`
- `inspect_crack.png`
- `cut_support.png`
- `chain_shock.png`
- `sealed_echo.png`
- `core_fracture.png`
