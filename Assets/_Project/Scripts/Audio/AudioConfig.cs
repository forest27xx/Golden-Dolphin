using System;
using System.Collections.Generic;
using UnityEngine;

namespace MemoryTower
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "MemoryTower/Audio Config")]
    public sealed class AudioConfig : ScriptableObject
    {
        [SerializeField] private AudioClip bgmMenu;
        [SerializeField] private AudioClip bgmGame;
        [SerializeField] private List<SfxEntry> sfxEntries = new List<SfxEntry>();

        public AudioClip GetBgm(string sceneName)
        {
            if (string.Equals(sceneName, "MainMenu", StringComparison.OrdinalIgnoreCase))
            {
                return bgmMenu;
            }

            if (string.Equals(sceneName, "Game", StringComparison.OrdinalIgnoreCase))
            {
                return bgmGame;
            }

            return null;
        }

        public AudioClip GetSfx(SfxType type)
        {
            SfxEntry entry = FindEntry(type);
            return entry == null ? null : entry.clip;
        }

        public float GetSfxVolume(SfxType type)
        {
            SfxEntry entry = FindEntry(type);
            return entry == null ? 1f : entry.volume;
        }

        private SfxEntry FindEntry(SfxType type)
        {
            if (sfxEntries == null)
            {
                return null;
            }

            foreach (SfxEntry entry in sfxEntries)
            {
                if (entry != null && entry.type == type)
                {
                    return entry;
                }
            }

            return null;
        }

        [Serializable]
        public sealed class SfxEntry
        {
            public SfxType type;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1f;
        }
    }
}
