# 🔧 Debug Guide - Nếu input không hoạt động

> **✅ ĐÃ SỬA**: Tất cả script đã được cập nhật để dùng **Unity's New Input System** (InputSystem package)

## Bước 1: Kiểm tra Console có log không

### ✅ Console phải hiển thị:
```
[SimpleInputTest] ✅ SCRIPT IS ACTIVE!
[TiktokReceiverTest] ===== STARTING =====
```

### ❌ Nếu KHÔNG thấy log:
→ **Nguyên nhân**: Script chưa được add vào scene  
→ **Giải pháp**: Xem Bước 2

---

## Bước 2: Setup Components trong Unity

### 2.1. Tạo GameObject cho SimpleInputTest (Test đơn giản)

1. Mở scene **Stadium** (ví dụ: `Stadium1_Small`)
2. Trong **Hierarchy**, click chuột phải → **Create Empty**
3. Đổi tên thành: **`InputTestHelper`**
4. Select **InputTestHelper**
5. Trong **Inspector** → **Add Component**
6. Tìm và add: **`SimpleInputTest`**
7. ✅ Save scene

### 2.2. Tạo GameObject cho TiktokReceiver

1. Trong **Hierarchy**, click chuột phải → **Create Empty**
2. Đổi tên thành: **`TiktokReceiver`**
3. Select **TiktokReceiver**
4. Trong **Inspector** → **Add Component**
5. Tìm và add: **`TiktokReceiver`** script
6. ✅ Check **Show Debug Logs**
7. ✅ Save scene

### 2.3. Tạo GameObject cho TiktokReceiverTest

1. Trong **Hierarchy**, click chuột phải → **Create Empty**
2. Đổi tên thành: **`TiktokReceiverTest`**
3. Select **TiktokReceiverTest**
4. Trong **Inspector** → **Add Component**
5. Tìm và add: **`TiktokReceiverTest`** script
6. ✅ Check **Show On Screen Instructions**
7. ✅ Save scene

### Kết quả trong Hierarchy:
```
Stadium1_Small
├── MatchManager
├── ...
├── InputTestHelper (SimpleInputTest)
├── TiktokReceiver (TiktokReceiver)
└── TiktokReceiverTest (TiktokReceiverTest)
```

---

## Bước 3: Test trong Unity Editor

### 3.1. Test SimpleInputTest (Test cơ bản)

1. **Play** game
2. Bấm **BẤT KỲ PHÍM NÀO**
3. Kiểm tra **Console** (Window → General → Console)

**✅ Kỳ vọng thấy:**
```
[SimpleInputTest] ⚡⚡⚡ SOME KEY WAS PRESSED!
[SimpleInputTest] ⚡ T KEY PRESSED!
```

**❌ Nếu KHÔNG thấy:**
- Script chưa được add vào GameObject
- GameObject bị disabled
- Console bị filter (bỏ check Collapse)

### 3.2. Test TiktokReceiver (Test đầy đủ)

1. **Play** game
2. **Vào trong trận đấu** (quan trọng!)
3. Bấm **T** hoặc **Y**

**✅ Kỳ vọng thấy:**
```
[TiktokReceiverTest] ⚡ KEY PRESSED: T
[TiktokReceiverTest] ⚡ Triggering Super Kick...
[TiktokReceiver] Received SuperKick event from TikTok
[TiktokReceiver] SuperKick activated!
[MatchManager] Super Kick activated - dimming light
```

---

## Bước 4: Troubleshooting

### ❌ Vấn đề: Không thấy log gì cả

**Nguyên nhân:**
- Script chưa được add vào GameObject
- GameObject bị disabled

**Giải pháp:**
1. Check Hierarchy xem có GameObject `InputTestHelper` không
2. Select GameObject → Check Inspector có component không
3. Check GameObject có tick ✅ enabled không

---

### ❌ Vấn đề: Log "TiktokReceiver NOT FOUND"

**Log:**
```
[TiktokReceiverTest] ❌ TiktokReceiver NOT FOUND in scene!
```

