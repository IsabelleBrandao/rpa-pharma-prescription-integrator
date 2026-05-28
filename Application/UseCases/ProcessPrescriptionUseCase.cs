using RPA_PHARMA.Core.Contracts;
using RPA_PHARMA.Core.Models;
using System;

namespace RPA_PHARMA.Application.UseCases
{
    public class ProcessPrescriptionUseCase
    {
        // O Use Case depende apenas das abstrações (Interfaces), nunca das implementações (Azure/Protheus)
        private readonly IPrescriptionExtractor _extractor;
        private readonly IOrderGateway _orderGateway;

        // Injeção de Dependência via Construtor
        public ProcessPrescriptionUseCase(IPrescriptionExtractor extractor, IOrderGateway orderGateway)
        {
            _extractor = extractor;
            _orderGateway = orderGateway;
        }

        /// <summary>
        /// Executa o fluxo principal de processamento de uma receita médica.
        /// </summary>
        /// <param name="pdfFilePath">Caminho do arquivo baixado temporariamente.</param>
        /// <param name="logisticsSelection">Opção de logística vinda da fila (UiPath Apps).</param>
        /// <returns>Retorna o ID do pedido gerado no ERP.</returns>
        public string Execute(string pdfFilePath, string logisticsSelection)
        {
            // 1. EXTRAÇÃO (Chama a porta de entrada)
            PrescriptionDTO prescription = _extractor.Extract(pdfFilePath, logisticsSelection);

            if (prescription == null || prescription.Patient == null || prescription.Doctor == null)
            {
                // No REFramework, isso deve ser tratado como System Exception (Falha na IA/Leitura)
                throw new InvalidOperationException("Falha crítica: O extrator não conseguiu montar o objeto da receita.");
            }

            // 2. REGRA DE NEGÓCIO (Domain Logic)
            // Aqui blindamos o ERP de receber lixo. Se falhar, é Business Rule Exception.
            if (prescription.ExpiryDate.HasValue && prescription.ExpiryDate.Value < DateTime.Now)
            {
                throw new ArgumentException($"BUSINESS_RULE: A receita do paciente {prescription.Patient.Name} está vencida desde {prescription.ExpiryDate.Value:dd/MM/yyyy}.");
            }

            if (prescription.Medications == null || prescription.Medications.Count == 0)
            {
                throw new ArgumentException($"BUSINESS_RULE: Nenhum medicamento foi identificado na receita do paciente {prescription.Patient.Name}.");
            }

            // 3. INTEGRAÇÃO (Chama a porta de saída)
            string orderId = _orderGateway.RegisterOrder(prescription);

            if (string.IsNullOrWhiteSpace(orderId))
            {
                throw new InvalidOperationException("Falha na integração: O ERP não retornou o número do pedido.");
            }

            // 4. ARQUIVAMENTO
            _orderGateway.AttachDocumentToOrder(orderId, pdfFilePath);

            // Fluxo concluído com sucesso
            return orderId;
        }
    }
}