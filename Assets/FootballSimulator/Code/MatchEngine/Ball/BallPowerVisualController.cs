using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FStudio.Events;
using FStudio.MatchEngine;
using FStudio.MatchEngine.Events;
using FStudio.MatchEngine.Balls;

namespace FStudio.MatchEngine.Balls {
    /// <summary>
    /// Controls the visual appearance of the ball during powerful shots.
    /// Swaps to a power ball prefab when shot velocity exceeds threshold.
    /// 
    /// Combo Super Kick: Khi IsSuperKick && SuperKickComboCount > 1,
    /// luôn kích hoạt power ball và spawn clone balls tại vị trí bóng gốc
    /// khi bóng đã bay (sau 1 frame để velocity được apply).
    /// Clone balls có random power/height riêng (bước nhảy 0.1f), tắt collider với nhau.
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

        [Header("Clone Ball Settings")]
        [SerializeField]
        [Tooltip("Thời gian tự hủy clone ball (giây)")]
        private float cloneBallLifetime = 5f;
        
        [Header("Clone Ball Random Range (bước nhảy 0.1f)")]
        [SerializeField] private float clonePowerMin = 0.5f;
        [SerializeField] private float clonePowerMax = 2.0f;
        [SerializeField] private float cloneHeightMin = 0.3f;
        [SerializeField] private float cloneHeightMax = 1.5f;

        [Header("Debug")]
        [SerializeField]
        private bool enableDebugLogs = true;

        private GameObject currentPowerBallInstance;
        private bool isPowerBallActive = false;
        private float powerBallActivatedTime;
        private Ball ball;
        
        // Danh sách clone balls đang tồn tại (để tắt collider giữa chúng)
        private List<GameObject> activeCloneBalls = new List<GameObject>();

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
            
            // Clean up clone balls
            CleanupCloneBalls();
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
            if (ball == null) {
                return;
            }

            bool isSuperKickCombo = MatchManager.Current != null 
                && MatchManager.Current.IsSuperKick 
                && MatchManager.Current.SuperKickComboCount > 1;
            
            // Khi là super kick combo, luôn kích hoạt power ball (bỏ qua threshold)
            if (isSuperKickCombo) {
                if (enableDebugLogs) {
                    Debug.Log($"[BallPowerVisualController] Super Kick COMBO x{MatchManager.Current.SuperKickComboCount} detected! Force activating power ball + clone balls.");
                }
                
                if (powerBallPrefab != null) {
                    ActivatePowerBall();
                }
                
                // Spawn clone balls sau 1 frame (chờ velocity được apply vào bóng gốc)
                int comboCount = MatchManager.Current.SuperKickComboCount;
                StartCoroutine(SpawnCloneBallsDelayed(comboCount));
                
                return;
            }

