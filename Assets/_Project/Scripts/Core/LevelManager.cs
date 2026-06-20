using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MemoryTower
{
    public sealed class LevelManager : MonoBehaviour
    {
        private const int InitialHandSize = 5;
        private const int HandLimit = 7;

        [SerializeField] private UIManager uiManager;

        private readonly DeckManager deckManager = new DeckManager();
        private readonly BuildingModel buildingModel = new BuildingModel();
        private readonly CollapseSystem collapseSystem = new CollapseSystem();
        private readonly RewardSystem rewardSystem = new RewardSystem();
        private readonly CardEffectResolver cardEffectResolver = new CardEffectResolver();

        private Dictionary<string, CardConfig> cardLookup;
        private LevelConfig currentLevel;
        private int currentLevelIndex;
        private int actionsRemaining;
        private int selectedHandIndex = -1;
        private int noDamageActionCount;
        private int weakHintUses;
        private LevelOutcome outcome = LevelOutcome.Playing;

        private void Start()
        {
            EnsureInitialized();
            StartLevel(GameState.Instance.requestedLevelIndex);
        }

        public void StartLevel(int levelIndex)
        {
            EnsureInitialized();
            currentLevelIndex = Mathf.Clamp(levelIndex, 0, BuiltInConfigs.Levels.Count - 1);
            GameState.Instance.requestedLevelIndex = currentLevelIndex;
            currentLevel = BuiltInConfigs.Levels[currentLevelIndex];
            actionsRemaining = currentLevel.actionCount;
            selectedHandIndex = -1;
            noDamageActionCount = 0;
            weakHintUses = 0;
            outcome = LevelOutcome.Playing;

            buildingModel.Generate(currentLevel);
            collapseSystem.Initialize(currentLevel.collapseThreshold);
            rewardSystem.Initialize();
            cardEffectResolver.Reset();
            uiManager.HideResult();

            deckManager.Initialize(BuildDeckIdsForLevel(currentLevelIndex), cardLookup);
            deckManager.Draw(InitialHandSize, HandLimit);

            uiManager.RenderBuilding(buildingModel, this);
            RefreshUi(CreateStartInstruction());
        }

        public void HandleCardClicked(int handIndex)
        {
            if (outcome != LevelOutcome.Playing)
            {
                return;
            }

            CardConfig card = deckManager.GetHandCard(handIndex);
            if (card == null)
            {
                return;
            }

            if (actionsRemaining <= 0)
            {
                RefreshUi("行动次数已耗尽。");
                EvaluateOutcome();
                return;
            }

            if (card.RequiresTarget)
            {
                selectedHandIndex = handIndex;
                RefreshUi("已选择「" + card.displayName + "」，请选择一个建筑方块。");
                return;
            }

            PlayCard(handIndex, null);
        }

        public void HandleBlockClicked(BlockModel block)
        {
            if (outcome != LevelOutcome.Playing)
            {
                return;
            }

            if (selectedHandIndex < 0)
            {
                RefreshUi("请先选择一张需要目标的手牌。");
                return;
            }

            CardConfig card = deckManager.GetHandCard(selectedHandIndex);
            if (card == null)
            {
                selectedHandIndex = -1;
                RefreshUi("手牌已变化，请重新选择。");
                return;
            }

            if (block == null || !block.IsSelectable)
            {
                RefreshUi("这个方块已经坍塌，不能再选择。");
                return;
            }

            if (card.targetType == CardTargetType.CoreBlock && !block.isCoreBlock)
            {
                RefreshUi("「核心裂解」只能打在红色核心块上。");
                return;
            }

            if (block.isCoreBlock && card.targetType != CardTargetType.CoreBlock)
            {
                RefreshUi("红色核心只能用「听见回声」加入手牌的「核心裂解」击破；普通破坏牌不能命中。");
                return;
            }

            PlayCard(selectedHandIndex, block);
        }

        public void HandleRedrawHand()
        {
            if (outcome != LevelOutcome.Playing)
            {
                return;
            }

            if (actionsRemaining <= 0)
            {
                RefreshUi("行动次数已耗尽，不能换手。");
                EvaluateOutcome();
                return;
            }

            int replaced = deckManager.Hand.Count;
            if (replaced == 0)
            {
                RefreshUi("当前没有手牌可替换。");
                return;
            }

            selectedHandIndex = -1;
            actionsRemaining--;
            deckManager.RedrawHand(HandLimit);
            collapseSystem.Add(replaced);
            noDamageActionCount++;
            RunSupportCheckIfNeeded();
            FinishAction("替换了 " + replaced + " 张手牌。");
        }

        public void HandleWeakHint()
        {
            if (!ShouldShowWeakHint())
            {
                return;
            }

            CardConfig coreFracture;
            if (!cardLookup.TryGetValue("core_fracture", out coreFracture))
            {
                return;
            }

            if (deckManager.AddCardToHand(coreFracture, HandLimit))
            {
                weakHintUses++;
                RefreshUi("你听见危楼深处的回声：「核心裂解」已加入手牌，它只能打红色核心。");
            }
            else
            {
                RefreshUi("手牌已满，暂时无法加入核心裂解。");
            }
        }

        public void RetryLevel()
        {
            StartLevel(currentLevelIndex);
        }

        public void NextLevel()
        {
            if (currentLevelIndex + 1 >= BuiltInConfigs.Levels.Count)
            {
                ReturnToMenu();
                return;
            }

            StartLevel(currentLevelIndex + 1);
        }

        public void ReturnToMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }

        private void PlayCard(int handIndex, BlockModel target)
        {
            CardConfig card = deckManager.GetHandCard(handIndex);
            if (card == null)
            {
                return;
            }

            if (card.RequiresTarget && target == null)
            {
                RefreshUi("这张牌需要选择一个目标。");
                return;
            }

            selectedHandIndex = -1;
            CardResolution resolution = cardEffectResolver.Resolve(card, target, buildingModel, collapseSystem, rewardSystem);
            deckManager.RemoveFromHand(handIndex, card.isOneShot);
            actionsRemaining--;

            rewardSystem.CollectFragments(resolution.newlyCollapsed);
            bool supportCheckedThisAction = false;
            if (resolution.forceSupportCheck || collapseSystem.ShouldCheckSupport())
            {
                rewardSystem.CollectFragments(buildingModel.RunSupportCheck());
                supportCheckedThisAction = true;
            }

            if (deckManager.Hand.Count < InitialHandSize)
            {
                int drawn = deckManager.Draw(1, HandLimit);
                if (drawn > 0)
                {
                    collapseSystem.Add(1);
                }
            }

            if (!supportCheckedThisAction)
            {
                RunSupportCheckIfNeeded();
            }

            if (resolution.hpChanged)
            {
                noDamageActionCount = 0;
            }
            else
            {
                noDamageActionCount++;
            }

            FinishAction("打出了「" + card.displayName + "」。");
        }

        private void FinishAction(string message)
        {
            EvaluateOutcome();
            RefreshUi(message);
        }

        private void EvaluateOutcome()
        {
            if (outcome != LevelOutcome.Playing)
            {
                return;
            }

            bool hasRequiredFragments = rewardSystem.FragmentsThisLevel >= currentLevel.requiredFragments;
            bool victory;

            if (currentLevel.hasFinalCore)
            {
                victory = hasRequiredFragments && buildingModel.IsCoreCollapsed();
            }
            else
            {
                victory = hasRequiredFragments && buildingModel.IsBuildingCollapsed();
            }

            if (victory)
            {
                outcome = LevelOutcome.Victory;
                GameState.Instance.MarkLevelComplete(currentLevel, currentLevelIndex, rewardSystem.FragmentsThisLevel);
                SaveManager.Save(GameState.Instance);
                uiManager.ShowResult(true, CreateVictoryMessage(), currentLevelIndex + 1 < BuiltInConfigs.Levels.Count, this);
                return;
            }

            bool defeat = actionsRemaining <= 0;
            if (currentLevel.hasFinalCore)
            {
                defeat = defeat || collapseSystem.IsOutOfControl();
            }
            else if (collapseSystem.IsOutOfControl())
            {
                defeat = true;
            }

            if (defeat)
            {
                outcome = LevelOutcome.Defeat;
                uiManager.ShowResult(false, CreateDefeatMessage(), false, this);
            }
        }

        private void RunSupportCheckIfNeeded()
        {
            if (!collapseSystem.ShouldCheckSupport())
            {
                return;
            }

            rewardSystem.CollectFragments(buildingModel.RunSupportCheck());
        }

        private IEnumerable<string> BuildDeckIdsForLevel(int levelIndex)
        {
            List<string> result = new List<string>();
            LevelConfig level = BuiltInConfigs.Levels[levelIndex];
            foreach (string cardId in level.initialDeckCardIds)
            {
                CardConfig card;
                if (!cardLookup.TryGetValue(cardId, out card))
                {
                    continue;
                }

                if (card.cardType == CardType.Basic || GameState.Instance.unlockedCardIds.Contains(cardId))
                {
                    result.Add(cardId);
                }
            }

            while (result.Count < 8)
            {
                result.Add(result.Count % 2 == 0 ? "tap" : "strike");
            }

            return result;
        }

        private bool ShouldShowWeakHint()
        {
            if (currentLevel == null || !currentLevel.hasFinalCore || weakHintUses >= 2 || outcome != LevelOutcome.Playing)
            {
                return false;
            }

            return noDamageActionCount >= 2 || collapseSystem.Value >= Mathf.CeilToInt(currentLevel.collapseThreshold * 0.7f);
        }

        private string CreateVictoryMessage()
        {
            if (currentLevel.hasFinalCore)
            {
                return "核心块已经坍塌，最后的记忆碎片被保留下来。";
            }

            return "获得 " + rewardSystem.FragmentsThisLevel + " 个记忆碎片，危楼进入可控坍塌。";
        }

        private string CreateStartInstruction()
        {
            if (currentLevel != null && currentLevel.hasFinalCore)
            {
                return "最终关：普通牌不能打红色核心。先观察、稳住或封存回声，等「听见回声」生成核心裂解。";
            }

            return "选择一张手牌开始拆解。";
        }

        private string CreateDefeatMessage()
        {
            if (collapseSystem.IsOutOfControl())
            {
                return "坍塌值达到失控阈值，记忆碎片散落。";
            }

            return "行动次数耗尽，仍未满足本关目标。";
        }

        private void RefreshUi(string instruction)
        {
            uiManager.UpdateHud(currentLevel, collapseSystem, actionsRemaining, rewardSystem.FragmentsThisLevel, deckManager, GameState.Instance.totalFragments);
            uiManager.RenderHand(deckManager, this, selectedHandIndex);
            uiManager.RefreshBlockViews();
            uiManager.SetInstruction(instruction);
            uiManager.SetWeakHintVisible(ShouldShowWeakHint());
        }

        private void EnsureInitialized()
        {
            if (cardLookup == null)
            {
                cardLookup = BuiltInConfigs.CreateCardLookup();
            }

            if (uiManager == null)
            {
                uiManager = FindFirstObjectByType<UIManager>();
            }

            if (uiManager == null)
            {
                uiManager = gameObject.AddComponent<UIManager>();
            }

            uiManager.BuildGameUi(this);
        }
    }
}
