using UnityEngine;
using UnityEngine.InputSystem;
using FStudio.MatchEngine.Enums;
using FStudio.Data;
using FStudio.Database;
using System.Collections.Generic;
using System.Linq;

namespace FStudio.MatchEngine {
    /// <summary>
    /// Quản lý tính năng Call5Enemy: Sinh 5 cầu thủ AI tấn công khi bấm phím I
    /// </summary>
    public class Call5EnemyManager : MonoBehaviour {
        private int countCall = 0;
        private float timeDelayNextCall = 0f;
        private bool isFreezing = false;
        private float freezeStartTime = 0f;
        private MatchStatus previousMatchStatus;
        
        private const float FREEZE_DURATION = 6f;
        private const float DELAY_BETWEEN_CALLS = 5f;
        
        // Vị trí spawn cho 5 cầu thủ
        private readonly Vector3[] spawnPositions = new Vector3[5];
        private const float ANTI_OVERLAP_RADIUS = 2f;
        
        // UI Canvas - Được gán từ MatchManager sau khi component được tạo
        private GameObject call5EnemyUI;
        
        /// <summary>
        /// Khởi tạo reference cho UI. Được gọi từ MatchManager.
        /// </summary>
        public void Initialize(GameObject uiCanvas) {
            call5EnemyUI = uiCanvas;
            
            if (call5EnemyUI != null) {
                // Đảm bảo UI bị ẩn
                call5EnemyUI.SetActive(false);
                Debug.Log("[Call5Enemy] Call5EnemyUI initialized and set to inactive");
            } else {
                Debug.LogWarning("[Call5Enemy] Call5EnemyUI reference is null!");
            }
        }
        
        /// <summary>
        /// Public method để trigger Call5Enemy từ bên ngoài (ví dụ: từ TikTok receiver)
        /// </summary>
        public void TriggerCall5Enemy() {
            if (MatchManager.Current == null) {
                Debug.LogWarning("[Call5Enemy] Cannot trigger - MatchManager is null");
                return;
            }
            
            var matchFlags = MatchManager.Current.MatchFlags;
            
            // Chỉ trigger khi đang Playing và không đang freeze
            if (matchFlags.HasFlag(MatchStatus.Playing) && !isFreezing) {
                countCall++;
                Debug.Log($"[Call5Enemy] Triggered from external source. countCall = {countCall}");
            } else {
                Debug.LogWarning("[Call5Enemy] Cannot trigger - match is not in Playing state or is freezing");
            }
        }
        
        void Update() {
            // Chỉ hoạt động khi đang trong trận và không đang freeze bởi tính năng này
            if (MatchManager.Current == null) return;
            
            var matchFlags = MatchManager.Current.MatchFlags;
            
            // Chỉ xử lý input và timer khi đang Playing (không freeze)
            if (matchFlags.HasFlag(MatchStatus.Playing) && !isFreezing) {
                // HandleInput(); // DISABLED: Input được xử lý bởi TiktokReceiver
                HandleCallTimer();
            }
            
            // Xử lý freeze riêng biệt
            if (isFreezing) {
                HandleFreeze();
            }
        }
        
        // DISABLED: Input I được xử lý bởi TiktokReceiver thay vì trực tiếp
        // private void HandleInput() {
        //     // Bắt phím I (sử dụng Unity Input System)
        //     if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame) {
        //         countCall++;
        //         Debug.Log($"[Call5Enemy] Key I pressed. countCall = {countCall}");
        //     }
        // }
        
        private void HandleFreeze() {
            float elapsed = Time.time - freezeStartTime;
            
            if (elapsed >= FREEZE_DURATION) {
                // Kết thúc đóng băng
                UnfreezeMatch();
            }
        }
        
        private void HandleCallTimer() {
            if (countCall > 0) {
                timeDelayNextCall += Time.deltaTime;
                
                if (timeDelayNextCall >= DELAY_BETWEEN_CALLS) {
                    // Kích hoạt tính năng
                    ActivateCall5Enemy();
                    
                    // Reset
                    countCall--;
                    timeDelayNextCall = 0f;
                }
            }
        }
        
