// RunePuzzleManager.cs – Attach to "RunePuzzle" parent
using UnityEngine;
using System.Collections.Generic;

public class RunePuzzleManager : MonoBehaviour
{
    [Header("Rune Stones (Drag in order: 1,2,3)")]
    public DestructibleWall_FixedForReal[] runeStones;  // Your destructible script

    [Header("Gate & FX")]
    public XRDoor_Fixed gate;  // Drag your gate's script/component
    public ParticleSystem successVFX;
    public AudioClip successSound;
    public AudioClip wrongSound;

    [Header("Sequence & Feedback")]
    public int[] correctSequence = { 0, 1, 2 };  // Index order: destroy 0 first, then 1, then 2
    private List<int> playerSequence = new();
    private int currentStep = 0;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        if (runeStones.Length != 3) Debug.LogError("Need exactly 3 runes!");
    }

    public void OnRuneDestroyed(int runeIndex)  // Called by destructible scripts
    {
        playerSequence.Add(runeIndex);

        if (runeIndex == correctSequence[currentStep])
        {
            currentStep++;
            if (currentStep >= correctSequence.Length)
            {
                CompletePuzzle();
            }
            else
            {

            }
        }
        else
        {
            ResetPuzzle();
        }
    }

    void CompletePuzzle()
    {
        // OPEN GATE!
        if (gate) gate.OpenDoor();  // Uses your door's public method

        successVFX?.Play();
        if (successSound) audioSource.PlayOneShot(successSound);

        // Optional: Disable future destroys
        enabled = false;
    }

    void ResetPuzzle()
    {
        playerSequence.Clear();
        currentStep = 0;

        if (wrongSound) audioSource.PlayOneShot(wrongSound);

        // Visual reset: Respawn runes? Or just glow pulse (add Animator later)
        Debug.Log("Wrong order! Reset.");
    }
}