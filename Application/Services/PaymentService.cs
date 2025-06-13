using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Services;
using AutoMapper;
using HealthcareApi.Application.DTO;
using HealthcareApi.Application.Interfaces;
using HealthcareApi.Application.IUnitOfWork;
using HealthcareApi.Domain.Models;
using Microsoft.Extensions.Logging;



namespace Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PaymentService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        // Changed return type to Task<string> to match the controller's expectation
        public async Task<string> ProcessPaymentAsync(PaymentDto dto)
        {
            _logger.LogInformation("Starting Payment processing for AppointmentId {AppointmentId}", dto.AppointmentId);

            // Explicitly begin the transaction
            

            try
            {
                // 1. Retrieve necessary Account IDs (delegated to AccountRepository via UoW)
                _logger.LogInformation("Retrieving account IDs for PatientPersonId {PatientPersonId} and ClinicName {ClinicName}", dto.PatientPersonId, dto.ClinicName);
                var fromAccountId = await _unitOfWork.Accounts.GetAccountIdByPersonIdAsync(dto.PatientPersonId);
                if (fromAccountId == null)
                {
                    throw new InvalidOperationException($"Patient's account not found for Person ID: {dto.PatientPersonId}.");
                }

                var toAccountId = await _unitOfWork.Accounts.GetAccountIdByClinicNameAsync(dto.ClinicName);
                if (toAccountId == null)
                {
                    throw new InvalidOperationException($"Clinic's account not found for Clinic Name: {dto.ClinicName}.");
                }

                // 2. Verify Appointment exists (delegated to AppointmentRepository via UoW)
                _logger.LogInformation("Verifying existence of AppointmentId {AppointmentId}", dto.AppointmentId);
                var appointment = await _unitOfWork.Appointments.GetAppointmentByIdAsync(dto.AppointmentId);
                if (appointment == null)
                {
                    throw new InvalidOperationException($"Appointment with ID {dto.AppointmentId} not found.");
                }

                // 3. Map DTO to Payment entity
                var payment = _mapper.Map<Payment>(dto);
                payment.FromAccountId = fromAccountId.Value;
                payment.ToAccountId = toAccountId.Value;
                payment.PaymentDate = DateTime.Now;
                payment.Status = "Completed";

                // 4. Create the Payment Record (delegated to PaymentRepository via UoW)
                _logger.LogInformation("Creating payment record for AppointmentId {AppointmentId}", dto.AppointmentId);
                await _unitOfWork.Payments.CreatePaymentAsync(payment);

                // 5. Update Account Balances (delegated to AccountRepository via UoW)
                _logger.LogInformation("Updating account balances for payment.");
                await _unitOfWork.Accounts.UpdateBalanceAsync(fromAccountId.Value, -dto.Amount);
                await _unitOfWork.Accounts.UpdateBalanceAsync(toAccountId.Value, dto.Amount);

                // 6. Update Appointment Status (as per your original example)
                _logger.LogInformation("Updating appointment status for AppointmentId {AppointmentId}", dto.AppointmentId);
                appointment.Status = "Paid";
                await _unitOfWork.Appointments.UpdateAppointmentAsync(appointment.AppointmentId, appointment);

                // Explicitly commit the transaction
               

                _logger.LogInformation("Payment processed successfully for AppointmentId {AppointmentId}", dto.AppointmentId);
                return "Payment processed successfully.";
            }
            catch (Exception ex)
            {
                // Explicitly rollback the transaction on any error
                _logger.LogError(ex, "Failed to process payment for AppointmentId {AppointmentId}. Rolling back transaction.", dto.AppointmentId);
                await _unitOfWork.RollbackAsync(); // Rollback must be awaited

                // Re-throw specific business exceptions, or a generic one if it's unexpected
                if (ex is InvalidOperationException)
                {
                    throw; // Re-throw to be caught by the controller for 400 Bad Request
                }
                else
                {
                    // For unexpected exceptions, throw a new generic exception to prevent exposing internal details
                    throw new ApplicationException("An unexpected error occurred during payment processing.", ex);
                }
            }
           
        }
        public async Task<PaymentDto?> GetPaymentByIdAsync(int paymentId)
        {
            _logger.LogInformation("Attempting to retrieve payment with ID {PaymentId}", paymentId);
            try
            {
                
                var payment = await _unitOfWork.Payments.GetPaymentByIdAsync(paymentId);
                if (payment == null)
                {
                    _logger.LogWarning("Payment with ID {PaymentId} not found.", paymentId);
                    return null;
                }
                _logger.LogInformation("Successfully retrieved payment with ID {PaymentId}", paymentId);
                return _mapper.Map<PaymentDto>(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve payment with ID {PaymentId}.", paymentId);
                throw new ApplicationException($"An unexpected error occurred while retrieving payment {paymentId}.", ex);
            }
        }

        public async Task<string> UpdatePaymentAsync(int paymentId, PaymentDto dto)
        {
            _logger.LogInformation("Starting update for payment with ID {PaymentId}", paymentId);

             // Start transaction for update

            try
            {
                // 1. Verify Payment exists
                var existingPayment = await _unitOfWork.Payments.GetPaymentByIdAsync(paymentId);
                if (existingPayment == null)
                {
                    throw new InvalidOperationException($"Payment with ID {paymentId} not found.");
                }

                // 2. Map DTO to existing Payment entity, overwriting fields
                // Ensure Amount change logic is handled if needed (e.g., if changing amount affects balances)
                // For simplicity, this assumes a direct update of the payment record.
                // If amount changes, you'd need additional logic to adjust account balances,
                // potentially by reversing the original amount and applying the new amount.
                // For this refactor, we'll assume basic record update.
                _mapper.Map(dto, existingPayment); // Maps dto to existingPayment

                // Ensure PaymentId is set correctly from the path parameter
                existingPayment.PaymentId = paymentId;

                // Update date/status if applicable, or leave as is if not part of DTO
                // existingPayment.PaymentDate = DateTime.Now; // Update if desired
                // existingPayment.Status = dto.Status ?? existingPayment.Status; // Update if provided in DTO

                // If PatientPersonId or ClinicName are changed in DTO, you'd need to re-fetch/update FromAccountId/ToAccountId.
                // For now, assuming these are stable for update or handled by business rules.
                if (dto.PatientPersonId != 0 && existingPayment.FromAccountId != await _unitOfWork.Accounts.GetAccountIdByPersonIdAsync(dto.PatientPersonId))
                {
                    var newFromAccountId = await _unitOfWork.Accounts.GetAccountIdByPersonIdAsync(dto.PatientPersonId);
                    if (newFromAccountId == null) throw new InvalidOperationException($"New patient's account not found for Person ID: {dto.PatientPersonId}.");
                    existingPayment.FromAccountId = newFromAccountId.Value;
                }
                if (!string.IsNullOrEmpty(dto.ClinicName) && existingPayment.ToAccountId != await _unitOfWork.Accounts.GetAccountIdByClinicNameAsync(dto.ClinicName))
                {
                    var newToAccountId = await _unitOfWork.Accounts.GetAccountIdByClinicNameAsync(dto.ClinicName);
                    if (newToAccountId == null) throw new InvalidOperationException($"New clinic's account not found for Clinic Name: {dto.ClinicName}.");
                    existingPayment.ToAccountId = newToAccountId.Value;
                }


                // 3. Update the Payment Record
                var updated = await _unitOfWork.Payments.UpdatePaymentAsync(existingPayment);
                if (!updated)
                {
                    throw new InvalidOperationException($"Failed to update payment with ID {paymentId}. No record found or changes applied.");
                }

                
                _logger.LogInformation("Payment with ID {PaymentId} updated successfully.", paymentId);
                return $"Payment with ID {paymentId} updated successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update payment with ID {PaymentId}. Rolling back transaction.", paymentId);
                await _unitOfWork.RollbackAsync();
                if (ex is InvalidOperationException)
                {
                    throw;
                }
                throw new ApplicationException($"An unexpected error occurred while updating payment {paymentId}.", ex);
            }
            
        }

        public async Task<string> DeletePaymentAsync(int paymentId)
        {
            _logger.LogInformation("Starting delete for payment with ID {PaymentId}", paymentId);

            // NOTE: In a real financial system, direct deletion of payments is generally avoided.
            // Instead, a "reversal" or "refund" transaction is recorded.
            // This implementation performs a hard delete for demonstration purposes.

            await _unitOfWork.BeginTransactionAsync(); // Start transaction for delete

            try
            {
                // 1. Get payment details to potentially reverse balance impact if it's a true financial reversal
                // For a hard delete, we are just removing the record. If this implies a balance reversal,
                // you would need to fetch the payment, then reverse its impact on accounts, then delete.
                // For simplicity, let's assume this is a direct record deletion.
                var paymentToDelete = await _unitOfWork.Payments.GetPaymentByIdAsync(paymentId);
                if (paymentToDelete == null)
                {
                    throw new InvalidOperationException($"Payment with ID {paymentId} not found for deletion.");
                }

                // If this deletion implies a financial reversal, you would add the balance reversal logic here:
                // await _unitOfWork.Accounts.UpdateBalanceAsync(paymentToDelete.FromAccountId, paymentToDelete.Amount); // Re-add to patient
                // await _unitOfWork.Accounts.UpdateBalanceAsync(paymentToDelete.ToAccountId, -paymentToDelete.Amount); // Remove from clinic

                // 2. Delete the Payment record
                var deleted = await _unitOfWork.Payments.DeletePaymentAsync(paymentId);
                if (!deleted)
                {
                    throw new InvalidOperationException($"Failed to delete payment with ID {paymentId}. No record found or deleted.");
                }

                await _unitOfWork.CommitAsync();
                _logger.LogInformation("Payment with ID {PaymentId} deleted successfully.", paymentId);
                return $"Payment with ID {paymentId} deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete payment with ID {PaymentId}. Rolling back transaction.", paymentId);
                await _unitOfWork.RollbackAsync();
                if (ex is InvalidOperationException)
                {
                    throw;
                }
                throw new ApplicationException($"An unexpected error occurred while deleting payment {paymentId}.", ex);
            }
          
        }
    }
}