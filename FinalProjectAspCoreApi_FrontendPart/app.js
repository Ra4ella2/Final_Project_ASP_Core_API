const API_BASE = "https://localhost:7295";
const FALLBACK_IMG = "https://via.placeholder.com/600x400?text=No+Image";
const TOKEN_KEY = "userAccessToken";
const CART_KEY = "cart";

document.addEventListener("DOMContentLoaded", () => {
  const grid = document.getElementById("productsGrid");
  const statusEl = document.getElementById("status");

  const headerBtn = document.getElementById("headerAuthBtn");
  const userInfo = document.getElementById("userInfo");

  const cartBtn = document.getElementById("cartBtn");
  const cartCountEl = document.getElementById("cartCount");

  const ordersBtn = document.getElementById("ordersBtn");

  const authModal = document.getElementById("authModal");
  const authBackdrop = document.getElementById("authBackdrop");
  const authClose = document.getElementById("authClose");
  const authForm = document.getElementById("authForm");
  const loginBtn = document.getElementById("loginBtn");
  const registerBtn = document.getElementById("registerBtn");
  const authMsg = document.getElementById("authMsg");

  const cartModal = document.getElementById("cartModal");
  const cartBackdrop = document.getElementById("cartBackdrop");
  const cartClose = document.getElementById("cartClose");
  const cartList = document.getElementById("cartList");
  const cartTotal = document.getElementById("cartTotal");
  const cartMsg = document.getElementById("cartMsg");
  const checkoutBtn = document.getElementById("checkoutBtn");

  const ordersModal = document.getElementById("ordersModal");
  const ordersBackdrop = document.getElementById("ordersBackdrop");
  const ordersClose = document.getElementById("ordersClose");
  const ordersList = document.getElementById("ordersList");
  const ordersMsg = document.getElementById("ordersMsg");

  const orderSearchInput = document.getElementById("orderSearchInput");
  const orderSearchBtn = document.getElementById("orderSearchBtn");
  const orderSearchResetBtn = document.getElementById("orderSearchResetBtn");

  if (!grid) return console.error("Нет #productsGrid");

  let productsCache = [];

  headerBtn.addEventListener("click", () => {
    if (getToken()) {
      logout();
      setCart([]);
      updateHeader();
      setStatus("Вы вышли");
      grid.innerHTML = "";
      loadProducts();
    } else {
      openAuth();
    }
  });

  authBackdrop.addEventListener("click", closeAuth);
  authClose.addEventListener("click", closeAuth);
  loginBtn.addEventListener("click", onLogin);
  registerBtn.addEventListener("click", onRegister);

  cartBtn?.addEventListener("click", () => {
    if (!requireAuth()) return;
    openCart();
  });

  cartBackdrop.addEventListener("click", closeCart);
  cartClose.addEventListener("click", closeCart);
  checkoutBtn.addEventListener("click", onCheckout);

  ordersBtn?.addEventListener("click", () => {
    if (!requireAuth()) return;
    openOrders();
  });

  ordersBackdrop.addEventListener("click", closeOrders);
  ordersClose.addEventListener("click", closeOrders);

  orderSearchBtn?.addEventListener("click", () => {
    if (!requireAuth()) return;
    const id = Number(orderSearchInput?.value);
    if (!id || id <= 0) {
      ordersMsg.textContent = "Введи корректный ID заказа";
      ordersMsg.className = "hint error";
      return;
    }
    loadOrderById(id);
  });

  orderSearchInput?.addEventListener("keydown", (e) => {
    if (e.key === "Enter") orderSearchBtn.click();
  });

  orderSearchResetBtn?.addEventListener("click", () => {
    orderSearchInput.value = "";
    loadOrders();
  });

  updateHeader();
  if (!getToken()) setCart([]);
  updateCartCount();
  loadProducts();

  function setStatus(text) {
    statusEl.textContent = text || "";
  }

  function getRolesFromToken(token) {
    const payload = decodeJwt(token);
    if (!payload) return [];
    const roleKey = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
    const r = payload[roleKey];
    if (!r) return [];
    return Array.isArray(r) ? r : [r];
  }

  function isAdminToken(token) {
    return getRolesFromToken(token).includes("Admin");
  }

  function getToken() {
    return localStorage.getItem(TOKEN_KEY);
  }

  function setToken(token) {
    localStorage.setItem(TOKEN_KEY, token);
  }

  function logout() {
    localStorage.removeItem(TOKEN_KEY);
  }

  function decodeJwt(token) {
    try {
      const payload = token.split(".")[1];
      const json = atob(payload.replace(/-/g, "+").replace(/_/g, "/"));
      return JSON.parse(json);
    } catch {
      return null;
    }
  }

  function updateHeader() {
    const token = getToken();
    if (!token) {
      headerBtn.textContent = "Вход";
      userInfo.textContent = "";
      return;
    }

    const payload = decodeJwt(token);
    const email = payload?.email ?? "";

    headerBtn.textContent = "Выход";
    userInfo.textContent = email ? `👤 ${email}` : "👤 Пользователь";
  }

  function openAuth() {
    authModal.hidden = false;
    authMsg.textContent = "";
    authMsg.className = "hint";
    authForm?.querySelector('input[name="email"]')?.focus();
  }

  function closeAuth() {
    authModal.hidden = true;
    authMsg.textContent = "";
    authMsg.className = "hint";
  }

  function requireAuth() {
    if (getToken()) return true;
    openAuth();
    return false;
  }

  async function apiFetch(path, { method = "GET", body = null, auth = true } = {}) {
    const headers = { "Content-Type": "application/json" };
    if (auth) {
      const token = getToken();
      if (token) headers.Authorization = `Bearer ${token}`;
    }

    const res = await fetch(`${API_BASE}${path}`, {
      method,
      headers,
      body: body ? JSON.stringify(body) : null,
    });

    const text = await res.text().catch(() => "");
    let data;
    try { data = text ? JSON.parse(text) : null; } catch { data = text; }

    if (!res.ok) {
      const msg = typeof data === "string"
        ? data
        : (data?.message || data?.error || text || `HTTP ${res.status}`);

      const err = new Error(msg);
      err.status = res.status;
      err.data = data;
      throw err;
    }

    return data;
  }

  function showHttpError(whereEl, e) {
    const s = e.status;
    let msg = e.message || "Ошибка";

    if (s === 401) msg = "Нужно войти";
    if (s === 403) msg = "Нет прав";
    if (s === 404) msg = "Не найдено";

    whereEl.textContent = msg;
    whereEl.className = "hint error";

    if (s === 401) {
      logout();
      updateHeader();
      openAuth();
    }
  }

  function getCart() {
    try { return JSON.parse(localStorage.getItem(CART_KEY) || "[]"); }
    catch { return []; }
  }

  function setCart(items) {
    localStorage.setItem(CART_KEY, JSON.stringify(items));
    updateCartCount();
  }

  function updateCartCount() {
    const cart = getCart();
    const count = cart.reduce((sum, x) => sum + (Number(x.quantity) || 0), 0);
    cartCountEl.textContent = String(count);
  }

  function cartTotalSum() {
    const cart = getCart();
    return cart.reduce((sum, x) => sum + (Number(x.price) || 0) * (Number(x.quantity) || 0), 0);
  }

  function addToCart(product) {
    const cart = getCart();
    const found = cart.find(x => x.productId === product.id);

    if (found) {
      if ((found.quantity + 1) > product.stock) return;
      found.quantity += 1;
    } else {
      if (product.stock <= 0) return;
      cart.push({
        productId: product.id,
        name: product.name,
        price: product.price,
        imageUrl: product.imageUrl || null,
        quantity: 1
      });
    }
    setCart(cart);
  }

  function setQty(productId, qty) {
    const cart = getCart();
    const item = cart.find(x => x.productId === productId);
    if (!item) return;

    const p = productsCache.find(pp => pp.id === productId);
    const max = p ? p.stock : 999999;

    const newQty = Math.max(1, Math.min(max, qty));
    item.quantity = newQty;
    setCart(cart);
  }

  function removeFromCart(productId) {
    const cart = getCart().filter(x => x.productId !== productId);
    setCart(cart);
  }

  function openCart() {
    cartModal.hidden = false;
    cartMsg.textContent = "";
    cartMsg.className = "hint";
    renderCart();
  }

  function closeCart() {
    cartModal.hidden = true;
    cartMsg.textContent = "";
    cartMsg.className = "hint";
  }

  function renderCart() {
    const cart = getCart();
    const total = cartTotalSum();
    cartTotal.textContent = total.toFixed(2);

    if (!cart.length) {
      cartList.innerHTML = `<div class="muted">Корзина пуста</div>`;
      checkoutBtn.disabled = true;
      return;
    }

    checkoutBtn.disabled = false;

    cartList.innerHTML = cart.map(x => {
      const img = x.imageUrl ? x.imageUrl : FALLBACK_IMG;
      return `
        <div class="item">
          <img src="${img}" onerror="this.onerror=null;this.src='${FALLBACK_IMG}'" alt="">
          <div>
            <div class="item-title">${x.name}</div>
            <div class="item-sub">${Number(x.price).toFixed(2)} × ${x.quantity}</div>
          </div>
          <div class="qty">
            <button class="mini" data-dec="${x.productId}" type="button">−</button>
            <div class="num">${x.quantity}</div>
            <button class="mini" data-inc="${x.productId}" type="button">+</button>
            <button class="mini" data-del="${x.productId}" type="button">Удалить</button>
          </div>
        </div>
      `;
    }).join("");

    cartList.querySelectorAll("[data-inc]").forEach(b => b.addEventListener("click", () => {
      const id = Number(b.dataset.inc);
      const item = getCart().find(x => x.productId === id);
      if (!item) return;
      setQty(id, item.quantity + 1);
      renderCart();
    }));

    cartList.querySelectorAll("[data-dec]").forEach(b => b.addEventListener("click", () => {
      const id = Number(b.dataset.dec);
      const item = getCart().find(x => x.productId === id);
      if (!item) return;
      setQty(id, item.quantity - 1);
      renderCart();
    }));

    cartList.querySelectorAll("[data-del]").forEach(b => b.addEventListener("click", () => {
      const id = Number(b.dataset.del);
      removeFromCart(id);
      renderCart();
    }));
  }

  async function onCheckout() {
    cartMsg.textContent = "";
    cartMsg.className = "hint";

    const cart = getCart();
    if (!cart.length) return;

    const body = {
      items: cart.map(x => ({ productId: x.productId, quantity: x.quantity }))
    };

    try {
      const res = await apiFetch("/api/user/create/order", {
        method: "POST",
        body,
        auth: true
      });

      setCart([]);
      renderCart();

      cartMsg.textContent = `Заказ создан! ID: ${res?.orderId ?? ""}`.trim();
      cartMsg.className = "hint ok";

      loadProducts();
    } catch (e) {
      showHttpError(cartMsg, e);
    }
  }

  function openOrders() {
    ordersModal.hidden = false;
    ordersMsg.textContent = "";
    ordersMsg.className = "hint";
    loadOrders();
  }

  function closeOrders() {
    ordersModal.hidden = true;
    ordersMsg.textContent = "";
    ordersMsg.className = "hint";
  }

  async function loadOrderById(id) {
    ordersList.innerHTML = `<div class="muted">Загрузка...</div>`;
    ordersMsg.textContent = "";
    ordersMsg.className = "hint";

    try {
      const o = await apiFetch(`/api/user/myOrders/${id}`, { auth: true });

      ordersList.innerHTML = renderOrdersHtml([o]);
      bindOrdersHandlers();

      ordersMsg.textContent = `Найден заказ #${id}`;
      ordersMsg.className = "hint ok";
    } catch (e) {
      showHttpError(ordersMsg, e);
      ordersList.innerHTML = `<div class="muted">Заказ не найден</div>`;
    }
  }

  async function loadOrders() {
    ordersList.innerHTML = `<div class="muted">Загрузка...</div>`;
    ordersMsg.textContent = "";
    ordersMsg.className = "hint";

    try {
      const orders = await apiFetch("/api/user/myOrders", { auth: true });

      if (!Array.isArray(orders) || orders.length === 0) {
        ordersList.innerHTML = `<div class="muted">Заказов нет</div>`;
        return;
      }

      ordersList.innerHTML = renderOrdersHtml(orders);
      bindOrdersHandlers();
    } catch (e) {
      showHttpError(ordersMsg, e);
      ordersList.innerHTML = `<div class="muted">Не удалось загрузить заказы</div>`;
    }
  }

  function renderOrdersHtml(orders) {
    return orders.map(o => {
      const itemsCount = (o.items || []).length;
      const canCancel = o.status !== "Shipped" && o.status !== "Completed";

      return `
        <div class="item order-item">
          <div>
            <div class="item-title">Заказ #${o.id}</div>
            <div class="item-sub">
              ${new Date(o.createdAt).toLocaleString()} • 
              ${o.status} • 
              позиций: ${itemsCount}
            </div>

            <div class="row end" style="margin-top:8px; gap:8px;">
              <button class="btn ghost" data-details="${o.id}" type="button">Детали</button>
              <button class="btn modalbtn" data-cancel="${o.id}" type="button" ${canCancel ? "" : "disabled"}>Отменить</button>
            </div>

            <div class="muted small"
                id="orderDetails_${o.id}"
                style="margin-top:8px; display:none;">
            </div>
          </div>
        </div>
      `;
    }).join("");
  }

  function bindOrdersHandlers() {
    ordersList.querySelectorAll("[data-details]").forEach(b => b.addEventListener("click", async () => {
      const id = Number(b.dataset.details);
      const box = document.getElementById(`orderDetails_${id}`);

      const isOpen = box.dataset.open === "1";
      if (isOpen) {
        box.style.display = "none";
        box.dataset.open = "0";
        return;
      }

      box.style.display = "block";
      box.dataset.open = "1";
      box.style.whiteSpace = "pre-line";

      if (box.dataset.loaded === "1") return;

      box.textContent = "Загрузка деталей...";

      try {
        const d = await apiFetch(`/api/user/myOrders/${id}`, { auth: true });
        const lines = (d.items || []).map(i =>
          `• ${i.productName} × ${i.quantity} = ${(i.unitPrice * i.quantity).toFixed(2)}`
        );
        box.textContent = lines.join("\n") || "Нет позиций";
        box.dataset.loaded = "1";
      } catch (e) {
        box.textContent = e.message || "Ошибка";
        box.dataset.loaded = "0";
      }
    }));

    ordersList.querySelectorAll("[data-cancel]").forEach(b => b.addEventListener("click", async () => {
      const id = Number(b.dataset.cancel);
      try {
        await apiFetch(`/api/user/myOrder/${id}/status`, { method: "PATCH", auth: true });
        ordersMsg.textContent = `Заказ #${id} отменён`;
        ordersMsg.className = "hint ok";
        loadOrders();
        loadProducts();
      } catch (e) {
        showHttpError(ordersMsg, e);
      }
    }));
  }

  function normalizeProduct(p) {
    return {
      id: p.id ?? p.Id,
      name: p.name ?? p.Name ?? "",
      price: Number(p.price ?? p.Price ?? 0),
      stock: Number(p.stock ?? p.Stock ?? 0),
      imageUrl: p.imageUrl ?? p.ImageUrl ?? null,
    };
  }

  function renderProducts(products) {
    if (!Array.isArray(products) || products.length === 0) {
      grid.innerHTML = `<div class="muted">Товаров нет</div>`;
      return;
    }

    grid.innerHTML = products.map(raw => {
      const p = normalizeProduct(raw);
      const img = p.imageUrl ? p.imageUrl : "";

      return `
        <article class="card">
          <img class="product-img"
               src="${img || FALLBACK_IMG}"
               alt="${p.name}"
               loading="lazy"
               onerror="this.onerror=null;this.src='${FALLBACK_IMG}'" />

          <div class="card-title">${p.name}</div>
          <div class="muted">Stock: ${p.stock}</div>
          <div class="price">${p.price.toFixed(2)}</div>

          <div class="card-actions">
            <button class="btn modalbtn addBtn" data-add="${p.id}" ${p.stock <= 0 ? "disabled" : ""} type="button">
              ${p.stock <= 0 ? "Нет в наличии" : "В корзину"}
            </button>
          </div>
        </article>
      `;
    }).join("");

    grid.querySelectorAll(".addBtn").forEach(btn => {
      btn.addEventListener("click", () => {
        if (!requireAuth()) return;
        const id = Number(btn.dataset.add);
        const raw = products.find(x => (x.id ?? x.Id) === id);
        if (!raw) return;
        const p = normalizeProduct(raw);
        addToCart(p);
      });
    });
  }

  async function loadProducts() {
    setStatus("Загрузка товаров...");
    grid.innerHTML = "";

    try {
      const products = await apiFetch("/api/user/products", { auth: false });
      const normalized = (Array.isArray(products) ? products : []).map(normalizeProduct);
      productsCache = normalized;

      setStatus(`Загружено: ${normalized.length}`);
      renderProducts(normalized);
    } catch (e) {
      setStatus("");
      if (e.status === 401) {
        logout();
        updateHeader();
        grid.innerHTML = `<div class="muted">Нужно войти, чтобы увидеть товары</div>`;
        return;
      }
      grid.innerHTML = `<div class="muted">Ошибка: ${e.message}</div>`;
    }
  }

  async function onLogin() {
    authMsg.textContent = "";
    authMsg.className = "hint";

    const fd = new FormData(authForm);
    const email = String(fd.get("email") || "").trim();
    const password = String(fd.get("password") || "");

    try {
      const res = await apiFetch("/api/auth/login", {
        method: "POST",
        body: { email, password },
        auth: false
      });

      if (!res?.accessToken) throw new Error("Нет accessToken в ответе");

      if (isAdminToken(res.accessToken)) {
        authMsg.textContent = "Админ не может входить в пользовательский интерфейс. Открой админ-панель.";
        authMsg.className = "hint error";
        return;
      }

      setToken(res.accessToken);
      updateHeader();
      closeAuth();
      loadProducts();
    } catch (e) {
      authMsg.textContent = e.message || "Ошибка входа";
      authMsg.className = "hint error";
    }
  }

  async function onRegister() {
    authMsg.textContent = "";
    authMsg.className = "hint";

    const fd = new FormData(authForm);
    const email = String(fd.get("email") || "").trim();
    const password = String(fd.get("password") || "");

    try {
      await apiFetch("/api/auth/register", {
        method: "POST",
        body: { email, password },
        auth: false
      });

      authMsg.textContent = "Регистрация успешна. Теперь нажми «Войти».";
      authMsg.className = "hint ok";
    } catch (e) {
      authMsg.textContent = e.message || "Ошибка регистрации";
      authMsg.className = "hint error";
    }
  }
});
