using FluentResults;
using Providers.Domain.Entities;
using Providers.Domain.Interfaces;
using System.Text.RegularExpressions;

namespace Providers.Application.Validators
{
    public class ProviderValidator : IValidator<Provider>
    {
        private static readonly Regex LettersAndSpaces =
            new(@"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ ]+$", RegexOptions.Compiled);

        private static readonly Regex Email =
            new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        private static readonly Regex Phone =
            new(@"^[0-9]{7,12}(-[0-9]{1})?$", RegexOptions.Compiled);

        public Result Validate(Provider p)
        {
            var r = Result.Ok();
            if (string.IsNullOrWhiteSpace(p.first_name))
            {
                r = r.WithFieldError("first_name", "El nombre es obligatorio.");
            }
            else
            {
                var v = p.first_name.Trim();

                if (v.Length is < 2 or > 50)
                    r = r.WithFieldError("first_name", "El nombre debe tener entre 2 y 50 caracteres.");

                if (!LettersAndSpaces.IsMatch(v))
                    r = r.WithFieldError("first_name", "El nombre solo debe tener letras y espacios.");
            }

            if (string.IsNullOrWhiteSpace(p.last_name))
            {
                r = r.WithFieldError("last_name", "El apellido es obligatorio.");
            }
            else
            {
                var v = p.last_name.Trim();

                if (v.Length is < 2 or > 50)
                    r = r.WithFieldError("last_name", "El apellido debe tener entre 2 y 50 caracteres.");

                if (!LettersAndSpaces.IsMatch(v))
                    r = r.WithFieldError("last_name", "El apellido solo debe contener letras y espacios.");
            }

            if (!string.IsNullOrWhiteSpace(p.email))
            {
                var mail = p.email.Trim();

                if (mail.Length > 100)
                    r = r.WithFieldError("email", "El correo no debe exceder 100 caracteres.");

                if (!Email.IsMatch(mail))
                    r = r.WithFieldError("email", "El correo no tiene un formato válido.");
            }

            if (string.IsNullOrWhiteSpace(p.phone))
            {
                r = r.WithFieldError("phone", "El teléfono es obligatorio.");
            }
            else
            {
                var v = p.phone.Trim();

                if (!Phone.IsMatch(v))
                {
                    r = r.WithFieldError(
                        "phone",
                        "El teléfono debe tener 7–12 dígitos, sin letras ni caracteres especiales (opcionalmente con guion y dígito verificador)."
                    );
                }
            }

            return r;
        }
    }
}
