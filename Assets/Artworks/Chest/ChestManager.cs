// ChestManager_Fixed.cs – Attach/replace on TreasureChest parent
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ChestManager_Fixed : MonoBehaviour
{
    [Header("Lid Hinge (Drag Lid here)")]
    public Transform lid;  // Must have HingeJoint!

    [Header("Key Spawn")]
    public GameObject keyPrefab;
    public Transform keySpawnPoint;

    [Header("FX")]
    public ParticleSystem openVFX;
    public AudioClip openSound;

    [Header("XR")]
    public XRSimpleInteractable handle;

    private HingeJoint hinge;
    private JointMotor motor;
    private AudioSource audioSource;
    private bool isOpen = false;

    void Awake()
    {
        if (lid == null) lid = transform.Find("Lid");  // Auto-find
        hinge = lid.GetComponent<HingeJoint>();
        if (hinge == null) Debug.LogError("Lid needs HingeJoint!");

        audioSource = gameObject.AddComponent<AudioSource>();
        motor = hinge.motor;
        motor.force = 3000f;
        hinge.motor = motor;
        hinge.useMotor = false;
    }

    void OnEnable() => handle?.activated.AddListener(OpenChest);
    void OnDisable() => handle?.activated.RemoveListener(OpenChest);

    void OpenChest(ActivateEventArgs args)
    {
        if (isOpen) return;
        isOpen = true;

        // Physics open lid (swings perfectly!)
        motor.targetVelocity = 150f;  // Positive = opens up/out
        hinge.motor = motor;
        hinge.useMotor = true;

        // Spawn key with slight float-up
        SpawnKey();

        // FX + haptic
        openVFX?.Play();
        if (openSound) audioSource.PlayOneShot(openSound);

        // Stop motor at 90° (like door)
        StartCoroutine(StopLidMotor());
    }

    void SpawnKey()
    {
        if (keyPrefab == null)
        {
            Debug.LogError("Assign Key Prefab to ChestManager!");
            return;
        }
        if (keySpawnPoint == null)
        {
            Debug.LogError("Assign KeySpawnPoint!");
            return;
        }

        // Spawn + float up slightly for drama
        GameObject key = Instantiate(keyPrefab, keySpawnPoint.position, keySpawnPoint.rotation);
        key.transform.localScale *= 0.8f;  // Slightly smaller for fit
        Rigidbody rb = key.GetComponent<Rigidbody>();
        if (rb) rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);  // Bob up

        Debug.Log("Key spawned at: " + keySpawnPoint.position);  // Confirm in Console
    }

    IEnumerator StopLidMotor()
    {
        yield return new WaitForSeconds(1f);  // Let it swing
        while (hinge.angle < 85f)
            yield return new WaitForSeconds(0.1f);
        hinge.useMotor = false;
    }
}