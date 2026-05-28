using Newtonsoft.Json.Linq;
using RPA_PHARMA.Core.Models;
using RPA_PHARMA.Helpers; 
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RPA_PHARMA.Infrastructure.Adapters.AzureAI
{
    public static class PrescriptionMapper
    {
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1.5);

        public static PrescriptionDTO MapToDTO(string azureJsonResponse, string logisticsSelection)
        {
            var rawData = JObject.Parse(azureJsonResponse);
            var dto = new PrescriptionDTO();

            // Pegamos o texto bruto onde tudo está misturado
            string contentBlock = rawData.SelectToken("analyzeResult.content")?.ToString() ?? "";

            dto.TransactionReference = GenerateHash(contentBlock);
            
            // Regex para capturar datas (Ex: 04/03/2026 ou 04 de Março de 2026)
            var dateMatch = Regex.Match(contentBlock, @"\d{2}\/\d{2}\/\d{4}", RegexOptions.None, RegexTimeout);
            if (dateMatch.Success && DateTime.TryParse(dateMatch.Value, out DateTime issueDate))
            {
                dto.IssueDate = issueDate;
            }

            // Regex para capturar a data de validade (Ex: Validade: 27/05/2026)
            var expiryDateMatch = Regex.Match(contentBlock, @"Validade:?\s*(\d{2}\/\d{2}\/\d{4})", RegexOptions.None, RegexTimeout);
            if (expiryDateMatch.Success && DateTime.TryParse(expiryDateMatch.Groups[1].Value, out DateTime expiryDate))
            {
                dto.ExpiryDate = expiryDate;
            }

            dto.Patient = ExtractPatient(contentBlock);
            dto.Doctor = ExtractDoctor(contentBlock);
            dto.Medications = ExtractMedications(contentBlock);
            
            dto.Logistics = new LogisticsDTO { DistributorName = logisticsSelection };

            return dto;
        }

        private static DoctorDTO ExtractDoctor(string content)
        {
            // Captura o nome na linha contendo Dr., Dra., Dr(a)., etc.
            var nameMatch = Regex.Match(content, @"(?:Dr\.|Dra\.|Dr\(a\)\.?|Dr|Dra)\s+([A-Za-zÀ-ÿ\s.-]+?)(?=\s+CRM|\s+M[ée]dico|\n|$)", RegexOptions.IgnoreCase, RegexTimeout);
            // Captura o padrão CRM, aceitando dois pontos opcional, traço, barra, etc. (Ex: CRM: 149557 - SP ou CRM 12345/SP)
            var crmMatch = Regex.Match(content, @"CRM\s*:?\s*(\d+)\s*(?:[-/]?\s*([A-Z]{2}))?", RegexOptions.IgnoreCase, RegexTimeout);

            return new DoctorDTO
            {
                Name = nameMatch.Success ? StringHelper.NormalizeForUi(nameMatch.Groups[1].Value.Trim()) : "",
                // CRM higienizado (só números)
                CRM = crmMatch.Success ? DocumentHelper.Sanitize(crmMatch.Groups[1].Value) : "",
                
                Address = new AddressDTO
                {
                    City = Regex.Match(content, @"Bras[íi]lia-?DF", RegexOptions.IgnoreCase, RegexTimeout).Success ? "BRASILIA" : "",
                    State = crmMatch.Success && crmMatch.Groups[2].Success ? crmMatch.Groups[2].Value.ToUpper() : "DF",
                    ZipCode = DocumentHelper.Sanitize(Regex.Match(content, @"CEP:?\s*(\d{5}-?\d{3})", RegexOptions.None, RegexTimeout).Groups[1].Value)
                }
            };
        }

        private static PatientDTO ExtractPatient(string content)
        {
            // Captura nome do paciente (entre "Paciente:" e "CPF:")
            var nameMatch = Regex.Match(content, @"Paciente:\s*(.+?)\s*CPF:", RegexOptions.None, RegexTimeout);
            // Captura CPF
            var cpfMatch = Regex.Match(content, @"\d{3}\.\d{3}\.\d{3}-\d{2}", RegexOptions.None, RegexTimeout);

            return new PatientDTO
            {
                Name = nameMatch.Success ? StringHelper.NormalizeForUi(nameMatch.Groups[1].Value.Trim()) : "",
                Document = cpfMatch.Success ? DocumentHelper.Sanitize(cpfMatch.Value) : ""
            };
        }

        private static List<MedicationItemDTO> ExtractMedications(string content)
        {
            var medicationsList = new List<MedicationItemDTO>();

            // 1. Localiza a presença do medicamento (IGF+ ou TIRZEPATIDA)
            var medNameMatch = Regex.Match(content, @"(?:IGF\+?|TIRZEPATIDA)", RegexOptions.IgnoreCase, RegexTimeout);
            if (medNameMatch.Success)
            {
                // Extrai a linha ou o bloco do medicamento para descrição
                string description = "";
                // Tenta ver se está no formato com parênteses: (MEDICAMENTO)
                var parenMatch = Regex.Match(content, @"\(((?:IGF\+|TIRZEPATIDA).*?)\)", RegexOptions.IgnoreCase, RegexTimeout);
                if (parenMatch.Success)
                {
                    description = parenMatch.Groups[1].Value.Trim();
                }
                else
                {
                    // Caso contrário, pega a linha inteira contendo o medicamento
                    var lineMatch = Regex.Match(content, @"^[^\n]*(?:IGF\+?|TIRZEPATIDA)[^\n]*", RegexOptions.Multiline | RegexOptions.IgnoreCase, RegexTimeout);
                    description = lineMatch.Success ? lineMatch.Value.Trim() : medNameMatch.Value;
                }

                // 2. Localiza a quantidade (ex: 2 unidades, 1 unids, 2 unids, etc.)
                int qty = 1; // Default
                var qtyMatch = Regex.Match(content, @"\b(\d+)\s*(?:unid(?:ade)?s?|unids?)\b", RegexOptions.IgnoreCase, RegexTimeout);
                if (qtyMatch.Success)
                {
                    int.TryParse(qtyMatch.Groups[1].Value, out qty);
                }
                else
                {
                    // Fallback caso a quantidade esteja no formato " - X unids" colado
                    var altQtyMatch = Regex.Match(content, @"-\s*(\d+)\s*unid", RegexOptions.IgnoreCase, RegexTimeout);
                    if (altQtyMatch.Success)
                    {
                        int.TryParse(altQtyMatch.Groups[1].Value, out qty);
                    }
                }

                medicationsList.Add(new MedicationItemDTO
                {
                    Description = StringHelper.NormalizeForUi(description),
                    Quantity = qty
                });
            }

            return medicationsList;
        }
        
        /// <summary>
        /// Gera um hash SHA256 determinístico dos primeiros 16 caracteres a partir do conteúdo bruto da receita.
        /// Garante que reprocessar a mesma receita gerará o mesmo hash de transação único.
        /// </summary>
        private static string GenerateHash(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return "00000000";
            
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(content.Trim());
                byte[] hash = sha256.ComputeHash(bytes);
                var sb = new System.Text.StringBuilder();
                // Utiliza os primeiros 8 bytes (16 caracteres em hexadecimal) para o hash único
                for (int i = 0; i < 8; i++)
                {
                    sb.Append(hash[i].ToString("x2"));
                }
                return sb.ToString().ToUpper();
            }
        }
    }
}