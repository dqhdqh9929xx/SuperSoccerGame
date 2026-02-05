# TikTok Receiver - Hướng dẫn sử dụng

## Mô tả
`TiktokReceiver` là component để nhận event từ **TikTok stream** (hoặc nguồn bên ngoài) và trigger các tính năng đặc biệt trong game.

## Tính năng hỗ trợ

### 1. **Super Kick** (Phím U)
- **Mô tả**: Cú sút siêu mạnh về phía khung thành Home
- **Method**: `OnSuperKickEvent()` hoặc `TriggerSuperKick()`
- **Hiệu ứng**: 
  - Ánh sáng directional light tối đi
  - Cầu thủ đang cầm bóng sút siêu mạnh
  - Nếu bóng đang tự do, cầu thủ đầu tiên chạm bóng sẽ sút ngay

### 2. **Call 5 Enemy** (Phím I)
- **Mô tả**: Sinh 5 cầu thủ AI tấn công vào sân
- **Method**: `OnCall5EnemyEvent()` hoặc `TriggerCall5Enemy()`
- **Hiệu ứng**:
  - Đóng băng trận đấu 6 giây
  - Spawn 5 cầu thủ AI (ST, ST_L, ST_R, LW, RW)
  - 5 cầu thủ mới sẽ tham gia tấn công

---

## Cách sử dụng

### Setup trong Unity

1. **Tạo GameObject mới** trong scene:
   - Tên: `TiktokReceiver`
   - Vị trí: Bất kỳ (khuyến nghị: đặt cùng level với MatchManager)

2. **Add component** `TiktokReceiver`:
   ```
   GameObject → Add Component → TiktokReceiver
   ```

3. **Cấu hình**:
   - ✅ Check `showDebugLogs` nếu muốn xem log debug

### Cách 1: Gọi từ code C#

```csharp
// Tìm TiktokReceiver trong scene
TiktokReceiver receiver = FindObjectOfType<TiktokReceiver>();

// Trigger Super Kick
receiver.TriggerSuperKick();

// Trigger Call 5 Enemy
receiver.TriggerCall5Enemy();
```

### Cách 2: Gọi từ UI Button (UnityEvent)

1. Tạo UI Button
2. Trong `OnClick()` event:
   - Add `TiktokReceiver` object
   - Chọn method: `TriggerSuperKick()` hoặc `TriggerCall5Enemy()`

### Cách 3: Gọi từ WebGL / JavaScript

```javascript
// Super Kick
SendMessage('TiktokReceiver', 'TriggerSuperKick');

// Call 5 Enemy
SendMessage('TiktokReceiver', 'TriggerCall5Enemy');

// Hoặc dùng command string
SendMessage('TiktokReceiver', 'OnTikTokCommand', 'superkick');
SendMessage('TiktokReceiver', 'OnTikTokCommand', 'call5enemy');
```

### Cách 4: Dùng Command String

```csharp
TiktokReceiver receiver = FindObjectOfType<TiktokReceiver>();

// Các command được hỗ trợ cho Super Kick:
receiver.OnTikTokCommand("superkick");
receiver.OnTikTokCommand("super_kick");
receiver.OnTikTokCommand("u");

// Các command được hỗ trợ cho Call 5 Enemy:
receiver.OnTikTokCommand("call5enemy");
receiver.OnTikTokCommand("call_5_enemy");
receiver.OnTikTokCommand("i");
```

---

## API Reference

### Public Methods

#### `OnSuperKickEvent()`
Trigger tính năng Super Kick.

**Điều kiện**:
- ✅ MatchManager phải tồn tại
- ✅ Trận đấu đang trong trạng thái Playing

**Kết quả**:
- Kích hoạt chế độ Super Kick
- Ánh sáng tối đi
- Cầu thủ sút siêu mạnh

---

#### `OnCall5EnemyEvent()`
Trigger tính năng Call 5 Enemy.

**Điều kiện**:
- ✅ MatchManager phải tồn tại
- ✅ Call5EnemyManager phải được khởi tạo
- ✅ Trận đấu đang trong trạng thái Playing
- ✅ Không đang trong trạng thái freeze

**Kết quả**:
- Tăng `countCall` lên 1
- Sau 5 giây delay → spawn 5 cầu thủ AI
- Đóng băng trận đấu 6 giây

---

#### `TriggerSuperKick()`
Alias của `OnSuperKickEvent()` - dễ dàng gọi từ UnityEvent.

---

#### `TriggerCall5Enemy()`
Alias của `OnCall5EnemyEvent()` - dễ dàng gọi từ UnityEvent.

---

#### `OnTikTokCommand(string command)`
Trigger tính năng dựa trên string command.

**Parameters**:
- `command`: Tên lệnh (case-insensitive)

