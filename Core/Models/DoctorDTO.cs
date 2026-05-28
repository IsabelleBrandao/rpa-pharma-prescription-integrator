using System;
using System.Collections.Generic;

namespace RPA_PHARMA.Core.Models
{
public class DoctorDTO
    {
        public string Name { get; set; }
        public string CRM { get; set; }
        public string Phone { get; set; }
        public AddressDTO Address { get; set; }
    }
}