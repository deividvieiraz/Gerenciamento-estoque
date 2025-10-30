# Performance Cache - API

Este projeto simula um sistema de gerenciamento de produtos e movimentação de estoque, com integração ao Redis para cache e persistência no banco de dados. O sistema oferece funcionalidades para cadastrar produtos, realizar movimentações de estoque e gerar relatórios de status.

## Regras de Negócio Implementadas

1. **Cadastro de Produto**:
   - Cada produto possui um código SKU, nome, categoria, preço unitário e quantidade mínima.
   - Produtos perecíveis devem possuir número de lote e data de validade.
   - Validação de categoria (para produtos perecíveis): Caso seja um produto perecível, é necessário informar o lote e a data de validade.
   - Ao cadastrar um produto, é verificado se todas as informações obrigatórias estão preenchidas corretamente.

2. **Movimentação de Estoque**:
   - As movimentações podem ser de entrada ou saída.
   - Para entradas (`INBOUND`), a quantidade de estoque é aumentada.
   - Para saídas (`OUTBOUND`), é verificado se há estoque suficiente antes de realizar a movimentação.
   - Produtos perecíveis têm a validade verificada para garantir que não sejam movidos com data de validade expirando.

3. **Relatórios**:
   - **Valor total do estoque**: Calculado como a soma do valor de cada produto (quantidade * preço unitário).
   - **Produtos prestes a vencer**: Lista os produtos perecíveis com data de validade dentro dos próximos 7 dias.
   - **Produtos com estoque abaixo do mínimo**: Gera alerta para produtos que estão abaixo da quantidade mínima definida.

## Entidades

- A entidade `Product` representa os produtos do sistema, incluindo atributos como código SKU, nome, categoria (perishável ou não), preço unitário e a quantidade mínima.
- A entidade `StockMovement` representa as movimentações de estoque, com tipo de movimentação (`INBOUND` ou `OUTBOUND`), quantidade, data, lote (para produtos perecíveis) e data de validade.
- A entidade `Category` define os tipos de categoria de produto (por exemplo, `PERISHABLE` e `NON_PERISHABLE`).

## Exemplos de Requisições API

### 1. **Cadastrar Produto**
**Endpoint**: `POST /api/product`

**Request Body**:
```json
{
  "SKUCode": 12345,
  "Name": "Produto Perecível",
  "Category": "PERISHABLE",
  "UnitPrice": 50.0,
  "MinimumQuantity": 10,
  "LotNumber": "L123",
  "ExpirationDate": "2025-12-31"
}

{
  "message": "Produto adicionado com sucesso",
  "timestamp": "2023-10-29T10:00:00Z"
}

