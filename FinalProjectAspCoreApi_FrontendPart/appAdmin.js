const API_BASE = "https://localhost:7295";

document.addEventListener("DOMContentLoaded", () => {
  let accessToken = null;

  const loginScreen = document.getElementById("loginScreen");
  const adminPanel = document.getElementById("adminPanel");

  const adminLoginForm = document.getElementById("adminLoginForm");
  const loginMsg = document.getElementById("loginMsg");

  const adminInfo = document.getElementById("adminInfo");
  const logoutBtn = document.getElementById("logoutBtn");

  const tabs = document.querySelectorAll(".tab");
  const productsSection = document.getElementById("productsSection");
  const ordersSection = document.getElementById("ordersSection");
  const usersSection = document.getElementById("usersSection");

  const reloadProductsBtn = document.getElementById("reloadProductsBtn");
  const productsStatus = document.getElementById("productsStatus");
  const productsTbody = document.querySelector("#productsTable tbody");

  const productSearchId = document.getElementById("productSearchId");
  const productSearchBtn = document.getElementById("productSearchBtn");
  const productClearBtn = document.getElementById("productClearBtn");

  const newName = document.getElementById("newName");
  const newPrice = document.getElementById("newPrice");
  const newStock = document.getElementById("newStock");
  const newImageUrl = document.getElementById("newImageUrl");
  const newActive = document.getElementById("newActive");
  const createProductBtn = document.getElementById("createProductBtn");
  const createMsg = document.getElementById("createMsg");

  const reloadOrdersBtn = document.getElementById("reloadOrdersBtn");
  const ordersStatus = document.getElementById("ordersStatus");
  const ordersTbody = document.querySelector("#ordersTable tbody");

  const orderSearchId = document.getElementById("orderSearchId");
  const orderSearchBtn = document.getElementById("orderSearchBtn");
  const orderClearBtn = document.getElementById("orderClearBtn");

  const reloadUsersBtn = document.getElementById("reloadUsersBtn");
  const usersStatus = document.getElementById("usersStatus");
  const usersTbody = document.querySelector("#usersTable tbody");

  const R = {
    productsList: "/api/admin/product",
    productGetById: (id) => `/api/admin/product/${id}`,
    productCreate: "/api/admin/product/post",

    productPatchName: (id) => `/api/admin/product/${id}/name`,
    productPatchPrice: (id) => `/api/admin/product/${id}/price`,
    productPatchStock: (id) => `/api/admin/product/${id}/stock`,
    productPatchActive: (id) => `/api/admin/product/${id}/active`,
    productPatchImage: (id) => `/api/admin/product/${id}/image`,

    productDelete: (id) => `/api/admin/product/${id}/delete`,
    productReturn: (id) => `/api/admin/product/${id}/return`,

    ordersList: "/api/admin/order",
    orderGetById: (id) => `/api/admin/order/${id}`,
    orderPatchStatus: (id) => `/api/admin/order/${id}/status/`,

    usersList: "/api/admin/users",
  };

  const ORDER_STATUSES = ["Created", "Paid", "Shipped", "Completed", "Cancelled"];

  adminLoginForm.addEventListener("submit", (e) => {
    e.preventDefault();
    login();
  });

  logoutBtn.addEventListener("click", () => {
    accessToken = null;
    showLogin("Вы вышли");
  });

  reloadProductsBtn.addEventListener("click", loadProducts);
  createProductBtn.addEventListener("click", createProduct);
  reloadOrdersBtn.addEventListener("click", loadOrders);
  reloadUsersBtn.addEventListener("click", loadUsers);

  tabs.forEach(t => t.addEventListener("click", () => switchTab(t.dataset.tab)));

  productSearchBtn?.addEventListener("click", searchProductById);
  productClearBtn?.addEventListener("click", () => {
    if (productSearchId) productSearchId.value = "";
    loadProducts();
  });
  productSearchId?.addEventListener("keydown", (e) => {
    if (e.key === "Enter") searchProductById();
  });

  orderSearchBtn?.addEventListener("click", searchOrderById);
  orderClearBtn?.addEventListener("click", () => {
    if (orderSearchId) orderSearchId.value = "";
    loadOrders();
  });
  orderSearchId?.addEventListener("keydown", (e) => {
    if (e.key === "Enter") searchOrderById();
  });

  showLogin();

  async function login() {
    setLoginMsg("");

    const fd = new FormData(adminLoginForm);
    const email = String(fd.get("email") || "").trim();
    const password = String(fd.get("password") || "");

    if (!email || !password) return setLoginMsg("Заполни email и пароль");

    try {
      const res = await apiFetch("/api/auth/login", {
        method: "POST",
        auth: false,
        body: { email, password }
      });

      if (!res?.accessToken) throw new Error("Нет accessToken");

      if (!isAdminToken(res.accessToken)) {
        setLoginMsg("Доступ запрещён: не Admin");
        return;
      }

      accessToken = res.accessToken;
      showPanel(accessToken);
      switchTab("products");
      loadProducts();
    } catch (e) {
      setLoginMsg(e.message || "Ошибка входа");
    }
  }

  function showLogin(msg = "") {
    loginScreen.hidden = false;
    adminPanel.hidden = true;
    adminLoginForm.reset();
    setLoginMsg(msg);
    adminInfo.textContent = "";
  }

  function showPanel(token) {
    loginScreen.hidden = true;
    adminPanel.hidden = false;

    const payload = decodeJwt(token);
    const email = payload?.email ?? "";

    adminInfo.textContent = email ? `👤 ${email}` : "👤 Admin";
  }

  function switchTab(name) {
    tabs.forEach(x => x.classList.toggle("active", x.dataset.tab === name));
    productsSection.hidden = name !== "products";
    ordersSection.hidden = name !== "orders";
    usersSection.hidden = name !== "users";

    if (!accessToken) return showLogin("Нужно войти");

    if (name === "products") loadProducts();
    if (name === "orders") loadOrders();
    if (name === "users") loadUsers();
  }

  async function loadProducts() {
    if (!accessToken) return showLogin("Нужно войти");

    productsStatus.textContent = "Загрузка...";
    productsTbody.innerHTML = "";

    try {
      const list = await apiFetch(R.productsList, { auth: true });
      const products = Array.isArray(list) ? list : [];

      productsStatus.textContent = `Загружено: ${products.length}`;
      renderProducts(products);
    } catch (e) {
      productsStatus.textContent = `Ошибка: ${e.message || e}`;
      if (e.status === 401) forceRelogin();
    }
  }

  async function searchProductById() {
    if (!accessToken) return showLogin("Нужно войти");

    const id = Number(productSearchId?.value || 0);
    if (!id) return loadProducts();

    productsStatus.textContent = `Поиск товара #${id}...`;
    productsTbody.innerHTML = "";

    try {
      const product = await apiFetch(R.productGetById(id), { auth: true });
      productsStatus.textContent = `Найден товар #${id}`;
      renderProducts([product]);
    } catch (e) {
      if (e.status === 404) {
        productsStatus.textContent = `Товар #${id} не найден`;
        productsTbody.innerHTML = "";
        return;
      }
      productsStatus.textContent = `Ошибка: ${e.message || e}`;
      if (e.status === 401) forceRelogin();
    }
  }

  function renderProducts(products) {
    productsTbody.innerHTML = (products || []).map(p => {
      const id = p.id ?? p.Id;
      const name = p.name ?? p.Name ?? "";
      const price = Number(p.price ?? p.Price ?? 0);
      const stock = Number(p.stock ?? p.Stock ?? 0);
      const isActive = Boolean(p.isActive ?? p.IsActive);
      const isDeleted = Boolean(p.isDeleted ?? p.IsDeleted);
      const imageUrl = p.imageUrl ?? p.ImageUrl ?? "";

      return `
        <tr data-id="${id}">
          <td>${id}</td>
          <td><input data-field="name" value="${name}" /></td>
          <td><input data-field="price" type="number" step="0.01" value="${price.toFixed(2)}" /></td>
          <td><input data-field="stock" type="number" step="1" value="${stock}" /></td>
          <td><input data-field="active" type="checkbox" ${isActive ? "checked" : ""} /></td>
          <td>${isDeleted ? "🗑️" : "—"}</td>
          <td><input data-field="imageUrl" value="${imageUrl}" placeholder="https://..." /></td>
          <td class="small">
            <button class="btn" data-save type="button">Сохранить</button>
            <button class="btn" data-del type="button" ${isDeleted ? "disabled" : ""}>Удалить</button>
            <button class="btn" data-ret type="button" ${isDeleted ? "" : "disabled"}>Вернуть</button>
            <div class="hint" data-rowmsg></div>
          </td>
        </tr>
      `;
    }).join("");

    productsTbody.querySelectorAll("[data-save]").forEach(btn => {
      btn.addEventListener("click", async () => {
        const tr = btn.closest("tr");
        const id = Number(tr.dataset.id);
        const msg = tr.querySelector("[data-rowmsg]");
        msg.textContent = "";
        msg.className = "hint";

        const name = tr.querySelector('[data-field="name"]').value.trim();
        const price = Number(tr.querySelector('[data-field="price"]').value);
        const stock = Number(tr.querySelector('[data-field="stock"]').value);
        const active = !!tr.querySelector('[data-field="active"]').checked;
        const imageUrl = tr.querySelector('[data-field="imageUrl"]').value.trim();

        try {
          await apiFetch(R.productPatchName(id), { method: "PATCH", auth: true, body: { name } });
          await apiFetch(R.productPatchPrice(id), { method: "PATCH", auth: true, body: { price } });
          await apiFetch(R.productPatchStock(id), { method: "PATCH", auth: true, body: { stock } });
          await apiFetch(R.productPatchActive(id), { method: "PATCH", auth: true, body: { isActive: active } });
          await apiFetch(R.productPatchImage(id), { method: "PATCH", auth: true, body: { imageUrl: imageUrl || null } });

          msg.textContent = "Сохранено ✅";
          msg.className = "hint ok";

          const currentSearch = Number(productSearchId?.value || 0);
          if (currentSearch) searchProductById();
          else loadProducts();
        } catch (e) {
          msg.textContent = e.message || "Ошибка";
          msg.className = "hint error";
          if (e.status === 401) forceRelogin();
        }
      });
    });

    productsTbody.querySelectorAll("[data-del]").forEach(btn => {
      btn.addEventListener("click", async () => {
        const tr = btn.closest("tr");
        const id = Number(tr.dataset.id);
        const msg = tr.querySelector("[data-rowmsg]");
        msg.textContent = "";
        msg.className = "hint";

        try {
          await apiFetch(R.productDelete(id), { method: "DELETE", auth: true });
          msg.textContent = "Удалено (soft) ✅";
          msg.className = "hint ok";

          const currentSearch = Number(productSearchId?.value || 0);
          if (currentSearch) searchProductById();
          else loadProducts();
        } catch (e) {
          msg.textContent = e.message || "Ошибка";
          msg.className = "hint error";
          if (e.status === 401) forceRelogin();
        }
      });
    });

    productsTbody.querySelectorAll("[data-ret]").forEach(btn => {
      btn.addEventListener("click", async () => {
        const tr = btn.closest("tr");
        const id = Number(tr.dataset.id);
        const msg = tr.querySelector("[data-rowmsg]");
        msg.textContent = "";
        msg.className = "hint";

        try {
          await apiFetch(R.productReturn(id), { method: "PATCH", auth: true });
          msg.textContent = "Восстановлено ✅";
          msg.className = "hint ok";

          const currentSearch = Number(productSearchId?.value || 0);
          if (currentSearch) searchProductById();
          else loadProducts();
        } catch (e) {
          msg.textContent = e.message || "Ошибка";
          msg.className = "hint error";
          if (e.status === 401) forceRelogin();
        }
      });
    });
  }

  async function createProduct() {
    if (!accessToken) return showLogin("Нужно войти");

    setCreateMsg("");

    const name = String(newName.value || "").trim();
    const price = Number(newPrice.value || 0);
    const stock = Number(newStock.value || 0);
    const imageUrl = String(newImageUrl.value || "").trim();
    const isActive = !!newActive.checked;

    if (!name) return setCreateMsg("Название обязательно");
    if (Number.isNaN(price) || price <= 0) return setCreateMsg("Цена некорректна");
    if (Number.isNaN(stock) || stock < 0) return setCreateMsg("Stock некорректен");

    try {
      await apiFetch(R.productCreate, {
        method: "POST",
        auth: true,
        body: { name, price, stock, imageUrl: imageUrl || null, isActive }
      });

      setCreateMsg("Создано ✅", true);

      newName.value = "";
      newPrice.value = "";
      newStock.value = "";
      newImageUrl.value = "";
      newActive.checked = true;

      if (productSearchId) productSearchId.value = "";
      loadProducts();
    } catch (e) {
      setCreateMsg(e.message || "Ошибка создания");
      if (e.status === 401) forceRelogin();
    }
  }

  function setCreateMsg(text, ok = false) {
    createMsg.textContent = text || "";
    createMsg.className = "hint " + (ok ? "ok" : "error");
  }

  async function loadOrders() {
    if (!accessToken) return showLogin("Нужно войти");

    ordersStatus.textContent = "Загрузка...";
    ordersTbody.innerHTML = "";

    try {
      const list = await apiFetch(R.ordersList, { auth: true });
      const orders = Array.isArray(list) ? list : [];

      ordersStatus.textContent = `Загружено: ${orders.length}`;
      renderOrders(orders);
    } catch (e) {
      ordersStatus.textContent = `Ошибка: ${e.message || e}`;
      if (e.status === 401) forceRelogin();
    }
  }

  async function searchOrderById() {
    if (!accessToken) return showLogin("Нужно войти");

    const id = Number(orderSearchId?.value || 0);
    if (!id) return loadOrders();

    ordersStatus.textContent = `Поиск заказа #${id}...`;
    ordersTbody.innerHTML = "";

    try {
      const order = await apiFetch(R.orderGetById(id), { auth: true });
      ordersStatus.textContent = `Найден заказ #${id}`;
      renderOrders([order]);
    } catch (e) {
      if (e.status === 404) {
        ordersStatus.textContent = `Заказ #${id} не найден`;
        ordersTbody.innerHTML = "";
        return;
      }
      ordersStatus.textContent = `Ошибка: ${e.message || e}`;
      if (e.status === 401) forceRelogin();
    }
  }

  function renderOrders(orders) {
    ordersTbody.innerHTML = (orders || []).map(o => {
      const id = o.id ?? o.Id;
      const userId = o.userId ?? o.UserId ?? "";
      const createdAt = o.createdAt ?? o.CreatedAt;
      const status = o.status ?? o.Status ?? "";
      const items = o.items ?? o.Items ?? [];

      const itemsText = (items || []).map(i =>
        `${i.productName ?? i.ProductName ?? ""} × ${i.quantity ?? i.Quantity ?? 0}`
      ).join(", ");

      const options = ORDER_STATUSES.map(s =>
        `<option value="${s}" ${s === status ? "selected" : ""}>${s}</option>`
      ).join("");

      return `
        <tr data-id="${id}">
          <td>${id}</td>
          <td class="small">${userId}</td>
          <td class="small">${new Date(createdAt).toLocaleString()}</td>
          <td><select data-status>${options}</select></td>
          <td class="small">${itemsText}</td>
          <td class="small">
            <button class="btn" data-save-order type="button">Сохранить</button>
            <div class="hint" data-rowmsg></div>
          </td>
        </tr>
      `;
    }).join("");

    ordersTbody.querySelectorAll("[data-save-order]").forEach(btn => {
      btn.addEventListener("click", async () => {
        const tr = btn.closest("tr");
        const id = Number(tr.dataset.id);
        const msg = tr.querySelector("[data-rowmsg]");
        const status = tr.querySelector("[data-status]").value;

        msg.textContent = "";
        msg.className = "hint";

        try {
          await apiFetch(R.orderPatchStatus(id), {
            method: "PATCH",
            auth: true,
            body: { status }
          });

          msg.textContent = "Обновлено ✅";
          msg.className = "hint ok";

          const currentSearch = Number(orderSearchId?.value || 0);
          if (currentSearch) searchOrderById();
          else loadOrders();
        } catch (e) {
          msg.textContent = e.message || "Ошибка";
          msg.className = "hint error";
          if (e.status === 401) forceRelogin();
        }
      });
    });
  }

  async function loadUsers() {
    if (!accessToken) return showLogin("Нужно войти");

    usersStatus.textContent = "Загрузка...";
    usersTbody.innerHTML = "";

    try {
      const list = await apiFetch(R.usersList, { auth: true });
      const users = Array.isArray(list) ? list : [];

      usersStatus.textContent = `Загружено: ${users.length}`;

      usersTbody.innerHTML = users.map(u => {
        const id = u.id ?? u.Id;
        const email = u.email ?? u.Email ?? "";
        return `
          <tr>
            <td class="small">${id}</td>
            <td>${email}</td>
          </tr>
        `;
      }).join("");
    } catch (e) {
      usersStatus.textContent = `Ошибка: ${e.message || e}`;
      if (e.status === 401) forceRelogin();
    }
  }

  async function apiFetch(path, { method = "GET", body = null, auth = true } = {}) {
    const headers = { "Content-Type": "application/json" };

    if (auth) {
      if (!accessToken) {
        const err = new Error("Нужно войти");
        err.status = 401;
        throw err;
      }
      headers.Authorization = `Bearer ${accessToken}`;
    }

    const res = await fetch(`${API_BASE}${path}`, {
      method,
      headers,
      body: body ? JSON.stringify(body) : null
    });

    const text = await res.text().catch(() => "");
    let data;
    try { data = text ? JSON.parse(text) : null; } catch { data = text; }

    if (!res.ok) {
      const msg = typeof data === "string" ? data : (data?.message || data?.error || text);
      const err = new Error(msg || `HTTP ${res.status}`);
      err.status = res.status;
      throw err;
    }
    return data;
  }

  function forceRelogin() {
    accessToken = null;
    showLogin("Сессия истекла — войди заново");
  }

  function decodeJwt(token) {
    try {
      const payload = token.split(".")[1];
      return JSON.parse(atob(payload.replace(/-/g, "+").replace(/_/g, "/")));
    } catch {
      return null;
    }
  }

  function getRolesFromToken(token) {
    const p = decodeJwt(token);
    if (!p) return [];

    const roles = p["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ?? p.role ?? p.roles;
    if (!roles) return [];
    return Array.isArray(roles) ? roles : [roles];
  }

  function isAdminToken(token) {
    return getRolesFromToken(token).some(r => String(r).toLowerCase() === "admin");
  }

  function setLoginMsg(text) {
    loginMsg.textContent = text || "";
    loginMsg.className = "hint" + (text ? " error" : "");
  }

});
