# 🚗 Sistema de Gerenciamento de Veículos com Cache Redis

## 📋 Visão Geral

Este projeto implementa uma API REST para gerenciamento de veículos utilizando **Arquitetura em Camadas**, **Injeção de Dependência**, **Cache Redis** e **Tratamento de Erros Robusto**. O sistema demonstra boas práticas de desenvolvimento .NET e padrões arquiteturais modernos.

## 🏗️ Arquitetura do Projeto

### **Padrão de Arquitetura: Camadas (Layered Architecture)**

```
┌─────────────────────────────────────┐
│           API Layer                 │ ← Controllers, Program.cs
├─────────────────────────────────────┤
│         Business Layer              │ ← Domain Models
├─────────────────────────────────────┤
│        Data Access Layer            │ ← Repository Pattern
├─────────────────────────────────────┤
│         Infrastructure              │ ← MySQL, Redis
└─────────────────────────────────────┘
```

### **Estrutura de Pastas**
```
GerenciamentoVeiculos/
├── Domain/                    # Camada de Domínio
│   └── Vehicle.cs            # Entidade do domínio
├── Repository/               # Camada de Acesso a Dados
│   ├── IVehicleRepository.cs # Interface (Contrato)
│   └── VehicleRepository.cs  # Implementação
├── performance-cache/        # Camada de Apresentação (API)
│   ├── Controllers/
│   │   └── VehicleController.cs
│   ├── Program.cs
│   └── appsettings.json
└── slnPerformance.sln       # Solution File
```

## 🎯 Conceitos Implementados

### 1. **Injeção de Dependência (Dependency Injection)**
- **O que é**: Padrão que permite que uma classe receba suas dependências externas em vez de criá-las internamente
- **Benefícios**: 
  - Facilita testes unitários
  - Reduz acoplamento entre classes
  - Melhora manutenibilidade
- **Implementação**: Usando o container DI nativo do .NET

### 2. **Repository Pattern**
- **O que é**: Padrão que abstrai a lógica de acesso a dados
- **Benefícios**:
  - Separação de responsabilidades
  - Facilita mudanças de banco de dados
  - Melhora testabilidade

### 3. **Cache Redis**
- **O que é**: Sistema de cache em memória para melhorar performance
- **Benefícios**:
  - Reduz carga no banco de dados
  - Melhora tempo de resposta
  - Escalabilidade

### 4. **Tratamento de Erros Robusto**
- **Middleware Global**: Captura exceções não tratadas
- **Logging Estruturado**: Registra eventos importantes
- **Respostas Padronizadas**: Formato consistente de erros

## 🛠️ Tecnologias Utilizadas

- **.NET 8.0** - Framework principal
- **ASP.NET Core Web API** - Para criar a API REST
- **MySQL** - Banco de dados principal
- **Redis** - Sistema de cache
- **Dapper** - Micro-ORM para acesso a dados
- **Swagger/OpenAPI** - Documentação da API
- **Serilog** - Sistema de logging

## 📦 Passo a Passo para Criar o Projeto

### **Passo 1: Criar a Solution e Projetos**

```bash
# Criar a solution
dotnet new sln -n GerenciamentoVeiculos

# Criar projeto Domain (Class Library)
dotnet new classlib -n Domain
dotnet sln add Domain/Domain.csproj

# Criar projeto Repository (Class Library)
dotnet new classlib -n Repository
dotnet sln add Repository/Repository.csproj

# Criar projeto API (Web API)
dotnet new webapi -n performance-cache
dotnet sln add performance-cache/performance-cache.csproj
```

### **Passo 2: Configurar Dependências entre Projetos**

```bash
# Repository depende de Domain
cd Repository
dotnet add reference ../Domain/Domain.csproj

# API depende de Repository e Domain
cd ../performance-cache
dotnet add reference ../Repository/Repository.csproj
dotnet add reference ../Domain/Domain.csproj
```

### **Passo 3: Instalar Pacotes NuGet**

```bash
# No projeto Repository
cd Repository
dotnet add package Dapper
dotnet add package MySqlConnector

# No projeto API
cd ../performance-cache
dotnet add package StackExchange.Redis
dotnet add package Newtonsoft.Json
```

### **Passo 4: Criar a Entidade Vehicle (Domain Layer)**

**Domain/Vehicle.cs**
```csharp
namespace Domain
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Plate { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }
}
```

### **Passo 5: Criar Interface do Repository**

**Repository/IVehicleRepository.cs**
```csharp
using Domain;

namespace Repository
{
    public interface IVehicleRepository
    {
        Task<IEnumerable<Vehicle>> GetAllVehiclesAsync();
        Task<Vehicle> AddVehicleAsync(Vehicle vehicle);
        Task UpdateVehicleAsync(Vehicle vehicle);
        Task DeleteVehicleAsync(int id);
    }
}
```

### **Passo 6: Implementar o Repository**

**Repository/VehicleRepository.cs**
```csharp
using Dapper;
using Domain;
using MySqlConnector;

namespace Repository
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly string _connectionString;

        public VehicleRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Vehicle>> GetAllVehiclesAsync()
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            return await connection.QueryAsync<Vehicle>("SELECT * FROM vehicle");
        }

        public async Task<Vehicle> AddVehicleAsync(Vehicle vehicle)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"
                INSERT INTO vehicle (brand, model, year, plate, color)
                VALUES (@Brand, @Model, @Year, @Plate, @Color);
                SELECT LAST_INSERT_ID();
            ";

            int newId = await connection.QuerySingleAsync<int>(sql, vehicle);
            vehicle.Id = newId;
            return vehicle;
        }

        public async Task UpdateVehicleAsync(Vehicle vehicle)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"
                UPDATE vehicle
                SET brand = @Brand, model = @Model, year = @Year, 
                    plate = @Plate, color = @Color
                WHERE id = @Id;
            ";

            await connection.ExecuteAsync(sql, vehicle);
        }

        public async Task DeleteVehicleAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = "DELETE FROM vehicle WHERE id = @Id;";
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }
}
```

