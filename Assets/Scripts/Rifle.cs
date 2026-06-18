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

        RaycastHit hit;
        Vector3 rayOrigin = fpsCamera != null ? fpsCamera.transform.position : transform.position;
        Vector3 rayDirection = fpsCamera != null ? fpsCamera.transform.forward : transform.forward;

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, range))
        {
            Debug.Log("Hit: " + hit.collider.name);

            // Deal damage if we hit a damageable object (like another player controller or enemy)
            PlayerController target = hit.collider.GetComponent<PlayerController>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            // Spawn hit impact visual
            if (impactEffect != null)
            {
                GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 2f);
            }
        }
    }
}
