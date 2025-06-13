using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HealthcareApi.Domain.IRepositories.IDapperRepositories;
using HealthcareApi.Domain.Models;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Dapper;

namespace HealthcareApi.Infrastructure.Repositories.DapperRepository
{
    public class DapperAccountRepository : IDapperAccountRepository
    {
            private readonly IDbConnection _connection;
            private readonly IDbTransaction? _transaction;
            public DapperAccountRepository(IDbConnection connection, IDbTransaction? transaction = null)
            {
                
                _connection = connection;
                _transaction = transaction;
                
            }
            public async Task<int?> GetAccountIdByPersonIdAsync(int personId)
        {
            var sql = "SELECT AccountId FROM Billing.Accounts WHERE PersonId = @PersonId;";
            // Use _getTransaction() to ensure this operation runs within the current transaction, if active.
            return await _connection.QuerySingleOrDefaultAsync<int?>(sql, new { PersonId = personId }, _transaction);
        }

        /// <summary>
        /// Retrieves the AccountId for a given Clinic Name.
        /// </summary>
        /// <param name="clinicName">The name of the clinic.</param>
        /// <returns>The AccountId if found, otherwise null.</returns>
        public async Task<int?> GetAccountIdByClinicNameAsync(string clinicName)
        {
            // Join with Clinical.Clinics to find the AccountId linked to the clinic name
            var sql = "SELECT ba.AccountId FROM Billing.Accounts ba JOIN Clinical.Clinics c ON ba.ClinicId = c.ClinicId WHERE c.Name = @ClinicName;";
            return await _connection.QuerySingleOrDefaultAsync<int?>(sql, new { ClinicName = clinicName }, _transaction);
        }

        /// <summary>
        /// Updates the balance of a specific account.
        /// </summary>
        /// <param name="accountId">The ID of the account to update.</param>
        /// <param name="amountChange">The amount to add to or subtract from the balance (positive for increase, negative for decrease).</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task UpdateBalanceAsync(int accountId, decimal amountChange)
        {
            var sql = "UPDATE Billing.Accounts SET Balance = Balance + @AmountChange WHERE AccountId = @AccountId;";
            await _connection.ExecuteAsync(sql, new { AmountChange = amountChange, AccountId = accountId }, _transaction);
        }

        /// <summary>
        /// Retrieves a single account record by its ID.
        /// </summary>
        /// <param name="accountId">The ID of the account to retrieve.</param>
        /// <returns>The Account object if found, otherwise null.</returns>
        public async Task<Account?> GetAccountByIdAsync(int accountId)
        {
            var sql = @"SELECT 
                    AccountId,
                    PersonId,
                    ClinicId,
                    Balance
                FROM Billing.Accounts 
                WHERE AccountId = @AccountId;";

            return await _connection.QuerySingleOrDefaultAsync<Account>(sql, new { AccountId = accountId }, _transaction);
        }

        /// <summary>
        /// Creates a new account record in the database.
        /// </summary>
        /// <param name="account">The Account object containing the data to insert.</param>
        /// <returns>The Account object with the newly generated AccountId.</returns>
        public async Task<Account> CreateAccountAsync(Account account)
        {
            // The CHK_Account_Type constraint in your DB schema ensures only one of PersonId or ClinicId is non-null.
            // Dapper will map the C# Account object's properties to SQL parameters.
            var sql = @"
                INSERT INTO Billing.Accounts (PersonId, ClinicId, Balance)
                VALUES (@PersonId, @ClinicId, @Balance);
                SELECT CAST(SCOPE_IDENTITY() as int);"; // Retrieve the newly generated ID

            var id = await _connection.QuerySingleAsync<int>(sql, account, _transaction);
            account.AccountId = id; // Assign the new ID back to the object
            return account;
        }

        /// <summary>
        /// Updates an existing account record in the database.
        /// </summary>
        /// <param name="account">The Account object with updated data (must include AccountId).</param>
        /// <returns>True if the account was updated, false otherwise.</returns>
        public async Task<bool> UpdateAccountAsync(Account account)
        {
            var sql = @"
                UPDATE Billing.Accounts
                SET
                    PersonId = @PersonId,
                    ClinicId = @ClinicId,
                    Balance = @Balance
                WHERE AccountId = @AccountId;";

            var affectedRows = await _connection.ExecuteAsync(sql, account, _transaction);
            return affectedRows > 0; // True if one or more rows were updated
        }

        /// <summary>
        /// Deletes an account record from the database by its ID.
        /// </summary>
        /// <param name="accountId">The ID of the account to delete.</param>
        /// <returns>True if the account was deleted, false otherwise.</returns>
        public async Task<bool> DeleteAccountAsync(int accountId)
        {
            var sql = "DELETE FROM Billing.Accounts WHERE AccountId = @AccountId;";
            var affectedRows = await _connection.ExecuteAsync(sql, new { AccountId = accountId }, _transaction);
            return affectedRows > 0; // True if one or more rows were deleted
        }
        /// <summary>
        /// Retrieves the AccountId for a given ClinicId.
        /// </summary>
        /// <param name="clinicId">The ID of the clinic.</param>
        /// <returns>The AccountId if found, otherwise null.</returns>
        public async Task<int?> GetAccountIdByClinicIdAsync(int clinicId)
        {
            var sql = "SELECT AccountId FROM Billing.Accounts WHERE ClinicId = @ClinicId;";
            return await _connection.QuerySingleOrDefaultAsync<int?>(sql, new { ClinicId = clinicId }, _transaction);
        }
    }
}

