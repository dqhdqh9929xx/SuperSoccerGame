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
        private const float SUPER_KICK_POWER_MULTIPLIER = 1.5f;
        private const float SUPER_KICK_HEIGHT_MULTIPLIER = 0.7f; // Điều chỉnh độ cao của cú sút (1.0 = giữ nguyên, < 1.0 = thấp hơn, > 1.0 = cao hơn)
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
                    
                    // Apply multipliers: horizontal (X, Z) gets SUPER_KICK_POWER_MULTIPLIER, height (Y) gets SUPER_KICK_HEIGHT_MULTIPLIER
                    var target = new Vector3(
                        baseTarget.x * SUPER_KICK_POWER_MULTIPLIER,
                        baseTarget.y * SUPER_KICK_HEIGHT_MULTIPLIER,
                        baseTarget.z * SUPER_KICK_POWER_MULTIPLIER
                    );
                    
                    Player.Shoot(target);
                }
                return true;
            }

            return false;
        }
    }
}
