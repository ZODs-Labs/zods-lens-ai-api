using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZODs.Api.Common.Attributes;
using ZODs.Api.Repository;
using ZODs.Api.Repository.Entities.Entities;
using System.ComponentModel.DataAnnotations;

namespace ZODs.Api.Controllers
{
    [AllowAnonymous]
    [AllowNoSubscription]
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ZodsContext zodsContext;

        public SubscriptionsController(ZodsContext zodsContext)
        {
            this.zodsContext = zodsContext;
        }

        [HttpPost("webexpertai")]
        public async Task<IActionResult> AddSubscription([FromBody][EmailAddress][Required] string emailAddress)
        {
            emailAddress = emailAddress.ToLowerInvariant();
            var emailExists = await this.zodsContext.Subscriptions.AnyAsync(x => x.EmailAddress == emailAddress);

            if (emailExists)
            {
                return UnprocessableEntity("Email already subscribed.");
            }

            var subscription = new Subscription
            {
                Source = "webexpertai",
                EmailAddress = emailAddress,
                CreatedAt = DateTime.UtcNow,
            };

            await this.zodsContext.Subscriptions.AddAsync(subscription);
            await this.zodsContext.SaveChangesAsync();

            return Ok();
        }
    }
}
