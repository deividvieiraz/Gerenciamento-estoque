using Dapper;
using Domain;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class StockMovementRepository : IStockMovementRepository
    {
        private readonly MySqlConnection _connection;

        public StockMovementRepository(string connectionString)
        {
            _connection = new MySqlConnection(connectionString);
        }
        public async Task<int> AddStockMovementAsync(StockMovement movement)
        {
            if (movement == null)
                throw new ArgumentNullException(nameof(movement), "Invalid Stock Movement.");

            await _connection.OpenAsync();
            string sql = @"
                INSERT INTO stock_movement (MovementType, Quantity, Date, Batch, ExpirationDate)
                VALUES (@MovementType, @Quantity, @Date, @Batch, @ExpirationDate);
                SELECT LAST_INSERT_ID();
            ";
            var id = await _connection.ExecuteScalarAsync<int>(sql, movement);
            await _connection.CloseAsync();
            return id;
        }

        public async Task<IEnumerable<StockMovement>> GetAllStockMovementsAsync()
        {
            await _connection.OpenAsync();
            string sql = "SELECT * FROM stock_movement;";
            var movements = await _connection.QueryAsync<StockMovement>(sql);
            await _connection.CloseAsync();
            return movements;
        }
        public async Task<IEnumerable<StockMovement>> GetStockMovementsByProductAsync(int productSkuCode)
        {
            await _connection.OpenAsync();
            string sql = "SELECT * FROM stock_movement WHERE ProductSkuCode = @ProductSkuCode;";
            var movements = await _connection.QueryAsync<StockMovement>(sql, new { ProductSkuCode = productSkuCode });
            await _connection.CloseAsync();
            return movements;
        }
        public async Task UpdateStockMovementAsync(StockMovement movement)
        {
            if (movement == null || movement.Quantity <= 0)
                throw new ArgumentException("Invalid Stock Movement", nameof(movement));

            await _connection.OpenAsync();
            string sql = @"
                UPDATE stock_movement
                SET MovementType = @MovementType, Quantity = @Quantity, Date = @Date, Batch = @Batch, ExpirationDate = @ExpirationDate
                WHERE Id = @Id;
            ";
            await _connection.ExecuteAsync(sql, movement);
            await _connection.CloseAsync();
        }

        public async Task DeleteStockMovementAsync(int movementId)
        {
            if (movementId <= 0)
                throw new ArgumentException("Invalid ID", nameof(movementId));

            await _connection.OpenAsync();
            string sql = "DELETE FROM stock_movement WHERE Id = @Id;";
            await _connection.ExecuteAsync(sql, new { Id = movementId });
            await _connection.CloseAsync();
        }
    }
}
