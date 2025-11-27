using Providers.Application.Interfaces;
using Providers.Application.Validators;
using Providers.Domain.Entities;
using Providers.Domain.Interfaces;
using FluentResults;
using System.Text.RegularExpressions;

namespace Providers.Application.Services
{
    public class ProviderService : IProviderService
    {
        private readonly IRepository<Provider> _providerRepository;
        private readonly IValidator<Provider> _validator;

        public ProviderService(IRepository<Provider> providerRepository, IValidator<Provider> validator)
        {
            _providerRepository = providerRepository;
            _validator = validator;
        }

        private static readonly Regex MultiSpace = new(@"\s+", RegexOptions.Compiled);

        private static string NormalizeName(string? s)
            => string.IsNullOrWhiteSpace(s) ? string.Empty : MultiSpace.Replace(s.Trim(), " ");

        private static string NormalizeEmail(string? s)
            => string.IsNullOrWhiteSpace(s) ? string.Empty : s.Trim().ToLowerInvariant();

        public async Task<Provider> RegisterAsync(Provider entity, int actorId)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            // Normalización
            entity.first_name = NormalizeName(entity.first_name);
            entity.last_name = NormalizeName(entity.last_name);
            entity.email = NormalizeEmail(entity.email);
            entity.phone = entity.phone?.Trim() ?? string.Empty;

            // Validación de dominio
            var validationResult = _validator.Validate(entity);
            if (validationResult.IsFailed)
                throw new ValidationException(
                    "Validación de dominio falló para Proveedor.",
                    validationResult.Errors.ToDictionary()
                );

            // Reglas de unicidad
            var all = await _providerRepository.GetAll();

            if (!string.IsNullOrWhiteSpace(entity.email) &&
                all.Any(c => !c.is_deleted &&
                    string.Equals((c.email ?? "").Trim(), entity.email, StringComparison.OrdinalIgnoreCase)))
            {
                throw new DomainException("El correo ya existe.");
            }

            if (!string.IsNullOrWhiteSpace(entity.phone) &&
                all.Any(c => !c.is_deleted &&
                    string.Equals((c.phone ?? "").Trim(), entity.phone, StringComparison.OrdinalIgnoreCase)))
            {
                throw new DomainException("El teléfono ya existe.");
            }

            // Auditoría de creación
            entity.created_by = actorId;
            entity.created_at = DateTime.Now;

            // Opcional: reflejar también en updated_* al crear
            entity.updated_by = actorId;
            entity.updated_at = entity.created_at;

            return await _providerRepository.Create(entity);
        }

        public async Task<Provider?> GetByIdAsync(int id)
        {
            return await _providerRepository.GetById(new Provider { id = id });
        }

        public async Task<IEnumerable<Provider>> ListAsync()
        {
            return await _providerRepository.GetAll();
        }

        public async Task UpdateAsync(Provider entity, int actorId)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            var current = await _providerRepository.GetById(new Provider { id = entity.id })
                          ?? throw new NotFoundException($"Proveedor con ID {entity.id} no encontrado.");

            // Normalización y asignación de nuevos valores
            current.first_name = NormalizeName(entity.first_name);
            current.last_name = NormalizeName(entity.last_name);
            current.email = NormalizeEmail(entity.email);
            current.phone = entity.phone?.Trim() ?? string.Empty;

            // Validación de dominio
            var validationResult = _validator.Validate(current);
            if (validationResult.IsFailed)
                throw new ValidationException(
                    "Validación de dominio falló para Proveedor.",
                    validationResult.Errors.ToDictionary()
                );

            var all = await _providerRepository.GetAll();

            if (!string.IsNullOrWhiteSpace(current.email) &&
                all.Any(c => c.id != current.id && !c.is_deleted &&
                             string.Equals((c.email ?? "").Trim(), current.email, StringComparison.OrdinalIgnoreCase)))
                throw new DomainException("El correo ya existe.");

            if (!string.IsNullOrWhiteSpace(current.phone) &&
                all.Any(c => c.id != current.id && !c.is_deleted &&
                             string.Equals((c.phone ?? "").Trim(), current.phone, StringComparison.OrdinalIgnoreCase)))
                throw new DomainException("El teléfono ya existe.");

            // Auditoría de actualización
            current.updated_by = actorId;
            current.updated_at = DateTime.Now;

            await _providerRepository.Update(current);
        }

        public async Task SoftDeleteAsync(int id, int actorId)
        {
            var current = await _providerRepository.GetById(new Provider { id = id })
                          ?? throw new NotFoundException($"Proveedor con ID {id} no encontrado.");

            current.is_deleted = true;
            current.updated_by = actorId;
            current.updated_at = DateTime.Now;

            await _providerRepository.Delete(current);
        }
    }

    // Excepciones de dominio

    public class DomainException : Exception
    {
        public DomainException(string m) : base(m) { }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string m) : base(m) { }
    }

    public class ValidationException : Exception
    {
        public Dictionary<string, string> Errors { get; }

        public ValidationException(string message, Dictionary<string, string> errors)
            : base(message)
        {
            Errors = errors;
        }
    }
}
