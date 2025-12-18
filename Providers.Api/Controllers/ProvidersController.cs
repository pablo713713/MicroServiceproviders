using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Providers.Application.Interfaces;
using Providers.Application.Services;
using Providers.Domain.Entities;
using System.Net;

namespace Providers.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class ProvidersController : ControllerBase
    {
        private readonly IProviderService _providerService;

        public ProvidersController(IProviderService providerService)
        {
            _providerService = providerService;
        }

        private static int ParseActorId(string? header)
        {
            return int.TryParse(header, out var id) ? id : 0;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Provider), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Register(
            [FromBody] Provider provider,
            [FromHeader(Name = "X-Actor-Id")] string? actorHeader)
        {
            try
            {
                var actorId = ParseActorId(actorHeader);
                var createdProvider = await _providerService.RegisterAsync(provider, actorId);
                return CreatedAtAction(nameof(GetById), new { id = createdProvider.id }, createdProvider);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    errors = ex.Errors
                });
            }
            catch (DomainException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] Provider provider,
            [FromHeader(Name = "X-Actor-Id")] string? actorHeader)
        {
            try
            {
                var actorId = ParseActorId(actorHeader);
                provider.id = id;
                await _providerService.UpdateAsync(provider, actorId);
                return NoContent();
            }
            catch (ValidationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    errors = ex.Errors
                });
            }
            catch (DomainException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Provider>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAll()
        {
            var providers = await _providerService.ListAsync();
            return Ok(providers);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Provider), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var provider = await _providerService.GetByIdAsync(id);
            if (provider == null)
            {
                return NotFound(new { error = $"Proveedor con ID {id} no encontrado." });
            }

            return Ok(provider);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Delete(
            int id,
            [FromHeader(Name = "X-Actor-Id")] string? actorHeader)
        {
            try
            {
                var actorId = ParseActorId(actorHeader);
                await _providerService.SoftDeleteAsync(id, actorId);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }
}
