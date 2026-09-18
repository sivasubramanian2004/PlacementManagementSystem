using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Data.Entities
{
    public class ApplicationNumberCounter
    {
        public int Id { get; set; }

        public int Year { get; set; }

        public int LastSequence { get; set; }
    }
}
/* 
 
 
 Year = 2026
Sequence = 1
→ A260001

Sequence = 25
→ A260025

Sequence = 9999
→ A269999

Sequence = 10000
→ A2610000

D4 means minimum 4 digits, so overflow problem illa.
 
 
 */