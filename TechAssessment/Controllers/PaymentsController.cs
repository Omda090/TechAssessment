using Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Stripe;

namespace TechAssessment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly AplicationDbContext _context;

        public PaymentsController(AplicationDbContext context)
        {
            _context = context;
        }

        //[HttpPost("create-payment-intent")]
        //public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentRequest request)
        //{
        //    // Get product details
        //    var product = await _context.Products.FindAsync(request.ProductId);
        //    if (product == null) return BadRequest("Product not found");

        //    // Create payment intent
        //    var options = new PaymentIntentCreateOptions
        //    {
        //        Amount = product.Price,
        //        Currency = "usd",
        //        PaymentMethod = request.PaymentMethodId,
        //        Confirm = true,
        //    };

        //    var service = new PaymentIntentService();
        //    PaymentIntent paymentIntent;

        //    try
        //    {
        //        paymentIntent = service.Create(options);

        //        // Save payment details to the database
        //        var payment = new Payment
        //        {
        //            PaymentIntentId = paymentIntent.Id,
        //            ProductName = product.Name,
        //            Amount = product.Price,
        //            Status = paymentIntent.Status,
        //            CreatedAt = DateTime.UtcNow
        //        };

        //        await _context.Payments.AddAsync(payment);
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (StripeException ex)
        //    {
        //        return BadRequest(new { error = ex.Message });
        //    }

        //    return Ok(new { paymentIntent.Id, paymentIntent.Status });
        //}

        [HttpPost("create-payment-intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentRequest request)
        {
            // Validate request
            if (string.IsNullOrEmpty(request.PaymentMethodId))
            {
                return BadRequest(new { error = "PaymentMethodId is required." });
            }

            // Get product details
            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null)
            {
                return BadRequest(new { error = "Product not found." });
            }

            // Ensure product price is valid
            if (product.Price <= 0)
            {
                return BadRequest(new { error = "Invalid product price." });
            }

            // Create PaymentIntent options with return_url
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(product.Price * 100), // Stripe expects the amount in cents
                Currency = "usd",
                PaymentMethod = request.PaymentMethodId,
                ConfirmationMethod = "manual", // Optional: can be "automatic" if you want auto-confirm
                Confirm = true,
                ReturnUrl = "https://yourwebsite.com/payment-confirmation" // Replace with your actual return URL
            };

            var service = new PaymentIntentService();
            PaymentIntent paymentIntent;

            try
            {
                // Create the PaymentIntent
                paymentIntent = await service.CreateAsync(options);

                // Save payment details to the database
                var payment = new Payment
                {
                    PaymentIntentId = paymentIntent.Id,
                    ProductName = product.Name,
                    Amount = product.Price,
                    Status = paymentIntent.Status,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Payments.AddAsync(payment);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    PaymentIntentId = paymentIntent.Id,
                    PaymentStatus = paymentIntent.Status,
                    ProductName = product.Name,
                    Amount = product.Price,
                    RedirectUrl = paymentIntent.NextAction?.RedirectToUrl?.Url // Provide the redirect URL if applicable
                });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }




        [HttpGet]
        public async Task<IActionResult> GetPayments()
        {
            var payments = await _context.Payments.ToListAsync();
            return Ok(payments);
        }
    }
}
