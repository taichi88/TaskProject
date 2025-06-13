

using HealthcareApi.Application.DTO;
using HealthcareApi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareApi.Api.Controllers
{
    public class AccountController
    {
            [ApiController]
            [Route("api/[controller]")]
            public class PaymentController : ControllerBase
            {
                private readonly IPaymentService _paymentService;

                public PaymentController(IPaymentService paymentService)
                {
                    _paymentService = paymentService;
                }

                [HttpPost("process")]
                public async Task<IActionResult> ProcessPayment([FromBody] PaymentDto dto)
                {
                    if (dto == null || !ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }

                    try
                    {
                        var resultMessage = await _paymentService.ProcessPaymentAsync(dto);

                        if (resultMessage.Contains("successfully", StringComparison.OrdinalIgnoreCase))
                        {
                            return Ok(new { message = resultMessage });
                        }
                        else
                        {
                            return BadRequest(new { message = resultMessage });
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        return BadRequest(new { error = ex.Message });
                    }
                    catch (ApplicationException ex)
                    {
                        Console.Error.WriteLine($"Error processing payment: {ex.Message}");
                        return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred while processing the payment." });
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Unhandled exception in PaymentController: {ex.Message}");
                        return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unhandled error occurred." });
                    }
                }

                // --- New Controller Endpoints for Payment CRUD ---

                [HttpGet("{id}")]
                public async Task<IActionResult> GetPayment(int id)
                {
                    try
                    {
                        var paymentDto = await _paymentService.GetPaymentByIdAsync(id);
                        if (paymentDto == null)
                        {
                            return NotFound(new { message = $"Payment with ID {id} not found." });
                        }
                        return Ok(paymentDto);
                    }
                    catch (ApplicationException ex)
                    {
                        Console.Error.WriteLine($"Error retrieving payment {id}: {ex.Message}");
                        return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred while retrieving the payment." });
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Unhandled exception in GetPayment: {ex.Message}");
                        return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unhandled error occurred." });
                    }
                }

                [HttpPut("{id}")]
                public async Task<IActionResult> UpdatePayment(int id, [FromBody] PaymentDto dto)
                {
                    if (dto == null || !ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }

                    // Ensure the ID in the route matches the DTO's ID (if DTO has one)
                    // Or, assign the route ID to the DTO for consistency
                    if (dto.PaymentId == null || dto.PaymentId != id)
                    {
                        dto.PaymentId = id;
                    }

                    try
                    {
                        var resultMessage = await _paymentService.UpdatePaymentAsync(id, dto);
                        if (resultMessage.Contains("successfully", StringComparison.OrdinalIgnoreCase))
                        {
                            return Ok(new { message = resultMessage });
                        }
                        else
                        {
                            return BadRequest(new { message = resultMessage });
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        return NotFound(new { error = ex.Message }); // Use NotFound if the ID doesn't exist for update
                    }
                    catch (ApplicationException ex)
                    {
                        Console.Error.WriteLine($"Error updating payment {id}: {ex.Message}");
                        return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred while updating the payment." });
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Unhandled exception in UpdatePayment: {ex.Message}");
                        return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unhandled error occurred." });
                    }
                }

                [HttpDelete("{id}")]
                public async Task<IActionResult> DeletePayment(int id)
                {
                    try
                    {
                        var resultMessage = await _paymentService.DeletePaymentAsync(id);
                        if (resultMessage.Contains("successfully", StringComparison.OrdinalIgnoreCase))
                        {
                            // HTTP 204 No Content is often used for successful DELETE with no response body
                            return NoContent();
                        }
                        else
                        {
                            return BadRequest(new { message = resultMessage });
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        return NotFound(new { error = ex.Message }); // Use NotFound if the ID doesn't exist for deletion
                    }
                    catch (ApplicationException ex)
                    {
                        Console.Error.WriteLine($"Error deleting payment {id}: {ex.Message}");
                        return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred while deleting the payment." });
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Unhandled exception in DeletePayment: {ex.Message}");
                        return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unhandled error occurred." });
                    }
                }
            }
        }
    }