**Supported commands**:
| Command | Feature |
|---------|---------|
| `superkick`, `super_kick`, `u` | Super Kick |
| `call5enemy`, `call_5_enemy`, `i` | Call 5 Enemy |

**Example**:
```csharp
receiver.OnTikTokCommand("superkick");  // → Trigger Super Kick
receiver.OnTikTokCommand("I");          // → Trigger Call 5 Enemy
```

---

## Debug & Testing

### Kiểm tra log

Khi `showDebugLogs = true`, bạn sẽ thấy các log sau:

```
[TiktokReceiver] Call5EnemyManager reference cached successfully
[TiktokReceiver] Received SuperKick event from TikTok
[TiktokReceiver] SuperKick activated!
[TiktokReceiver] Received Call5Enemy event from TikTok
[TiktokReceiver] Call5Enemy triggered!
[TiktokReceiver] Received command: superkick
```

### Test trong Unity Editor

1. Tạo test script:

```csharp
using UnityEngine;

public class TiktokReceiverTest : MonoBehaviour {
    void Update() {
        // Bấm phím T để test Super Kick
        if (Input.GetKeyDown(KeyCode.T)) {
            FindObjectOfType<TiktokReceiver>()?.TriggerSuperKick();
        }
        
        // Bấm phím Y để test Call 5 Enemy
        if (Input.GetKeyDown(KeyCode.Y)) {
            FindObjectOfType<TiktokReceiver>()?.TriggerCall5Enemy();
        }
    }
}
```

2. Add script vào GameObject bất kỳ
3. Chạy game và test:
   - **T** → Super Kick
   - **Y** → Call 5 Enemy

---

## Tích hợp với TikTok Stream

### Ví dụ: Tích hợp với TikTok API

```csharp
using UnityEngine;
using System.Net.Http;
using System.Threading.Tasks;

public class TikTokStreamIntegration : MonoBehaviour {
    private TiktokReceiver receiver;
    
    void Start() {
        receiver = FindObjectOfType<TiktokReceiver>();
        
        // Bắt đầu lắng nghe TikTok stream
        StartListeningToTikTok();
    }
    
    async void StartListeningToTikTok() {
        // Pseudo code - tích hợp với TikTok API của bạn
        while (true) {
            string command = await GetTikTokCommand();
            
            if (!string.IsNullOrEmpty(command)) {
                receiver.OnTikTokCommand(command);
            }
            
            await Task.Delay(100);
        }
    }
    
    async Task<string> GetTikTokCommand() {
        // Implement TikTok API call ở đây
        // Return "superkick" hoặc "call5enemy" dựa trên gift/comment
        return null;
    }
}
```

### Ví dụ: Map TikTok Gift → Game Feature

```csharp
public class TikTokGiftMapper : MonoBehaviour {
    private TiktokReceiver receiver;
    
    void Start() {
        receiver = FindObjectOfType<TiktokReceiver>();
    }
    
    // Được gọi khi nhận gift từ TikTok
    public void OnTikTokGift(string giftName, int count) {
        Debug.Log($"Received TikTok gift: {giftName} x{count}");
        
        switch (giftName.ToLower()) {
            case "rose":
            case "🌹":
                if (count >= 10) {
                    receiver.TriggerSuperKick();
                }
                break;
                
            case "finger heart":
            case "❤️":
                if (count >= 5) {
                    receiver.TriggerCall5Enemy();
                }
                break;
        }
    }
}
```

---

## Lưu ý

### ⚠️ Quan trọng

1. **MatchManager phải tồn tại**
   - Đảm bảo đã vào trong trận đấu (không phải menu)
   - Component chỉ hoạt động khi `MatchManager.Current != null`

2. **Call5EnemyManager phải được khởi tạo**
   - Tự động được khởi tạo bởi MatchManager khi tạo trận
   - Nếu null → sẽ có warning log

3. **Trạng thái trận đấu**
   - Super Kick: Chỉ hoạt động khi trận đang Playing
   - Call 5 Enemy: Chỉ hoạt động khi trận đang Playing và không freeze

### 💡 Tips

- Dùng `showDebugLogs = true` khi đang develop
- Test bằng phím T và Y trước khi tích hợp với TikTok API thật
- Kiểm tra Console log để debug

---

## Changelog

### Version 1.1 (2026-02-04)
- ✅ **Vô hiệu hóa input U và I gốc**
- ✅ Giờ chỉ trigger qua TiktokReceiver
- ✅ Tránh conflict input từ nhiều nguồn

### Version 1.0 (2026-02-04)
- ✅ Tạo TiktokReceiver component
- ✅ Hỗ trợ Super Kick event
- ✅ Hỗ trợ Call 5 Enemy event
- ✅ Hỗ trợ command string
- ✅ Auto-cache Call5EnemyManager reference
- ✅ Debug logging
