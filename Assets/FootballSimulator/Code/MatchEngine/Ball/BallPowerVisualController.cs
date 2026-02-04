using UnityEngine;
using FStudio.Events;
using FStudio.MatchEngine.Events;
using FStudio.MatchEngine.Balls;

namespace FStudio.MatchEngine.Balls {
    /// <summary>
    /// Controls the visual appearance of the ball during powerful shots.
    /// Swaps to a power ball prefab when shot velocity exceeds threshold.
    /// </summary>
    public class BallPowerVisualController : MonoBehaviour {
        [Header("Power Ball Settings")]
        [SerializeField] 
        [Tooltip("The prefab to spawn when a powerful shot is detected")]
        private GameObject powerBallPrefab;
        
        [SerializeField]
        [Tooltip("Minimum shot power (velocity magnitude) to activate power ball visual")]
        private float powerThreshold = 25f;
        
        [SerializeField]
        [Tooltip("Velocity magnitude below which power ball visual is deactivated")]
        private float deactivateVelocity = 5f;

        [SerializeField]
        [Tooltip("Minimum time (seconds) the power ball must stay active before it can be deactivated")]
        private float minActiveTime = 0.3f;

        [Header("References")]
        [SerializeField]
        [Tooltip("The normal ball visual that will be hidden during power shots")]
        private GameObject normalBallVisual;

        [Header("Debug")]
        [SerializeField]
        private bool enableDebugLogs = true;

        private GameObject currentPowerBallInstance;
        private bool isPowerBallActive = false;
        private float powerBallActivatedTime;
        private Ball ball;

        private void OnEnable() {
            EventManager.Subscribe<PlayerShootEvent>(OnPlayerShoot);
            
            if (enableDebugLogs) {
                Debug.Log("[BallPowerVisualController] Enabled and subscribed to PlayerShootEvent");
            }
        }

        private void OnDisable() {
            EventManager.UnSubscribe<PlayerShootEvent>(OnPlayerShoot);
            
            // Clean up power ball if exists
            if (isPowerBallActive) {
                DeactivatePowerBall();
            }
        }

        private void Start() {
            ball = Ball.Current;
            
            if (ball == null) {
                Debug.LogError("[BallPowerVisualController] Ball.Current is null! Controller will not work.");
                enabled = false;
                return;
            }

            if (powerBallPrefab == null) {
                Debug.LogWarning("[BallPowerVisualController] PowerBall prefab is not assigned! Please assign it in the inspector.");
            }
        }

        /// <summary>
        /// Find the normal ball visual at runtime. Called just before activation.
        /// </summary>
        private void FindNormalBallVisual() {
            if (normalBallVisual != null) {
                return; // Already assigned
            }

            if (ball == null || ball.ballAssetPoint == null) {
                return;
            }

            // Look for ball visual in children
            if (ball.ballAssetPoint.childCount > 0) {
                normalBallVisual = ball.ballAssetPoint.GetChild(0).gameObject;
                if (enableDebugLogs) {
                    Debug.Log($"[BallPowerVisualController] Auto-found normal ball visual: {normalBallVisual.name}");
                }
            } else {
                if (enableDebugLogs) {
                    Debug.LogWarning("[BallPowerVisualController] No children found in ballAssetPoint. Ball visual may not be loaded yet.");
                }
            }
        }

        private void Update() {
            if (!isPowerBallActive || ball == null) {
                return;
            }

            // Check if ball is being held by a player (caught/controlled)
            if (ball.HolderPlayer != null) {
                if (enableDebugLogs) {
                    Debug.Log($"[BallPowerVisualController] Deactivating power ball. Ball is held by player.");
                }
                DeactivatePowerBall();
                return;
            }

            // Check minimum active time has passed
            float activeTime = Time.time - powerBallActivatedTime;
            if (activeTime < minActiveTime) {
                return; // Don't deactivate yet
            }

            // Check if ball velocity has decreased below threshold
            float currentVelocity = ball.Velocity.magnitude;
            
            if (currentVelocity < deactivateVelocity) {
                if (enableDebugLogs) {
                    Debug.Log($"[BallPowerVisualController] Deactivating power ball. Velocity: {currentVelocity:F2} < {deactivateVelocity}, active time: {activeTime:F2}s");
                }
                DeactivatePowerBall();
            }
        }

