// DestructibleWall_FixedForReal.cs – FINAL: Colliders disabled + Cleanup fixed
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DestructibleWall_FixedForReal : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float explosionForce = 1800f;
    public float explosionRadius = 6f;
    public float chunkLifetime = 12f;

    [Header("Optional Polish")]
    public ParticleSystem explosionVFX;
    public AudioClip boomSound;

    [Header("XR Interactable (drag WallMesh interactable here)")]
    public XRSimpleInteractable wallInteractable;

    [Header("Puzzle (Optional – drag PuzzleManager here if part of puzzle)")]
    public RunePuzzleManager puzzleManager;
    public int runeIndex = -1;

    private AudioSource audioSource;
    private bool isDestroyed = false;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        if (wallInteractable == null)
            wallInteractable = GetComponentInChildren<XRSimpleInteractable>();
    }

    void OnEnable() => wallInteractable?.activated.AddListener(_ => Explode());
    void OnDisable() => wallInteractable?.activated.RemoveListener(_ => Explode());

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.GetComponent<SpellProjectile>() && !isDestroyed)
            Explode();
    }

    public void Explode()
    {
        // Puzzle notification first
        if (puzzleManager && runeIndex >= 0)
            puzzleManager.OnRuneDestroyed(runeIndex);

        if (isDestroyed) return;
        isDestroyed = true;

        // 1. Hide intact wall mesh + DISABLE ITS COLLIDER
        foreach (var mr in GetComponentsInChildren<MeshRenderer>())
        {
            if (mr.gameObject.name.ToLower().Contains("wall") ||
                mr.gameObject.name.ToLower().Contains("intact"))
            {
                mr.enabled = false;
            }
        }

        foreach (var col in GetComponentsInChildren<Collider>())
        {
            if (col.gameObject.name.ToLower().Contains("wall") ||
                col.gameObject.name.ToLower().Contains("intact"))
            {
                col.enabled = false;  // ← THIS REMOVES THE INVISIBLE WALL!
            }
        }

        // 2. Activate + show + blast chunks
        foreach (Transform child in transform)
        {
            if (child.name.ToLower().Contains("wall") || child.name.ToLower().Contains("intact"))
                continue;

            child.gameObject.SetActive(true);

            MeshRenderer chunkMR = child.GetComponent<MeshRenderer>();
            if (chunkMR) chunkMR.enabled = true;

            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.isKinematic = false;
                rb.WakeUp();

                Vector3 dir = (child.position - transform.position).normalized + Vector3.up * 0.3f;
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 1.5f, ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * 300f, ForceMode.Impulse);
            }
        }

        // FX
        explosionVFX?.Play();
        if (boomSound) audioSource.PlayOneShot(boomSound);

        // Proper cleanup
        StartCoroutine(Cleanup());
    }

    IEnumerator Cleanup()
    {
        yield return new WaitForSeconds(chunkLifetime);
        Destroy(gameObject);
    }
}