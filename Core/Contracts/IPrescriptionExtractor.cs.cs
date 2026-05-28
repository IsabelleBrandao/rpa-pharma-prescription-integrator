using RPA_PHARMA.Core.Models;

namespace RPA_PHARMA.Core.Contracts
{
    public interface IPrescriptionExtractor
    {
        PrescriptionDTO Extract(string pdfFilePath, string logisticsSelection);
    }
}