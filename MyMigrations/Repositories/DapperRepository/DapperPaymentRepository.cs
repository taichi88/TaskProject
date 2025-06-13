using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HealthcareApi.Domain.IRepositories;
using HealthcareApi.Domain.IRepositories.IDapperRepositories;
using HealthcareApi.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Dapper;




namespace HealthcareApi.Infrastructure.Repositories.DapperRepository
{
    public class DapperPaymentRepository : IDapperPaymentRepository
    {
        private readonly IDbConnection _connection;
        private readonly IDbTransaction? _transaction; // Can be null if no active transaction

        // Constructor receives the DbContext (e.g., from EF Core)
        // and extracts the IDbConnection and IDbTransaction from it.
        public DapperPaymentRepository(IDbConnection connection, IDbTransaction? transaction = null)
        {
            _connection = connection;
            _transaction = transaction;
        }

        /// <summary>
        /// Retrieves a single payment record by its ID.
        /// </summary>
        /// <param name="paymentId">The ID of the payment to retrieve.</param>
        /// <returns>The Payment object if found, otherwise null.</returns>
        public async Task<Payment?> GetPaymentByIdAsync(int paymentId)
        {
            var sql = @"
        SELECT 
            PaymentId,
            AppointmentId,
            FromAccountId,
            ToAccountId,
            Amount,
            PaymentDate,
            PaymentMethod,
            Status,
            Notes
        FROM Billing.Payments 
        WHERE PaymentId = @PaymentId;";

            return await _connection.QuerySingleOrDefaultAsync<Payment>(sql, new { PaymentId = paymentId }, _transaction);
        }

        /// <summary>
        /// Creates a new payment record in the database.
        /// </summary>
        /// <param name="payment">The Payment object containing the data to insert.</param>
        /// <returns>The Payment object with the newly generated PaymentId.</returns>
        public async Task<Payment> CreatePaymentAsync(Payment payment)
        {
            // Corrected SQL to match your schema (FromAccountId, ToAccountId)
            // And use GETDATE() directly for PaymentDate as it's defaulted in DB
            var sql = @"
                INSERT INTO Billing.Payments (AppointmentId, FromAccountId, ToAccountId, Amount, PaymentMethod, Status, Notes)
                VALUES (@AppointmentId, @FromAccountId, @ToAccountId, @Amount, @PaymentMethod, @Status, @Notes);
                SELECT CAST(SCOPE_IDENTITY() as int);"; // SCOPE_IDENTITY() returns the last identity value inserted into an IDENTITY column in the same scope.

            // Execute the insert and retrieve the new ID within the current transaction (if active)
            var id = await _connection.QuerySingleAsync<int>(sql, payment, _transaction);
            payment.PaymentId = id; // Assign the newly generated ID back to the payment object
            return payment;
        }

        /// <summary>
        /// Updates an existing payment record in the database.
        /// </summary>
        /// <param name="payment">The Payment object with updated data (must include PaymentId).</param>
        /// <returns>True if the payment was updated, false otherwise.</returns>
        public async Task<bool> UpdatePaymentAsync(Payment payment)
        {
            var sql = @"
                UPDATE Billing.Payments
                SET
                    AppointmentId = @AppointmentId,
                    FromAccountId = @FromAccountId,
                    ToAccountId = @ToAccountId,
                    Amount = @Amount,
                    PaymentMethod = @PaymentMethod,
                    Status = @Status,
                    Notes = @Notes
                WHERE PaymentId = @PaymentId;";

            // Execute the update within the current transaction (if active)
            var affectedRows = await _connection.ExecuteAsync(sql, payment, _transaction);
            return affectedRows > 0; // Returns true if at least one row was updated
        }

        /// <summary>
        /// Deletes a payment record from the database by its ID.
        /// </summary>
        /// <param name="paymentId">The ID of the payment to delete.</param>
        /// <returns>True if the payment was deleted, false otherwise.</returns>
        public async Task<bool> DeletePaymentAsync(int paymentId)
        {
            var sql = "DELETE FROM Billing.Payments WHERE PaymentId = @PaymentId;";
            // Execute the delete within the current transaction (if active)
            var affectedRows = await _connection.ExecuteAsync(sql, new { PaymentId = paymentId }, _transaction);
            return affectedRows > 0; // Returns true if at least one row was deleted
        }
    }
}
