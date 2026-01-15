using UnityEngine;
using System.Collections.Generic;

public class WandAudioIntegration : MonoBehaviour
{
    [System.Serializable]
    public class VoiceLineEntry
    {
        public string eventName;
        public AudioClip voiceClip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [System.Serializable]
    public class SpellSoundEntry
    {
        public string spellName;
        public AudioClip castSound;
        public AudioClip impactSound;
        [Range(0f, 1f)] public float castVolume = 1f;
        [Range(0f, 1f)] public float impactVolume = 1f;
    }

    // === ALL YOUR VOICE LINES (15 events) ===
    [Header("VOICE LINES - Assign your recordings here")]
    public VoiceLineEntry wandPickupVoice;
    public VoiceLineEntry wandDropVoice;
    public VoiceLineEntry wandRecallVoice; // <- ADD THIS LINE

    public VoiceLineEntry lowManaVoice;
    public VoiceLineEntry lowHealthVoice;
    public VoiceLineEntry tutorialKillVoice;
    public VoiceLineEntry newWaveVoice;
    public VoiceLineEntry tutorialSpellChangeVoice;
    public VoiceLineEntry tutorialAimVoice;
    public VoiceLineEntry tutorialPickupVoice;
    public VoiceLineEntry pauseEnterVoice;
    public VoiceLineEntry pauseExitVoice;
    public VoiceLineEntry watchOutVoice;
    public VoiceLineEntry youMissedVoice;
    public VoiceLineEntry wrongDirectionVoice;
    public VoiceLineEntry nauseousVoice;
    public VoiceLineEntry youLoseVoice;
    public VoiceLineEntry levelAdvancedVoice;



    // === SPELL SOUNDS ===
    [Header("SPELL SOUNDS")]
    public List<SpellSoundEntry> spellSounds = new List<SpellSoundEntry>();

    // === UI SOUNDS ===
    [Header("UI SOUNDS")]
    public AudioClip menuOpenSound;
    public AudioClip menuCloseSound;
    public AudioClip spellChangeSound;
    [Range(0f, 1f)] public float uiVolume = 0.7f;

    // === SETTINGS ===
    [Header("Settings")]
    [Range(0f, 1f)] public float globalVoiceVolume = 0.8f;
    public bool enableVoiceLines = true;

    // Dictionary for quick spell sound lookup
    private Dictionary<string, SpellSoundEntry> spellSoundDict = new Dictionary<string, SpellSoundEntry>();

    void Awake()
    {
        // Initialize spell dictionary
        foreach (var entry in spellSounds)
        {
            if (!string.IsNullOrEmpty(entry.spellName))
            {
                spellSoundDict[entry.spellName] = entry;
            }
        }
    }

    // === VOICE LINE PLAYERS ===
    public void PlayWandPickup() => PlayVoice(wandPickupVoice);
    public void PlayWandDrop() => PlayVoice(wandDropVoice);
    public void PlayLowMana() => PlayVoice(lowManaVoice);
    public void PlayLowHealth() => PlayVoice(lowHealthVoice);
    public void PlayTutorialKill() => PlayVoice(tutorialKillVoice);
    public void PlayNewWave() => PlayVoice(newWaveVoice);
    public void PlayTutorialSpellChange() => PlayVoice(tutorialSpellChangeVoice);
    public void PlayTutorialAim() => PlayVoice(tutorialAimVoice);
    public void PlayTutorialPickup() => PlayVoice(tutorialPickupVoice);
    public void PlayPauseEnter() => PlayVoice(pauseEnterVoice);
    public void PlayPauseExit() => PlayVoice(pauseExitVoice);
    public void PlayWatchOut() => PlayVoice(watchOutVoice);
    public void PlayYouMissed() => PlayVoice(youMissedVoice);
    public void PlayWrongDirection() => PlayVoice(wrongDirectionVoice);
    public void PlayNauseous() => PlayVoice(nauseousVoice);
    public void PlayYouLose() => PlayVoice(youLoseVoice);
    public void PlayLevelAdvanced() => PlayVoice(levelAdvancedVoice);
// Add this method to play recall:
public void PlayWandRecall() => PlayVoice(wandRecallVoice);

// Update the list of voice line players to include:

    private void PlayVoice(VoiceLineEntry voiceEntry)
    {
        if (!enableVoiceLines || voiceEntry == null || voiceEntry.voiceClip == null) return;
        
        // Use YOUR existing AudioManager
        AudioManager.Instance.PlaySFX(voiceEntry.voiceClip, voiceEntry.volume * globalVoiceVolume);
        Debug.Log($"🗣️ Playing voice: {voiceEntry.eventName}");
    }

    // === SPELL SOUND PLAYERS ===
    public void PlaySpellCast(string spellName, Vector3 position)
    {
        if (spellSoundDict.TryGetValue(spellName, out SpellSoundEntry entry))
        {
            if (entry.castSound != null)
            {
                AudioManager.Instance.PlaySFXAtPosition(entry.castSound, position, entry.castVolume);
            }
        }
    }

    public void PlaySpellImpact(string spellName, Vector3 position)
    {
        if (spellSoundDict.TryGetValue(spellName, out SpellSoundEntry entry))
        {
            if (entry.impactSound != null)
            {
                AudioManager.Instance.PlaySFXAtPosition(entry.impactSound, position, entry.impactVolume);
            }
        }
    }

    // === UI SOUND PLAYERS ===
    public void PlayMenuOpen()
    {
        if (menuOpenSound != null)
            AudioManager.Instance.PlaySFX(menuOpenSound, uiVolume);
    }

    public void PlayMenuClose()
    {
        if (menuCloseSound != null)
            AudioManager.Instance.PlaySFX(menuCloseSound, uiVolume);
    }

    public void PlaySpellChange()
    {
        if (spellChangeSound != null)
            AudioManager.Instance.PlaySFX(spellChangeSound, uiVolume * 0.5f);
    }
}