using Dapper;
using Domain;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly MySqlConnection _connection;

        public ProductRepository(string connectionString)
        {
            _connection = new MySqlConnection(connectionString);
        }

        public async Task<int> AddProductAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product), "Invalid product");

            await _connection.OpenAsync();
            string sql = @"
                INSERT INTO product (SKUCode, Name, Category, UnitPrice, MinimumQuantity, Quantity, CreationDate, LotNumber, ExpirationDate)
                VALUES (@SKUCode, @Name, @Category, @UnitPrice, @MinimumQuantity, @Quantity, @CreationDate, @LotNumber, @ExpirationDate);
                SELECT LAST_INSERT_ID();
            ";
            var id = await _connection.ExecuteScalarAsync<int>(sql, product);
            await _connection.CloseAsync();
            return id;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            await _connection.OpenAsync();
            string sql = "SELECT * FROM product;";
            var products = await _connection.QueryAsync<Product>(sql);
            await _connection.CloseAsync();
            return products;
        }

        public async Task<IEnumerable<Product>> GetProductsBelowMinimumStockAsync()
        {
            await _connection.OpenAsync();
            string sql = "SELECT * FROM product WHERE Quantity < MinimumQuantity;";
            var products = await _connection.QueryAsync<Product>(sql);
            await _connection.CloseAsync();
            return products;
        }

        public async Task UpdateProductAsync(Product product)
        {
            if (product == null || product.SKUCode <= 0)
                throw new ArgumentException("Invalid product", nameof(product));

            await _connection.OpenAsync();
            string sql = @"
                UPDATE product
                SET Name = @Name, Category = @Category, UnitPrice = @UnitPrice, MinimumQuantity = @MinimumQuantity,
                    Quantity = @Quantity, LotNumber = @LotNumber, ExpirationDate = @ExpirationDate
                WHERE SKUCode = @SKUCode;
            ";
            await _connection.ExecuteAsync(sql, product);
            await _connection.CloseAsync();
        }

        public async Task DeleteProductAsync(int skuCode)
        {
            if (skuCode <= 0)
                throw new ArgumentException("SKU inválido.", nameof(skuCode));

            await _connection.OpenAsync();
            string sql = "DELETE FROM product WHERE SKUCode = @SKUCode;";
            await _connection.ExecuteAsync(sql, new { SKUCode = skuCode });
            await _connection.CloseAsync();
        }
    }
}
