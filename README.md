# FCGNotification - Microsserviço de Notificações

## Descrição

O **FCGNotification** é um microsserviço responsável por gerenciar e enviar notificações para usuários em tempo real. Ele consome eventos de outros microsserviços (como criação de usuários e processamento de pagamentos) através de uma fila de mensagens RabbitMQ e dispara notificações correspondentes.

Este microsserviço é parte de uma arquitetura de microsserviços distribuída baseada em eventos, utilizando **MassTransit** para a integração com **RabbitMQ**.

---

## Funcionalidades Principais

- **Notificações de Bem-vindo**: Envia email de boas-vindas quando um novo usuário é criado
- **Confirmação de Compra**: Envia confirmação de compra quando um pagamento é processado com sucesso
- **Consumo de Eventos Assíncrono**: Processa eventos de forma assíncrona através de filas RabbitMQ
- **API REST**: Expõe endpoints para testes e validação da integração (Simulador)

---

## Tecnologias Utilizadas

| Tecnologia | Versão | Propósito |
|------------|--------|----------|
| .NET | 8.0 | Framework principal |
| MassTransit | 8.3.5 | Message broker abstraction |
| RabbitMQ | 3.x | Message broker |
| Swagger/Swashbuckle | 10.2.1 | Documentação da API |
| ASP.NET Core Web API | 10.0.6 | Framework Web |

---

## Dependências

- **MassTransit.RabbitMQ**: Integração com RabbitMQ
- **Microsoft.AspNetCore.OpenApi**: Suporte a OpenAPI
- **Swashbuckle.AspNetCore**: Geração automática da documentação Swagger

---

## Como Executar

### Pré-requisitos

- .NET 8.0 SDK ou superior instalado
- Docker e Docker Compose (para execução containerizada)
- RabbitMQ em execução (ou use o docker-compose fornecido)

### Opção 1: Executar com Docker Compose

```bash
# Na raiz do projeto FiapTC2
docker-compose up
```

Este comando inicia:
- **RabbitMQ** na porta 5672 (AMQP) e 15672 (Management UI)
- **FCGNotification API** na porta 5001

### Opção 2: Executar Localmente

1. Certifique-se de que o RabbitMQ está em execução:

```bash
# Windows (se RabbitMQ estiver instalado localmente)
rabbitmq-server

# Ou use Docker apenas para RabbitMQ
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

2. Configure as variáveis de ambiente em `appsettings.Development.json`:

```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest",
    "UserCreatedQueue": "user-created-queue",
    "NotificationPaymentProcessedQueue": "payment-processed-queue"
  }
}
```

3. Restaure as dependências e execute a aplicação:

```bash
cd NotificationsAPI
dotnet restore
dotnet run
```

A API estará disponível em `http://localhost:5000`

---

## Eventos Consumidos

### 1. UserCreatedEvent
Disparado quando um novo usuário é criado no microsserviço FCGUser.

**Payload:**
```json
{
  "userId": "uuid",
  "name": "string",
  "email": "string@example.com"
}
```

**Ação**: Envia email de boas-vindas ao usuário

**Fila**: `user-created-queue`

---

### 2. PaymentProcessedEvent
Disparado quando um pagamento é processado no microsserviço FCGPayment.

**Payload:**
```json
{
  "userId": "uuid",
  "gameId": "uuid",
  "price": "decimal",
  "status": "Approved|Declined"
}
```

**Ação**: Se o status for "Approved", envia confirmação de compra para o usuário

**Fila**: `payment-processed-queue`

**Exchange**: `payment.exchange` (Fanout)

---

## Endpoints da API

### Documentação Interativa
- **Swagger UI**: http://localhost:5000/swagger
- **OpenAPI JSON**: http://localhost:5000/swagger/v1/swagger.json

### Endpoints de Teste

#### Publicar Evento de Usuário Criado
```
GET /publish-user
```

**Resposta**:
```json
{
  "message": "Evento UserCreated enviado!"
}
```

Simula a criação de um novo usuário e dispara a notificação de boas-vindas.

---

#### Publicar Evento de Pagamento Processado
```
GET /publish-payment
```

**Resposta**:
```json
{
  "message": "Evento PaymentProcessed enviado!"
}
```

