using UnityEditor;
using UnityEngine;

namespace MemoryTower.EditorTools
{
    public static class LevelBootSelfTest
    {
        [MenuItem("Memory Tower/Self-Test Level Boot")]
        public static void Run()
        {
            // 设值后消费应取回该值
            LevelTestBoot.SetTarget(3);
            int consumed;
            if (!LevelTestBoot.TryConsumeTarget(out consumed) || consumed != 3)
            {
                throw new System.Exception("Self-test failed: consume did not return the value that was set.");
            }

            // 一次性消费：紧接着再消费应为空
            int again;
            if (LevelTestBoot.TryConsumeTarget(out again))
            {
                throw new System.Exception("Self-test failed: target was not cleared after first consume.");
            }

            Debug.Log("Level boot self-test passed.");
        }
    }
}
