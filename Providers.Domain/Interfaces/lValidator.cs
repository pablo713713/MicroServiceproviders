
using FluentResults;

namespace Providers.Domain.Interfaces
{
    public interface IValidator<T>
    {
        Result Validate(T entity);
    }
}