/*
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Collections.Generic;
using System.Linq;

public class VoiceSpellCaster : MonoBehaviour
{
    public static VoiceSpellCaster Instance;

    [Header("References")]
    public SpellManager spellManager;
    public WandControllerV3 wandController;
    public SpellHUD spellHUD;

    [Header("Voice Recognition Settings")]
    public ConfidenceLevel minimumConfidence = ConfidenceLevel.Medium;
    public bool autoRestartOnError = true;

    [Header("Visual Feedback")]
    public GameObject voiceActivationEffect; // Optional particle effect
    public AudioClip voiceRecognizedSound; // Optional audio feedback

    [Header("Debug")]
    public bool logEverything = true;
    public bool showRecognitionInConsole = true;

    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, Spell> voiceCommandMap = new Dictionary<string, Spell>();
    private bool isListening = false;
    private AudioSource audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        if (spellManager == null)
        {
            Debug.LogError("❌ SpellManager not assigned to VoiceSpellCaster!");
            return;
        }

        BuildVoiceCommandMap();
        StartVoiceRecognition();
    }

    void BuildVoiceCommandMap()
    {
        voiceCommandMap.Clear();

        if (spellManager == null || spellManager.learnedSpells.Count == 0)
        {
            Debug.LogWarning("⚠️ No spells available to map to voice commands");
            return;
        }

        Debug.Log("🗣️ Building voice command map...");

        foreach (Spell spell in spellManager.learnedSpells)
        {
            if (spell == null) continue;

            // Use spell name as voice command (lowercase, no spaces)
            string command = spell.spellName.ToLower().Replace(" ", "");
            voiceCommandMap[command] = spell;

            Debug.Log($"   ✅ '{command}' → {spell.spellName}");

            // Also add alternate pronunciations for common spells
            if (spell.spellName.Contains("Fire"))
            {
                voiceCommandMap["fireball"] = spell;
                voiceCommandMap["fire"] = spell;
            }
            else if (spell.spellName.Contains("Ice"))
            {
                voiceCommandMap["ice"] = spell;
                voiceCommandMap["freeze"] = spell;
            }
            else if (spell.spellName.Contains("Lightning") || spell.spellName.Contains("Thunder"))
            {
                voiceCommandMap["lightning"] = spell;
                voiceCommandMap["thunder"] = spell;
                voiceCommandMap["shock"] = spell;
            }
            else if (spell.spellName.Contains("Shield") || spell.spellName.Contains("Protect"))
            {
                voiceCommandMap["shield"] = spell;
                voiceCommandMap["protect"] = spell;
                voiceCommandMap["defense"] = spell;
            }
            else if (spell.spellName.Contains("Hit"))
            {
                voiceCommandMap["hit"] = spell;
                voiceCommandMap["attack"] = spell;
            }
            else if (spell.spellName.Contains("Earth"))
            {
                voiceCommandMap["earth"] = spell;
                voiceCommandMap["nature"] = spell;
            }
            else if (spell.spellName.Contains("Wind"))
            {
                voiceCommandMap["wind"] = spell;
                voiceCommandMap["breeze"] = spell;
            }
        }

        Debug.Log($"✅ Voice commands mapped: {voiceCommandMap.Count} total");
    }

    void StartVoiceRecognition()
    {
        if (isListening)
        {
            Debug.Log("Already listening, restarting...");
            StopVoiceRecognition();
        }

        if (voiceCommandMap.Count == 0)
        {
            Debug.LogError("❌ No voice commands to recognize! Check SpellManager.");
            return;
        }

        try
        {
            // Create keyword recognizer with all spell names
            string[] keywords = voiceCommandMap.Keys.ToArray();
            keywordRecognizer = new KeywordRecognizer(keywords, minimumConfidence);

            keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;

            keywordRecognizer.Start();
            isListening = true;

            Debug.Log($"<color=green>🎤 VOICE SPELL CASTING ACTIVE 🎤</color>");
            Debug.Log($"<color=yellow>💬 Listening for: {string.Join(", ", keywords)}</color>");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Failed to start voice recognition: {e.Message}");
            Debug.LogError("Make sure you have microphone permissions enabled!");
        }
    }

    void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        string command = args.text.ToLower();

        if (showRecognitionInConsole)
        {
            Debug.Log($"<color=cyan>🎤 HEARD: '{args.text}' (Confidence: {args.confidence})</color>");
        }

        // Check if we have this spell
        if (voiceCommandMap.ContainsKey(command))
        {
            Spell spell = voiceCommandMap[command];
            CastSpellByVoice(spell);
        }
        else
        {
            Debug.LogWarning($"⚠️ Recognized '{command}' but no spell mapped to it");
        }
    }

    void CastSpellByVoice(Spell spell)
    {
        if (spell == null)
        {
            Debug.LogError("❌ Tried to cast null spell");
            return;
        }

        Debug.Log($"<color=green>✨ CASTING BY VOICE: {spell.spellName} ✨</color>");

        // Visual feedback
        ShowVoiceActivationFeedback();

        // Audio feedback
        if (voiceRecognizedSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(voiceRecognizedSound);
        }

        // Update UI to show the spell being cast
        if (spellHUD != null)
        {
            spellHUD.ShowVoiceCastFeedback(spell.spellName);
        }

        // Cast the spell through the wand
        if (wandController != null && wandController.isHeldByRightHand)
        {
            wandController.VoiceCastSpell(spell);
        }
        else
        {
            Debug.LogWarning("⚠️ Cannot cast - wand not held or WandController missing");

            // Alternative: cast from player position if wand not held
            if (wandController != null)
            {
                Vector3 castPos = wandController.GetWandTipPosition();
                Vector3 castDir = wandController.GetWandTipDirection();
                spell.Cast(castPos, castDir);
            }
        }
    }

    void ShowVoiceActivationFeedback()
    {
        if (voiceActivationEffect != null && wandController != null)
        {
            Vector3 effectPos = wandController.GetWandTipPosition();
            GameObject effect = Instantiate(voiceActivationEffect, effectPos, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }

    public void StopVoiceRecognition()
    {
        if (keywordRecognizer != null)
        {
            if (keywordRecognizer.IsRunning)
            {
                keywordRecognizer.Stop();
            }

            keywordRecognizer.OnPhraseRecognized -= OnPhraseRecognized;
            keywordRecognizer.Dispose();
            keywordRecognizer = null;
        }

        isListening = false;
        Debug.Log("🎤 Voice recognition stopped");
    }

    public void RestartVoiceRecognition()
    {
        StopVoiceRecognition();
        BuildVoiceCommandMap(); // Rebuild in case spells changed
        Invoke(nameof(StartVoiceRecognition), 0.5f);
    }

    // Public method to add custom voice commands
    public void AddVoiceCommand(string command, Spell spell)
    {
        string cleanCommand = command.ToLower().Replace(" ", "");
        voiceCommandMap[cleanCommand] = spell;

        Debug.Log($"✅ Added voice command: '{cleanCommand}' → {spell.spellName}");

        // Restart to apply changes
        RestartVoiceRecognition();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            StopVoiceRecognition();
        }
        else if (autoRestartOnError)
        {
            Invoke(nameof(StartVoiceRecognition), 1f);
        }
    }

    void OnDestroy()
    {
        StopVoiceRecognition();
    }

    // Debug helper - call this to see all available commands
    [ContextMenu("List All Voice Commands")]
    public void ListAllCommands()
    {
        Debug.Log("=== AVAILABLE VOICE COMMANDS ===");
        foreach (var kvp in voiceCommandMap)
        {
            Debug.Log($"   '{kvp.Key}' → {kvp.Value.spellName}");
        }
        Debug.Log("================================");
    }
}*/