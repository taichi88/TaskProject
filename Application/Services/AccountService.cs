using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using HealthcareApi.Application.DTO;
using HealthcareApi.Application.Interfaces;
using HealthcareApi.Domain.Models;
using Microsoft.Extensions.Logging;
using HealthcareApi.Application.IUnitOfWork;

namespace Application.Services
{
    public  class AccountService : IAccountService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AccountService> _logger;

        public AccountService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<AccountService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AccountDto> CreateAccountAsync(AccountDto dto)
        {
            _logger.LogInformation("Starting account creation for PersonId: {PersonId}, ClinicId: {ClinicId}", dto.PersonId, dto.ClinicId);

            // Explicitly begin the transaction for a write operation
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Business validation: An account must be linked to either a PersonId OR a ClinicId, but not both or neither.
                if ((dto.PersonId.HasValue && dto.ClinicId.HasValue) || (!dto.PersonId.HasValue && !dto.ClinicId.HasValue))
                {
                    throw new InvalidOperationException("An account must be linked to either a PersonId OR a ClinicId, but not both or neither.");
                }

                // Check if an account for this Person/Clinic already exists (business rule)
                if (dto.PersonId.HasValue)
                {
                    var existingAccount = await _unitOfWork.Accounts.GetAccountIdByPersonIdAsync(dto.PersonId.Value);
                    if (existingAccount.HasValue)
                    {
                        throw new InvalidOperationException($"Account already exists for Person ID: {dto.PersonId.Value}.");
                    }
                }
                else if (dto.ClinicId.HasValue)
                {
                    var existingAccount = await _unitOfWork.Accounts.GetAccountIdByClinicIdAsync(dto.ClinicId.Value);
                    if (existingAccount.HasValue)
                    {
                        throw new InvalidOperationException($"Account already exists for Clinic ID: {dto.ClinicId.Value}.");
                    }
                }

                var account = _mapper.Map<Account>(dto);
                account.Balance = 0.00m; // New accounts typically start with 0 balance

                var createdAccount = await _unitOfWork.Accounts.CreateAccountAsync(account);

                await _unitOfWork.CommitAsync();
                _logger.LogInformation("Account created successfully with ID: {AccountId}", createdAccount.AccountId);
                return _mapper.Map<AccountDto>(createdAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create account for PersonId: {PersonId}, ClinicId: {ClinicId}. Rolling back transaction.", dto.PersonId, dto.ClinicId);
                await _unitOfWork.RollbackAsync(); // Rollback on error

                if (ex is InvalidOperationException)
                {
                    throw; // Re-throw business validation errors for the controller
                }
                throw new ApplicationException("An unexpected error occurred while creating the account.", ex);
            }
            

        }