        private void OnPlayerShoot(PlayerShootEvent shootEvent) {
            if (ball == null || powerBallPrefab == null) {
                return;
            }

            // Check if shot power exceeds threshold
            if (shootEvent.Power >= powerThreshold) {
                if (enableDebugLogs) {
                    Debug.Log($"[BallPowerVisualController] Powerful shot detected! Power: {shootEvent.Power:F2} >= {powerThreshold}. Activating power ball.");
                }
                ActivatePowerBall();
            } else {
                if (enableDebugLogs) {
                    Debug.Log($"[BallPowerVisualController] Normal shot. Power: {shootEvent.Power:F2} < {powerThreshold}");
                }
            }
        }

        private void ActivatePowerBall() {
            // Don't activate if already active
            if (isPowerBallActive) {
                if (enableDebugLogs) {
                    Debug.Log("[BallPowerVisualController] Power ball already active. Skipping.");
                }
                return;
            }

            if (ball == null || ball.ballAssetPoint == null) {
                Debug.LogError("[BallPowerVisualController] Cannot activate power ball - Ball or ballAssetPoint is null");
                return;
            }

            // Try to find normal ball visual if not already found
            FindNormalBallVisual();

            // Hide normal ball visual
            if (normalBallVisual != null) {
                normalBallVisual.SetActive(false);
                if (enableDebugLogs) {
                    Debug.Log("[BallPowerVisualController] Normal ball visual hidden");
                }
            } else {
                if (enableDebugLogs) {
                    Debug.LogWarning("[BallPowerVisualController] Normal ball visual is null! Cannot hide it.");
                }
            }

            // Instantiate power ball as child of ballAssetPoint
            if (powerBallPrefab != null) {
                currentPowerBallInstance = Instantiate(
                    powerBallPrefab, 
                    ball.ballAssetPoint
                );
                
                // Reset local transform to match parent
                currentPowerBallInstance.transform.localPosition = Vector3.zero;
                currentPowerBallInstance.transform.localRotation = Quaternion.identity;
                // Keep original scale from prefab (don't modify)

                if (enableDebugLogs) {
                    Debug.Log($"[BallPowerVisualController] Power ball instantiated: {currentPowerBallInstance.name}");
                    Debug.Log($"[BallPowerVisualController] Power ball world position: {currentPowerBallInstance.transform.position}");
                    Debug.Log($"[BallPowerVisualController] Power ball local scale: {currentPowerBallInstance.transform.localScale}, world scale: {currentPowerBallInstance.transform.lossyScale}");
                    Debug.Log($"[BallPowerVisualController] Power ball active: {currentPowerBallInstance.activeSelf}, layer: {currentPowerBallInstance.layer}");
                }
            } else {
                if (enableDebugLogs) {
                    Debug.LogWarning("[BallPowerVisualController] Power ball prefab is null! Cannot instantiate.");
                }
            }

            isPowerBallActive = true;
            powerBallActivatedTime = Time.time;
        }

        private void DeactivatePowerBall() {
            if (!isPowerBallActive) {
                return;
            }

            // Show normal ball visual
            if (normalBallVisual != null) {
                normalBallVisual.SetActive(true);
                if (enableDebugLogs) {
                    Debug.Log("[BallPowerVisualController] Normal ball visual restored");
                }
            }

            // Destroy power ball instance
            if (currentPowerBallInstance != null) {
                Destroy(currentPowerBallInstance);
                currentPowerBallInstance = null;
                
                if (enableDebugLogs) {
                    Debug.Log("[BallPowerVisualController] Power ball destroyed");
                }
            }

            isPowerBallActive = false;
        }

        // Public method to manually trigger power ball (useful for testing)
        public void TestActivatePowerBall() {
            ActivatePowerBall();
        }

        // Public method to manually deactivate power ball (useful for testing)
        public void TestDeactivatePowerBall() {
            DeactivatePowerBall();
        }

        // Property to check if power ball is currently active
        public bool IsPowerBallActive => isPowerBallActive;
    }
}
