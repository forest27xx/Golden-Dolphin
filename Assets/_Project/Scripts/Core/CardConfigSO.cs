using System;
using UnityEngine;

namespace MemoryTower
{
    [CreateAssetMenu(fileName = "CardConfig", menuName = "MemoryTower/Configs/Card Config")]
    public sealed class CardConfigSO : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private CardType cardType;
        [SerializeField] private string description;
        [SerializeField] private int damage;
        [SerializeField] private int collapseDelta;
        [SerializeField] private CardTargetType targetType;
        [SerializeField] private bool isOneShot;
        [SerializeField] private int unlockLevel;

        public string Id
        {
            get { return id; }
        }

        public CardConfig ToConfig()
        {
            return new CardConfig(
                id,
                displayName,
                cardType,
                description,
                damage,
                collapseDelta,
                targetType,
                isOneShot,
                unlockLevel);
        }

        public void Initialize(CardConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            this.id = config.id;
            this.displayName = config.displayName;
            this.cardType = config.cardType;
            this.description = config.description;
            this.damage = config.damage;
            this.collapseDelta = config.collapseDelta;
            this.targetType = config.targetType;
            this.isOneShot = config.isOneShot;
            this.unlockLevel = config.unlockLevel;
        }
    }
}
