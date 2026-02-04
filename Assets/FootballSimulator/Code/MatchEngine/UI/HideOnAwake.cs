using UnityEngine;

namespace FStudio.MatchEngine.UI {
    /// <summary>
    /// Tự động ẩn GameObject này ngay khi Awake (trước khi render frame đầu tiên).
    /// Dùng cho UI cần ẩn ngay từ đầu nhưng vẫn cần tìm bằng GameObject.Find().
    /// </summary>
    public class HideOnAwake : MonoBehaviour {
        void Awake() {
            // Ẩn GameObject này ngay lập tức
            gameObject.SetActive(false);
        }
    }
}
