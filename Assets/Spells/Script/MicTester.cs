/*
using UnityEngine;
using UnityEngine.Windows.Speech;

public class MicTester : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== MICROPHONE TEST START ===");

        // 1. Check if Unity has microphone permission
        string[] devices = Microphone.devices;

        if (devices.Length == 0)
        {
            Debug.LogError("❌ NO MICROPHONES FOUND!");
            Debug.Log("Please check:");
            Debug.Log("1. Microphone is connected");
            Debug.Log("2. Windows/Mac mic permissions");
            Debug.Log("3. Try Unity's Window > Audio > Test Microphone");
        }
        else
        {
            Debug.Log($"✅ Found {devices.Length} microphone(s):");
            foreach (string device in devices)
            {
                Debug.Log($"   - {device}");
            }

            // 2. Test basic microphone recording
            TestMicrophoneRecording(devices[0]);
        }

        Debug.Log("=== MICROPHONE TEST END ===");
    }

    void TestMicrophoneRecording(string device)
    {
        Debug.Log($"Testing microphone: {device}");

        // Try to record for 1 second
        AudioClip clip = Microphone.Start(device, false, 1, 44100);

        if (clip == null)
        {
            Debug.LogError("❌ Failed to start microphone recording!");
        }
        else
        {
            Debug.Log("✅ Microphone recording started successfully");

            // Wait then check
            Invoke(nameof(CheckRecording), 1.1f);
        }
    }

    void CheckRecording()
    {
        Debug.Log("✅ Microphone is working!");
        Microphone.End(null);

        // Now test speech recognition
        Invoke(nameof(TestSpeechRecognition), 0.5f);
    }

    void TestSpeechRecognition()
    {
        Debug.Log("=== SPEECH RECOGNITION TEST ===");

        // Simple keyword recognizer test
        string[] keywords = { "test", "hello", "fireball" };
        KeywordRecognizer recognizer = new KeywordRecognizer(keywords);

        recognizer.OnPhraseRecognized += (args) =>
        {
            Debug.Log($"✅ SPEECH WORKED! Heard: '{args.text}' with {args.confidence} confidence");
        };

        recognizer.Start();
        Debug.Log("🎤 Listening for: 'test', 'hello', or 'fireball'");
        Debug.Log("Speak one of these words clearly...");

        // Auto-stop after 10 seconds
        Invoke(nameof(StopTest), 10f);
    }

    void StopTest()
    {
        Debug.Log("Test complete. Check console above for results.");
    }
}
*/