        private void ActivateCall5Enemy() {
            Debug.Log("[Call5Enemy] Activating Call5Enemy feature!");
            
            // Hiện UI thông báo
            ShowUI();
            
            // 1. Đóng băng tất cả cầu thủ
            FreezeMatch();
            
            // 2. Sinh 5 cầu thủ AI
            SpawnEnemyPlayers();
        }
        
        private void FreezeMatch() {
            previousMatchStatus = MatchManager.Current.MatchFlags;
            MatchManager.Current.MatchFlags = MatchStatus.Freeze;
            isFreezing = true;
            freezeStartTime = Time.time;
            
            Debug.Log("[Call5Enemy] Match frozen for " + FREEZE_DURATION + " seconds");
        }
        
        private void UnfreezeMatch() {
            // Ẩn UI thông báo
            HideUI();
            
            // Khôi phục trạng thái trước đó hoặc Playing
            if (previousMatchStatus.HasFlag(MatchStatus.Playing)) {
                MatchManager.Current.MatchFlags = MatchStatus.Playing;
            } else {
                MatchManager.Current.MatchFlags = previousMatchStatus;
            }
            
            isFreezing = false;
            
            Debug.Log("[Call5Enemy] Match unfrozen");
        }
        
        private void ShowUI() {
            if (call5EnemyUI != null) {
                call5EnemyUI.SetActive(true);
                Debug.Log("[Call5Enemy] UI shown - Calling 5 enemy players!");
            }
        }
        
        private void HideUI() {
            if (call5EnemyUI != null) {
                call5EnemyUI.SetActive(false);
                Debug.Log("[Call5Enemy] UI hidden");
            }
        }
        
        private void SpawnEnemyPlayers() {
            // Xác định AI team (team đối phương của user)
            GameTeam aiTeam = null;
            
            if (MatchManager.Current.UserTeam == MatchManager.Current.GameTeam1) {
                aiTeam = MatchManager.Current.GameTeam2;
            } else if (MatchManager.Current.UserTeam == MatchManager.Current.GameTeam2) {
                aiTeam = MatchManager.Current.GameTeam1;
            } else {
                // Nếu không có UserTeam, mặc định spawn ở GameTeam2
                aiTeam = MatchManager.Current.GameTeam2;
            }
            
            if (aiTeam == null) {
                Debug.LogError("[Call5Enemy] Could not determine AI team!");
                return;
            }
            
            Debug.Log($"[Call5Enemy] Spawning 5 players for team: {aiTeam.Team.Team.TeamName}");
            
            // Tính toán vị trí spawn
            CalculateSpawnPositions(aiTeam);
            
            // Tạo 5 cầu thủ mới với vị trí tấn công
            Positions[] attackingPositions = new Positions[] {
                Positions.ST,      // Tiền đạo trung tâm
                Positions.ST_L,    // Tiền đạo trái
                Positions.ST_R,    // Tiền đạo phải
                Positions.LW,      // Cánh trái
                Positions.RW       // Cánh phải
            };
            
            for (int i = 0; i < 5; i++) {
                SpawnPlayer(aiTeam, attackingPositions[i], spawnPositions[i], i);
            }
            
            Debug.Log($"[Call5Enemy] Successfully spawned 5 enemy players!");
        }
        
        private void CalculateSpawnPositions(GameTeam aiTeam) {
            // Xác định khu vực spawn dựa trên hướng tấn công của team
            Vector3 baseSpawnPos;
            
            // Lấy vị trí khung thành mục tiêu của AI team
            var aiGoalNet = aiTeam.TeamId == 0 ? MatchManager.Current.goalNet1 : MatchManager.Current.goalNet2;
            var targetGoalNet = aiTeam.TeamId == 0 ? MatchManager.Current.goalNet2 : MatchManager.Current.goalNet1;
            
            // Spawn ở gần giữa sân, hướng về khung thành đối phương
            float spawnX;
            if (targetGoalNet.Position.x > aiGoalNet.Position.x) {
                // Tấn công về phía x lớn hơn
                spawnX = MatchManager.Current.fieldEndX * 0.55f;
            } else {
                // Tấn công về phía x nhỏ hơn
                spawnX = MatchManager.Current.fieldEndX * 0.45f;
            }
            
            baseSpawnPos = new Vector3(
                spawnX,
                0,
                MatchManager.Current.fieldEndY / 2f
            );
            
            // Tạo pattern spawn hình quạt (fan formation)
            float spacing = 5f;
            spawnPositions[0] = baseSpawnPos; // Trung tâm (ST)
            spawnPositions[1] = baseSpawnPos + new Vector3(-spacing, 0, spacing);  // Trái trên (ST_L)
            spawnPositions[2] = baseSpawnPos + new Vector3(spacing, 0, spacing);   // Phải trên (ST_R)
            spawnPositions[3] = baseSpawnPos + new Vector3(-spacing * 1.5f, 0, 0); // Trái (LW)
            spawnPositions[4] = baseSpawnPos + new Vector3(spacing * 1.5f, 0, 0);  // Phải (RW)
            
            // Chống spawn chồng lên nhau
            for (int i = 0; i < 5; i++) {
                spawnPositions[i] = GetSafeSpawnPosition(spawnPositions[i]);
            }
        }
        