**Nguyên nhân:**
- Chưa tạo GameObject `TiktokReceiver`

**Giải pháp:**
- Làm theo Bước 2.2 ở trên

---

### ❌ Vấn đề: Log "MatchManager is null"

**Log:**
```
[TiktokReceiver] Cannot trigger SuperKick - MatchManager is null
```

**Nguyên nhân:**
- Bạn đang ở menu, chưa vào trong trận đấu

**Giải pháp:**
- Phải **vào trong trận đấu** (trong sân) mới trigger được
- TiktokReceiver chỉ hoạt động khi có MatchManager

---

### ❌ Vấn đề: Log "Call5EnemyManager is null"

**Log:**
```
[TiktokReceiver] Cannot trigger Call5Enemy - Call5EnemyManager is null
```

**Nguyên nhân:**
- Chưa vào trong trận đấu
- Call5EnemyManager chưa được khởi tạo

**Giải pháp:**
- Đợi vài giây sau khi vào trận để MatchManager khởi tạo xong
- Check Console có log `[MatchManager] Call5EnemyManager initialized` không

---

### ❌ Vấn đề: Bấm T/Y nhưng không thấy effect

**Có log nhưng không thấy hiệu ứng:**

**Super Kick:**
- Kiểm tra trận đang Playing (không pause, không freeze)
- Ánh sáng phải tối đi một chút
- Cầu thủ cầm bóng sẽ sút ngay

**Call 5 Enemy:**
- Đợi **5 giây** (có delay)
- UI sẽ hiện lên trong 6 giây
- 5 cầu thủ sẽ spawn ra

---

## Bước 5: Checklist hoàn chỉnh

Kiểm tra tất cả các bước sau:

### Setup:
- [ ] Đã tạo GameObject `InputTestHelper` với component `SimpleInputTest`
- [ ] Đã tạo GameObject `TiktokReceiver` với component `TiktokReceiver`
- [ ] Đã tạo GameObject `TiktokReceiverTest` với component `TiktokReceiverTest`
- [ ] Đã check ✅ "Show Debug Logs" và "Show On Screen Instructions"
- [ ] Đã Save scene

### Test:
- [ ] Chạy game và thấy log "SCRIPT IS ACTIVE"
- [ ] Bấm bất kỳ phím nào và thấy log "SOME KEY WAS PRESSED"
- [ ] Bấm T và thấy log "T KEY PRESSED"
- [ ] Vào trong trận đấu (trong sân)
- [ ] Bấm T và thấy log "SuperKick activated"
- [ ] Bấm Y và thấy log "Call5Enemy triggered"

---

## Bước 6: Test nhanh bằng code

Nếu vẫn không hoạt động, tạo script test đơn giản:

```csharp
using UnityEngine;
using FStudio.MatchEngine;

public class QuickTest : MonoBehaviour {
    void Update() {
        if (Input.GetKeyDown(KeyCode.T)) {
            Debug.Log("T pressed!");
            
            if (MatchManager.Current != null) {
                MatchManager.Current.SetSuperKick(true);
                Debug.Log("SuperKick triggered!");
            } else {
                Debug.LogError("MatchManager is null - are you in a match?");
            }
        }
    }
}
```

Add script này vào bất kỳ GameObject nào và test.

---

## Liên hệ Debug

Nếu vẫn không hoạt động, hãy cung cấp:

1. **Screenshot Hierarchy** (để xem GameObject)
2. **Screenshot Console** (để xem log)
3. **Bạn đang ở đâu**: Menu hay trong trận?
4. **Build target**: PC, WebGL, hay Mobile?

---

## Tips

💡 Nhấn **Ctrl + Shift + C** để mở Console  
💡 Bỏ check **Collapse** trong Console để thấy tất cả log  
💡 Check **Error Pause** để game dừng khi có lỗi  
💡 Dùng **SimpleInputTest** để test input cơ bản trước  

---

Chúc may mắn! 🚀
