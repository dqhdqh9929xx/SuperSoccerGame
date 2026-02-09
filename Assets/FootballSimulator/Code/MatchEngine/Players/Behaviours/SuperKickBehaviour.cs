using UnityEngine;
using System.Linq;
using FStudio.MatchEngine.Enums;
using FStudio.MatchEngine.EngineOptions;
using FStudio.MatchEngine.Balls;

namespace FStudio.MatchEngine.Players.Behaviours {
    /// <summary>
    /// Super Kick: only in normal play. When IsSuperKick is set (e.g. by pressing U),
    /// Case 1: if ball is held, the holder shoots at Home goal immediately.
    /// Case 2: if ball is free, the first player to touch (hold) the ball will shoot at Home goal.
    /// Target goal is always Home (goal with smallest position.z).
    /// Mode is cleared when ball goes out of play (OutEvent or ShootWentOutEvent).
    /// 
    /// Random power/height với bước nhảy 0.1f (nhiều case hơn 3 case cũ).
    /// Clone balls được spawn bởi BallPowerVisualController khi bóng đã bay và biến đổi power.
    /// </summary>
    public class SuperKickBehaviour : AbstractShootingBehaviour {
        // Random power range (bước nhảy 0.1f)
        private const float POWER_MIN = 0.5f;
        private const float POWER_MAX = 2.0f;
        
        // Random height range (bước nhảy 0.1f)
        private const float HEIGHT_MIN = 0.3f;
        private const float HEIGHT_MAX = 1.5f;
        
        // Bước nhảy random
        private const float RANDOM_STEP = 0.1f;
        
        private float currentPowerMultiplier = 1.5f;
        private float currentHeightMultiplier = 0.7f;
        
        private const float MIN_Z_DISTANCE_SQR_TO_ANGLE_CHECK = 4f;
        private const float MAX_X_DISTANCE_SQR_TO_ANGLE_CHECK = 6f;
        private const float MAX_ANGLE = 80f;

        private (Transform shootPoint, float angleFree)? shootingTarget;
        private Vector3 shootingDir;

        private bool IsNormalSituation() {
            if (!IsRoughValidated()) return false;
            if (Player.IsThrowHolder || Player.IsCornerHolder || Player.IsGoalKickHolder) return false;
            if (Player.IsGK && Player.IsGKUntouchable) return false;
            return true;
        }

        private bool CanShootAtGoal(GoalNet net) {
            var goalToMe = Player.Position - net.Position;
            if (Mathf.Abs(goalToMe.z) > MIN_Z_DISTANCE_SQR_TO_ANGLE_CHECK &&
                Mathf.Abs(goalToMe.x) <= MAX_X_DISTANCE_SQR_TO_ANGLE_CHECK) {
                if (AngleToGoal(net) > MAX_ANGLE) return false;
            }
            return true;
        }

        /// <summary>
        /// Random một giá trị trong khoảng [min, max] với bước nhảy step.
        /// Ví dụ: RandomWithStep(0.5f, 2.0f, 0.1f) → 0.5, 0.6, 0.7, ..., 1.9, 2.0
        /// </summary>
        private float RandomWithStep(float min, float max, float step) {
            int steps = Mathf.RoundToInt((max - min) / step);
            int randomStep = Random.Range(0, steps + 1);
            return min + randomStep * step;
        }

        public override bool Behave(bool isAlreadyActive) {
            if (MatchManager.Current == null || !MatchManager.Current.IsSuperKick)
                return false;

            if (ball.HolderPlayer != Player)
                return false;

            if (!IsNormalSituation())
                return false;

            var homeGoal = MatchManager.Current.HomeGoalNet;

            if (!isAlreadyActive) {
                if (!CanShootAtGoal(homeGoal)) return false;

                var shootVector = homeGoal.GetShootingVector(Player, opponents);
                if (shootVector.shootPoint == null) return false;

                // Random power và height với bước nhảy 0.1f
                currentPowerMultiplier = RandomWithStep(POWER_MIN, POWER_MAX, RANDOM_STEP);
                currentHeightMultiplier = RandomWithStep(HEIGHT_MIN, HEIGHT_MAX, RANDOM_STEP);
                
                Debug.Log($"[SuperKick] Main ball - Power: {currentPowerMultiplier:F1}, Height: {currentHeightMultiplier:F1}");

                shootingTarget = shootVector;
                shootingDir = shootVector.shootPoint.position - Player.Position;
                isAlreadyActive = true;
            }

            if (isAlreadyActive && shootingTarget.HasValue) {
                var st = shootingTarget.Value;
                Player.GameTeam.KeepPlayerBehavioursForAShortTime();
                Player.CurrentAct = Acts.Shoot;
                Player.Stop(in deltaTime);

                if (Player.LookTo(in deltaTime, shootingDir)) {
                    var shootPowerByAngleFree = EngineOptions_ShootingSettings.Current.shootPowerModByAngleFree.Evaluate(st.angleFree);
                    var baseTarget = homeGoal.GetShootingVectorFromPoint(Player, st.shootPoint) * shootPowerByAngleFree;
                    
                    // Quả bóng chính: áp dụng random multiplier đã chọn
                    var mainTarget = new Vector3(
                        baseTarget.x * currentPowerMultiplier,
                        baseTarget.y * currentHeightMultiplier,
                        baseTarget.z * currentPowerMultiplier
                    );
                    
                    // Sút quả bóng chính
                    // Clone balls sẽ được BallPowerVisualController spawn khi bóng biến đổi power
                    Player.Shoot(mainTarget);
                    
                    int comboCount = MatchManager.Current.SuperKickComboCount;
                    if (comboCount > 1) {
                        Debug.Log($"[SuperKick] COMBO x{comboCount}! Clone balls will spawn when ball transforms to power mode.");
                    }
                }
                return true;
            }

            return false;
        }
    }
}