        private Vector3 GetSafeSpawnPosition(Vector3 desiredPos) {
            // Kiểm tra xem có cầu thủ nào ở gần vị trí này không
            var allPlayers = MatchManager.AllPlayers;
            
            if (allPlayers != null) {
                foreach (var player in allPlayers) {
                    if (player == null || player.PlayerController == null) continue;
                    
                    float dist = Vector3.Distance(player.Position, desiredPos);
                    if (dist < ANTI_OVERLAP_RADIUS) {
                        // Dịch vị trí ra một chút theo hướng ngẫu nhiên
                        Vector2 randomOffset = Random.insideUnitCircle * (ANTI_OVERLAP_RADIUS + 1f);
                        desiredPos += new Vector3(randomOffset.x, 0, randomOffset.y);
                    }
                }
            }
            
            // Clamp trong sân
            desiredPos.x = Mathf.Clamp(desiredPos.x, 2f, MatchManager.Current.fieldEndX - 2f);
            desiredPos.z = Mathf.Clamp(desiredPos.z, 2f, MatchManager.Current.fieldEndY - 2f);
            desiredPos.y = 0;
            
            return desiredPos;
        }
        
        private void SpawnPlayer(GameTeam team, Positions position, Vector3 spawnPos, int index) {
            try {
                // Lấy player từ database của team hoặc tạo mới
                PlayerEntry playerEntry;
                
                if (team.Team.Team.Players != null && team.Team.Team.Players.Length > 0) {
                    // Lấy player ngẫu nhiên từ đội hình hiện có
                    var randomIndex = Random.Range(0, team.Team.Team.Players.Length);
                    playerEntry = team.Team.Team.Players[randomIndex];
                } else {
                    // Tạo player entry giả với stats cao (vì là tính năng đặc biệt)
                    playerEntry = ScriptableObject.CreateInstance<PlayerEntry>();
                    
                    playerEntry.topSpeed = Random.Range(75, 90);
                    playerEntry.acceleration = Random.Range(75, 90);
                    playerEntry.agility = Random.Range(70, 85);
                    playerEntry.shooting = Random.Range(75, 95);
                    playerEntry.passing = Random.Range(70, 85);
                    playerEntry.dribbleSpeed = Random.Range(70, 85);
                    playerEntry.ballControl = Random.Range(70, 85);
                    playerEntry.height = Random.Range(175, 190);
                    playerEntry.weight = Random.Range(70, 85);
                    playerEntry.strength = Random.Range(70, 85);
                    playerEntry.positioning = Random.Range(70, 85);
                    playerEntry.longBall = Random.Range(65, 80);
                    playerEntry.tackling = Random.Range(50, 70);
                }
                
                // Tạo MatchPlayer
                var matchPlayer = new MatchPlayer(
                    team.GamePlayers.Length + index + 100, // Số áo đặc biệt
                    playerEntry,
                    position
                );
                
                // Spawn player thông qua GameTeam
                var newPlayer = team.SpawnPlayerDynamically(matchPlayer, spawnPos);
                
                if (newPlayer != null) {
                    Debug.Log($"[Call5Enemy] Successfully spawned player #{matchPlayer.Number} at position {position} at {spawnPos}");
                } else {
                    Debug.LogWarning($"[Call5Enemy] Failed to spawn player at position {position}");
                }
                
            } catch (System.Exception ex) {
                Debug.LogError($"[Call5Enemy] Error spawning player: {ex.Message}");
            }
        }
    }
}
