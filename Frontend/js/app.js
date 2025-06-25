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
const connectWsBtn = document.getElementById('connectWsBtn');
const disconnectWsBtn = document.getElementById('disconnectWsBtn');
const orderIdInput = document.getElementById('orderIdInput');
const checkOrderBtn = document.getElementById('checkOrderBtn');
const cancelOrderBtn = document.getElementById('cancelOrderBtn');
const ordersUserIdInput = document.getElementById('ordersUserId');
const checkOrdersBtn = document.getElementById('checkOrdersBtn');
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
    gravity: 'top',
    position: 'right',
    backgroundColor: 'linear-gradient(to right, #00b09b, #96c93d)',
  }).showToast();
  log(`PUSH: ${message}`);
}

let socket;

function connectWebSocket() {
  if (socket &&
      (socket.readyState === WebSocket.OPEN ||
       socket.readyState === WebSocket.CONNECTING)) {
    log('WebSocket уже подключен или подключается.');
    return;
  }

  const userId = userIdInput.value;
  if (!userId) {
    log('UserID не может быть пустым для WebSocket', true);
    return;
  }

  log(`Подключение к WebSocket для пользователя ${userId}...`);
  socket = new WebSocket(`${WEBSOCKET_URL}?userId=${userId}`);

  socket.onopen = () => {
    log(`WebSocket соединение установлено для пользователя ${userId}`);
  };

  socket.onmessage = (event) => {
    showNotification(event.data);
  };

  socket.onclose = () => {
    log('WebSocket соединение закрыто.');
    socket = null;
  };

  socket.onerror = (error) => {
    log('WebSocket ошибка: ' + error.message, true);
  };
}

function disconnectWebSocket() {
  if (socket && socket.readyState === WebSocket.OPEN) {
    socket.close();
  } else {
    log('WebSocket не подключен.');
  }
}

connectWsBtn.addEventListener('click', connectWebSocket);
disconnectWsBtn.addEventListener('click', disconnectWebSocket);


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
      headers: {'Content-Type': 'application/json'},
      body: JSON.stringify({userId})
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
      headers: {'Content-Type': 'application/json'},
      body: JSON.stringify({userId, amount})
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
    const response =
        await fetch(`${API_GATEWAY_URL}/api/accounts?userId=${userId}`);
    if (!response.ok) throw new Error(`Ошибка сервера: ${response.status}`);
    const account = await response.json();
    log(`Баланс пользователя ${userId}: ${account.balance}`);
  } catch (error) {
    log(`Ошибка при проверке баланса: ${error.message}`, true);
  }
});

createOrderBtn.addEventListener('click', async () => {
  const ordersUserId = ordersUserIdInput.value;
  const amount = parseFloat(orderAmountInput.value);
  const description = orderDescriptionInput.value;
  log(`Создание заказа для ${ordersUserId} на сумму ${amount}...`);
  try {
    const response = await fetch(`${API_GATEWAY_URL}/api/orders`, {
      method: 'POST',
      headers: {'Content-Type': 'application/json'},
      body: JSON.stringify({ordersUserId, amount, description})
    });
    if (!response.ok) throw new Error(`Ошибка сервера: ${response.status}`);
    const orderId = await response.json();
    log(`Заказ успешно создан. OrderID: ${orderId}`);
  } catch (error) {
    log(`Ошибка при создании заказа: ${error.message}`, true);
  }
});

cancelOrderBtn.addEventListener('click', async () => {
  const orderId = orderIdInput.value;
  if (!orderId) {
    log('OrderID не может быть пустым', true);
    return;
  }

  log(`Отправка запроса на отмену заказа ${orderId}...`);
  try {
    const response =
        await fetch(`${API_GATEWAY_URL}/api/orders/${orderId}/cancel`, {
          method: 'PATCH',
        });

    if (response.status === 204) {
      log(`Заказ ${orderId} успешно отменен.`);
    } else {
      const errorData = await response.json();
      throw new Error(
          `Ошибка сервера: ${response.status} - ${errorData.error}`);
    }
  } catch (error) {
    log(`Ошибка при отмене заказа: ${error.message}`, true);
  }
});

checkOrderBtn.addEventListener('click', async () => {
  const orderId = orderIdInput.value;
  if (!orderId) {
    log('OrderID не может быть пустым', true);
    return;
  }

  log(`Запрос на просмотр заказа ${userId}...`);
  try {
    const response = await fetch(`${API_GATEWAY_URL}/api/orders/${orderId}`);
    if (!response.ok) throw new Error(`Ошибка сервера: ${response.status}`);
    const order = await response.json();
    log(`Заказ ${orderId}: Сумма - ${order.amount}, Описание - ${order.description}, Статус - ${order.status}`);
  } catch (error) {
    log(`Ошибка при просмотре заказа: ${error.message}`, true);
  }
});

checkOrdersBtn.addEventListener('click', async () => {
  const ordersUserId = ordersUserIdInput.value;
  if (!ordersUserId) {
    log('UserID не может быть пустым', true);
    return;
  }

  log(`Запрос на просмотр заказов пользователя ${ordersUserId}...`);
  try {
    const response =
        await fetch(`${API_GATEWAY_URL}/api/orders?userId=${ordersUserId}`);
    if (!response.ok) throw new Error(`Ошибка сервера: ${response.status}`);
    const orders = await response.json();
    if (orders.length === 0) {
      log(`Нет заказов для пользователя ${ordersUserId}`);
    } else {
      orders.forEach(order => {
        log(`Заказ ${order.id}: Сумма - ${order.amount}, Описание - ${order.description}, Статус - ${order.status}`);
      });
    }
  } catch (error) {
    log(`Ошибка при просмотре заказов: ${error.message}`, true);
  }
});