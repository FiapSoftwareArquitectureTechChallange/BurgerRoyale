﻿using System.Text.RegularExpressions;

namespace BurgerRoyale.Domain.Validation
{
    public static class Validate
    {
        public static bool IsCpfValid(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            // Remove caracteres não numéricos do CPF
            cpf = new string(cpf.Where(char.IsDigit).ToArray());

            if (cpf.Length != 11)
                return false;

            // Verifica se todos os dígitos são iguais, o que é inválido
            if (cpf.All(digit => digit == cpf[0]))
                return false;

            int[] numeros = cpf.Take(9).Select(digit => int.Parse(digit.ToString())).ToArray();
            int[] digitosVerificadores = cpf.Skip(9).Take(2).Select(digit => int.Parse(digit.ToString())).ToArray();

            // Calcula o primeiro dígito verificador
            int primeiroDigitoVerificador = CalcularDigitoVerificador(numeros, 10);

            // Calcula o segundo dígito verificador
            int segundoDigitoVerificador = CalcularDigitoVerificador(numeros.Concat(new[] { primeiroDigitoVerificador }), 11);

            return primeiroDigitoVerificador == digitosVerificadores[0] && segundoDigitoVerificador == digitosVerificadores[1];
        }
        private static int CalcularDigitoVerificador(IEnumerable<int> numeros, int multiplicador)
        {
            int soma = numeros.Select((num, index) => num * (multiplicador - index)).Sum();
            int resto = soma % 11;
            return (resto < 2) ? 0 : (11 - resto);
        }

        public static bool IsEmailValid(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Use uma expressão regular para validar o formato do email
                var regex = new Regex(@"^[\w\.-]+@[\w\.-]+\.\w+$");
                return regex.IsMatch(email);
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
    }
}