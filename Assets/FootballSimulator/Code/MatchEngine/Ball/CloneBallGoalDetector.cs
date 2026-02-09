using UnityEngine;
using FStudio.Events;
using FStudio.MatchEngine;
using FStudio.MatchEngine.Events;

namespace FStudio.MatchEngine.Balls {
    /// <summary>
    /// Gắn vào clone ball để phát hiện khi bóng clone vào gôn.
    /// Sử dụng 2 cơ chế:
    /// 1. OnTriggerEnter/Stay - phát hiện trigger zone "GoalAction" (giống bóng gốc)
    /// 2. Update position check - kiểm tra vị trí bóng mỗi frame (fallback chống tunneling)
    /// 
    /// KHÔNG check MatchStatus.Playing vì khi bóng gốc ghi bàn trước,
    /// game state đổi và sẽ block clone balls.
    /// </summary>
    public class CloneBallGoalDetector : MonoBehaviour {
        private bool hasScored = false;
        
        // Thời điểm spawn, dùng để tránh false positive ngay khi tạo
        private float spawnTime;
        private const float MIN_TIME_BEFORE_SCORE = 0.1f;

        private void Start() {
            spawnTime = Time.time;
        }

        private void OnTriggerEnter(Collider other) {
            HandleTrigger(other);
        }

        private void OnTriggerStay(Collider other) {
            HandleTrigger(other);
        }

        private void HandleTrigger(Collider other) {
            if (hasScored) return;
            if (Time.time - spawnTime < MIN_TIME_BEFORE_SCORE) return;
            if (MatchManager.Current == null) return;

            var tag = other.tag;

            if (tag == "GoalAction") {
                ScoreGoal();
            }
        }

        private void Update() {
            // Fallback: kiểm tra vị trí bóng mỗi frame (chống tunneling xuyên trigger zone)
            if (hasScored) return;
            if (Time.time - spawnTime < MIN_TIME_BEFORE_SCORE) return;
            if (MatchManager.Current == null) return;

            var sizeOfField = MatchManager.Current.SizeOfField;
            var pos = transform.position;

            // Bóng đã vượt qua đường biên ngang (vào khu vực sau gôn)
            // Kiểm tra bóng có ở trong phạm vi chiều dọc của gôn không
            // (gôn nằm ở khoảng giữa sân theo trục Z)
            float goalZCenter = sizeOfField.y / 2f;
            float goalHalfWidth = 3.66f; // Nửa chiều rộng gôn tiêu chuẩn (~7.32m)
            float goalHeight = 2.44f;    // Chiều cao gôn tiêu chuẩn

            bool inGoalZRange = Mathf.Abs(pos.z - goalZCenter) < goalHalfWidth;
            bool inGoalYRange = pos.y < goalHeight;

            if (inGoalZRange && inGoalYRange) {
                // Bóng vượt qua đường biên bên trái (x < 0)
                if (pos.x < -0.5f) {
                    ScoreGoal();
                }
                // Bóng vượt qua đường biên bên phải (x > sizeOfField.x)
                else if (pos.x > sizeOfField.x + 0.5f) {
                    ScoreGoal();
                }
            }
        }

        private void ScoreGoal() {
            if (hasScored) return;
            hasScored = true;

            var sizeOfField = MatchManager.Current.SizeOfField;
            var position = transform.position;
            var goal = position.x > sizeOfField.x / 2;

            Debug.Log($"[CloneBallGoalDetector] GOAL! Clone ball scored at {position}! (HomeOrAway: {!goal})");

            // Fire GoalEvent giống bóng gốc
            EventManager.Trigger(new GoalEvent(!goal));

            // Tự hủy clone ball ngay sau khi ghi bàn
            Destroy(gameObject);
        }
    }
}
