using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MemoryTower.EditorTools
{
    public static class MemoryTowerSmokeTest
    {
        [MenuItem("Memory Tower/Run Smoke Test")]
        public static void Run()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameState.Instance.ResetProgress();

            GameObject root = new GameObject("SmokeTestRoot");
            UIManager uiManager = root.AddComponent<UIManager>();
            LevelManager levelManager = root.AddComponent<LevelManager>();
            SerializedObject serializedLevel = new SerializedObject(levelManager);
            serializedLevel.FindProperty("uiManager").objectReferenceValue = uiManager;
            serializedLevel.ApplyModifiedPropertiesWithoutUndo();

            AssertVisualAssetsLoad();
            levelManager.StartLevel(0);

            BuildingModel building = GetPrivateField<BuildingModel>(levelManager, "buildingModel");
            DeckManager deck = GetPrivateField<DeckManager>(levelManager, "deckManager");
            RewardSystem rewards = GetPrivateField<RewardSystem>(levelManager, "rewardSystem");
            CollapseSystem collapse = GetPrivateField<CollapseSystem>(levelManager, "collapseSystem");
            BlockModel memoryBlock = FindFirstMemoryBlock(building);

            if (memoryBlock == null)
            {
                throw new System.Exception("Smoke test failed: tutorial has no memory block.");
            }

            for (int i = 0; i < 5 && !memoryBlock.IsCollapsed; i++)
            {
                int handIndex = FindBestDamageCard(deck);
                if (handIndex < 0)
                {
                    throw new System.Exception("Smoke test failed: no playable damage card in hand.");
                }

                levelManager.HandleCardClicked(handIndex);
                levelManager.HandleBlockClicked(memoryBlock);
            }

            if (!memoryBlock.IsCollapsed)
            {
                throw new System.Exception("Smoke test failed: memory block was not collapsed.");
            }

            if (rewards.FragmentsThisLevel < 1)
            {
                throw new System.Exception("Smoke test failed: collapsing a memory block did not award a fragment.");
            }

            if (collapse.Value <= 0)
            {
                throw new System.Exception("Smoke test failed: collapse value did not change.");
            }

            levelManager.StartLevel(0);
            for (int i = 0; i < 5; i++)
            {
                levelManager.HandleRedrawHand();
            }

            LevelOutcome outcome = GetPrivateField<LevelOutcome>(levelManager, "outcome");
            if (outcome != LevelOutcome.Defeat)
            {
                throw new System.Exception("Smoke test failed: action exhaustion did not trigger defeat.");
            }

            AssertAllDemoLevelsReachVictory(levelManager);
            AssertFinalWeakHintFlow(levelManager);

            Debug.Log("Memory Tower smoke test passed.");
        }

        private static void AssertVisualAssetsLoad()
        {
            string[] requiredSprites =
            {
                VisualAssets.MenuBackground,
                VisualAssets.GameBackground,
                VisualAssets.PanelDark,
                VisualAssets.ButtonIdle,
                VisualAssets.CardBasic,
                VisualAssets.CardFunction,
                "Art/Building/block_normal",
                "Art/Building/block_memory",
                VisualAssets.Fragment
            };

            foreach (string path in requiredSprites)
            {
                if (VisualAssets.LoadSprite(path) == null)
                {
                    throw new System.Exception("Smoke test failed: missing sprite resource " + path);
                }
            }

            Dictionary<string, CardConfig> cards = BuiltInConfigs.CreateCardLookup();
            foreach (CardConfig card in cards.Values)
            {
                if (VisualAssets.CardSprite(card, false) == null)
                {
                    throw new System.Exception("Smoke test failed: missing card sprite for " + card.id);
                }
            }
        }

        private static void AssertAllDemoLevelsReachVictory(LevelManager levelManager)
        {
            SaveManager.Clear();
            GameState.Instance.ResetProgress();
            Dictionary<string, CardConfig> cards = BuiltInConfigs.CreateCardLookup();

            for (int levelIndex = 0; levelIndex < BuiltInConfigs.Levels.Count; levelIndex++)
            {
                levelManager.StartLevel(levelIndex);
                LevelConfig level = BuiltInConfigs.Levels[levelIndex];
                BuildingModel building = GetPrivateField<BuildingModel>(levelManager, "buildingModel");
                DeckManager deck = GetPrivateField<DeckManager>(levelManager, "deckManager");

                if (level.hasFinalCore)
                {
                    BlockModel core = FindCoreBlock(building);
                    if (core == null)
                    {
                        throw new System.Exception("Smoke test failed: final level has no core block.");
                    }

                    ForceHand(deck, cards, "strike", "strike", "strike");
                    int coreHp = core.hp;
                    PlayTargetCard(levelManager, deck, "strike", core);
                    if (core.hp != coreHp)
                    {
                        throw new System.Exception("Smoke test failed: ordinary damage changed core HP.");
                    }

                    ForceHand(deck, cards, "core_fracture", "core_fracture", "stabilize", "inspect_crack", "sealed_echo");
                    PlayTargetCard(levelManager, deck, "core_fracture", core);
                    ForceHand(deck, cards, "core_fracture", "stabilize", "inspect_crack", "sealed_echo", "tap");
                    PlayTargetCard(levelManager, deck, "core_fracture", core);
                }
                else
                {
                    List<BlockModel> memoryBlocks = FindMemoryBlocks(building);
                    if (memoryBlocks.Count < level.requiredFragments)
                    {
                        throw new System.Exception("Smoke test failed: level " + level.id + " has fewer memory blocks than required fragments.");
                    }

                    foreach (BlockModel block in memoryBlocks)
                    {
                        ForceHand(deck, cards, "strike", "strike", "strike", "stabilize", "tap");
                        PlayTargetCard(levelManager, deck, "strike", block);
                    }
                }

                LevelOutcome outcome = GetPrivateField<LevelOutcome>(levelManager, "outcome");
                if (outcome != LevelOutcome.Victory)
                {
                    throw new System.Exception("Smoke test failed: level " + level.id + " did not reach victory in scripted playthrough.");
                }
            }

            if (GameState.Instance.completedLevelIds.Count != BuiltInConfigs.Levels.Count)
            {
                throw new System.Exception("Smoke test failed: scripted playthrough did not mark every level complete.");
            }
        }

        private static void AssertFinalWeakHintFlow(LevelManager levelManager)
        {
            GameState.Instance.ResetProgress();
            Dictionary<string, CardConfig> cards = BuiltInConfigs.CreateCardLookup();
            int finalIndex = BuiltInConfigs.Levels.Count - 1;
            levelManager.StartLevel(finalIndex);

            BuildingModel building = GetPrivateField<BuildingModel>(levelManager, "buildingModel");
            DeckManager deck = GetPrivateField<DeckManager>(levelManager, "deckManager");
            BlockModel core = FindCoreBlock(building);
            if (core == null)
            {
                throw new System.Exception("Smoke test failed: final weak hint flow has no core block.");
            }

            ForceHand(deck, cards, "stabilize", "sealed_echo", "tap", "strike", "stabilize");
            PlayNonTargetCard(levelManager, deck, "stabilize");
            PlayNonTargetCard(levelManager, deck, "sealed_echo");
            levelManager.HandleWeakHint();
            if (FindCardInHand(deck, "core_fracture") < 0)
            {
                throw new System.Exception("Smoke test failed: weak hint did not add core_fracture. noDamage="
                    + GetPrivateField<int>(levelManager, "noDamageActionCount")
                    + ", weakHintUses=" + GetPrivateField<int>(levelManager, "weakHintUses")
                    + ", outcome=" + GetPrivateField<LevelOutcome>(levelManager, "outcome")
                    + ", hand=" + CreateHandDebugString(deck));
            }

            PlayTargetCard(levelManager, deck, "core_fracture", core);

            ForceHand(deck, cards, "stabilize", "sealed_echo", "tap", "strike", "stabilize");
            PlayNonTargetCard(levelManager, deck, "stabilize");
            PlayNonTargetCard(levelManager, deck, "sealed_echo");
            levelManager.HandleWeakHint();
            if (FindCardInHand(deck, "core_fracture") < 0)
            {
                throw new System.Exception("Smoke test failed: second weak hint did not add core_fracture. noDamage="
                    + GetPrivateField<int>(levelManager, "noDamageActionCount")
                    + ", weakHintUses=" + GetPrivateField<int>(levelManager, "weakHintUses")
                    + ", outcome=" + GetPrivateField<LevelOutcome>(levelManager, "outcome")
                    + ", hand=" + CreateHandDebugString(deck));
            }

            PlayTargetCard(levelManager, deck, "core_fracture", core);

            LevelOutcome outcome = GetPrivateField<LevelOutcome>(levelManager, "outcome");
            if (outcome != LevelOutcome.Victory)
            {
                throw new System.Exception("Smoke test failed: final weak hint flow did not reach victory.");
            }
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            return (T)field.GetValue(target);
        }

        private static BlockModel FindFirstMemoryBlock(BuildingModel building)
        {
            foreach (BlockModel block in building.Blocks)
            {
                if (block.isMemoryBlock)
                {
                    return block;
                }
            }

            return null;
        }

        private static List<BlockModel> FindMemoryBlocks(BuildingModel building)
        {
            List<BlockModel> result = new List<BlockModel>();
            foreach (BlockModel block in building.Blocks)
            {
                if (block.isMemoryBlock)
                {
                    result.Add(block);
                }
            }

            return result;
        }

        private static BlockModel FindCoreBlock(BuildingModel building)
        {
            foreach (BlockModel block in building.Blocks)
            {
                if (block.isCoreBlock)
                {
                    return block;
                }
            }

            return null;
        }

        private static void ForceHand(DeckManager deck, Dictionary<string, CardConfig> cards, params string[] cardIds)
        {
            deck.Hand.Clear();
            foreach (string cardId in cardIds)
            {
                CardConfig card;
                if (!cards.TryGetValue(cardId, out card))
                {
                    throw new System.Exception("Smoke test failed: unknown card id " + cardId);
                }

                deck.Hand.Add(card);
            }
        }

        private static void PlayTargetCard(LevelManager levelManager, DeckManager deck, string cardId, BlockModel target)
        {
            int handIndex = FindCardInHand(deck, cardId);
            if (handIndex < 0)
            {
                throw new System.Exception("Smoke test failed: hand does not contain " + cardId);
            }

            levelManager.HandleCardClicked(handIndex);
            levelManager.HandleBlockClicked(target);
        }

        private static void PlayNonTargetCard(LevelManager levelManager, DeckManager deck, string cardId)
        {
            int handIndex = FindCardInHand(deck, cardId);
            if (handIndex < 0)
            {
                throw new System.Exception("Smoke test failed: hand does not contain " + cardId);
            }

            levelManager.HandleCardClicked(handIndex);
        }

        private static int FindCardInHand(DeckManager deck, string cardId)
        {
            for (int i = 0; i < deck.Hand.Count; i++)
            {
                if (deck.Hand[i].id == cardId)
                {
                    return i;
                }
            }

            return -1;
        }

        private static string CreateHandDebugString(DeckManager deck)
        {
            List<string> ids = new List<string>();
            foreach (CardConfig card in deck.Hand)
            {
                ids.Add(card.id);
            }

            return string.Join(",", ids.ToArray());
        }

        private static int FindBestDamageCard(DeckManager deck)
        {
            int fallback = -1;
            for (int i = 0; i < deck.Hand.Count; i++)
            {
                CardConfig card = deck.Hand[i];
                if (card.id == "strike")
                {
                    return i;
                }

                if (card.damage > 0)
                {
                    fallback = i;
                }
            }

            return fallback;
        }
    }
}
