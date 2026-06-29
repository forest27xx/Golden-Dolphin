# Audio System Design Document — Task #3

日期：2026-06-30
分支：feature/audio-system
状态：设计完成，待实现

## 1. 目标

为《记忆危楼》MVP 添加完整的音频系统，覆盖 BGM 和 SFX 两类音频播放，支持分组音量控制，并通过事件驱动的接口与现有 LevelManager / UIManager 对接。

### 设计原则

- **不侵入核心逻辑**：Audio 系统作为独立模块，通过事件接口被调用，不修改 Cards / Building / Collapse 的规则代码。
- **数据驱动**：音频映射表用 ScriptableObject 配置，不在运行时硬编码 AudioClip 引用。
- **Singleton + DontDestroyOnLoad**：AudioManager 跨场景存活，BGM 不因场景切换中断。
- **遵循 AGENTS.md 约定**：使用 MemoryTower 命名空间，[SerializeField] private 字段，小模型/服务类 + MonoBehaviour 场景绑定。

## 2. 架构总览

```
AudioManager (MonoBehaviour, Singleton)
├── AudioChannel (BGM)
│   ├── AudioSource (loop)
│   └── volume scale: 0~1, mapped to Master * BGM
├── AudioChannel (SFX)
│   ├── AudioSource (one-shot pool, 8 slots)
│   └── volume scale: 0~1, mapped to Master * SFX
└── AudioConfig (ScriptableObject)
    ├── BGM clips: Dictionary<SceneName, AudioClip>
    └── SFX clips: Dictionary<SfxType, AudioClip>
```

## 3. 音频事件枚举

根据现有玩法循环，定义以下 SFX 触发点：

| SFX 类型 | 枚举值 | 触发位置 | 描述 |
|---|---|---|---|
| 卡牌选中 | CardSelected | LevelManager.HandleCardClicked | 选中需要目标的手牌 |
| 卡牌打出 | CardPlayed | LevelManager.PlayCard | 通用出牌音 |
| 方块受击 | BlockDamaged | BuildingModel.ApplyDamage | HP 下降但未坍塌 |
| 方块坍塌 | BlockCollapsed | BuildingModel.CollapseBlock | HP 归零坍塌 |
| 支撑检查 | SupportCheck | BuildingModel.RunSupportCheck | 支撑检查开始 |
| 不稳定预警 | UnstableWarning | BuildingModel.RunSupportCheck | 方块进入不稳定状态 |
| 碎片获得 | FragmentGained | RewardSystem.CollectFragments | 获得记忆碎片 |
| 抽牌 | CardDrawn | DeckManager.Draw | 抽牌 |
| 换手 | HandRedrawn | LevelManager.HandleRedrawHand | 替换手牌 |
| 听见回声 | WeakHint | LevelManager.HandleWeakHint | 核心裂解加入手牌 |
| 坍塌值增加 | CollapsePressure | CollapseSystem.Add | 坍塌值上升（可选，阈值报警） |
| 胜利 | Victory | LevelManager.EvaluateOutcome | 关卡胜利 |
| 失败 | Defeat | LevelManager.EvaluateOutcome | 关卡失败 |
| UI 点击 | UiClick | UIManager 按钮点击 | 通用按钮音 |
| 关卡开始 | LevelStart | LevelManager.StartLevel | 关卡初始化 |
| BGM 切换 | BgmSwitch | AudioManager.PlayBgm | 场景切换 |

## 4. 类设计

### 4.1 SfxType 枚举

```csharp
namespace MemoryTower
{
    public enum SfxType
    {
        CardSelected,
        CardPlayed,
        BlockDamaged,
        BlockCollapsed,
        SupportCheck,
        UnstableWarning,
        FragmentGained,
        CardDrawn,
        HandRedrawn,
        WeakHint,
        CollapsePressure,
        Victory,
        Defeat,
        UiClick,
        LevelStart
    }
}
```

### 4.2 AudioConfig (ScriptableObject)

```csharp
[CreateAssetMenu(fileName = "AudioConfig", menuName = "MemoryTower/Audio Config")]
public sealed class AudioConfig : ScriptableObject
{
    [SerializeField] private AudioClip bgmMenu;
    [SerializeField] private AudioClip bgmGame;
    [SerializeField] private List<SfxEntry> sfxEntries = new List<SfxEntry>();

    public AudioClip GetBgm(string sceneName) { ... }
    public AudioClip GetSfx(SfxType type) { ... }
}

[Serializable]
public sealed class SfxEntry
{
    public SfxType type;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
}
```

### 4.3 AudioManager (MonoBehaviour Singleton)

```csharp
public sealed class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioConfig config;
    [Range(0f, 1f)] [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float bgmVolume = 0.6f;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;

    // SFX one-shot pool
    private AudioSource[] sfxPool;  // 8 slots, round-robin
    private AudioSource bgmSource;
    private int sfxPoolIndex;

    // Public API
    public void PlaySfx(SfxType type);
    public void PlayBgm(string sceneName);
    public void StopBgm();
    public void SetMasterVolume(float v);
    public void SetBgmVolume(float v);
    public void SetSfxVolume(float v);
    public void ToggleMute();
}
```

