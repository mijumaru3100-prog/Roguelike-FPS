using UnityEngine;

namespace Project.UI
{
    [RequireComponent(typeof(LineRenderer))]
    public class HologramCrosshair : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Player Manager to track active weapon and aiming states.")]
        [SerializeField] private PlayerManager playerManager;
        
        [Tooltip("The main camera used for raycasting. If null, Camera.main will be used.")]
        [SerializeField] private Camera mainCamera;
        
        [Tooltip("3D Hologram Crosshair Prefab to instantiate at hit point.")]
        [SerializeField] private GameObject crosshairPrefab;

        [Header("Raycast Settings")]
        [SerializeField] private float maxDistance = 100f;
        [SerializeField] private LayerMask hitLayers = ~0; // Default to all layers
        [SerializeField] private float offsetFromWall = 0.02f;

        [Header("Beam (Projector) Settings")]
        [SerializeField] private Color beamNormalColor = new Color(0f, 0.8f, 1f, 0.3f);
        [SerializeField] private Color beamEnemyColor = new Color(1f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private float beamStartWidth = 0.01f;
        [SerializeField] private float beamEndWidth = 0.05f;
        [SerializeField] private bool useWidthCone = true; // Wider at end to look like a projection cone

        [Header("Hologram Reticle Settings")]
        [SerializeField] private float baseScale = 0.15f;
        [SerializeField] private bool autoScaleWithDistance = true;
        [SerializeField] private float minDistanceScale = 0.5f;
        [SerializeField] private float maxDistanceScale = 3f;
        
        [Header("Micro-Animations")]
        [SerializeField] private float rotateSpeed = 45f;
        [SerializeField] private float pulseSpeed = 4f;
        [SerializeField] private float pulseAmount = 0.08f;

        // Runtime states
        private LineRenderer lineRenderer;
        private GameObject spawnedCrosshair;
        private Transform crosshairTransform;
        
        // Inside the prefab, we can search for inner/outer rotating rings
        private Transform innerRing;
        private Transform outerRing;

        private void Start()
        {
            if (playerManager == null)
            {
                playerManager = FindFirstObjectByType<PlayerManager>();
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            lineRenderer = GetComponent<LineRenderer>();
            
            // Setup LineRenderer basic properties
            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 2;
            
            // Instantiating the Hologram Reticle
            if (crosshairPrefab != null)
            {
                spawnedCrosshair = Instantiate(crosshairPrefab);
                crosshairTransform = spawnedCrosshair.transform;
                
                // Let's attempt to find sub-elements to spin in opposite directions for cool FX
                innerRing = crosshairTransform.Find("InnerRing");
                outerRing = crosshairTransform.Find("OuterRing");
                
                spawnedCrosshair.SetActive(false);
            }
        }

        private void Update()
        {
            UpdateHologram();
        }

        private void UpdateHologram()
        {
            if (mainCamera == null) return;

            // Check if weapon is reloading or if there is no active weapon
            bool isWeaponActive = playerManager != null && playerManager.currentWeapon != null;
            bool isReloading = isWeaponActive && playerManager.currentWeapon.isReloading;
            
            // Get muzzle source position if active weapon exists, otherwise fall back to camera position
            Vector3 muzzlePos = Vector3.zero;
            bool hasMuzzle = false;

            if (isWeaponActive && playerManager.currentWeapon.muzzlePoint != null)
            {
                muzzlePos = playerManager.currentWeapon.muzzlePoint.position;
                hasMuzzle = true;
            }

            // Raycast from camera center forward
            Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            RaycastHit hit;
            Vector3 targetPoint = ray.GetPoint(maxDistance);
            bool hasHit = false;
            bool isEnemy = false;

            if (Physics.Raycast(ray, out hit, maxDistance, hitLayers))
            {
                targetPoint = hit.point;
                hasHit = true;

                // Check if hit object is an enemy
                if (hit.collider.CompareTag("Enemy") || ((1 << hit.collider.gameObject.layer) & playerManager?.enemyLayer) != 0)
                {
                    isEnemy = true;
                }
            }

            // Hide everything during reload or if inactive
            if (isReloading)
            {
                if (spawnedCrosshair != null) spawnedCrosshair.SetActive(false);
                lineRenderer.enabled = false;
                return;
            }

            // Define holographic color theme (Red alert on enemy, Cool Cyan otherwise)
            Color holoColor = isEnemy ? beamEnemyColor : beamNormalColor;

            // 1. Update Beam/Laser Projection
            if (hasMuzzle)
            {
                lineRenderer.enabled = true;
                lineRenderer.SetPosition(0, muzzlePos);
                lineRenderer.SetPosition(1, targetPoint);

                // Set widths (cone projection logic)
                lineRenderer.startWidth = beamStartWidth;
                lineRenderer.endWidth = useWidthCone ? beamEndWidth : beamStartWidth;

                // Apply neon color/hologram color
                lineRenderer.startColor = holoColor;
                lineRenderer.endColor = holoColor;
            }
            else
            {
                lineRenderer.enabled = false;
            }

            // 2. Update Holographic Reticle at hit point
            if (spawnedCrosshair != null)
            {
                if (hasHit)
                {
                    spawnedCrosshair.SetActive(true);
                    
                    // Position slightly offset along the normal
                    crosshairTransform.position = targetPoint + (hit.normal * offsetFromWall);
                    
                    // Align with hit surface normal
                    crosshairTransform.rotation = Quaternion.LookRotation(hit.normal);

                    // Dynamic Scale to maintain comfortable screen size
                    float distance = Vector3.Distance(mainCamera.transform.position, targetPoint);
                    float currentScale = baseScale;
                    
                    if (autoScaleWithDistance)
                    {
                        currentScale = baseScale * Mathf.Clamp(distance * 0.1f, minDistanceScale, maxDistanceScale);
                    }

                    // Apply Micro-Animation Pulsing
                    float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
                    crosshairTransform.localScale = Vector3.one * (currentScale * pulse);

                    // Apply Micro-Animation Spinning
                    if (innerRing != null)
                    {
                        innerRing.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
                    }
                    if (outerRing != null)
                    {
                        outerRing.Rotate(Vector3.forward, -rotateSpeed * 0.7f * Time.deltaTime);
                    }
                    else if (innerRing == null)
                    {
                        // Fallback: rotate the whole crosshair if child rings don't exist
                        crosshairTransform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime, Space.Self);
                    }

                    // Apply Color properties to renderer materials if present
                    Renderer[] renderers = spawnedCrosshair.GetComponentsInChildren<Renderer>();
                    foreach (var r in renderers)
                    {
                        if (r.material.HasProperty("_Color"))
                        {
                            r.material.SetColor("_Color", holoColor);
                        }
                        if (r.material.HasProperty("_EmissionColor"))
                        {
                            r.material.SetColor("_EmissionColor", holoColor * 2.0f); // Make it glow!
                        }
                    }
                }
                else
                {
                    spawnedCrosshair.SetActive(false);
                }
            }
        }
    }
}
