using UnityEngine;
using System.Linq;
using FStudio.MatchEngine.Enums;
using FStudio.MatchEngine.EngineOptions;

namespace FStudio.MatchEngine.Players.Behaviours {
    /// <summary>
    /// Super Kick: only in normal play. When IsSuperKick is set (e.g. by pressing U),
    /// Case 1: if ball is held, the holder shoots at Home goal immediately.
    /// Case 2: if ball is free, the first player to touch (hold) the ball will shoot at Home goal.
    /// Target goal is always Home (goal with smallest position.z).
    /// Mode is cleared when ball goes out of play (OutEvent or ShootWentOutEvent).
    /// </summary>
    public class SuperKickBehaviour : AbstractShootingBehaviour {
        // Random power system - 3 cases
        private float currentPowerMultiplier = 1.5f;
        private readonly float[] powerVariants = new float[] { 
            0.8f,  // Case 1: WEAK - Sút yếu hơn
            1.5f,  // Case 2: NORMAL - Sút bình thường  
            1.8f   // Case 3: STRONG - Sút mạnh nhất
        };
        
        // Random height system - 3 cases
        private float currentHeightMultiplier = 0.7f;
        private readonly float[] heightVariants = new float[] { 
            0.7f,  // Case 1: LOW - Bóng bay thấp (ground shot)
            0.85f,  // Case 2: MID - Bóng bay trung bình
            1.05f   // Case 3: HIGH - Bóng bay cao (lob shot)
        };
        
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

                // Random chọn 1 trong 3 độ cao
                int randomHeightIndex = Random.Range(0, heightVariants.Length);
                currentHeightMultiplier = heightVariants[randomHeightIndex];
                
                // Random chọn 1 trong 3 lực sút
                int randomPowerIndex = Random.Range(0, powerVariants.Length);
                currentPowerMultiplier = powerVariants[randomPowerIndex];
                
                string heightType = randomHeightIndex == 0 ? "LOW (0.7f)" : 
                                  randomHeightIndex == 1 ? "MID (0.85f)" : "HIGH (1.05f)";
                string powerType = randomPowerIndex == 0 ? "WEAK (0.8f)" :
                                  randomPowerIndex == 1 ? "NORMAL (1.5f)" : "STRONG (1.8f)";
                
                Debug.Log($"[SuperKick] Height: {heightType}, Power: {powerType}");

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
                    
                    // Apply multipliers: horizontal (X, Z) gets random power multiplier, height (Y) gets random height
                    var target = new Vector3(
                        baseTarget.x * currentPowerMultiplier,
                        baseTarget.y * currentHeightMultiplier,
                        baseTarget.z * currentPowerMultiplier
                    );
                    
                    Player.Shoot(target);
                }
                return true;
            }

            return false;
        }
    }
}