### 4.4 AudioEvents (静态事件总线)

为了不侵入核心逻辑代码，使用 C# event 做松耦合：

```csharp
public static class AudioEvents
{
    public static event Action<SfxType> OnSfxRequested;
    public static event Action<string> OnBgmRequested;

    public static void RequestSfx(SfxType type) => OnSfxRequested?.Invoke(type);
    public static void RequestBgm(string sceneName) => OnBgmRequested?.Invoke(sceneName);
}
```

核心系统通过 `AudioEvents.RequestSfx(SfxType.XXX)` 触发音效，AudioManager 订阅事件并播放。这样 BuildingModel / CardEffectResolver 等非 MonoBehaviour 类也能触发音效。

## 5. 集成点

### 5.1 LevelManager 集成

在以下方法中添加 AudioEvents.RequestSfx 调用：

| 方法 | 事件 |
|---|---|
| StartLevel | SfxType.LevelStart + BGM 切换 |
| HandleCardClicked (requires target) | SfxType.CardSelected |
| PlayCard | SfxType.CardPlayed |
| HandleRedrawHand | SfxType.HandRedrawn |
| HandleWeakHint | SfxType.WeakHint |
| EvaluateOutcome (victory) | SfxType.Victory |
| EvaluateOutcome (defeat) | SfxType.Defeat |

### 5.2 BuildingModel 集成

| 方法 | 事件 |
|---|---|
| ApplyDamage (hpChanged, not collapsed) | SfxType.BlockDamaged |
| CollapseBlock | SfxType.BlockCollapsed |
| RunSupportCheck (开始) | SfxType.SupportCheck |
| RunSupportCheck (进入 unstable) | SfxType.UnstableWarning |

### 5.3 RewardSystem 集成

| 方法 | 事件 |
|---|---|
| CollectFragments (gained > 0) | SfxType.FragmentGained |

### 5.4 DeckManager 集成

| 方法 | 事件 |
|---|---|
| Draw (drawn > 0) | SfxType.CardDrawn |

### 5.5 UIManager 集成

| 位置 | 事件 |
|---|---|
| 所有 Button.onClick | SfxType.UiClick (可选，可在 AudioManager 中统一监听) |

### 5.6 MainMenuController 集成

| 方法 | 事件 |
|---|---|
| Start | BGM 切换到 Menu |
| StartNewGame / ContinueGame | BGM 切换到 Game |
| ShowSettings | 替换占位设置为真实音量控制 |

## 6. 音频文件组织

```
Assets/Audio/
├── BGM/
│   ├── menu.wav          # 主菜单 BGM
│   └── game.wav          # 游戏内 BGM
├── SFX/
│   ├── card_select.wav
│   ├── card_play.wav
│   ├── block_damage.wav
│   ├── block_collapse.wav
│   ├── support_check.wav
│   ├── unstable.wav
│   ├── fragment.wav
│   ├── draw.wav
│   ├── redraw.wav
│   ├── weak_hint.wav
│   ├── pressure.wav
│   ├── victory.wav
│   ├── defeat.wav
│   ├── ui_click.wav
│   └── level_start.wav
└── AudioConfig.asset     # ScriptableObject 配置
```

当前阶段音频文件由兰萌（原创音效/音乐）提供，暂用占位文件。AudioConfig.asset 在 Unity Editor 中手动创建并配置。

## 7. 设置面板更新

MainMenuController 的设置面板从占位文本改为真实功能：

- 主音量滑块 (0~100)
- 音乐音量滑块 (0~100)
- 音效音量滑块 (0~100)
- 静音按钮

通过 AudioManager.Instance.SetMasterVolume / SetBgmVolume / SetSfxVolume 实时控制。

## 8. 实现步骤

1. 创建 `Assets/_Project/Scripts/Audio/` 目录
2. 实现 SfxType 枚举
3. 实现 AudioConfig ScriptableObject
4. 实现 AudioEvents 静态事件总线
5. 实现 AudioManager MonoBehaviour
6. 在 LevelManager 中添加事件触发
7. 在 BuildingModel 中添加事件触发
8. 在 RewardSystem 中添加事件触发
9. 在 DeckManager 中添加事件触发
10. 更新 MainMenuController 设置面板
11. 创建 AudioConfig.asset 占位配置
12. 在场景中放置 AudioManager GameObject（或自动创建）

## 9. 注意事项

- AudioManager 在 Awake 中初始化，使用 DontDestroyOnLoad 跨场景存活
- 如果场景中已有 AudioManager，不再创建新的（防止重复）
- SFX 播放使用对象池，避免频繁创建/销毁 AudioSource
- BGM 切换时使用淡入淡出（1秒过渡），避免硬切
- 音量设置持久化到 PlayerPrefs（与 SaveManager 同一存储方式）
- 所有音频触发使用事件总线，核心逻辑类不直接引用 AudioManager
- .meta 文件必须提交
- 不提交 Library/ Temp/ 等生成目录
