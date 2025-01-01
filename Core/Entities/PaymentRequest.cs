using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class PaymentRequest
    {

        public int ProductId { get; set; }
        public string PaymentMethodId { get; set; }
    }
}
