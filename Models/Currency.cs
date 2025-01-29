using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Currency
    {
        public int Id { get; set; }
        [MaxLength(3)]
        public string Code {  get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public decimal ExchangeRate { get; set; }
    }
}
