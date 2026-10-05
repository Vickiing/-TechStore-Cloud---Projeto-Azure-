// Endereço HTTPS encontrado no perfil de publicação local da API.
// Confirme a URL no Azure antes de publicar a tela.
const publishedApiUrl = "https://techstore-api-awfqa5a9c9f3f0ev.brazilsouth-01.azurewebsites.net";
window.TECHSTORE_API_BASE_URL = ["localhost", "127.0.0.1"].includes(window.location.hostname)
  ? "http://localhost:5175"
  : publishedApiUrl;
