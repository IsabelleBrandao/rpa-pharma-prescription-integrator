using System;
using System.Collections.Generic;

namespace RPA_PHARMA.Core.Models
{
    public class MedicationItemDTO
    {
        public string Description { get; set; }  // Ex: 1 TIRZEPATIDA 93,6mg/3,6ml
        public string Instructions { get; set; } // Ex: Aplicar SC 9,36mg/semana, uso contínuo. Uso SC em consultório.
        public int Quantity { get; set; }        // Ex: 3
    }
}