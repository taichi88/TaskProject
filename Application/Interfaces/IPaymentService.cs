using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HealthcareApi.Application.DTO;

namespace HealthcareApi.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<string> ProcessPaymentAsync(PaymentDto dto);
        Task<PaymentDto?> GetPaymentByIdAsync(int paymentId);
        Task<string> UpdatePaymentAsync(int paymentId, PaymentDto dto);
        Task<string> DeletePaymentAsync(int paymentId);
    }
}
