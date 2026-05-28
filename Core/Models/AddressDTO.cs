using System;
using System.Collections.Generic;

namespace RPA_PHARMA.Core.Models
{
    public class AddressDTO
    {
       // Logradouro (Ex: Rua Maestro Zico Seabra) [cite: 4, 29, 55]
        public string Street { get; set; }

        // Número (Ex: 279) [cite: 4, 29, 55]
        public string Number { get; set; }

        // Complemento (Opcional, comum em clínicas)
        public string Complement { get; set; }

        // Bairro (Ex: Saudade) [cite: 4, 29, 55]
        public string Neighborhood { get; set; }

        // Cidade (Ex: Araçatuba ou Brasília) [cite: 4, 29, 55, 82, 97]
        public string City { get; set; }

        // UF (Ex: SP ou DF) [cite: 4, 29, 55, 82, 97]
        public string State { get; set; }

        // CEP (Ex: 71218010 ou 70711-070) [cite: 82, 97]
        public string ZipCode { get; set; }
    }
}