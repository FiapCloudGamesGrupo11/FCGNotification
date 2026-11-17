# CI/CD Workflow - FCGNotification Lambda

## 📋 Resumo do Fluxo

Este workflow automatiza o build, teste e deploy da função Lambda `FCG-Notification` sempre que há um push para `main` ou branches `feature/**`.

## 🔄 Fluxo de Execução

```
Push no GitHub (main ou feature/**)
         ↓
   ┌─────────────────────────────────┐
   │   Job 1: build-test             │
   ├─────────────────────────────────┤
   │ ✓ Setup .NET 8.0                │
   │ ✓ Restaurar dependências        │
   │ ✓ Build Release                 │
   │ ✓ Publish para Lambda           │
   └─────────────────────────────────┘
         ↓ (sucesso)
   ┌─────────────────────────────────┐
   │   Job 2: deploy-to-lambda       │
   ├─────────────────────────────────┤
   │ ✓ Autenticar via OIDC           │
   │ ✓ Restaurar dependências        │
   │ ✓ Publish Release               │
   │ ✓ Zipar artefatos               │
   │ ✓ Update Lambda code            │
   │ ✓ Publish nova versão           │
   └─────────────────────────────────┘
```

## 🔑 Configurações Importantes

### OIDC (GitHub ↔ AWS)
- **Role**: `arn:aws:iam::432342421632:role/GithubActionsOidcRole`
- **Região**: `us-east-1`
- **Status**: ✓ Já configurado

### Lambda
- **Função**: `FCG-Notification`
- **Runtime**: `dotnet8`
- **Handler**: `NotificationsAPI::NotificationsAPI.Function::FunctionHandler`

## 📊 Triggers do Workflow

| Evento | Branches | Ação |
|--------|----------|------|
| **Push** | `main` | ✓ Build + Deploy |
| **Push** | `feature/**` | ✓ Build + Deploy |
| **Pull Request** | `main` | ✓ Apenas Build/Test |

## 📦 Artefatos Gerados

1. **Build Output**: `./publish` (pasta compilada)
2. **ZIP**: `lambda-function.zip` (enviado para Lambda)

## 🚀 Como o Deploy Funciona

### Etapa 1: Autenticação
```bash
AWS OIDC Token → assume role GithubActionsOidcRole
```

### Etapa 2: Build & Publish
```bash
dotnet restore
dotnet publish --configuration Release
```

### Etapa 3: Zipar & Upload
```bash
zip -r lambda-function.zip .
aws lambda update-function-code --zip-file fileb://lambda-function.zip
```

### Etapa 4: Versioning
```bash
aws lambda publish-version --description "Build {sha} - Branch {ref}"
```

## ✅ Validações

- **NuGet Audit**: ✓ Verifica vulnerabilidades em pacotes (`-warnaserror`)
- **Build Release**: ✓ Apenas configuração Release
- **Wait Lambda**: ✓ Aguarda update ser concluído antes de continuar

## 📝 Logs & Debugging

Para ver os logs do workflow:
1. Vá para GitHub → Actions
2. Selecione o último workflow executado
3. Clique em `build-test` ou `deploy-to-lambda`
4. Verifique os logs de cada step

## 🔧 Como Modificar

### Mudar o nome da função Lambda
Edite em `.github/workflows/ci.yml`:
```yaml
env:
  LAMBDA_FUNCTION_NAME: seu-novo-nome
```

### Mudar a região AWS
Edite em `.github/workflows/ci.yml`:
```yaml
aws-region: sua-region
```

### Adicionar variáveis de ambiente para a Lambda
Adicione um step antes do deploy:
```yaml
- name: Update Lambda Environment Variables
  run: |
    aws lambda update-function-configuration \
      --function-name FCG-Notification \
      --environment Variables={VAR1=value1,VAR2=value2}
```

## 🛑 Troubleshooting

### Erro: "User is not authorized to perform..."
- Verifique se a role OIDC tem permissões de Lambda
- Consulte o gerente de infraestrutura

### Erro: "Function code size is too large"
- O ZIP não pode exceder 50 MB
- Remova dependências não utilizadas do .csproj

### Timeout no deploy
- Aumente `--timeout=10m` em `kubectl rollout status` (se houver)
- Para Lambda, o timeout é geralmente rápido

## 📞 Contato

Dúvidas sobre este workflow? Consulte a documentação AWS:
- https://docs.aws.amazon.com/lambda/latest/dg/API_UpdateFunctionCode.html
- https://docs.aws.amazon.com/lambda/latest/dg/API_PublishVersion.html
