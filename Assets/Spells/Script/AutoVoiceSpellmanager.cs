/*
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Collections;

public class AutoVoiceSpellManager : MonoBehaviour
{
    public static AutoVoiceSpellManager Instance;

    private DictationRecognizer dictationRecognizer;
    private bool isListening = false;

    [Header("Debug")]
    public bool logEverything = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (logEverything) Debug.Log("🔊 Voice Manager Created");
    }

    void Start()
    {
        StartVoiceRecognition();
    }

    void StartVoiceRecognition()
    {
        if (isListening)
        {
            if (logEverything) Debug.Log("Already listening, stopping first...");
            StopVoiceRecognition();
        }

        if (logEverything) Debug.Log("🔄 Creating DictationRecognizer...");

        try
        {
            dictationRecognizer = new DictationRecognizer();

            dictationRecognizer.DictationResult += OnDictationResult;
            dictationRecognizer.DictationHypothesis += OnDictationHypothesis;
            dictationRecognizer.DictationComplete += OnDictationComplete;
            dictationRecognizer.DictationError += OnDictationError;

            if (logEverything) Debug.Log("✅ DictationRecognizer created");

            dictationRecognizer.Start();
            isListening = true;

            Debug.Log("<color=green>🎤🎤🎤 VOICE RECOGNITION ACTIVE 🎤🎤🎤</color>");
            Debug.Log("<color=yellow>💬 SPEAK NOW: 'test', 'hello', 'fireball'</color>");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Failed to start voice: {e.Message}");
        }
    }

    void OnDictationResult(string text, ConfidenceLevel confidence)
    {
        Debug.Log($"<color=green>✅ HEARD: '{text}' (Confidence: {confidence})</color>");

        // Process any word - just to test
        ProcessAnyWord(text.ToLower().Trim());
    }

    void OnDictationHypothesis(string text)
    {
        if (logEverything) Debug.Log($"🎤 Thinking: '{text}'");
    }

    void OnDictationComplete(DictationCompletionCause cause)
    {
        Debug.Log($"🎤 Dictation stopped: {cause}");

        isListening = false;

        // Auto-restart unless manually stopped
        if (cause != DictationCompletionCause.Complete)
        {
            Debug.Log("🔄 Auto-restarting in 2 seconds...");
            Invoke(nameof(StartVoiceRecognition), 2f);
        }
    }

    void OnDictationError(string error, int hresult)
    {
        Debug.LogError($"🎤 Voice error: {error} (HR: {hresult})");

        isListening = false;
        Invoke(nameof(StartVoiceRecognition), 3f);
    }

    void ProcessAnyWord(string word)
    {
        // Just log ANY word for testing
        Debug.Log($"📝 Processing word: '{word}'");

        if (word.Contains("test"))
        {
            Debug.Log("🎯 TEST COMMAND RECOGNIZED!");
        }
        else if (word.Contains("hello"))
        {
            Debug.Log("👋 HELLO THERE!");
        }
        else
        {
            Debug.Log($"🗣️ You said: {word}");
        }
    }

    void RestartVoiceRecognition()
    {
        StopVoiceRecognition();
        Invoke(nameof(StartVoiceRecognition), 0.5f);
    }

    void StopVoiceRecognition()
    {
        if (dictationRecognizer != null)
        {
            if (dictationRecognizer.Status == SpeechSystemStatus.Running)
            {
                dictationRecognizer.Stop();
            }

            dictationRecognizer.DictationResult -= OnDictationResult;
            dictationRecognizer.DictationHypothesis -= OnDictationHypothesis;
            dictationRecognizer.DictationComplete -= OnDictationComplete;
            dictationRecognizer.DictationError -= OnDictationError;

            dictationRecognizer.Dispose();
            dictationRecognizer = null;
        }

        isListening = false;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            // App lost focus - stop listening
            StopVoiceRecognition();
        }
        else
        {
            // App regained focus - restart
            Invoke(nameof(StartVoiceRecognition), 1f);
        }
    }

    void OnDestroy()
    {
        StopVoiceRecognition();
        Debug.Log("🎤 Voice Manager destroyed");
    }
}
*/