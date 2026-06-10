using System.Net.Mail;
using FinControl.Domain.Exceptions;

namespace FinControl.Domain.Common;

internal static class ValidacaoDominio
{
    public static string TextoObrigatorio(string? valor, string campo, int tamanhoMaximo)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new RegraDeNegocioException($"{campo} e obrigatorio.");
        }

        var texto = valor.Trim();

        if (texto.Length > tamanhoMaximo)
        {
            throw new RegraDeNegocioException($"{campo} deve ter no maximo {tamanhoMaximo} caracteres.");
        }

        return texto;
    }

    public static string? TextoOpcional(string? valor, string campo, int tamanhoMaximo)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        var texto = valor.Trim();

        if (texto.Length > tamanhoMaximo)
        {
            throw new RegraDeNegocioException($"{campo} deve ter no maximo {tamanhoMaximo} caracteres.");
        }

        return texto;
    }

    public static string EmailObrigatorio(string? valor, string campo, int tamanhoMaximo)
    {
        var email = TextoObrigatorio(valor, campo, tamanhoMaximo).ToLowerInvariant();

        try
        {
            var endereco = new MailAddress(email);

            if (!string.Equals(endereco.Address, email, StringComparison.OrdinalIgnoreCase))
            {
                throw new RegraDeNegocioException($"{campo} deve ser valido.");
            }
        }
        catch (FormatException)
        {
            throw new RegraDeNegocioException($"{campo} deve ser valido.");
        }

        return email;
    }

    public static void IdObrigatorio(int valor, string campo)
    {
        if (valor <= 0)
        {
            throw new RegraDeNegocioException($"{campo} deve ser informado.");
        }
    }

    public static void ValorPositivo(decimal valor, string campo)
    {
        if (valor <= 0)
        {
            throw new RegraDeNegocioException($"{campo} deve ser maior que zero.");
        }
    }

    public static void ValorNaoNegativo(decimal valor, string campo)
    {
        if (valor < 0)
        {
            throw new RegraDeNegocioException($"{campo} nao pode ser negativo.");
        }
    }
}
