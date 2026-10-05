const apiBaseUrl = (window.TECHSTORE_API_BASE_URL || "http://localhost:5175").replace(/\/+$/, "");
const endpoint = `${apiBaseUrl}/api/products`;
const form = document.querySelector("#product-form");
const list = document.querySelector("#product-list");
const status = document.querySelector("#status");
const count = document.querySelector("#product-count");
const submitButton = document.querySelector("#submit-button");
const cancelButton = document.querySelector("#cancel-button");
const refreshButton = document.querySelector("#refresh-button");
const formTitle = document.querySelector("#form-title");
const formDescription = document.querySelector("#form-description");
const money = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });
let products = [];
let editingId = null;
let busy = false;

function showStatus(message, error = false) {
  status.textContent = message;
  status.classList.toggle("error", error);
}

function setBusy(value) {
  busy = value;
  submitButton.disabled = value;
  cancelButton.disabled = value;
  refreshButton.disabled = value;
  list.querySelectorAll("button").forEach(button => { button.disabled = value; });
}

async function request(url, options) {
  let response;
  try {
    response = await fetch(url, options);
  } catch {
    throw new Error("Não foi possível conectar à API. Confira o endereço em config.js e o CORS da API.");
  }
  if (!response.ok) {
    const body = await response.json().catch(() => null);
    const validation = body?.errors && Object.values(body.errors).flat().join(" ");
    throw new Error(validation || body?.detail || body?.title || `A API retornou erro ${response.status}.`);
  }
  return response.status === 204 ? null : response.json();
}

function resetForm() {
  editingId = null;
  form.reset();
  formTitle.textContent = "Novo produto";
  formDescription.textContent = "Preencha os dados abaixo.";
  submitButton.textContent = "Cadastrar produto";
  cancelButton.hidden = true;
}

function startEdit(product) {
  editingId = product.id;
  form.elements.name.value = product.name;
  form.elements.description.value = product.description || "";
  form.elements.price.value = product.price;
  form.elements.stockQuantity.value = product.stockQuantity;
  formTitle.textContent = "Editar produto";
  formDescription.textContent = `Alterando o produto #${product.id}.`;
  submitButton.textContent = "Salvar alterações";
  cancelButton.hidden = false;
  showStatus("");
  document.querySelector("#name").focus();
  if (window.innerWidth < 761) form.scrollIntoView({ behavior: "smooth", block: "start" });
}

function render() {
  list.replaceChildren();
  count.textContent = `${products.length} ${products.length === 1 ? "produto" : "produtos"}`;
  if (products.length === 0) {
    const empty = document.createElement("p");
    empty.className = "empty";
    empty.textContent = "Nenhum produto cadastrado ainda.";
    list.append(empty);
    return;
  }

  for (const product of products) {
    const article = document.createElement("article");
    article.className = "product";
    const top = document.createElement("div");
    top.className = "product-top";
    const name = document.createElement("h3");
    name.textContent = product.name;
    const price = document.createElement("span");
    price.className = "product-price";
    price.textContent = money.format(product.price);
    top.append(name, price);

    const description = document.createElement("p");
    description.className = "product-description";
    description.textContent = product.description || "Sem descrição";

    const footer = document.createElement("div");
    footer.className = "product-footer";
    const stock = document.createElement("span");
    stock.className = "stock";
    stock.textContent = `${product.stockQuantity} em estoque`;
    const actions = document.createElement("div");
    actions.className = "product-actions";
    const edit = document.createElement("button");
    edit.className = "button secondary";
    edit.type = "button";
    edit.textContent = "Editar";
    edit.setAttribute("aria-label", `Editar ${product.name}`);
    edit.addEventListener("click", () => startEdit(product));
    const remove = document.createElement("button");
    remove.className = "button danger";
    remove.type = "button";
    remove.textContent = "Excluir";
    remove.setAttribute("aria-label", `Excluir ${product.name}`);
    remove.addEventListener("click", () => deleteProduct(product));
    actions.append(edit, remove);
    footer.append(stock, actions);
    article.append(top, description, footer);
    list.append(article);
  }
}

async function loadProducts() {
  if (busy) return;
  setBusy(true);
  showStatus("Carregando produtos...");
  try {
    products = await request(endpoint);
    render();
    showStatus("");
  } catch (error) {
    showStatus(error.message, true);
  } finally {
    setBusy(false);
  }
}

async function deleteProduct(product) {
  if (busy || !window.confirm(`Excluir “${product.name}”?`)) return;
  setBusy(true);
  try {
    await request(`${endpoint}/${product.id}`, { method: "DELETE" });
    products = products.filter(item => item.id !== product.id);
    if (editingId === product.id) resetForm();
    render();
    showStatus("Produto excluído.");
  } catch (error) {
    showStatus(error.message, true);
  } finally {
    setBusy(false);
  }
}

form.addEventListener("submit", async event => {
  event.preventDefault();
  if (busy || !form.reportValidity()) return;
  const payload = {
    name: form.elements.name.value.trim(),
    description: form.elements.description.value.trim(),
    price: Number(form.elements.price.value),
    stockQuantity: Number(form.elements.stockQuantity.value)
  };
  if (!payload.name || !Number.isFinite(payload.price) || payload.price < 0 ||
      !Number.isInteger(payload.stockQuantity) || payload.stockQuantity < 0) {
    showStatus("Confira os campos obrigatórios, o preço e o estoque.", true);
    return;
  }
  const wasEditing = editingId !== null;
  setBusy(true);
  try {
    await request(wasEditing ? `${endpoint}/${editingId}` : endpoint, {
      method: wasEditing ? "PUT" : "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    });
    resetForm();
    products = await request(endpoint);
    render();
    showStatus(wasEditing ? "Produto atualizado." : "Produto cadastrado.");
  } catch (error) {
    showStatus(error.message, true);
  } finally {
    setBusy(false);
  }
});

cancelButton.addEventListener("click", resetForm);
refreshButton.addEventListener("click", loadProducts);
loadProducts();