### **Passo 7: Configurar Injeção de Dependência**

**performance-cache/Program.cs**
```csharp
using Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🔥 INJEÇÃO DE DEPENDÊNCIA
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                           ?? "Server=localhost;Database=fiap;User=root;Password=123;Port=3306;";
    
    return new VehicleRepository(connectionString);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### **Passo 8: Configurar Connection Strings**

**performance-cache/appsettings.json**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=fiap;User=root;Password=123;Port=3306;",
    "RedisConnection": "localhost:6379"
  }
}
```

### **Passo 9: Criar Tabela no MySQL**

```sql
CREATE DATABASE fiap;
USE fiap;

CREATE TABLE vehicle (
    id INT AUTO_INCREMENT PRIMARY KEY,
    brand VARCHAR(100) NOT NULL,
    model VARCHAR(100) NOT NULL,
    year INT NOT NULL,
    plate VARCHAR(10) NOT NULL UNIQUE,
    color VARCHAR(50) NOT NULL
);

-- Inserir dados de exemplo
INSERT INTO vehicle (brand, model, year, plate, color) VALUES
('Toyota', 'Corolla', 2023, 'ABC-1234', 'Branco'),
('Honda', 'Civic', 2022, 'DEF-5678', 'Prata'),
('Ford', 'Focus', 2021, 'GHI-9012', 'Azul');
```

## 🚀 Como Executar

### **Pré-requisitos**
- .NET 8.0 SDK
- MySQL Server
- Redis Server

### **Executar o Projeto**
```bash
# Navegar para o projeto API
cd performance-cache

# Restaurar dependências
dotnet restore

# Executar o projeto
dotnet run
```

### **Acessar a API**
- **Swagger UI**: `https://localhost:7000/swagger`
- **API Base**: `https://localhost:7000/api/vehicle`

## 📚 Endpoints da API

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/vehicle` | Lista todos os veículos |
| POST | `/api/vehicle` | Cria um novo veículo |
| PUT | `/api/vehicle/{id}` | Atualiza um veículo |
| DELETE | `/api/vehicle/{id}` | Exclui um veículo |

## 🎓 Conceitos de Arquitetura Explicados

### **1. Separação de Responsabilidades**
- **Controller**: Recebe requisições HTTP e coordena operações
- **Repository**: Gerencia acesso a dados
- **Domain**: Contém as entidades de negócio

### **2. Inversão de Dependência**
- Controller depende da **interface** `IVehicleRepository`, não da implementação
- Facilita testes e mudanças de implementação

### **3. Cache Strategy**
- **Cache-Aside Pattern**: Aplicação gerencia o cache
- **TTL (Time To Live)**: Cache expira em 20 minutos
- **Cache Invalidation**: Cache é limpo quando dados são modificados

### **4. Error Handling**
- **Try-Catch**: Captura exceções específicas
- **Logging**: Registra eventos importantes
- **Graceful Degradation**: Sistema continua funcionando mesmo se Redis falhar

## 🔍 Benefícios da Arquitetura

1. **Manutenibilidade**: Código organizado e fácil de modificar
2. **Testabilidade**: Cada camada pode ser testada independentemente
3. **Escalabilidade**: Fácil adicionar novas funcionalidades
4. **Performance**: Cache Redis melhora tempo de resposta
5. **Robustez**: Tratamento de erros evita falhas do sistema

## 📖 Próximos Passos

- Implementar testes unitários
- Adicionar autenticação/autorização
- Implementar paginação
- Adicionar validação de dados com FluentValidation
- Implementar Health Checks
- Adicionar métricas com Application Insights

---

**Desenvolvido para fins educacionais - FIAP** 🎓


# 🎯 Exercícios Práticos - Sistema de Gerenciamento de Veículos

## 📚 Objetivo dos Exercícios

Este documento contém exercícios práticos para consolidar os conceitos de **Arquitetura em Camadas**, **Injeção de Dependência**, **Repository Pattern**, **Cache Redis** e **Tratamento de Erros** aprendidos no projeto principal.

---

## 🏗️ Exercício 1: Implementar Sistema de Categorias de Veículos

### **Contexto**
Você precisa expandir o sistema para incluir categorias de veículos (SUV, Sedan, Hatchback, etc.).


## 📚 Recursos de Aprendizado

### **Documentação Oficial**
- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/)
- [Redis Documentation](https://redis.io/documentation)
- [MySQL Documentation](https://dev.mysql.com/doc/)

### **Padrões e Boas Práticas**
- [Repository Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [Dependency Injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

### **Ferramentas**
- [Dapper](https://dapper-tutorial.net/)
- [FluentValidation](https://docs.fluentvalidation.net/)
- [Serilog](https://serilog.net/)
- [xUnit](https://xunit.net/)

---

## 🎯 Dicas para Resolução

### **1. Comece Simples**
- Implemente uma funcionalidade por vez
- Teste cada etapa antes de prosseguir
- Use o projeto principal como referência

### **2. Siga os Padrões**
- Mantenha a separação de camadas
- Use injeção de dependência
- Implemente tratamento de erros

### **3. Teste Constantemente**
- Execute testes após cada mudança
- Valide a funcionalidade manualmente
- Use o Swagger para testar a API

### **4. Documente o Processo**
- Comente o código
- Atualize o README
- Registre decisões arquiteturais

---

**Boa sorte com os exercícios! 🚀**

*Lembre-se: a prática leva à perfeição. Use estes exercícios para consolidar seu conhecimento e se preparar para projetos mais complexos.*
