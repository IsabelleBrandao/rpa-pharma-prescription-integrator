using System;
using System.Collections.Generic;

namespace RPA_PHARMA.Core.Models
{
    public class PrescriptionDTO
    {
        public string TransactionReference { get; set; } 
        
        // Datas
        public DateTime IssueDate { get; set; } // Data de emissão: 27/04/2026 [cite: 3, 28, 54]
        public DateTime? ExpiryDate { get; set; } // Data de Validade: 27/05/2026 [cite: 13, 14, 38, 39, 63, 64]
        
        // Composições
        public PatientDTO Patient { get; set; }
        public DoctorDTO Doctor { get; set; }
        public LogisticsDTO Logistics { get; set; }
        public List<MedicationItemDTO> Medications { get; set; }
    }
}