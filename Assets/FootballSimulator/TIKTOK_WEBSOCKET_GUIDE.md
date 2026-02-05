# 🎮 Hướng Dẫn Tích Hợp TikTok WebSocket với Unity

## 📋 Tổng Quan

Hệ thống này cho phép game Unity nhận events từ TikTok Live thông qua WebSocket và kích hoạt các tính năng trong game.

### Các Event Được Hỗ Trợ

1. **Like Event (Tim Live)** 💖
   - Người xem tap tim trên live → Tên user được thêm vào mảng
   - Khi đủ 100 tim → Random 1 người trigger Super Kick

2. **Rose Gift Event** 🌹
   - Người tặng quà Rose → Trigger Super Kick ngay lập tức
   - Tên người tặng hiển thị trên UI

3. **Perfume Gift Event** 💐
   - Người tặng quà Perfume → Trigger Call5Enemy (spawn 5 AI)
   - Không hiển thị tên (ẩn danh)

---

## 🔧 Setup trong Unity

### 1. Cài Đặt Dependencies

#### Option A: NativeWebSocket (Recommended)
```bash
# Cài qua Unity Package Manager
# Window → Package Manager → Add package from git URL:
https://github.com/endel/NativeWebSocket.git#upm
```

#### Option B: WebSocketSharp
Download từ: https://github.com/sta/websocket-sharp
Import vào project Unity

### 2. Tạo GameObject trong Scene

1. **Tạo GameObject mới**: `TiktokWebSocketManager`
2. **Add các components:**
   - `TiktokWebSocketClient`
   - `TiktokReceiver`
   - `TiktokHeartManager`
   - `TiktokReceiverTest`

### 3. Configure trong Inspector

#### TiktokWebSocketClient Settings:
```
Server Url: ws://localhost:8080
Auto Connect: ✓ (checked)
Reconnect Delay: 5
Max Reconnect Attempts: -1 (vô hạn)

Gift Identifiers:
- Rose Gift Identifier: "Rose"
- Perfume Gift Identifier: "Perfume"

Debug:
- Show Debug Logs: ✓
- Test Mode: ✓ (khi test offline)
```

#### TiktokReceiverTest Settings:
```
UI References:
- textCountHeart: [Link TextMeshProUGUI hiển thị số heart]
- currentNameSuperKick: [Link TextMeshProUGUI hiển thị tên winner]
```

---

## 📡 JSON Message Format

Server cần gửi messages theo format sau:

### 1. Like Event
```json
{
    "type": "like",
    "userName": "NguyenVanA",
    "likeCount": 1
}
```

### 2. Rose Gift Event
```json
{
    "type": "gift",
    "userName": "TranThiB",
    "giftName": "Rose",
    "giftId": 5655
}
```

### 3. Perfume Gift Event
```json
{
    "type": "gift",
    "userName": "LeVanC",
    "giftName": "Perfume",
    "giftId": 5658
}
```

---

## 🖥️ Server Implementation

### Node.js Example với TikTok Live Connector

#### 1. Cài đặt packages
```bash
npm install ws tiktok-live-connector
```

#### 2. Server Code (`server.js`)
```javascript
const WebSocket = require('ws');
const { WebcastPushConnection } = require('tiktok-live-connector');

// Tạo WebSocket server
const wss = new WebSocket.Server({ port: 8080 });
console.log('🚀 WebSocket server running on ws://localhost:8080');

// Kết nối TikTok Live
const tiktokUsername = 'your_tiktok_username'; // Thay bằng username TikTok của bạn
let tiktokConnection = new WebcastPushConnection(tiktokUsername);

// Connect to TikTok
tiktokConnection.connect().then(state => {
    console.log(`✅ Connected to @${state.roomInfo.owner.uniqueId} live!`);
}).catch(err => {
    console.error('❌ Failed to connect:', err);
});

// ===== LISTEN TIKTOK EVENTS =====

// Event: Like (Tim)
tiktokConnection.on('like', data => {
    console.log(`💖 ${data.uniqueId} liked ${data.likeCount} times`);
    
    // Gửi tới Unity
    broadcast({
        type: 'like',
        userName: data.uniqueId,
        likeCount: data.likeCount
    });
});

// Event: Gift (Quà)
tiktokConnection.on('gift', data => {
    console.log(`🎁 ${data.uniqueId} sent ${data.giftName} (x${data.repeatCount})`);
    
    // Gửi tới Unity (mỗi gift 1 message)
    for (let i = 0; i < data.repeatCount; i++) {
        broadcast({
            type: 'gift',
            userName: data.uniqueId,
            giftName: data.giftName,
            giftId: data.giftId
        });
    }
});

// Event: Share
tiktokConnection.on('share', data => {
    console.log(`📤 ${data.uniqueId} shared the stream!`);
});

// Event: Follow
tiktokConnection.on('follow', data => {
    console.log(`➕ ${data.uniqueId} followed!`);
});

// ===== WEBSOCKET SERVER =====

wss.on('connection', (ws) => {
    console.log('🔌 Unity client connected!');
    
    ws.on('close', () => {
        console.log('🔌 Unity client disconnected');
    });
    
    ws.on('error', (error) => {
        console.error('❌ WebSocket error:', error);
    });
});

// Broadcast message tới tất cả Unity clients
function broadcast(message) {
    const jsonMessage = JSON.stringify(message);
    console.log('📤 Broadcasting:', jsonMessage);
    
    wss.clients.forEach(client => {
        if (client.readyState === WebSocket.OPEN) {
            client.send(jsonMessage);
        }
    });
}

// Graceful shutdown
process.on('SIGINT', () => {
    console.log('\n👋 Shutting down...');
    tiktokConnection.disconnect();
    wss.close();
    process.exit();
});
```

