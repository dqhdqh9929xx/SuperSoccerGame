# 🎮 TikTok Receiver - Setup Guide (5 phút)

## Bước 1: Tạo GameObject TiktokReceiver

1. Mở scene **Stadium** (ví dụ: `Stadium1_Small`)
2. Click chuột phải vào **Hierarchy** → **Create Empty**
3. Đổi tên thành **`TiktokReceiver`**
4. Đặt vị trí cùng level với **MatchManager** (cho dễ tìm)

## Bước 2: Add Component

1. Select **TiktokReceiver** GameObject
2. Click **Add Component**
3. Tìm và add: **`TiktokReceiver`** script
4. ✅ Check **Show Debug Logs** (để xem log)

## Bước 3: Setup Test Script (Optional - để test)

1. Select **TiktokReceiver** GameObject (hoặc GameObject khác)
2. Click **Add Component**
3. Tìm và add: **`TiktokReceiverTest`** script
4. ✅ Check **Show On Screen Instructions**

## Bước 4: Test trong Unity

1. **Play** game
2. Vào trong trận đấu
3. Test các phím:
   - **T** → Trigger Super Kick
   - **Y** → Trigger Call 5 Enemy
   - **1** → Test command "superkick"
   - **2** → Test command "call5enemy"

4. Kiểm tra **Console** để xem log:
   ```
   [TiktokReceiver] SuperKick activated!
   [TiktokReceiver] Call5Enemy triggered!
   ```

> **📝 Lưu ý**: Phím **U** và **I** gốc đã bị vô hiệu hóa. Giờ chỉ trigger qua **TiktokReceiver** (phím T và Y).

---

## ✅ Checklist hoàn thành

- [ ] Đã tạo GameObject `TiktokReceiver`
- [ ] Đã add component `TiktokReceiver` script
- [ ] Đã test bấm phím T và Y trong trận đấu
- [ ] Thấy log trong Console
- [ ] Super Kick hoạt động (ánh sáng tối đi)
- [ ] Call 5 Enemy hoạt động (spawn 5 cầu thủ)

---

## 🎯 Các cách sử dụng

### Cách 1: Gọi từ C# Script

```csharp
TiktokReceiver receiver = FindObjectOfType<TiktokReceiver>();
receiver.TriggerSuperKick();
receiver.TriggerCall5Enemy();
```

### Cách 2: Gọi từ UI Button

1. Tạo UI Button
2. OnClick event:
   - Add `TiktokReceiver` object
   - Chọn `TriggerSuperKick()` hoặc `TriggerCall5Enemy()`

### Cách 3: Gọi từ WebGL (JavaScript)

```javascript
SendMessage('TiktokReceiver', 'TriggerSuperKick');
SendMessage('TiktokReceiver', 'TriggerCall5Enemy');
```

### Cách 4: Dùng Command String

```csharp
receiver.OnTikTokCommand("superkick");
receiver.OnTikTokCommand("call5enemy");
```

---

## 🔥 Tích hợp TikTok Stream

Tạo script mới `TikTokIntegration.cs`:

```csharp
using UnityEngine;

public class TikTokIntegration : MonoBehaviour {
    private TiktokReceiver receiver;
    
    void Start() {
        receiver = FindObjectOfType<TiktokReceiver>();
    }
    
    // Được gọi từ TikTok API của bạn
    public void OnTikTokGift(string giftName, int count) {
        Debug.Log($"TikTok Gift: {giftName} x{count}");
        
        // Map gift → feature
        if (giftName == "Rose" && count >= 10) {
            receiver.TriggerSuperKick();
        }
        else if (giftName == "Heart" && count >= 5) {
            receiver.TriggerCall5Enemy();
        }
    }
    
    // Được gọi từ TikTok comment
    public void OnTikTokComment(string comment) {
        Debug.Log($"TikTok Comment: {comment}");
        
        // Map comment → feature
        receiver.OnTikTokCommand(comment);
    }
}
```

---

## 📝 Lưu ý quan trọng

### ⚠️ Điều kiện hoạt động

1. **MatchManager phải tồn tại**
   - Chỉ hoạt động khi đã vào trong trận đấu
   - Không hoạt động ở menu

2. **Trạng thái trận đấu**
   - Phải ở trạng thái **Playing** (đang thi đấu)
   - Call5Enemy: không hoạt động khi đang freeze

### 💡 Tips

- Dùng `showDebugLogs = true` để debug
- Test bằng phím T/Y trước khi tích hợp TikTok API
- Kiểm tra Console log nếu có lỗi

---

## 🐛 Troubleshooting

### Vấn đề: Không thấy log

**Giải pháp**:
- Đảm bảo `Show Debug Logs` được check
- Mở Console: Window → General → Console

### Vấn đề: TiktokReceiver không tìm thấy Call5EnemyManager

**Giải pháp**:
- Đảm bảo đã vào trong trận đấu (không ở menu)
- MatchManager sẽ tự động tạo Call5EnemyManager

### Vấn đề: Super Kick không hoạt động

**Giải pháp**:
- Kiểm tra trận đấu đang ở trạng thái Playing
- Xem Console có warning không

### Vấn đề: Call 5 Enemy không spawn cầu thủ

**Giải pháp**:
- Đợi 5 giây sau khi trigger (có delay)
- Kiểm tra trận đấu không đang freeze
- Đảm bảo Call5EnemyUI đã được tạo trong scene

---

## 📚 Tài liệu chi tiết

Xem file **TIKTOK_RECEIVER_README.md** để biết thêm chi tiết về:
- API Reference đầy đủ
- Các ví dụ tích hợp
- Advanced usage

---

Chúc bạn thành công! 🎉
