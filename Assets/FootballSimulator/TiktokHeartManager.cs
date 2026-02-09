using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FStudio.MatchEngine;

/// <summary>
/// Cấu trúc lưu thông tin Super Kick entry trong queue
/// Mỗi entry chỉ chiếm 1 slot, comboCount quyết định số quả bóng nhân bản khi sút
/// </summary>
[System.Serializable]
public struct SuperKickEntry
{
    public string userName;
    public int comboCount; // Số quả bóng sẽ nhân bản khi sút (1 = bình thường, 3 = 3 quả, ...)
    
    public SuperKickEntry(string userName, int comboCount = 1)
    {
        this.userName = userName;
        this.comboCount = Mathf.Max(1, comboCount);
    }
}

/// <summary>
/// Quản lý hệ thống tap tim từ TikTok
/// - Thu thập 100 taps từ viewers
/// - Random chọn 1 người để trigger Super Kick
/// - Quản lý hàng đợi Super Kick (queue system)
/// - Combo gift: chỉ thêm 1 entry vào queue, nhân bản bóng khi sút
/// </summary>
public class TiktokHeartManager : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Số lượng heart cần để trigger Super Kick")]
    public int heartThreshold = 100;
    
    [Tooltip("Thời gian chờ giữa các Super Kick (giây)")]
    public float superKickDelay = 3f;
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    
    [Header("Queue Status (Read Only)")]
    [SerializeField] private int queueCount = 0;
    [SerializeField] private float currentCountdown = 0f;
    [SerializeField] private bool isWaitingForNextKick = false;
    
    // Mảng lưu tên người tap (100 phần tử)
    private string[] heartTappers = new string[100];
    
    // Index hiện tại trong mảng (0-99)
    private int currentIndex = 0;
    
    // Flag để kiểm tra xem Super Kick có đang active không
    private bool isSuperKickActive = false;
    
    // Tên người đang được hiển thị (đang Super Kick)
    private string selectedUserName = "";
    
    // ===== QUEUE SYSTEM =====
    // Danh sách user đang chờ trigger Super Kick (mỗi entry chứa userName + comboCount)
    public List<SuperKickEntry> ListViewerTiktokSuperKick = new List<SuperKickEntry>();
    
    // Countdown timer
    private float countdown = 0f;
    
    // Reference
    private TiktokReceiver tiktokReceiver;
    
    void Start()
    {
        // Khởi tạo mảng với giá trị rỗng
        ResetHeartArray();
        
        // Tìm TiktokReceiver
        tiktokReceiver = FindObjectOfType<TiktokReceiver>();
        
        if (tiktokReceiver == null)
        {
            Debug.LogWarning("[TiktokHeartManager] TiktokReceiver not found!");
        }
        
        if (showDebugLogs)
        {
            Debug.Log("[TiktokHeartManager] Initialized! Waiting for heart taps...");
        }
    }
    
    void Update()
    {
        // Cập nhật queue status cho debug
        queueCount = ListViewerTiktokSuperKick.Count;
        currentCountdown = countdown;
        isWaitingForNextKick = countdown > 0;
        
        // Kiểm tra trạng thái Super Kick từ MatchManager
        if (MatchManager.Current != null)
        {
            bool superKickStatus = MatchManager.Current.IsSuperKick;
            
            // Nếu Super Kick vừa tắt
            if (isSuperKickActive && !superKickStatus)
            {
                isSuperKickActive = false;
                if (showDebugLogs)
                {
                    Debug.Log("[TiktokHeartManager] ✅ Super Kick ended.");
                }
                
                // Clear tên hiện tại
                selectedUserName = "";
                
                // Bắt đầu countdown nếu còn user trong queue
                if (ListViewerTiktokSuperKick.Count > 0)
                {
                    countdown = superKickDelay;
                    if (showDebugLogs)
                    {
                        Debug.Log($"[TiktokHeartManager] ⏳ Starting countdown {superKickDelay}s for next Super Kick...");
                    }
                }
            }
            // Nếu Super Kick vừa bật
            else if (!isSuperKickActive && superKickStatus)
            {
                isSuperKickActive = true;
                if (showDebugLogs)
                {
                    Debug.Log("[TiktokHeartManager] ⚡ Super Kick active! (Hearts still accumulate in background)");
                }
            }
        }
        
        // ===== QUEUE PROCESSING =====
        // Nếu Super Kick không active và có user trong queue
        if (!isSuperKickActive && ListViewerTiktokSuperKick.Count > 0)
        {
            // Nếu chưa có countdown, bắt đầu countdown
            if (countdown <= 0)
            {
                countdown = superKickDelay;
                if (showDebugLogs)
                {
                    Debug.Log($"[TiktokHeartManager] ⏳ Starting countdown {superKickDelay}s...");
                }
            }
            
            // Countdown
            countdown -= Time.deltaTime;
            
            // Khi countdown hết, trigger Super Kick cho user đầu tiên
            if (countdown <= 0)
            {
                ProcessNextSuperKick();
            }
        }
    }
    
    /// <summary>
    /// Thêm một heart tap từ người dùng
    /// KHÔNG block khi Super Kick active - luôn cho tap và tích lũy vào queue
    /// </summary>
    /// <param name="userName">Tên người tap</param>
    public void AddHeartTap(string userName)
    {
        // Thêm tên vào mảng (không check isSuperKickActive - luôn cho tap)
        heartTappers[currentIndex] = userName;
        currentIndex++;
        
        if (showDebugLogs)
        {
            Debug.Log($"[TiktokHeartManager] 💖 Heart tap from {userName}! Count: {currentIndex}/{heartThreshold}");
        }
        
        // Kiểm tra xem đã đủ 100 chưa → Add vào queue và reset
        if (currentIndex >= heartThreshold)
        {
            TriggerSuperKickForRandomUser();
        }
    }
    
    /// <summary>
    /// Random chọn 1 người từ mảng và ADD VÀO QUEUE (không trigger trực tiếp)
    /// </summary>
    private void TriggerSuperKickForRandomUser()
    {
        if (showDebugLogs)
        {
            Debug.Log("[TiktokHeartManager] 🔥 Heart threshold reached! Selecting random winner...");
        }
        
        // Random index từ 0 đến 99
        int randomIndex = Random.Range(0, heartThreshold);
        string winnerName = heartTappers[randomIndex];
        
        if (showDebugLogs)
        {
            Debug.Log($"[TiktokHeartManager] 🎉 WINNER: {winnerName} (index {randomIndex})");
        }
        
        // Add vào queue thay vì trigger trực tiếp
        AddToSuperKickQueue(winnerName);
        
        // Reset mảng
        ResetHeartArray();
    }
    
    /// <summary>
    /// Thêm user vào hàng đợi Super Kick
    /// Chỉ thêm 1 entry duy nhất, comboCount quyết định số quả bóng nhân bản khi sút
    /// Ví dụ: combo 3 Rose → 1 entry với comboCount=3 → sút ra 3 quả bóng cùng lúc
    /// </summary>
    /// <param name="userName">Tên user</param>
    /// <param name="count">Số quả bóng nhân bản khi sút (combo count)</param>
    public void AddToSuperKickQueue(string userName, int count = 1)
    {
        if (string.IsNullOrEmpty(userName))
        {
            Debug.LogWarning("[TiktokHeartManager] Cannot add empty userName to queue!");
            return;
        }
        
        // Kiểm tra xem có thể xử lý ngay không (queue trống và super kick không active)
        bool canProcessImmediately = !isSuperKickActive && ListViewerTiktokSuperKick.Count == 0 && countdown <= 0;
        
        // Chỉ thêm 1 entry duy nhất vào queue, lưu comboCount để nhân bản bóng khi sút
        ListViewerTiktokSuperKick.Add(new SuperKickEntry(userName, count));
        
        if (showDebugLogs)
        {
            if (count > 1)
            {
                Debug.Log($"[TiktokHeartManager] ➕ Added {userName} (combo x{count} balls) to Super Kick queue! Total entries in queue: {ListViewerTiktokSuperKick.Count}");
            }
            else
            {
                Debug.Log($"[TiktokHeartManager] ➕ Added {userName} to Super Kick queue! Total entries in queue: {ListViewerTiktokSuperKick.Count}");
            }
        }
        
        // Nếu không có gì đang chạy, xử lý ngay lập tức (không chờ countdown)
        if (canProcessImmediately)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[TiktokHeartManager] ⚡ Queue was empty, processing immediately!");
            }
            ProcessNextSuperKick();
        }
    }
    
    /// <summary>
    /// Process Super Kick cho user đầu tiên trong queue
    /// Truyền comboCount xuống để nhân bản bóng khi sút
    /// </summary>
    private void ProcessNextSuperKick()
    {
        if (ListViewerTiktokSuperKick.Count == 0)
        {
            if (showDebugLogs)
            {
                Debug.Log("[TiktokHeartManager] Queue is empty, nothing to process.");
            }
            return;
        }
        
        // Lấy entry đầu tiên (chứa userName + comboCount)
        SuperKickEntry entry = ListViewerTiktokSuperKick[0];
        ListViewerTiktokSuperKick.RemoveAt(0);
        
        // Set tên hiện tại
        selectedUserName = entry.userName;
        
        if (showDebugLogs)
        {
            Debug.Log($"[TiktokHeartManager] ⚡ Processing Super Kick for: {entry.userName} (combo x{entry.comboCount} balls)");
            Debug.Log($"[TiktokHeartManager] Remaining in queue: {ListViewerTiktokSuperKick.Count}");
        }
        
        // Trigger Super Kick với combo count
        if (tiktokReceiver != null)
        {
            tiktokReceiver.TriggerSuperKick(entry.comboCount);
        }
        else
        {
            Debug.LogWarning("[TiktokHeartManager] Cannot trigger Super Kick - TiktokReceiver is null!");
        }
        
        // Reset countdown
        countdown = 0;
    }
    
    /// <summary>
    /// Reset mảng tap tim về trạng thái ban đầu
    /// </summary>
    private void ResetHeartArray()
    {
        for (int i = 0; i < heartThreshold; i++)
        {
            heartTappers[i] = "";
        }
        currentIndex = 0;
        
        if (showDebugLogs)
        {
            Debug.Log("[TiktokHeartManager] Heart array reset!");
        }
    }
    
    /// <summary>
    /// Get số heart hiện tại
    /// </summary>
    public int GetCurrentHeartCount()
    {
        return currentIndex;
    }
    
    /// <summary>
    /// Kiểm tra xem có đang trong Super Kick không
    /// </summary>
    public bool IsSuperKickActive()
    {
        return isSuperKickActive;
    }
    
    /// <summary>
    /// Lấy tên người được chọn random (winner)
    /// </summary>
    public string GetSelectedUserName()
    {
        return selectedUserName;
    }
    
    /// <summary>
    /// Clear tên người được chọn
    /// </summary>
    public void ClearSelectedUserName()
    {
        selectedUserName = "";
    }
    
    /// <summary>
    /// Lấy số lượng user đang chờ trong queue
    /// </summary>
    public int GetQueueCount()
    {
        return ListViewerTiktokSuperKick.Count;
    }
    
    /// <summary>
    /// Clear toàn bộ queue (dùng khi cần reset)
    /// </summary>
    public void ClearQueue()
    {
        ListViewerTiktokSuperKick.Clear();
        countdown = 0;
        if (showDebugLogs)
        {
            Debug.Log("[TiktokHeartManager] Queue cleared!");
        }
    }
}
