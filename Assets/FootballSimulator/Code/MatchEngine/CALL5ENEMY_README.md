# Tính năng Call5Enemy

## Mô tả
Tính năng Call5Enemy cho phép sinh thêm 5 cầu thủ AI với vai trò tấn công vào sân trong khi trận đấu đang diễn ra.

## Cách sử dụng

### Trong trận đấu:
1. **Bấm phím `I`** để tăng bộ đếm `countCall`
2. Đợi **5 giây** (thời gian delay giữa các lần gọi)
3. Tự động:
   - **UI thông báo** sẽ hiện lên
   - Tất cả cầu thủ trên sân sẽ đóng băng trong **6 giây**
   - 5 cầu thủ AI mới sẽ được sinh ra với vai trò tấn công
   - **UI thông báo** tự động tắt sau khi hết đóng băng

### Điều kiện hoạt động:
- ✅ Chỉ hoạt động khi trận đấu đang ở trạng thái `Playing` (đang thi đấu normal)
- ✅ Không hoạt động khi đã đang trong trạng thái đóng băng
- ✅ Có thể bấm `I` nhiều lần để tích lũy số lần gọi

### UI Canvas:
- **Tên:** `Call5EnemyUI`
- **Vị trí:** Phải có trong scene Stadium (ví dụ: Stadium1_Small)
- **Hiển thị:** Tự động hiện khi kích hoạt tính năng
- **Ẩn:** Tự động ẩn sau khi hết đóng băng 6 giây

## Chi tiết kỹ thuật

### Input System
Tính năng sử dụng **Unity Input System** (không phải Input cũ):
```csharp
if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame) {
    // Xử lý khi bấm phím I
}
```

### Các file đã chỉnh sửa:

1. **Call5EnemyManager.cs** (MỚI)
   - Quản lý logic chính của tính năng
   - Xử lý input phím I (Unity Input System)
   - Đếm thời gian delay
   - Đóng băng/mở băng trận đấu
   - Sinh cầu thủ mới

2. **GameTeam.cs**
   - Thêm method `SpawnPlayerDynamically()` để sinh cầu thủ động

3. **MatchManager.cs**
   - Thêm field `call5EnemyManager`
   - Khởi tạo Call5EnemyManager trong `CreateMatch()`
   - Cleanup trong `ClearMatch()`

### Thông số có thể tùy chỉnh:

```csharp
// Trong Call5EnemyManager.cs
private const float FREEZE_DURATION = 6f;        // Thời gian đóng băng (giây)
private const float DELAY_BETWEEN_CALLS = 5f;    // Delay giữa các lần gọi (giây)
private const float ANTI_OVERLAP_RADIUS = 2f;    // Bán kính chống chồng lấp
```

### Vị trí spawn:
- 5 cầu thủ được spawn theo hình quạt (fan formation)
- Vị trí: gần giữa sân, hướng về khung thành đối phương
- Có cơ chế tránh spawn chồng lên cầu thủ đang có sẵn

### Vai trò cầu thủ được spawn:
1. **ST** - Tiền đạo trung tâm
2. **ST_L** - Tiền đạo trái
3. **ST_R** - Tiền đạo phải
4. **LW** - Cánh trái
5. **RW** - Cánh phải

### Stats của cầu thủ mới:
- Nếu có dữ liệu team: lấy ngẫu nhiên từ đội hình
- Nếu không: tạo stats cao (75-95) vì là tính năng đặc biệt
- Số áo: +100 so với số cầu thủ hiện tại

### Xử lý reposition:
- Cầu thủ spawn động được đánh dấu với `IsDynamicallySpawned = true`
- Các method reposition (kickoff, corner, foul) chỉ reposition 11 cầu thủ gốc
- Cầu thủ spawn động sẽ tự do di chuyển theo AI, không bị force reposition
- Tránh lỗi `IndexOutOfRangeException` khi có nhiều hơn 11 cầu thủ

## Debug/Kiểm tra

### Log messages:
```
[Call5Enemy] Found Call5EnemyUI and set it to inactive
[Call5Enemy] Key I pressed. countCall = X
[Call5Enemy] Activating Call5Enemy feature!
[Call5Enemy] UI shown - Calling 5 enemy players!
[Call5Enemy] Match frozen for 6 seconds
[Call5Enemy] Spawning 5 players for team: [TÊN TEAM]
[Call5Enemy] Successfully spawned player #XXX at position ST at (x, y, z)
[Call5Enemy] UI hidden
[Call5Enemy] Match unfrozen
```

### Kiểm tra trong Unity:
1. **Tạo UI Canvas:**
   - Tạo Canvas trong scene Stadium
   - Đặt tên chính xác: `Call5EnemyUI`
   - Thêm các UI elements (Text, Image, Panel...) để hiển thị thông báo
   
2. **Chạy trận đấu:**
   - Mở Console (Ctrl + Shift + C)
   - Bấm phím I và theo dõi log
   - Kiểm tra UI có hiện lên không
   - Đợi 5 giây để xem UI và cầu thủ spawn
   - UI sẽ tự động tắt sau 6 giây

### Lỗi thường gặp:
⚠️ **Warning: Call5EnemyUI not found in scene**
- Nguyên nhân: Chưa tạo GameObject tên `Call5EnemyUI` trong scene
- Giải pháp: Tạo Canvas với tên chính xác `Call5EnemyUI` trong scene Stadium

## Lưu ý

⚠️ **Quan trọng:**
- Tính năng này thay đổi số lượng cầu thủ động trong trận
- Có thể ảnh hưởng đến performance nếu spawn quá nhiều
- Chưa có giới hạn số lượng cầu thủ tối đa
- Cầu thủ mới không bị xóa khi hết trận

💡 **Cải tiến có thể:**
- Thêm UI hiển thị `countCall`
- Thêm hiệu ứng VFX khi spawn
- Giới hạn số lượng cầu thủ tối đa trên sân
- Tự động xóa cầu thủ sau một khoảng thời gian
- Thêm âm thanh khi kích hoạt
- Cho phép tùy chỉnh phím bấm

## Tác giả
Tạo ngày: 2026-02-04
Version: 1.0
