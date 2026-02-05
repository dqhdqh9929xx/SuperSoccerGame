/**
 * Simple WebSocket Test Server
 * 
 * Server test đơn giản để kiểm tra Unity WebSocket client
 * KHÔNG cần TikTok Live connection
 * 
 * Cách sử dụng:
 * 1. Cài đặt: npm install ws
 * 2. Chạy: node test-websocket-server.js
 * 3. Start Unity và connect tới ws://localhost:8080
 * 4. Server sẽ tự động gửi test messages
 */

const WebSocket = require('ws');

// ===== CONFIG =====
const PORT = 8080;
const AUTO_SEND_INTERVAL = 3000; // Gửi test message mỗi 3 giây

// ===== WEBSOCKET SERVER =====
const wss = new WebSocket.Server({ port: PORT });

console.log('╔════════════════════════════════════════╗');
console.log('║   🧪 WebSocket Test Server Running   ║');
console.log('╚════════════════════════════════════════╝');
console.log(`\n📡 Listening on: ws://localhost:${PORT}`);
console.log('⏳ Waiting for Unity client...\n');

// ===== TEST DATA =====
const testUsers = [
    'TestUser_A',
    'TestUser_B',
    'TestUser_C',
    'TestUser_D',
    'TestUser_E'
];

let messageCounter = 0;

// ===== CONNECTION HANDLER =====
wss.on('connection', (ws) => {
    console.log('✅ Unity client connected!\n');
    console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
    
    // Welcome message
    ws.send(JSON.stringify({
        type: 'system',
        message: 'Connected to Test Server. Auto-sending test events...'
    }));
    
    // Auto send test messages
    const autoSendInterval = setInterval(() => {
        if (ws.readyState === WebSocket.OPEN) {
            sendRandomTestMessage(ws);
        }
    }, AUTO_SEND_INTERVAL);
    
    // Keyboard commands (nếu chạy interactive mode)
    console.log('\n🎮 INTERACTIVE COMMANDS:');
    console.log('  Press L → Send Like event');
    console.log('  Press R → Send Rose gift event');
    console.log('  Press P → Send Perfume gift event');
    console.log('  Press Q → Quit server\n');
    
    // Handle disconnect
    ws.on('close', () => {
        clearInterval(autoSendInterval);
        console.log('\n❌ Unity client disconnected');
        console.log('⏳ Waiting for new connection...\n');
    });
    
    ws.on('error', (error) => {
        console.error('❌ WebSocket error:', error.message);
    });
});

// ===== TEST MESSAGE GENERATORS =====

function sendRandomTestMessage(ws) {
    const rand = Math.random();
    
    if (rand < 0.5) {
        // 50% chance: Like event
        sendLikeEvent(ws);
    } else if (rand < 0.8) {
        // 30% chance: Rose gift
        sendRoseGift(ws);
    } else {
        // 20% chance: Perfume gift
        sendPerfumeGift(ws);
    }
}

function sendLikeEvent(ws) {
    const userName = testUsers[Math.floor(Math.random() * testUsers.length)];
    const message = {
        type: 'like',
        userName: userName,
        likeCount: 1
    };
    
    ws.send(JSON.stringify(message));
    messageCounter++;
    
    console.log(`[${messageCounter}] 💖 Sent LIKE event: ${userName}`);
}

function sendRoseGift(ws) {
    const userName = testUsers[Math.floor(Math.random() * testUsers.length)];
    const message = {
        type: 'gift',
        userName: userName,
        giftName: 'Rose',
        giftId: 5655
    };
    
    ws.send(JSON.stringify(message));
    messageCounter++;
    
    console.log(`[${messageCounter}] 🌹 Sent ROSE GIFT: ${userName} → Super Kick + Display Name`);
}

function sendPerfumeGift(ws) {
    const userName = testUsers[Math.floor(Math.random() * testUsers.length)];
    const message = {
        type: 'gift',
        userName: userName,
        giftName: 'Perfume',
        giftId: 5658
    };
    
    ws.send(JSON.stringify(message));
    messageCounter++;
    
    console.log(`[${messageCounter}] 💐 Sent PERFUME GIFT: Call5Enemy (Anonymous)`);
}

// ===== INTERACTIVE MODE =====
// Cho phép gửi manual commands từ terminal
if (process.stdin.isTTY) {
    const readline = require('readline');
    readline.emitKeypressEvents(process.stdin);
    process.stdin.setRawMode(true);
    
    process.stdin.on('keypress', (str, key) => {
        if (key.ctrl && key.name === 'c') {
            process.exit();
        }
        
        // Broadcast tới tất cả clients
        wss.clients.forEach(client => {
            if (client.readyState === WebSocket.OPEN) {
                switch(key.name) {
                    case 'l':
                        sendLikeEvent(client);
                        break;
                    case 'r':
                        sendRoseGift(client);
                        break;
                    case 'p':
                        sendPerfumeGift(client);
                        break;
                    case 'q':
                        console.log('\n👋 Shutting down...\n');
                        process.exit(0);
                        break;
                }
            }
        });
    });
}

// ===== TEST SEQUENCE (Optional) =====
// Gửi test sequence khi client mới connect
function sendTestSequence(ws) {
    console.log('\n🧪 Starting test sequence...\n');
    
    // 1. Send 5 likes
    console.log('Step 1: Sending 5 LIKE events...');
    for (let i = 0; i < 5; i++) {
        setTimeout(() => {
            sendLikeEvent(ws);
        }, i * 500);
    }
    
    // 2. Send Rose gift
    setTimeout(() => {
        console.log('\nStep 2: Sending ROSE GIFT...');
        sendRoseGift(ws);
    }, 3000);
    
    // 3. Send Perfume gift
    setTimeout(() => {
        console.log('\nStep 3: Sending PERFUME GIFT...');
        sendPerfumeGift(ws);
    }, 5000);
    
    // 4. Send 100 likes (để trigger Heart Manager)
    setTimeout(() => {
        console.log('\nStep 4: Sending 100 LIKE events (để test Heart Manager)...');
        for (let i = 0; i < 100; i++) {
            setTimeout(() => {
                sendLikeEvent(ws);
                if (i === 99) {
                    console.log('\n✅ Test sequence completed!\n');
                }
            }, i * 100);
        }
    }, 7000);
}

// Để enable test sequence, uncomment dòng này trong connection handler:
// sendTestSequence(ws);

// ===== GRACEFUL SHUTDOWN =====
process.on('SIGINT', () => {
    console.log('\n\n👋 Shutting down test server...');
    wss.close(() => {
        console.log('✅ Server closed');
        console.log('Goodbye! 🎉\n');
        process.exit(0);
    });
});

// ===== STATS =====
setInterval(() => {
    const clientCount = wss.clients.size;
    if (clientCount > 0) {
        console.log(`\n📊 Connected clients: ${clientCount} | Messages sent: ${messageCounter}`);
    }
}, 10000); // Log stats mỗi 10 giây
