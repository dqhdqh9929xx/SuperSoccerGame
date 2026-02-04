# Hướng dẫn tạo UI cho tính năng Call5Enemy

## Bước 1: Tạo Canvas

1. Trong Unity Editor, mở scene **Stadium1_Small** (hoặc scene Stadium khác)
2. Trong Hierarchy, click chuột phải → **UI → Canvas**
3. Đổi tên Canvas thành **`Call5EnemyUI`** (chính xác, phân biệt chữ hoa/thường)

## Bước 2: Cấu hình Canvas

### Canvas Component:
- **Render Mode:** Screen Space - Overlay
- **Pixel Perfect:** ✅ (tùy chọn)
- **Sort Order:** 100 (để hiển thị trên các UI khác)

### Canvas Scaler:
- **UI Scale Mode:** Scale With Screen Size
- **Reference Resolution:** 1920 x 1080 (hoặc theo thiết kế của bạn)
- **Match:** 0.5 (Width/Height)

## Bước 3: Thêm Panel nền

1. Click chuột phải vào **Call5EnemyUI** → **UI → Panel**
2. Đổi tên thành **Background**
3. Cấu hình:
   - **Color:** Đen với Alpha = 0.7 (để tạo overlay tối)
   - **Anchor:** Stretch (căng full màn hình)

## Bước 4: Thêm Text thông báo

1. Click chuột phải vào **Call5EnemyUI** → **UI → Text - TextMeshPro** (hoặc **Text** nếu không dùng TMP)
2. Đổi tên thành **MessageText**
3. Cấu hình:

### TextMeshPro (nếu dùng):
- **Text:** "⚠️ CALLING 5 ENEMY PLAYERS!\n🔒 All Players Frozen!"
- **Font Size:** 48-60
- **Alignment:** Center/Middle
- **Color:** Vàng (#FFFF00) hoặc Đỏ (#FF0000)
- **Outline:** ✅ Enable (màu đen, độ dày 0.2)
- **Position:** Center (0, 50, 0) - hơi lệch lên trên

### Regular Text (nếu không dùng TMP):
- **Text:** "CALLING 5 ENEMY PLAYERS!\nALL PLAYERS FROZEN!"
- **Font Size:** 36-48
- **Alignment:** Center/Middle
- **Color:** Vàng hoặc Đỏ
- **Best Fit:** ✅ (để tự động scale)

## Bước 5: Thêm Icon/Image (Tùy chọn)

1. Click chuột phải vào **Call5EnemyUI** → **UI → Image**
2. Đổi tên thành **WarningIcon**
3. Cấu hình:
   - **Sprite:** Icon cảnh báo (⚠️) hoặc icon cầu thủ
   - **Position:** Center (0, 150, 0) - phía trên text
   - **Size:** 128 x 128 hoặc tùy chỉnh
   - **Color:** Vàng hoặc Đỏ

## Bước 6: Thêm Animation (Tùy chọn nâng cao)

### Tạo animation nhấp nháy:
1. Select **MessageText**
2. Window → Animation → Animation
3. Click **Create** → Lưu tên **BlinkAnimation.anim**
4. Thêm keyframes:
   - 0.0s: Alpha = 1
   - 0.5s: Alpha = 0.3
   - 1.0s: Alpha = 1
5. Set **Loop:** ✅

### Tạo animation scale:
1. Select **Call5EnemyUI**
2. Tạo animation **ScaleIn.anim**
3. Thêm keyframes cho Scale:
   - 0.0s: Scale = (0, 0, 0)
   - 0.3s: Scale = (1.1, 1.1, 1)
   - 0.4s: Scale = (1, 1, 1)

## Bước 7: Test

1. **Trong Unity Editor:**
   - Chạy scene
   - Trong Hierarchy, tìm **Call5EnemyUI**
   - Tắt/bật Active để xem UI

2. **Trong Game:**
   - Chạy trận đấu
   - Bấm phím **I**
   - Đợi 5 giây
   - UI sẽ hiện trong 6 giây rồi tự động tắt

## Ví dụ Hierarchy Structure

```
Call5EnemyUI (Canvas)
├── Background (Panel - tối overlay)
├── WarningIcon (Image - icon cảnh báo)
├── MessageText (TextMeshPro - thông báo)
└── CountdownText (Text - đếm ngược) [Tùy chọn]
```

## Tips thiết kế UI đẹp

### Màu sắc:
- **Background:** Đen với Alpha 0.6-0.8
- **Text chính:** Vàng (#FFD700) hoặc Đỏ (#FF3333)
- **Outline/Shadow:** Đen để tạo độ tương phản
- **Icon:** Vàng hoặc Đỏ matching với text

### Font:
- Sử dụng font **bold** để dễ đọc
- TextMeshPro cho chất lượng tốt hơn
- Font size đủ lớn để dễ nhìn (48-60)

### Effects:
- Thêm **Drop Shadow** để text nổi bật
- Thêm **Outline** màu đen độ dày 0.2-0.3
- Animation nhấp nháy để thu hút sự chú ý
- Scale animation khi xuất hiện (pop-in effect)

### Layout:
- Căn giữa màn hình (Center/Middle)
- Text ở giữa, icon ở trên
- Để khoảng trống xung quanh (padding)

## Troubleshooting

### Vấn đề: UI không hiện
✅ Kiểm tra tên GameObject là chính xác **`Call5EnemyUI`**
✅ Kiểm tra Canvas Sort Order đủ cao
✅ Kiểm tra Console log có warning không

### Vấn đề: UI bị che bởi UI khác
✅ Tăng **Canvas Sort Order** lên cao hơn (100+)
✅ Kiểm tra **Render Mode** là Screen Space - Overlay

### Vấn đề: Text bị mờ hoặc vỡ
✅ Dùng **TextMeshPro** thay vì Text thường
✅ Tăng **Font Size**
✅ Enable **Best Fit** nếu dùng Text thường

## Mẫu thiết kế gợi ý

### Style 1: Minimalist
- Nền đen mờ
- Text vàng lớn
- Không có icon

### Style 2: Warning Style
- Nền đỏ/vàng mờ
- Text trắng với outline đen
- Icon cảnh báo lớn

### Style 3: Gaming Style
- Nền gradient
- Text với glow effect
- Animation pulsing
- Sound effects

Chúc bạn thiết kế UI đẹp! 🎨⚽