        public async Task<AccountDto?> GetAccountByIdAsync(int accountId)
        {
            _logger.LogInformation("Attempting to retrieve account with ID {AccountId}", accountId);
            try
            {
                // For a read operation, a transaction isn't usually needed unless specific isolation is required.
                // Repositories are assumed to handle their own connection for reads outside of a UoW scope if not passed.
                // However, since we're injecting a UoW, we'll access the repo through it.
                // For this read-only operation, we don't start a UoW-managed transaction.
                var account = await _unitOfWork.Accounts.GetAccountByIdAsync(accountId);
                if (account == null)
                {
                    _logger.LogWarning("Account with ID {AccountId} not found.", accountId);
                    return null;
                }
                _logger.LogInformation("Successfully retrieved account with ID {AccountId}", accountId);
                return _mapper.Map<AccountDto>(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve account with ID {AccountId}.", accountId);
                throw new ApplicationException($"An unexpected error occurred while retrieving account {accountId}.", ex);
            }
            // No finally _unitOfWork.Dispose() here because no transaction was explicitly started by this method.
            // The UoW is designed to be managed by the calling scope if it's involved in a transaction.
        }

        public async Task<AccountDto?> GetAccountByPersonIdAsync(int personId)
        {
            _logger.LogInformation("Attempting to retrieve account for PersonId {PersonId}", personId);
            try
            {
                // Note: GetAccountIdByPersonIdAsync returns int?, so we need to fetch the full Account object afterwards
                var accountId = await _unitOfWork.Accounts.GetAccountIdByPersonIdAsync(personId);
                if (!accountId.HasValue)
                {
                    _logger.LogWarning("Account for PersonId {PersonId} not found.", personId);
                    return null;
                }
                return await GetAccountByIdAsync(accountId.Value); // Reuse GetAccountByIdAsync
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve account for PersonId {PersonId}.", personId);
                throw new ApplicationException($"An unexpected error occurred while retrieving account for PersonId {personId}.", ex);
            }
        }

        public async Task<AccountDto?> GetAccountByClinicIdAsync(int clinicId)
        {
            _logger.LogInformation("Attempting to retrieve account for ClinicId {ClinicId}", clinicId);
            try
            {
                // UPDATED: Directly using the new GetAccountIdByClinicIdAsync from the repository
                var accountId = await _unitOfWork.Accounts.GetAccountIdByClinicIdAsync(clinicId);
                if (!accountId.HasValue)
                {
                    _logger.LogWarning("Account for ClinicId {ClinicId} not found.", clinicId);
                    return null;
                }
                return await GetAccountByIdAsync(accountId.Value); // Reuse GetAccountByIdAsync
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve account for ClinicId {ClinicId}.", clinicId);
                throw new ApplicationException($"An unexpected error occurred while retrieving account for ClinicId {clinicId}.", ex);
            }
        }


        public async Task<string> UpdateAccountAsync(int accountId, AccountDto dto)
        {
            _logger.LogInformation("Starting update for account with ID {AccountId}", accountId);

            // Explicitly begin the transaction for a write operation
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existingAccount = await _unitOfWork.Accounts.GetAccountByIdAsync(accountId);
                if (existingAccount == null)
                {
                    throw new InvalidOperationException($"Account with ID {accountId} not found.");
                }

                // Business validation for update: Only one of PersonId or ClinicId should be set if they are being updated
                // AND ensure it's not trying to unlink an account (i.e., making both null).
                if (dto.PersonId.HasValue && dto.ClinicId.HasValue)
                {
                    throw new InvalidOperationException("An account cannot be linked to both a PersonId and a ClinicId simultaneously.");
                }
                // If both are null, it's an attempt to unlink it completely, which is not allowed by schema CHK_Account_Type
                if (!dto.PersonId.HasValue && !dto.ClinicId.HasValue)
                {
                    throw new InvalidOperationException("An account must be linked to either a PersonId or a ClinicId. It cannot be unlinked from both.");
                }

                // If PersonId is changing, check for conflicts
                if (dto.PersonId.HasValue && existingAccount.PersonId != dto.PersonId.Value)
                {
                    var conflict = await _unitOfWork.Accounts.GetAccountIdByPersonIdAsync(dto.PersonId.Value);
                    if (conflict.HasValue && conflict.Value != accountId) // If account exists for new PersonId, and it's not this account itself
                    {
                        throw new InvalidOperationException($"Person ID {dto.PersonId.Value} is already linked to another account.");
                    }
                }
                // If ClinicId is changing, check for conflicts
                else if (dto.ClinicId.HasValue && existingAccount.ClinicId != dto.ClinicId.Value)
                {
                    var conflict = await _unitOfWork.Accounts.GetAccountIdByClinicIdAsync(dto.ClinicId.Value);
                    if (conflict.HasValue && conflict.Value != accountId)
                    {
                        throw new InvalidOperationException($"Clinic ID {dto.ClinicId.Value} is already linked to another account.");
                    }
                }


                // Map DTO to existing entity, preserving ID and potential unchanged fields
                _mapper.Map(dto, existingAccount);
                existingAccount.AccountId = accountId; // Ensure ID is preserved from route

                // Note: Balance updates should typically happen via specific transactions (e.g., PaymentService.ProcessPaymentAsync)
                // rather than direct update, but including it here if allowed by business rules.
                // existingAccount.Balance = dto.Balance; // Uncomment if direct balance updates are allowed

                var updated = await _unitOfWork.Accounts.UpdateAccountAsync(existingAccount);
                if (!updated)
                {
                    throw new InvalidOperationException($"Failed to update account with ID {accountId}. No record found or changes applied.");
                }

                await _unitOfWork.CommitAsync();
                _logger.LogInformation("Account with ID {AccountId} updated successfully.", accountId);
                return $"Account with ID {accountId} updated successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update account with ID {AccountId}. Rolling back transaction.", accountId);
                await _unitOfWork.RollbackAsync();

                if (ex is InvalidOperationException)
                {
                    throw;
                }
                throw new ApplicationException($"An unexpected error occurred while updating account {accountId}.", ex);
            }
          
        }

        public async Task<string> DeleteAccountAsync(int accountId)
        {
            _logger.LogInformation("Starting delete for account with ID {AccountId}", accountId);

            await _unitOfWork.BeginTransactionAsync(); // Start transaction for delete

            try
            {
                // Business validation: Check for associated payments or other related data before deleting
                // For a real financial system, you'd prevent deletion if there are associated payments or non-zero balance.
                var account = await _unitOfWork.Accounts.GetAccountByIdAsync(accountId);
                if (account == null)
                {
                    throw new InvalidOperationException($"Account with ID {accountId} not found for deletion.");
                }

                // Example business rule: Cannot delete accounts with non-zero balance
                if (account.Balance != 0)
                {
                    throw new InvalidOperationException($"Cannot delete account with ID {accountId} as it has a non-zero balance of {account.Balance:C}.");
                }

                // You might also need to check if there are any associated payments related to this account (FromAccountId or ToAccountId)
                // If there are, you'd need a business rule: soft delete, or prevent deletion.
                // Example check:
                // var paymentsLinked = await _unitOfWork.Payments.HasPaymentsLinkedToAccountAsync(accountId);
                // if (paymentsLinked) { throw new InvalidOperationException($"Cannot delete account with ID {accountId} as it has associated payment history."); }

                var deleted = await _unitOfWork.Accounts.DeleteAccountAsync(accountId);
                if (!deleted)
                {
                    throw new InvalidOperationException($"Failed to delete account with ID {accountId}. No record found or deleted.");
                }

                await _unitOfWork.CommitAsync();
                _logger.LogInformation("Account with ID {AccountId} deleted successfully.", accountId);
                return $"Account with ID {accountId} deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete account with ID {AccountId}. Rolling back transaction.", accountId);
                await _unitOfWork.RollbackAsync();

                if (ex is InvalidOperationException)
                {
                    throw;
                }
                throw new ApplicationException($"An unexpected error occurred while deleting account {accountId}.", ex);
            }
           
        }
    }

}

