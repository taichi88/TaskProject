using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HealthcareApi.Application.DTO;

namespace HealthcareApi.Application.Interfaces
{
    public interface IAccountService
    {
        Task<AccountDto> CreateAccountAsync(AccountDto dto);
        Task<AccountDto?> GetAccountByIdAsync(int accountId);
        Task<AccountDto?> GetAccountByPersonIdAsync(int personId); // Optional: if you need to fetch patient's account directly
        Task<AccountDto?> GetAccountByClinicIdAsync(int clinicId); // Optional: if you need to fetch clinic's account directly
        Task<string> UpdateAccountAsync(int accountId, AccountDto dto);
        Task<string> DeleteAccountAsync(int accountId);
    }
}
