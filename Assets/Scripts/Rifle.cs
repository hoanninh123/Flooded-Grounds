using System.Collections;
using UnityEngine;

public class Rifle : MonoBehaviour
{
    [Header("Rifle Settings")]
    public float damage = 20f;
    public float range = 100f;
    public float fireRate = 10f; // Shots per second
    public int maxAmmo = 30;
    public float reloadTime = 2f;

    [Header("References")]
    public Camera fpsCamera;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;
    public AudioSource audioSource;
    public AudioClip shootSound;
    public AudioClip reloadSound;

    private int currentAmmo;
    private float nextTimeToFire = 0f;
    private bool isReloading = false;

    // Expose weapon states to PlayerController
    public bool IsReloading => isReloading;
    public int CurrentAmmo => currentAmmo;
    public int MaxAmmo => maxAmmo;

    void Start()
    {
        currentAmmo = maxAmmo;
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        if (fpsCamera == null)
        {
            fpsCamera = Camera.main;
        }
    }

    void OnEnable()
    {
        isReloading = false;
    }

    void Update()
    {
        if (isReloading) return;

        // Auto reload if empty, or manual reload when pressing R
        if (currentAmmo <= 0 || (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo))
        {
            StartCoroutine(Reload());
            return;
        }

        // Fire weapon on Left Click
        if (Input.GetMouseButton(0) && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");

        if (audioSource != null && reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
        Debug.Log("Reloaded!");
    }

    Camera GetActiveCamera()
    {
        if (fpsCamera != null && fpsCamera.gameObject.activeInHierarchy)
        {
            return fpsCamera;
        }

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            return mainCam;
        }

        return FindObjectOfType<Camera>();
    }

    void Shoot()
    {
        currentAmmo--;

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        Camera activeCam = GetActiveCamera();
        Vector3 rayOrigin = activeCam != null ? activeCam.transform.position : transform.position;
        Vector3 rayDirection = activeCam != null ? activeCam.transform.forward : transform.forward;

        RaycastHit[] hits = Physics.RaycastAll(rayOrigin, rayDirection, range);
        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

        PlayerController owner = GetComponentInParent<PlayerController>();
        RaycastHit hit = new RaycastHit();
        bool hasHit = false;

        foreach (RaycastHit hitInfo in hits)
        {
            // Ignore any colliders belonging to the player who shot the weapon
            if (owner != null && hitInfo.collider.transform.IsChildOf(owner.transform))
            {
                continue;
            }

            hit = hitInfo;
            hasHit = true;
            break;
        }

        if (hasHit)
        {
            Debug.Log("Hit: " + hit.collider.name + " at " + hit.point);

            // Deal damage if we hit a damageable object (like another player controller or enemy)
            PlayerController target = hit.collider.GetComponentInParent<PlayerController>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            ObjectToHit objectToHit = hit.collider.GetComponentInParent<ObjectToHit>();
            if (objectToHit != null)
            {
                objectToHit.TakeDamage(damage);
            }

            // Spawn hit impact visual
            if (impactEffect != null)
            {
                GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 2f);
            }
        }
        else
        {
            Debug.Log("Raycast shot from " + rayOrigin + " in direction " + rayDirection + " did not hit any target.");
        }
    }
}
