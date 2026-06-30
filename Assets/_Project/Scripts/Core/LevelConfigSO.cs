using System;
using System.Collections.Generic;
using UnityEngine;

namespace MemoryTower
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "MemoryTower/Configs/Level Config")]
    public sealed class LevelConfigSO : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private int rows;
        [SerializeField] private int columns;
        [SerializeField] private int collapseThreshold;
        [SerializeField] private int actionCount;
        [SerializeField] private int requiredFragments;
        [SerializeField] private List<string> initialDeckCardIds = new List<string>();
        [SerializeField] private List<string> rewardCardIds = new List<string>();
        [SerializeField] private bool hasFinalCore;

        public string Id
        {
            get { return id; }
        }

        public LevelConfig ToConfig()
        {
            IEnumerable<string> initialDeckIds = initialDeckCardIds == null ? new string[0] : initialDeckCardIds;
            IEnumerable<string> rewardIds = rewardCardIds == null ? new string[0] : rewardCardIds;

            return new LevelConfig(
                id,
                displayName,
                rows,
                columns,
                collapseThreshold,
                actionCount,
                requiredFragments,
                initialDeckIds,
                rewardIds,
                hasFinalCore);
        }

        public void Initialize(LevelConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            this.id = config.id;
            this.displayName = config.displayName;
            this.rows = config.rows;
            this.columns = config.columns;
            this.collapseThreshold = config.collapseThreshold;
            this.actionCount = config.actionCount;
            this.requiredFragments = config.requiredFragments;
            this.initialDeckCardIds = new List<string>(config.initialDeckCardIds);
            this.rewardCardIds = new List<string>(config.rewardCardIds);
            this.hasFinalCore = config.hasFinalCore;
        }
    }
}
