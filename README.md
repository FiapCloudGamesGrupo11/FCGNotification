# FCGNotification

Função serverless em .NET 8 responsável por processar notificações da plataforma FIAP Cloud Games. O projeto foi preparado para execução como AWS Lambda no LocalStack, sem provisionar recursos na AWS real.

## Arquitetura

```text
FCGUser ----------------> SQS user-created -------------------------+
                                                                   |
                                                                   v
                                                        FCGNotification Lambda
                                                                   ^
                                                                   |
FCGPayment -------------> SQS notification-payment-processed ------+
```

A mesma função recebe eventos de duas filas. O ARN presente em cada mensagem SQS identifica o contrato que deve ser desserializado.

## Eventos consumidos

### UserCreatedEvent

Fila padrão: `user-created`

```json
{
  "userId": "a81bc81b-dead-4e5d-abff-90865d1e13b1",
  "name": "André",
  "email": "andre@example.com"
}
```

A função simula o envio de um e-mail de boas-vindas.

### PaymentProcessedEvent

Fila padrão: `notification-payment-processed`

```json
{
  "orderId": "order-123",
  "paymentId": "payment-123",
  "userId": "user-123",
  "gameId": "game-123",
  "amount": 99.90,
  "status": "Approved",
  "reason": null,
  "processedAt": "2026-09-08T12:00:00Z"
}
```

Um pagamento com status `Approved` gera uma confirmação. Qualquer outro status gera uma notificação de recusa com o motivo recebido.

## Processamento em lote

O handler recebe `SQSEvent` e processa todas as mensagens do lote. O retorno utiliza `SQSBatchResponse`: quando uma mensagem apresenta erro, somente o seu identificador é devolvido como falha. Com `ReportBatchItemFailures` configurado no mapeamento da Lambda, as mensagens processadas com sucesso não são repetidas.

## Configuração

Os nomes das filas podem ser substituídos por variáveis de ambiente:

| Variável | Valor padrão |
|---|---|
| `USER_CREATED_QUEUE` | `user-created` |
| `PAYMENT_PROCESSED_QUEUE` | `notification-payment-processed` |

O handler usado na criação da função é:

```text
NotificationsAPI::NotificationsAPI.Function::FunctionHandler
```

## Compilação

Pré-requisitos:

- .NET SDK 8
- Docker e Docker Compose para a execução integrada com LocalStack

Na raiz do repositório:

```powershell
dotnet restore .\NotificationsAPI\NotificationsAPI.csproj
dotnet build .\NotificationsAPI\NotificationsAPI.csproj --configuration Release
dotnet publish .\NotificationsAPI\NotificationsAPI.csproj --configuration Release --output .\artifacts\publish
```

O provisionamento do LocalStack, das filas e dos gatilhos pertence ao repositório FCGInfra e será documentado nele. Esta separação permite que este repositório concentre a lógica da função, enquanto o repositório de infraestrutura controla sua execução.

## Estrutura

```text
FCGNotification/
├── NotificationsAPI/
│   ├── Events/
│   │   ├── PaymentProcessedEvent.cs
│   │   └── UserCreatedEvent.cs
│   ├── Services/
│   │   └── NotificationService.cs
│   ├── Function.cs
│   ├── NotificationsAPI.csproj
│   └── aws-lambda-tools-defaults.json
└── README.md
```

## Execução local

Esta função não expõe Swagger nem endpoints HTTP. Na etapa de infraestrutura, os testes serão feitos enviando mensagens para as filas SQS do LocalStack e acompanhando os logs da Lambda.
