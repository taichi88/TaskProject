using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HealthcareApi.Domain.Models;

namespace HealthcareApi.Domain.IRepositories.IDapperRepositories
{
    public interface IDapperAccountRepository
    {
        Task<int?> GetAccountIdByPersonIdAsync(int personId);
        Task<int?> GetAccountIdByClinicNameAsync(string clinicName);
        Task UpdateBalanceAsync(int accountId, decimal amountChange);

        // CRUD operations for Account entity
        Task<Account?> GetAccountByIdAsync(int accountId);
        Task<Account> CreateAccountAsync(Account account);
        Task<bool> UpdateAccountAsync(Account account);
        Task<bool> DeleteAccountAsync(int accountId);

        Task<int?> GetAccountIdByClinicIdAsync(int clinicId);
    }
}