            // Logic bình thường: chỉ kích hoạt power ball khi power >= threshold
            if (powerBallPrefab != null && shootEvent.Power >= powerThreshold) {
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

        // ===== CLONE BALL SYSTEM =====

        /// <summary>
        /// Random một giá trị trong khoảng [min, max] với bước nhảy 0.1f.
        /// </summary>
        private float RandomWithStep(float min, float max, float step = 0.1f) {
            int steps = Mathf.RoundToInt((max - min) / step);
            int randomStep = Random.Range(0, steps + 1);
            return min + randomStep * step;
        }

        /// <summary>
        /// Chờ 1 frame rồi spawn clone balls.
        /// Lý do: Ball.Shoot() gọi EventManager.Trigger trước, rồi mới BallHit (apply velocity).
        /// Sau 1 frame, ball.Velocity và ball.transform.position đã chính xác.
        /// </summary>
        private IEnumerator SpawnCloneBallsDelayed(int comboCount) {
            // Chờ 0.5 giây để bóng gốc đã bay ra xa một chút
            yield return new WaitForSeconds(0.3f);
            
            if (ball == null) yield break;
            
            // Lấy velocity và position thực tế của bóng gốc (đã bay)
            Vector3 ballPosition = ball.transform.position;
            Vector3 ballVelocity = ball.Velocity;
            
            if (enableDebugLogs) {
                Debug.Log($"[BallPowerVisualController] Ball is flying! Position: {ballPosition}, Velocity: {ballVelocity} (mag: {ballVelocity.magnitude:F1})");
                Debug.Log($"[BallPowerVisualController] Spawning {comboCount - 1} clone ball(s)...");
            }
            
            // Dọn dẹp clone balls cũ trước khi spawn mới
            CleanupCloneBalls();
            
            // Spawn (comboCount - 1) quả clone
            for (int i = 1; i < comboCount; i++) {
                // Random power và height riêng cho mỗi quả clone
                float power = RandomWithStep(clonePowerMin, clonePowerMax);
                float height = RandomWithStep(cloneHeightMin, cloneHeightMax);
                
                // Tính velocity cho clone: dựa trên velocity bóng gốc, nhân random
                Vector3 cloneVelocity = new Vector3(
                    ballVelocity.x * power,
                    ballVelocity.y * height,
                    ballVelocity.z * power
                );
                
                if (enableDebugLogs) {
                    Debug.Log($"[BallPowerVisualController] Clone #{i} - Power: {power:F1}, Height: {height:F1}, Velocity: {cloneVelocity} (mag: {cloneVelocity.magnitude:F1})");
                }
                
                SpawnSingleCloneBall(ballPosition, cloneVelocity);
            }
            
            // Tắt collider giữa tất cả clone balls với nhau
            DisableCollisionBetweenClones();
            
            if (enableDebugLogs) {
                Debug.Log($"[BallPowerVisualController] Total balls: {comboCount} (1 main + {comboCount - 1} clones)");
            }
        }

        /// <summary>
        /// Spawn 1 quả bóng clone tại vị trí cho trước với velocity cho trước.
        /// Copy visual từ power ball prefab (nếu có) hoặc từ bóng gốc.
        /// </summary>
        private void SpawnSingleCloneBall(Vector3 position, Vector3 velocity) {
            if (ball == null) return;

            // Tạo clone ball GameObject
            GameObject cloneBall = new GameObject("SuperKick_CloneBall");
            cloneBall.transform.position = position;

            // Ưu tiên dùng power ball prefab cho visual (trông giống bóng power đang bay)
            if (powerBallPrefab != null) {
                var visual = Instantiate(powerBallPrefab, cloneBall.transform);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
            }
            else if (ball.ballAssetPoint != null && ball.ballAssetPoint.childCount > 0) {
                // Fallback: copy visual từ bóng gốc
                var visual = Instantiate(
                    ball.ballAssetPoint.GetChild(0).gameObject,
                    cloneBall.transform
                );
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localScale = ball.ballAssetPoint.GetChild(0).localScale;
            } else {
                // Fallback cuối: tạo sphere đơn giản
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.SetParent(cloneBall.transform);
                sphere.transform.localPosition = Vector3.zero;
                sphere.transform.localScale = Vector3.one * 0.22f;
                var sphereCol = sphere.GetComponent<Collider>();
                if (sphereCol != null) Destroy(sphereCol);
            }

            // Lấy thông tin Rigidbody từ bóng gốc
            var originalRb = ball.GetComponent<Rigidbody>();

            // Thêm Rigidbody cho clone ball
            var rb = cloneBall.AddComponent<Rigidbody>();
            if (originalRb != null) {
                rb.mass = originalRb.mass;
                rb.drag = originalRb.drag;
                rb.angularDrag = originalRb.angularDrag;
                rb.useGravity = originalRb.useGravity;
            } else {
                rb.mass = 0.45f;
                rb.drag = 0.1f;
                rb.useGravity = true;
            }
            rb.velocity = velocity;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // Chống tunneling qua trigger zone
            
            // Thêm chút spin ngẫu nhiên cho đẹp
            rb.angularVelocity = new Vector3(
                Random.Range(-10f, 10f),
                Random.Range(-5f, 5f),
                Random.Range(-10f, 10f)
            );

            // Thêm SphereCollider cho clone ball (để bounce với sân/lưới)
            var col = cloneBall.AddComponent<SphereCollider>();
            col.radius = 0.11f;

            // Đặt clone ball cùng layer với bóng gốc ("Ball") để trigger zones (GoalAction) hoạt động
            cloneBall.layer = ball.gameObject.layer;

            // Tắt va chạm giữa clone ball và bóng gốc (không ảnh hưởng đường bay nhau)
            if (ball.Collider != null) {
                Physics.IgnoreCollision(col, ball.Collider, true);
            }

            // Player vẫn có thể chặn/bắt clone ball bình thường

            // Thêm CloneBallGoalDetector để phát hiện bàn thắng khi clone vào gôn
            cloneBall.AddComponent<CloneBallGoalDetector>();

            // Lưu vào danh sách để quản lý
            activeCloneBalls.Add(cloneBall);

            // Tự hủy sau cloneBallLifetime giây
            Destroy(cloneBall, cloneBallLifetime);
        }

        /// <summary>
        /// Tắt collider va chạm giữa tất cả clone balls với nhau.
        /// </summary>
        private void DisableCollisionBetweenClones() {
            for (int i = 0; i < activeCloneBalls.Count; i++) {
                if (activeCloneBalls[i] == null) continue;
                var colA = activeCloneBalls[i].GetComponent<SphereCollider>();
                if (colA == null) continue;
                
                for (int j = i + 1; j < activeCloneBalls.Count; j++) {
                    if (activeCloneBalls[j] == null) continue;
                    var colB = activeCloneBalls[j].GetComponent<SphereCollider>();
                    if (colB == null) continue;
                    
                    Physics.IgnoreCollision(colA, colB, true);
                }
            }
        }

        /// <summary>
        /// Dọn dẹp danh sách clone balls (xóa null entries)
        /// </summary>
        private void CleanupCloneBalls() {
            // Xóa các entries đã bị Destroy
            activeCloneBalls.RemoveAll(go => go == null);
        }

        // ===== POWER BALL VISUAL =====

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
