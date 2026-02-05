# 🎮 TikTok WebSocket Unity Integration

## 📦 Files Created

### Unity Scripts
- **`TiktokWebSocketClient.cs`** - Main WebSocket client cho Unity
- **`TiktokReceiver.cs`** - Event receiver (đã có sẵn)
- **`TiktokHeartManager.cs`** - Quản lý heart taps (đã cập nhật)
- **`TiktokReceiverTest.cs`** - Test script (đã cập nhật)

### Server Files
- **`tiktok-server-example.js`** - Production server với TikTok Live
- **`test-websocket-server.js`** - Test server (không cần TikTok)
- **`package.json`** - Node.js dependencies

### Documentation
- **`TIKTOK_WEBSOCKET_GUIDE.md`** - Hướng dẫn chi tiết
- **`WebSocketTestMessages.json`** - Example JSON messages
- **`README_WEBSOCKET.md`** - File này

---

## 🚀 Quick Start

### Bước 1: Setup Server

```bash
# Navigate to FootballSimulator folder
cd Assets/FootballSimulator

# Install dependencies
npm install

# Test với mock server (không cần TikTok)
npm test

# Hoặc chạy với TikTok Live (cần sửa username trước)
npm start
```

### Bước 2: Setup Unity

1. **Cài đặt NativeWebSocket Package**
   - Unity → Window → Package Manager
   - Add package from git URL: `https://github.com/endel/NativeWebSocket.git#upm`

2. **Tạo GameObject trong Scene**
   - Tên: `TiktokWebSocketManager`
   - Add components:
     - TiktokWebSocketClient
     - TiktokReceiver  
     - TiktokHeartManager
     - TiktokReceiverTest

3. **Configure TiktokWebSocketClient**
   - Server URL: `ws://localhost:8080`
   - Auto Connect: ✓
   - Test Mode: ✓ (cho testing)
   - Show Debug Logs: ✓

4. **Link UI References trong TiktokReceiverTest**
   - `textCountHeart` → TextMeshProUGUI (hiển thị số hearts)
   - `currentNameSuperKick` → TextMeshProUGUI (hiển thị tên winner)

### Bước 3: Test

1. **Start test server:**
   ```bash
   npm test
   ```

2. **Play Unity**
   - Xem Console logs
   - Server sẽ tự động gửi test messages

3. **Kiểm tra events:**
   - 💖 Like → Tăng heart count
   - 🌹 Rose Gift → Super Kick + hiển thị tên
   - 💐 Perfume Gift → Call5Enemy (spawn AI)

---

## 📊 Event Flow

```
TikTok Live Viewer
      ↓
   (Action)
      ↓
┌─────────────────┐
│  WebSocket      │
│  Server         │ ← tiktok-server-example.js
│  (Node.js)      │
└─────────────────┘
      ↓ ws://localhost:8080
      ↓
┌─────────────────┐
│ Unity Game      │
│ WebSocketClient │ ← TiktokWebSocketClient.cs
└─────────────────┘
      ↓
   (Parse JSON)
      ↓
    ┌─────┬─────────┬──────────┐
    ↓     ↓         ↓          ↓
  Like   Rose    Perfume    Other
    ↓     ↓         ↓
 Heart  Super    Call5
 Manager Kick   Enemy
```

---

## 🎯 Event Mapping

| TikTok Action | JSON Message | Unity Result |
|--------------|--------------|--------------|
| 💖 Tim live | `{"type":"like","userName":"X"}` | Add to heart array |
| 🌹 Tặng Rose | `{"type":"gift","giftName":"Rose","userName":"X"}` | Super Kick + Display name |
| 💐 Tặng Perfume | `{"type":"gift","giftName":"Perfume","userName":"X"}` | Call5Enemy (anonymous) |

---

## 🧪 Testing Modes

### Mode 1: Offline Testing (No Server)
Unity Editor với các phím tắt:
- **T** → Test Super Kick
- **Y** → Test Call5Enemy  
- **U** → Test Like (random user)

### Mode 2: Mock Server Testing
```bash
npm test  # Chạy test-websocket-server.js
```
Server tự động gửi random events → Unity nhận và xử lý

### Mode 3: TikTok Live Testing
```bash
# Sửa username trong tiktok-server-example.js
const TIKTOK_USERNAME = 'your_username_here';

# Chạy
npm start

# Bắt đầu live stream
# Viewers tap tim/tặng quà → Game nhận events
```

---

## 📝 Troubleshooting

### ❌ "NativeWebSocket not found"
```
Solution: Cài package qua Package Manager
URL: https://github.com/endel/NativeWebSocket.git#upm
```

### ❌ "Connection refused"
```
Solution:
1. Check server đã chạy: npm test
2. Check URL: ws://localhost:8080 (không phải wss://)
3. Check firewall settings
```

### ❌ "Failed to connect to TikTok"
```
Solution:
1. Check username đúng chưa
2. Check user có đang live không
3. Thử lại sau vài giây
```

### ❌ Events không được trigger
```
Solution:
1. Check Unity Console logs (bật showDebugLogs)
2. Check server logs
3. Verify JSON format từ server
```

---

## 📚 Documentation

Chi tiết đầy đủ xem: **`TIKTOK_WEBSOCKET_GUIDE.md`**

---

## 🔄 Version History

**v1.0.0** (2026-02-04)
- ✅ WebSocket integration
- ✅ Like event support
- ✅ Rose Gift → Super Kick
- ✅ Perfume Gift → Call5Enemy
- ✅ Auto-reconnect
- ✅ Test modes
- ✅ Full documentation

---

## 💡 Tips

1. **Development**: Dùng test server (npm test) để dev nhanh
2. **Production**: Dùng TikTok server (npm start) khi ready
3. **Debug**: Bật showDebugLogs trong Unity để xem chi tiết
4. **Performance**: Nếu quá nhiều events, có thể thêm throttling

---

## 📞 Need Help?

1. Check Unity Console logs
2. Check server terminal logs  
3. Read TIKTOK_WEBSOCKET_GUIDE.md
4. Verify JSON format in WebSocketTestMessages.json

Good luck! 🎉🎮
