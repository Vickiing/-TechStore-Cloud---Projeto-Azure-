# Tela de produtos

`index.html`, `styles.css`, `app.js` e `config.js` formam uma página estática, sem compilação ou dependências. Ela consome a API da solução para listar, cadastrar, editar e excluir produtos.

Para testar localmente, execute a API com `dotnet run --project ../TechStore.Api --launch-profile http`. Sirva esta pasta em outra porta com um servidor HTTP estático (por exemplo, `python -m http.server 5500`) e acesse `http://localhost:5500`. A URL padrão da API é `http://localhost:5175`.

O `config.js` usa a API local durante o desenvolvimento e, fora de `localhost`/`127.0.0.1`, usa a URL HTTPS encontrada no perfil de publicação local da API. Confirme que essa URL continua ativa antes de publicar. Publique o conteúdo desta pasta na raiz do **Azure Blob Storage Static website** (contêiner `$web`, documento de índice `index.html`) ou como pasta da aplicação no **Azure Static Web Apps**. Adicione a origem exata da página publicada a `Cors:AllowedOrigins` na configuração da API (por exemplo, na variável de ambiente `Cors__AllowedOrigins__0`).

Esta página contém apenas os arquivos estáticos. Os dados dos produtos são mantidos pela API em um arquivo JSON. A API aceita `Products__FilePath` para definir o caminho desse arquivo no serviço hospedado; não há connection string de banco de dados nesta implementação.

A API atual não exige autenticação. Antes de expor a tela e a API publicamente, proteja os endpoints de escrita com uma solução de autenticação adequada; CORS controla o acesso pelo navegador, mas não substitui autenticação. O arquivo JSON da API também exige disco persistente e uma única instância do serviço.
