using System;

namespace MemoryTower
{
    public static class AudioEvents
    {
        public static event Action<SfxType> OnSfxRequested;
        public static event Action<string> OnBgmRequested;

        public static void RequestSfx(SfxType type)
        {
            OnSfxRequested?.Invoke(type);
        }

        public static void RequestBgm(string sceneName)
        {
            OnBgmRequested?.Invoke(sceneName);
        }
    }
}