Simula o processamento de um pagamento aprovado e dispara a confirmação de compra.

---

## Estrutura do Projeto

```
FCGNotification/
├── NotificationsAPI/
│   ├── Consumers/
│   │   ├── UserCreatedConsumer.cs      # Consumer para eventos de usuário criado
│   │   └── PaymentProcessedConsumer.cs # Consumer para eventos de pagamento
│   ├── Events/
│   │   ├── UserCreatedEvent.cs         # Definição do evento de usuário
│   │   └── PaymentProcessedEvent.cs    # Definição do evento de pagamento
│   ├── Services/
│   │   └── NotificationService.cs      # Lógica de envio de notificações
│   ├── Program.cs                       # Configuração da aplicação
│   ├── NotificationsAPI.csproj          # Arquivo de projeto
│   ├── appsettings.json                 # Configurações padrão
│   └── appsettings.Development.json     # Configurações de desenvolvimento
├── Dockerfile                           # Build do container
└── docker-compose.yml                   # Orquestração de containers
```

---

## Configuração do RabbitMQ

### appsettings.json

```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest",
    "UserCreatedQueue": "user-created-queue",
    "NotificationPaymentProcessedQueue": "payment-processed-queue"
  }
}
```

### Variáveis de Ambiente

Você pode sobrescrever as configurações via variáveis de ambiente:

```bash
export RabbitMQ__Host=rabbitmq
export RabbitMQ__Username=guest
export RabbitMQ__Password=guest
export RabbitMQ__UserCreatedQueue=user-created-queue
export RabbitMQ__NotificationPaymentProcessedQueue=payment-processed-queue
```

---

## Testando a Aplicação

### 1. Acessar o RabbitMQ Management UI
- URL: http://localhost:15672
- Usuário: `guest`
- Senha: `guest`

Aqui você pode monitorar as filas, mensagens e conexões.

---

### 2. Testar com o Swagger
1. Acesse http://localhost:5000/swagger
2. Clique em "Try it out" para cada endpoint
3. Execute as requisições de teste

---

### 3. Testar com cURL

```bash
# Testar evento de usuário criado
curl -X GET http://localhost:5000/publish-user

# Testar evento de pagamento processado
curl -X GET http://localhost:5000/publish-payment
```

---

### 4. Monitorar Logs

Os logs são exibidos no console da aplicação, mostrando:
- Eventos recebidos
- Notificações enviadas
- Erros de processamento

Exemplo de saída:
```
[EMAIL] Bem-vindo enviado para usuário 12345 e email user@example.com
[EMAIL] Compra confirmada para usuário 12345
```

---

## Fluxo de Eventos

```
┌─────────────────────┐
│  FCGUser Service    │
└──────────┬──────────┘
           │ (PublishEvent: UserCreatedEvent)
           ▼
    ┌─────────────────┐
    │  RabbitMQ       │
    │ user-created    │
    │ -queue          │
    └────────┬────────┘
             │
             ▼
   ┌──────────────────────────────┐
   │  FCGNotification Service     │
   │  - UserCreatedConsumer       │
   │  - NotificationService       │
   │  - SendWelcomeEmail()        │
   └──────────────────────────────┘

┌─────────────────────┐
│  FCGPayment Service │
└──────────┬──────────┘
           │ (PublishEvent: PaymentProcessedEvent)
           ▼
    ┌──────────────────────────┐
    │  RabbitMQ                │
    │  payment.exchange        │
    │  (Fanout)                │
    └────────┬─────────────────┘
             │
             ▼
   ┌──────────────────────────────┐
   │  FCGNotification Service     │
   │  - PaymentProcessedConsumer  │
   │  - NotificationService       │
   │  - SendPurchaseConfirmation()│
   └──────────────────────────────┘
```

---

##  Relacionamento com Outros Microsserviços

- **FCGUser**: Publica `UserCreatedEvent` quando um novo usuário é registrado
- **FCGPayment**: Publica `PaymentProcessedEvent` quando um pagamento é processado
- **FCGCatalog**: Não tem integração direta (pode ser estendido no futuro)


---

**Última atualização**: 2026-07-12  
**Versão**: 1.0.0
