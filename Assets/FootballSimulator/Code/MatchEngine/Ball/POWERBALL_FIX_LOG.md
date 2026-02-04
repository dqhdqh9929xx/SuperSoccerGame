# Power Ball Fix Log

## ⚠️ Vấn đề ban đầu (từ log của bạn)

Log hiển thị:
```
[BallPowerVisualController] Powerful shot detected! Power: 38.10 >= 5
[BallPowerVisualController] Power ball instantiated: powerBall(Clone)
[BallPowerVisualController] Deactivating power ball. Velocity: 0.00 < 5
[BallPowerVisualController] Power ball destroyed
```

**NHƯNG không có dòng:**
- `Normal ball visual hidden`
- `Normal ball visual restored`

**Và hình ảnh không thay đổi!**

## 🔍 Nguyên nhân

1. **Normal ball visual = null**: Ball visual được load **SAU** khi Ball object được instantiate, nên `Start()` không tìm thấy children.

2. **Power ball tồn tại quá ngắn**: GK bắt bóng ngay lập tức → velocity về 0 → power ball bị deactivate trong vài frame (không nhìn thấy được).

## ✅ Các fix đã apply

### Fix 1: Dynamic ball visual detection
- Không tìm ball visual trong `Start()` nữa
- Tìm ball visual **ngay trước khi activate** power ball
- Thêm warning logs nếu không tìm thấy

### Fix 2: Minimum active time
- Thêm parameter `minActiveTime` (default: 0.3 giây)
- Power ball sẽ **tồn tại tối thiểu 0.3 giây** trước khi có thể deactivate
- Giúp người chơi nhìn thấy power ball ngay cả khi bóng bị bắt ngay

### Fix 3: Deactivate khi ball được hold
- Check `ball.HolderPlayer != null`
- Deactivate ngay khi bóng được cầu thủ bắt giữ
- Tránh power ball vẫn hiển thị khi ball đã dừng

### Fix 4: Scale matching
- Power ball sẽ copy scale từ normal ball visual
- Đảm bảo kích thước phù hợp

### Fix 5: More debug logs
- Log position, scale, layer của power ball
- Log thời gian active
- Dễ dàng troubleshoot

## 🧪 Test lại

Khi test lần này, bạn sẽ thấy logs:

```
[BallPowerVisualController] Auto-found normal ball visual: BallGraphic1(Clone)
[BallPowerVisualController] Powerful shot detected! Power: 38.10 >= 25
[BallPowerVisualController] Normal ball visual hidden          ← MỚI!
[BallPowerVisualController] Power ball instantiated: powerBall(Clone)
[BallPowerVisualController] Power ball world position: (x, y, z)  ← MỚI!
[BallPowerVisualController] Power ball local scale: (1, 1, 1)     ← MỚI!
[BallPowerVisualController] Deactivating power ball. Ball is held by player. ← MỚI!
[BallPowerVisualController] Normal ball visual restored         ← MỚI!
[BallPowerVisualController] Power ball destroyed
```

## 🎮 Settings trong Inspector

Bây giờ bạn có thêm parameter:

**Min Active Time**: `0.3` (giây)
- Tăng lên nếu muốn power ball hiển thị lâu hơn
- Giảm xuống nếu muốn nhanh hơn
- Khuyến nghị: 0.2 - 0.5 giây

## 🔧 Nếu vẫn không thấy power ball

### Check 1: Xem logs
Tìm dòng:
- `Auto-found normal ball visual: XXX` ← Phải có!
- `Normal ball visual hidden` ← Phải có!
- `Power ball world position` ← Check vị trí có hợp lý không

### Check 2: PowerBall prefab
1. Mở `Assets/FootballSimulator/powerBall.prefab`
2. Check:
   - Prefab có MeshRenderer/SkinnedMeshRenderer không?
   - Material có assigned không?
   - Layer có đúng không? (nên để layer 8 - Ball layer)
   - Có bị hidden trong prefab không?

### Check 3: Camera culling mask
- Check camera có render layer 8 (Ball) không?
- Xem trong log: `Power ball active: True/False, layer: X`

### Check 4: Scale
- Xem log: `Power ball local scale: (x, y, z)`
- Nếu scale = (0,0,0) → prefab bị lỗi
- Nếu scale quá nhỏ → tăng scale trong prefab

### Check 5: Timing
- Thử tăng `minActiveTime` lên 1.0 giây
- Test xem power ball có xuất hiện không
- Nếu có → timing là vấn đề, điều chỉnh về 0.3-0.5

## 🎨 Customize Power Ball

### Để power ball nổi bật hơn:
1. Thêm **Particle System** (trail, sparks, aura)
2. Thêm **Trail Renderer** (đuôi sáng)
3. Thêm **Light component** (ánh sáng)
4. Thêm **Animation** (xoay, scale pulse)
5. Dùng **Emissive material** (phát sáng)

### Ví dụ setup trong powerBall prefab:
```
powerBall (GameObject)
├── Ball Model (MeshRenderer) - material emissive
├── Particle System - sparks/trail
├── Point Light - color cyan, intensity 2
└── Trail Renderer - gradient material
```

## 📊 Expected behavior

1. **Sút thường** (power < 25):
   - Power ball KHÔNG xuất hiện
   - Log: `Normal shot. Power: XX.XX < 25`

2. **Sút mạnh** (power >= 25):
   - Power ball xuất hiện NGAY LẬP TỨC
   - Bóng bình thường biến mất
   - Power ball tồn tại tối thiểu 0.3 giây
   - Khi bóng bị bắt hoặc chậm lại → power ball biến mất
   - Bóng bình thường xuất hiện lại

## 🚀 Next steps

1. Test lại trong Unity
2. Xem Console logs
3. Nếu vẫn không thấy → gửi logs mới cho tôi
4. Nếu thấy rồi → điều chỉnh parameters cho phù hợp
