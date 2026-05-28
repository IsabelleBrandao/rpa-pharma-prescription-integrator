using RPA_PHARMA.Core.Models;
using RPA_PHARMA.Core.Domain.Exceptions;
using System;
using System.Linq;

namespace RPA_PHARMA.Core.Domain
{
    public class PrescriptionComplianceValidator
    {
        /// <summary>
        /// Avalia a receita contra as regras de compliance da operação.
        /// Se a receita for inválida, lança uma PharmaBusinessException detalhando o motivo.
        /// </summary>
        public void ValidateForProcessing(PrescriptionDTO prescription)
        {
            // 1. Integridade Estrutural Básica (Paciente)
            if (prescription.Patient == null || string.IsNullOrWhiteSpace(prescription.Patient.Document))
                throw new PharmaBusinessException("Receita sem identificação válida do paciente (Documento ausente).");

            if (!IsValidCpf(prescription.Patient.Document))
                throw new PharmaBusinessException($"O documento do paciente (CPF: {prescription.Patient.Document}) é inválido.");

            // 2. Integridade Estrutural Básica (Médico)
            if (prescription.Doctor == null || string.IsNullOrWhiteSpace(prescription.Doctor.CRM))
                throw new PharmaBusinessException("Receita sem identificação válida do médico (CRM ausente).");

            if (string.IsNullOrWhiteSpace(prescription.Doctor.Name))
                throw new PharmaBusinessException("Receita sem identificação válida do médico (Nome ausente).");

            // 3. Integridade Estrutural Básica (Medicamentos)
            if (prescription.Medications == null || !prescription.Medications.Any())
                throw new PharmaBusinessException("Nenhum medicamento foi identificado na prescrição.");

            // 4. Validação Temporal
            if (prescription.IssueDate > DateTime.Now)
                throw new PharmaBusinessException("Inconsistência temporal: A data de emissão não pode ser no futuro.");

            if (prescription.ExpiryDate.HasValue && prescription.ExpiryDate.Value < DateTime.Now.Date)
                throw new PharmaBusinessException($"Receita vencida desde {prescription.ExpiryDate.Value:dd/MM/yyyy}.");

            // 5. Regra de Negócio Crítica (Cadeia de Frio / Logística)
            // Impede que medicamentos sensíveis sejam enviados por transportadoras inadequadas.
            bool requiresColdChain = prescription.Medications.Any(m => 
                m.Description.Contains("TIRZEPATIDA", StringComparison.OrdinalIgnoreCase) ||
                m.Description.Contains("IGF", StringComparison.OrdinalIgnoreCase));

            if (requiresColdChain)
            {
                string[] restrictedCarriers = new string[] { "Correios", "Sedex", "Loggi", "Lalamove", "Uber", "Motoboy" };
                
                string distributor = prescription.Logistics?.DistributorName ?? string.Empty;
                bool isRestricted = restrictedCarriers.Any(carrier => 
                    distributor.Contains(carrier, StringComparison.OrdinalIgnoreCase));

                if (isRestricted || string.IsNullOrWhiteSpace(distributor))
                {
                    throw new PharmaBusinessException($"Medicamentos refrigerados (cadeia de frio) exigem transportadora especializada. A transportadora selecionada '{distributor}' é inválida.");
                }
            }
        }

        /// <summary>
        /// Validação matemática de CPF pelo algoritmo de dígitos verificadores (Módulo 11).
        /// </summary>
        private bool IsValidCpf(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf)) return false;
            
            // Remove qualquer caractere que não seja número
            cpf = new string(cpf.Where(char.IsDigit).ToArray());
            
            if (cpf.Length != 11) return false;

            // Elimina CPFs comuns com todos os dígitos iguais
            if (new string(cpf[0], 11) == cpf) return false;

            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf = cpf.Substring(0, 9);
            int soma = 0;

            for (int i = 0; i < 9; i++)
                soma += (tempCpf[i] - '0') * multiplicador1[i];

            int resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            string digito = resto.ToString();
            tempCpf = tempCpf + digito;
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += (tempCpf[i] - '0') * multiplicador2[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = digito + resto.ToString();
            return cpf.EndsWith(digito);
        }
    }
}