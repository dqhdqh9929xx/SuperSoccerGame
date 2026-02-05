/**
 * TikTok WebSocket Server Example
 * 
 * Cách sử dụng:
 * 1. Cài đặt: npm install ws tiktok-live-connector
 * 2. Thay 'your_tiktok_username' bằng username TikTok thật
 * 3. Chạy: node tiktok-server-example.js
 * 4. Start Unity game và connect tới ws://localhost:8080
 * 5. Bắt đầu live stream trên TikTok
 */

const WebSocket = require('ws');
const { WebcastPushConnection } = require('tiktok-live-connector');

// ===== CONFIGURATION =====
const WEBSOCKET_PORT = 8080;
const TIKTOK_USERNAME = 'your_tiktok_username'; // Thay bằng username của bạn

// Gift mapping (có thể cần adjust dựa trên TikTok gift IDs)
const GIFT_MAPPING = {
    'Rose': ['Rose', 'Hoa hồng', 'rose'],
    'Perfume': ['Perfume', 'Nước hoa', 'perfume']
};

// ===== WEBSOCKET SERVER =====
const wss = new WebSocket.Server({ port: WEBSOCKET_PORT });
console.log(`🚀 WebSocket server running on ws://localhost:${WEBSOCKET_PORT}`);
console.log('👉 Unity client có thể connect đến địa chỉ này');

let connectedClients = 0;

wss.on('connection', (ws) => {
    connectedClients++;
    console.log(`✅ Unity client connected! (Total: ${connectedClients})`);
    
    // Gửi welcome message
    ws.send(JSON.stringify({
        type: 'system',
        message: 'Connected to TikTok WebSocket Server'
    }));
    
    ws.on('close', () => {
        connectedClients--;
        console.log(`❌ Unity client disconnected (Remaining: ${connectedClients})`);
    });
    
    ws.on('error', (error) => {
        console.error('WebSocket error:', error.message);
    });
});

// ===== TIKTOK LIVE CONNECTION =====
let tiktokConnection = new WebcastPushConnection(TIKTOK_USERNAME, {
    processInitialData: true,
    enableExtendedGiftInfo: true,
    enableWebsocketUpgrade: true,
    requestPollingIntervalMs: 1000
});

// Connect to TikTok
console.log(`\n🔗 Connecting to TikTok Live: @${TIKTOK_USERNAME}...`);

tiktokConnection.connect().then(state => {
    console.log(`✅ Connected to @${state.roomInfo.owner.uniqueId} live!`);
    console.log(`👥 Viewers: ${state.roomInfo.liveRoomStats.userCount}`);
    console.log(`💖 Likes: ${state.roomInfo.liveRoomStats.likeCount}`);
    console.log('\n🎉 Ready to receive events!\n');
}).catch(err => {
    console.error('❌ Failed to connect to TikTok:', err.message);
    console.error('\n💡 Tips:');
    console.error('  - Kiểm tra username đúng chưa');
    console.error('  - Kiểm tra user có đang live không');
    console.error('  - Thử lại sau vài giây\n');
});

// ===== TIKTOK EVENT HANDLERS =====

// 1. Like Event (Tim)
tiktokConnection.on('like', data => {
    const userName = data.uniqueId || 'Anonymous';
    const likeCount = data.likeCount || 1;
    
    console.log(`💖 LIKE: ${userName} liked ${likeCount} times`);
    
    // Gửi từng like riêng biệt (để tăng count chính xác)
    for (let i = 0; i < likeCount; i++) {
        broadcast({
            type: 'like',
            userName: userName,
            likeCount: 1
        });
    }
});

// 2. Gift Event (Quà)
tiktokConnection.on('gift', data => {
    const userName = data.uniqueId || 'Anonymous';
    const giftName = data.giftName;
    const giftId = data.giftId;
    const repeatCount = data.repeatCount || 1;
    
    console.log(`🎁 GIFT: ${userName} sent ${giftName} (ID: ${giftId}) x${repeatCount}`);
    
    // Kiểm tra xem có phải Rose hoặc Perfume không
    let normalizedGiftName = identifyGift(giftName);
    
    if (normalizedGiftName) {
        // Gửi message cho mỗi gift
        for (let i = 0; i < repeatCount; i++) {
            broadcast({
                type: 'gift',
                userName: userName,
                giftName: normalizedGiftName,
                giftId: giftId
            });
            
            console.log(`  → Sent to Unity: ${normalizedGiftName}`);
        }
    } else {
        console.log(`  → Gift '${giftName}' không được map (chỉ Rose và Perfume được xử lý)`);
    }
});

// 3. Share Event
tiktokConnection.on('share', data => {
    const userName = data.uniqueId || 'Anonymous';
    console.log(`📤 SHARE: ${userName} shared the stream`);
});

// 4. Follow Event
tiktokConnection.on('follow', data => {
    const userName = data.uniqueId || 'Anonymous';
    console.log(`➕ FOLLOW: ${userName} followed!`);
});

// 5. Comment Event
tiktokConnection.on('chat', data => {
    const userName = data.uniqueId || 'Anonymous';
    const comment = data.comment;
    console.log(`💬 COMMENT: ${userName}: ${comment}`);
});

// 6. Join Event
tiktokConnection.on('member', data => {
    const userName = data.uniqueId || 'Anonymous';
    console.log(`👋 JOIN: ${userName} joined the stream`);
});

// 7. Stats Update
tiktokConnection.on('roomUser', data => {
    console.log(`📊 STATS: ${data.viewerCount} viewers watching`);
});

// Connection events
tiktokConnection.on('streamEnd', () => {
    console.log('\n🛑 Stream ended!\n');
});

tiktokConnection.on('error', err => {
    console.error('❌ TikTok error:', err.message);
});

// ===== HELPER FUNCTIONS =====

/**
 * Broadcast message tới tất cả Unity clients
 */
function broadcast(message) {
    const jsonMessage = JSON.stringify(message);
    
    let sentCount = 0;
    wss.clients.forEach(client => {
        if (client.readyState === WebSocket.OPEN) {
            client.send(jsonMessage);
            sentCount++;
        }
    });
    
    if (sentCount === 0) {
        console.log('  ⚠️ No Unity clients connected to receive message');
    }
}

/**
 * Identify gift type (Rose hoặc Perfume)
 */
function identifyGift(giftName) {
    const lowerGiftName = giftName.toLowerCase();
    
    // Check Rose
    if (GIFT_MAPPING.Rose.some(name => lowerGiftName.includes(name.toLowerCase()))) {
        return 'Rose';
    }
    
    // Check Perfume
    if (GIFT_MAPPING.Perfume.some(name => lowerGiftName.includes(name.toLowerCase()))) {
        return 'Perfume';
    }
    
    return null; // Không phải Rose hay Perfume
}

// ===== GRACEFUL SHUTDOWN =====

process.on('SIGINT', () => {
    console.log('\n\n👋 Shutting down server...');
    
    // Disconnect TikTok
    if (tiktokConnection) {
        tiktokConnection.disconnect();
        console.log('✅ TikTok disconnected');
    }
    
    // Close WebSocket server
    wss.close(() => {
        console.log('✅ WebSocket server closed');
        console.log('Goodbye! 👋\n');
        process.exit(0);
    });
});

// ===== INFO =====
console.log('\n================================');
console.log('📋 EVENT MAPPING:');
console.log('================================');
console.log('💖 Like → Unity: Like Event → HeartManager');
console.log('🌹 Rose Gift → Unity: Super Kick + Display Name');
console.log('💐 Perfume Gift → Unity: Call5Enemy (No Name)');
console.log('================================\n');
