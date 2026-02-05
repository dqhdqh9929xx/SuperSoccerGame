# ⚡ QUICK FIX - Setup trong 1 phút

> **⚠️ QUAN TRỌNG**: Project này dùng **Unity's New Input System**. Tất cả script đã được cập nhật để tương thích!

## Bước 1: Tạo 1 GameObject duy nhất (30 giây)

1. Mở scene **Stadium1_Small** (hoặc scene stadium bạn đang dùng)
2. Click chuột phải vào **Hierarchy** → **Create Empty**
3. Đổi tên thành: **`DebugHelper`**

## Bước 2: Add 4 components (30 giây)

Select **DebugHelper**, trong **Inspector** click **Add Component** và add lần lượt:

1. **`SimpleInputTest`** ✅
2. **`TiktokReceiver`** ✅
3. **`TiktokReceiverTest`** ✅
4. **`DebugStatusDisplay`** ✅

## Bước 3: Check settings

- TiktokReceiver: ✅ **Show Debug Logs**
- TiktokReceiverTest: ✅ **Show On Screen Instructions**
- DebugStatusDisplay: ✅ **Show Debug Panel**

## Bước 4: Save và Play

1. **Ctrl + S** để save scene
2. **Ctrl + P** để play game
3. Vào trong trận đấu

## Bước 5: Test

Bấm **F1** để hiện debug panel (hiển thị status của tất cả components)

Bấm **T** → Super Kick  
Bấm **Y** → Call 5 Enemy  
Bấm **U** → Add Heart (+10, test TikTok viewer)  
  - Khi đủ 100 heart → Auto trigger Super Kick  

---

## ✅ Kỳ vọng thấy

### Trong Console:
```
[SimpleInputTest] ✅ SCRIPT IS ACTIVE!
[DebugStatusDisplay] ✅ Started!
[TiktokReceiverTest] ✅ TiktokReceiver FOUND!
[SimpleInputTest] ⚡ T KEY PRESSED!
[TiktokReceiver] SuperKick activated!
```

### Trên màn hình:
- Góc trái trên: Hướng dẫn phím
- Bấm F1: Debug panel với status đầy đủ

---

## ❌ Nếu vẫn không hoạt động

### Console không có log gì:
→ Bạn chưa add component vào GameObject  
→ Kiểm tra lại Bước 2

### Log "NOT IN MATCH":
→ Bạn đang ở menu  
→ **Phải vào trong trận đấu** (trong sân) mới test được

### Log "TiktokReceiver NOT FOUND":
→ Component chưa được add  
→ Kiểm tra Inspector của GameObject `DebugHelper`

---

## 🎯 Kết quả cuối cùng

Hierarchy của bạn sẽ có:

```
Stadium1_Small
├── MatchManager
├── ...
└── DebugHelper
    ├── SimpleInputTest (Component)
    ├── TiktokReceiver (Component)
    ├── TiktokReceiverTest (Component)
    └── DebugStatusDisplay (Component)
```

---

## 💡 Tips

- **F1**: Bật/tắt debug panel
- **Console** (Ctrl+Shift+C): Xem log chi tiết
- **Bỏ Collapse** trong Console để thấy tất cả log

---

Nếu vẫn không hoạt động → Xem **DEBUG_GUIDE.md** cho hướng dẫn chi tiết!
