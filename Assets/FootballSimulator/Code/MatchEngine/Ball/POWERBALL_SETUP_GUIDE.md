# Power Ball Visual Effect - Hướng dẫn Setup

## 📋 Tổng quan
Script `BallPowerVisualController.cs` tự động thay đổi visual của bóng thành "power ball" khi phát hiện cú sút mạnh (dựa trên velocity).

## 🚀 Cách Setup

### Bước 1: Mở Ball Prefab
1. Vào `Assets/FootballSimulator/Code/MatchEngine/Ball.prefab`
2. Hoặc tìm Ball object trong Scene khi đang chạy game

### Bước 2: Add Component
1. Select Ball GameObject
2. Click "Add Component"
3. Tìm và add "Ball Power Visual Controller"

### Bước 3: Configure Inspector

#### **Power Ball Settings:**
- **Power Ball Prefab**: Kéo prefab `Assets/FootballSimulator/powerBall.prefab` vào đây
- **Power Threshold**: Ngưỡng velocity để kích hoạt power ball (mặc định: 25)
  - Giá trị càng thấp = power ball xuất hiện càng dễ
  - Test với giá trị 20-30 để tìm balance phù hợp
- **Deactivate Velocity**: Velocity để tắt power ball (mặc định: 5)
  - Khi bóng chậm lại dưới giá trị này → trở về visual bình thường

#### **References:**
- **Normal Ball Visual**: 
  - Script sẽ tự động tìm (child đầu tiên của BallRendererPoint)
  - Nếu không tìm được, kéo child object chứa visual bóng hiện tại vào đây
  - Thường là `BallGraphic1(Clone)` hoặc `BallGraphic2(Clone)`

#### **Debug:**
- **Enable Debug Logs**: Bật để xem logs chi tiết (khuyến nghị bật lúc test)

### Bước 4: Save Prefab
1. Apply changes vào prefab
2. Hoặc save scene nếu bạn đang modify Ball object trong scene

## 🎮 Test

### Cách test trong game:
1. Start match
2. Sút bóng mạnh (long shot, power shot)
3. Xem Console logs:
   ```
   [BallPowerVisualController] Powerful shot detected! Power: 28.5 >= 25. Activating power ball.
   [BallPowerVisualController] Normal ball visual hidden
   [BallPowerVisualController] Power ball instantiated
   ```
4. Khi bóng chậm lại, power ball sẽ tự động tắt

### Test manual (không cần sút):
1. Chọn Ball object trong Hierarchy khi game đang chạy
2. Tìm component "Ball Power Visual Controller"
3. Click button "Test Activate Power Ball" hoặc "Test Deactivate Power Ball"

## ⚙️ Điều chỉnh

### Để power ball xuất hiện thường xuyên hơn:
- Giảm **Power Threshold** xuống (ví dụ: 20 hoặc 15)

### Để power ball tồn tại lâu hơn:
- Giảm **Deactivate Velocity** xuống (ví dụ: 3 hoặc 2)

### Để tìm giá trị velocity phù hợp:
1. Bật **Enable Debug Logs**
2. Chơi game và xem logs khi sút bóng
3. Logs sẽ hiện: `Power: XX.XX`
4. Dùng giá trị đó để điều chỉnh threshold

## 🔍 Troubleshooting

### Power ball không xuất hiện:
- ✅ Check Console có log `Powerful shot detected!` không?
  - Nếu có log `Normal shot` → Tăng power threshold
  - Nếu không có log nào → Check component có enabled không
- ✅ Check Power Ball Prefab đã assign chưa?
- ✅ Check Ball.Current có null không? (xem Console logs khi Start)

### Power ball bị duplicate:
- Script tự động handle việc này, không spawn nhiều lần
- Nếu vẫn bị → Check có nhiều BallPowerVisualController trên cùng object không

### Normal ball không hiện lại:
- Check **Normal Ball Visual** reference có đúng không
- Check Console logs xem có error khi deactivate không

## 📊 Cách hoạt động

```
PlayerShootEvent triggered
         ↓
Check shootEvent.Power >= powerThreshold?
         ↓ YES
Hide normalBallVisual
         ↓
Spawn powerBallPrefab as child of ballAssetPoint
         ↓
Every Update: Check ball.Velocity.magnitude
         ↓
velocity < deactivateVelocity?
         ↓ YES
Destroy powerBallInstance
         ↓
Show normalBallVisual again
```

## 🎨 Customize PowerBall Prefab

File: `Assets/FootballSimulator/powerBall.prefab`

Bạn có thể:
- Thêm Particle Systems
- Thêm Trail Renderer
- Thêm Light effects
- Thêm Animation
- Thay đổi Material/Shader

## 📝 Notes

- Script này **không** ảnh hưởng đến physics của bóng
- Chỉ thay đổi visual/graphics
- Tương thích với hệ thống BallLoader hiện có
- Không cần modify code khác
