using RPA_PHARMA.Core.Models;

namespace RPA_PHARMA.Core.Contracts
{
    public interface IOrderGateway
    {
        string RegisterOrder(PrescriptionDTO prescription);
        void AttachDocumentToOrder(string orderId, string pdfFilePath);
    }
}