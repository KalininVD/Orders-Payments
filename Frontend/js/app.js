const API_GATEWAY_URL = 'http://localhost:8080';
const WEBSOCKET_URL = 'ws://localhost:8080/ws';

const userIdInput = document.getElementById('userId');
const createAccountBtn = document.getElementById('createAccountBtn');
const checkBalanceBtn = document.getElementById('checkBalanceBtn');
const depositAmountInput = document.getElementById('depositAmount');
const depositBtn = document.getElementById('depositBtn');
const orderAmountInput = document.getElementById('orderAmount');
const orderDescriptionInput = document.getElementById('orderDescription');
const createOrderBtn = document.getElementById('createOrderBtn');
const logContainer = document.getElementById('log-container');

function log(message, isError = false) {
    const logEntry = document.createElement('div');
    logEntry.textContent = `[${new Date().toLocaleTimeString()}] ${message}`;
    if (isError) {
        logEntry.style.color = '#ff6b6b';
    }
    logContainer.appendChild(logEntry);
    logContainer.scrollTop = logContainer.scrollHeight;
}

function showNotification(message) {
    Toastify({
        text: message,
        duration: 5000,
        gravity: "top",
        position: "right",
        backgroundColor: "linear-gradient(to right, #00b09b, #96c93d)",
    }).showToast();
    log(`PUSH: ${message}`);
}

let socket;
function connectWebSocket(userId) {
    if (socket && socket.readyState === WebSocket.OPEN) {
        socket.close();
    }
    
    socket = new WebSocket(`${WEBSOCKET_URL}?userId=${userId}`);

    socket.onopen = () => {
        log(`WebSocket соединение установлено для пользователя ${userId}`);
    };

    socket.onmessage = (event) => {
        showNotification(event.data);
    };

    socket.onclose = () => {
        log('WebSocket соединение закрыто.');
    };

    socket.onerror = (error) => {
        log('WebSocket ошибка: ' + error.message, true);
    };
}


createAccountBtn.addEventListener('click', async () => {
    const userId = userIdInput.value;
    if (!userId) {
        log('UserID не может быть пустым', true);
        return;
    }
    log(`Отправка запроса на создание счета для ${userId}...`);
    try {
        const response = await fetch(`${API_GATEWAY_URL}/api/accounts`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ userId })
        });
        if (!response.ok) throw new Error(`Ошибка сервера: ${response.status}`);
        const accountId = await response.json();
        log(`Счет успешно создан. AccountID: ${accountId}`);
    } catch (error) {
        log(`Ошибка при создании счета: ${error.message}`, true);
    }
});

depositBtn.addEventListener('click', async () => {
    const userId = userIdInput.value;
    const amount = parseFloat(depositAmountInput.value);
    log(`Пополнение счета для ${userId} на ${amount}...`);
    try {
        const response = await fetch(`${API_GATEWAY_URL}/api/accounts/deposit`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ userId, amount })
        });
        if (!response.ok) throw new Error(`Ошибка сервера: ${response.status}`);
        log('Счет успешно пополнен.');
    } catch (error) {
        log(`Ошибка при пополнении: ${error.message}`, true);
    }
});

checkBalanceBtn.addEventListener('click', async () => {
    const userId = userIdInput.value;
    log(`Запрос баланса для ${userId}...`);
    try {
        const response = await fetch(`${API_GATEWAY_URL}/api/accounts/user/${userId}`);
        if (!response.ok) throw new Error(`Ошибка сервера: ${response.status}`);
        const account = await response.json();
        log(`Баланс пользователя ${userId}: ${account.balance}`);
    } catch (error) {
        log(`Ошибка при проверке баланса: ${error.message}`, true);
    }
});

createOrderBtn.addEventListener('click', async () => {
    const userId = userIdInput.value;
    const amount = parseFloat(orderAmountInput.value);
    const description = orderDescriptionInput.value;
    log(`Создание заказа для ${userId} на сумму ${amount}...`);
    try {
        const response = await fetch(`${API_GATEWAY_URL}/api/orders`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ userId, amount, description })
        });
        if (!response.ok) throw new Error(`Ошибка сервера: ${response.status}`);
        const orderId = await response.json();
        log(`Заказ успешно создан. OrderID: ${orderId}. Подключаемся к WebSocket...`);

        connectWebSocket(userId);
    } catch (error) {
        log(`Ошибка при создании заказа: ${error.message}`, true);
    }
});