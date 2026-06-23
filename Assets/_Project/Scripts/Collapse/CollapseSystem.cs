using UnityEngine;

namespace MemoryTower
{
    public sealed class CollapseSystem
    {
        public int Value { get; private set; }
        public int Threshold { get; private set; }

        public int LossThreshold
        {
            get { return Mathf.CeilToInt(Threshold * 1.5f); }
        }

        public void Initialize(int threshold)
        {
            Threshold = Mathf.Max(1, threshold);
            Value = 0;
        }

        public void Add(int delta)
        {
            Value = Mathf.Max(0, Value + delta);
        }

        public bool ShouldCheckSupport()
        {
            return Value >= Threshold;
        }

        public bool IsOutOfControl()
        {
            return Value >= LossThreshold;
        }

        public int Percent()
        {
            return Mathf.RoundToInt((float)Value / Threshold * 100f);
        }
    }
}
