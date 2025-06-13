using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HealthcareApi.Domain.Models;

namespace HealthcareApi.Domain.IRepositories.IDapperRepositories
{
    public interface IDapperPaymentRepository
    {
        Task<Payment?> GetPaymentByIdAsync(int paymentId);
        Task<Payment> CreatePaymentAsync(Payment payment);
        Task<bool> UpdatePaymentAsync(Payment payment);
        Task<bool> DeletePaymentAsync(int paymentId);
    }
}
