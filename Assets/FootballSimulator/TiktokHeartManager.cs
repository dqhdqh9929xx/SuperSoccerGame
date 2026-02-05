using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FStudio.MatchEngine;

/// <summary>
/// Quản lý hệ thống tap tim từ TikTok
/// - Thu thập 100 taps từ viewers
/// - Random chọn 1 người để trigger Super Kick
/// </summary>
public class TiktokHeartManager : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Số lượng heart cần để trigger Super Kick")]
    public int heartThreshold = 100;
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    
    // Mảng lưu tên người tap (100 phần tử)
    private string[] heartTappers = new string[100];
    
    // Index hiện tại trong mảng (0-99)
    private int currentIndex = 0;
    
    // Flag để kiểm tra xem Super Kick có đang active không
    private bool isSuperKickActive = false;
    
    // Tên người được chọn random khi đủ 100 hearts
    private string selectedUserName = "";
    
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
        // Kiểm tra trạng thái Super Kick từ MatchManager
        if (MatchManager.Current != null)
        {
            bool superKickStatus = MatchManager.Current.IsSuperKick;
            
            // Nếu Super Kick vừa tắt, cho phép tap lại
            if (isSuperKickActive && !superKickStatus)
            {
                isSuperKickActive = false;
                if (showDebugLogs)
                {
                    Debug.Log("[TiktokHeartManager] ✅ Super Kick ended. Heart tapping enabled.");
                }
            }
            // Nếu Super Kick vừa bật, block tap
            else if (!isSuperKickActive && superKickStatus)
            {
                isSuperKickActive = true;
                if (showDebugLogs)
                {
                    Debug.Log("[TiktokHeartManager] ⛔ Super Kick active. Heart tapping disabled.");
                }
            }
        }
    }
    
    /// <summary>
    /// Thêm một heart tap từ người dùng
    /// </summary>
    /// <param name="userName">Tên người tap</param>
    public void AddHeartTap(string userName)
    {
        // Nếu Super Kick đang active, không cho tap
        if (isSuperKickActive)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[TiktokHeartManager] ⛔ Heart tap from {userName} BLOCKED - Super Kick is active!");
            }
            return;
        }
        
        // Thêm tên vào mảng
        heartTappers[currentIndex] = userName;
        currentIndex++;
        
        if (showDebugLogs)
        {
            Debug.Log($"[TiktokHeartManager] 💖 Heart tap from {userName}! Count: {currentIndex}/{heartThreshold}");
        }
        
        // Kiểm tra xem đã đủ 100 chưa
        if (currentIndex >= heartThreshold)
        {
            TriggerSuperKickForRandomUser();
        }
    }
    
    /// <summary>
    /// Random chọn 1 người từ mảng và trigger Super Kick
    /// </summary>
    private void TriggerSuperKickForRandomUser()
    {
        if (showDebugLogs)
        {
            Debug.Log("[TiktokHeartManager] 🔥 Heart threshold reached! Selecting random winner...");
        }
        
        // Random index từ 0 đến 99
        int randomIndex = Random.Range(0, heartThreshold);
        selectedUserName = heartTappers[randomIndex];
        
        if (showDebugLogs)
        {
            Debug.Log($"[TiktokHeartManager] 🎉 WINNER: {selectedUserName} (index {randomIndex})");
            Debug.Log($"[TiktokHeartManager] Triggering Super Kick for {selectedUserName}!");
        }
        
        // Trigger Super Kick
        if (tiktokReceiver != null)
        {
            tiktokReceiver.TriggerSuperKick();
        }
        else
        {
            Debug.LogWarning("[TiktokHeartManager] Cannot trigger Super Kick - TiktokReceiver is null!");
        }
        
        // Reset mảng
        ResetHeartArray();
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
}