#### 3. Chạy Server
```bash
node server.js
```

---

## 🧪 Testing Guide

### 1. Test Offline (Không cần TikTok Live)

Trong Unity Editor, bấm các phím sau:

- **T**: Test Super Kick (trigger trực tiếp)
- **Y**: Test Call5Enemy (trigger trực tiếp)
- **U**: Test Like event (random user tap tim)

### 2. Test với Mock WebSocket Server

#### Tạo file `test-server.js`
```javascript
const WebSocket = require('ws');
const wss = new WebSocket.Server({ port: 8080 });

console.log('🧪 Test WebSocket server on ws://localhost:8080');

wss.on('connection', (ws) => {
    console.log('✅ Client connected');
    
    // Test: Gửi Like event mỗi 2 giây
    const likeInterval = setInterval(() => {
        ws.send(JSON.stringify({
            type: 'like',
            userName: `User${Math.floor(Math.random() * 100)}`
        }));
    }, 2000);
    
    // Test: Gửi Rose gift sau 10 giây
    setTimeout(() => {
        ws.send(JSON.stringify({
            type: 'gift',
            userName: 'TestUser_Rose',
            giftName: 'Rose'
        }));
    }, 10000);
    
    // Test: Gửi Perfume gift sau 15 giây
    setTimeout(() => {
        ws.send(JSON.stringify({
            type: 'gift',
            userName: 'TestUser_Perfume',
            giftName: 'Perfume'
        }));
    }, 15000);
    
    ws.on('close', () => {
        clearInterval(likeInterval);
        console.log('❌ Client disconnected');
    });
});
```

Chạy:
```bash
node test-server.js
```

### 3. Test với TikTok Live Thật

1. Chạy server.js với TikTok username của bạn
2. Bật Unity Editor và Play
3. Bắt đầu TikTok Live stream
4. Yêu cầu viewers:
   - Tap tim → Test Like event
   - Tặng quà Rose → Test Super Kick
   - Tặng quà Perfume → Test Call5Enemy

---

## 🎯 API Reference

### TiktokWebSocketClient Methods

#### Connection
```csharp
// Kết nối đến server
public void Connect()

// Ngắt kết nối
public void Disconnect()

// Check trạng thái
public bool IsConnected()
public string GetStatus()
```

#### Testing/Simulation (chỉ hoạt động khi Test Mode = true)
```csharp
// Simulate Like event
public void SimulateLikeEvent(string userName)

// Simulate Rose gift
public void SimulateRoseGift(string userName)

// Simulate Perfume gift
public void SimulatePerfumeGift(string userName = "Anonymous")
```

### TiktokHeartManager Methods

```csharp
// Thêm heart tap
public void AddHeartTap(string userName)

// Lấy số heart hiện tại
public int GetCurrentHeartCount()

// Lấy tên winner (người được chọn random)
public string GetSelectedUserName()

// Clear tên winner
public void ClearSelectedUserName()

// Check Super Kick có đang active không
public bool IsSuperKickActive()
```

---

## 🐛 Troubleshooting

### Lỗi: "WebSocket not supported in WebGL build"
**Giải pháp**: WebSocket native không hoạt động trong WebGL. Cần dùng jslib hoặc disable WebSocket cho WebGL builds.

### Lỗi: Connection refused
**Giải pháp**: 
- Kiểm tra server đã chạy chưa
- Kiểm tra URL đúng không (ws:// không phải wss://)
- Kiểm tra firewall

### Lỗi: NativeWebSocket not found
**Giải pháp**: Cài package NativeWebSocket qua Package Manager

### Events không được nhận
**Giải pháp**:
- Check log trong Unity Console
- Bật `showDebugLogs = true`
- Kiểm tra JSON format từ server

---

## 📊 Flow Diagram

```
TikTok Live Stream
       ↓
    Viewers
       ↓
  (Like/Gift)
       ↓
Server (Node.js)
  ↓ WebSocket
Unity Game
  ↓ Events
  ├─→ Like → HeartManager → Add to array → (100 hearts) → Super Kick
  ├─→ Rose Gift → UI Display Name + Super Kick
  └─→ Perfume Gift → Call5Enemy (no name display)
```

---

## 📝 Changelog

### Version 1.0 (2026-02-04)
- ✅ WebSocket connection với auto-reconnect
- ✅ Hỗ trợ Like event
- ✅ Hỗ trợ Rose Gift (Super Kick + display name)
- ✅ Hỗ trợ Perfume Gift (Call5Enemy anonymous)
- ✅ Test mode cho offline testing
- ✅ UI integration

---

## 👤 Contact & Support

Nếu cần hỗ trợ, hãy check:
1. Unity Console logs (bật Debug mode)
2. Server logs
3. Network connection

Happy Streaming! 🎮🎉
