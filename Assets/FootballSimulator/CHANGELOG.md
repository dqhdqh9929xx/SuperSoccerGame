# Changelog - SuperSoccerGame

## [1.1.1] - 2026-02-04 (HOTFIX)

### 🐛 Bug Fix

#### Input System Compatibility
- **Fix: InvalidOperationException khi bấm phím**
  - Nguyên nhân: Script dùng old Input API (`Input.GetKeyDown`) nhưng project dùng New Input System
  - Giải pháp: Chuyển tất cả script sang dùng `UnityEngine.InputSystem.Keyboard`
  - Files đã sửa:
    - `TiktokReceiverTest.cs`
    - `SimpleInputTest.cs`
    - `DebugStatusDisplay.cs`
  - ✅ Giờ input hoạt động bình thường!

---

## [1.1.0] - 2026-02-04

### ✨ Tính năng mới

#### TiktokReceiver System
- **Thêm `TiktokReceiver.cs`**: Component để nhận event từ TikTok stream
- **Thêm `TiktokReceiverTest.cs`**: Tool để test TiktokReceiver (phím T và Y)
- Hỗ trợ trigger từ nhiều nguồn: C#, UI Button, WebGL, Command String

#### Tài liệu
- **Thêm `TIKTOK_RECEIVER_README.md`**: Tài liệu chi tiết API
- **Thêm `TIKTOK_SETUP_GUIDE.md`**: Hướng dẫn setup nhanh 5 phút
- **Thêm `CHANGELOG.md`**: File này

### 🔧 Sửa lỗi

#### Call5Enemy Feature
- **Fix: Cầu thủ spawn bị đứng yên**
  - File: `GameTeam.cs` (dòng 353)
  - Thay đổi: `for (int i=0; i < 11; i++)` → `for (int i=0; i < GamePlayers.Length; i++)`
  - Nguyên nhân: Vòng lặp hardcoded chỉ xử lý 11 cầu thủ đầu
  - Kết quả: 5 cầu thủ spawn động giờ hoạt động bình thường

### ⚙️ Thay đổi

#### Input System Refactor
- **Vô hiệu hóa input U và I gốc**
  - File: `TeamInputListener.cs`
    - Comment out `RegisterAction("SuperKick", SuperKickInput)`
    - Comment out method `SuperKickInput()`
  - File: `Call5EnemyManager.cs`
    - Comment out `HandleInput()` call
    - Comment out method `HandleInput()`
  - Lý do: Tránh conflict, tập trung input vào TiktokReceiver
  - **Breaking change**: Phím U và I không còn hoạt động trực tiếp

#### Call5EnemyManager API
- **Thêm public method `TriggerCall5Enemy()`**
  - Cho phép trigger từ bên ngoài (TiktokReceiver)
  - Có validation và log chi tiết

---

## [1.0.0] - 2026-02-04 (Trước khi thay đổi)

### Tính năng có sẵn

#### Call5Enemy Feature
- Bấm phím **I** để spawn 5 cầu thủ AI tấn công
- Đóng băng trận đấu 6 giây
- Spawn vị trí: ST, ST_L, ST_R, LW, RW
- ❌ **Lỗi**: Cầu thủ spawn bị đứng yên

#### Super Kick Feature
- Bấm phím **U** để kích hoạt cú sút siêu mạnh
- Ánh sáng directional light tối đi
- Tự động tắt khi bóng out
- ✅ Hoạt động bình thường

---

## Migration Guide

### Từ phiên bản 1.0 lên 1.1

#### Input đã thay đổi

**❌ Không dùng được nữa:**
```csharp
// Phím U và I không còn hoạt động trực tiếp
// Đã bị comment out trong TeamInputListener.cs và Call5EnemyManager.cs
```

**✅ Cách mới (khuyến nghị):**
```csharp
// Sử dụng TiktokReceiver
TiktokReceiver receiver = FindObjectOfType<TiktokReceiver>();
receiver.TriggerSuperKick();     // Thay cho phím U
receiver.TriggerCall5Enemy();    // Thay cho phím I
```

**✅ Test trong Unity Editor:**
```
Phím T → Super Kick (thay cho U)
Phím Y → Call 5 Enemy (thay cho I)
```

#### Code Migration

Nếu bạn có code gọi trực tiếp:

**Trước:**
```csharp
// Không còn hoạt động
MatchManager.Current.SetSuperKick(true);  // Phím U đã disabled
```

**Sau:**
```csharp
// Cách 1: Qua TiktokReceiver
TiktokReceiver receiver = FindObjectOfType<TiktokReceiver>();
receiver.TriggerSuperKick();

// Cách 2: Gọi trực tiếp vẫn work (nếu bạn có code)
MatchManager.Current.SetSuperKick(true);  // Vẫn hoạt động
```

**Trước:**
```csharp
// Không còn hoạt động (phím I đã disabled)
// HandleInput() trong Call5EnemyManager
```

**Sau:**
```csharp
// Cách 1: Qua TiktokReceiver
TiktokReceiver receiver = FindObjectOfType<TiktokReceiver>();
receiver.TriggerCall5Enemy();

// Cách 2: Gọi trực tiếp method mới
Call5EnemyManager manager = MatchManager.Current.GetComponent<Call5EnemyManager>();
manager.TriggerCall5Enemy();
```

---

## Known Issues

### Không có issue quan trọng

Tất cả tính năng đã được test và hoạt động tốt.

---

## TODO / Cải tiến tương lai

### Tính năng
- [ ] Thêm UI hiển thị `countCall` trên màn hình
- [ ] Thêm VFX effect khi spawn 5 cầu thủ
- [ ] Giới hạn số lượng cầu thủ tối đa trên sân
- [ ] Tự động xóa cầu thủ spawn sau thời gian nhất định
- [ ] Thêm âm thanh khi kích hoạt Call5Enemy

### TikTok Integration
- [ ] Ví dụ tích hợp TikTok API hoàn chỉnh
- [ ] Map TikTok gift → game feature
- [ ] Map TikTok comment → game command
- [ ] WebGL build example

### Performance
- [ ] Optimize khi có > 20 cầu thủ trên sân
- [ ] Pool cầu thủ spawn để tái sử dụng

---

## Contributors

- Initial development: DuyManhh
- Date: 2026-02-04

---

## License

Internal project - SuperSoccerGame
