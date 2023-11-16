using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParser.Models.Products
{
    /// <summary>
    /// Внешняя обойма.
    /// </summary>
    public class OuterHolder : Product
    {
        public OuterHolder(Product product) : base(product)
        {
            
        }
    }
}
