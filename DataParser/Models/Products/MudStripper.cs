using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParser.Models.Products
{
    /// <summary>
    /// Грязесъемник
    /// </summary>
    public class MudStripper : Product
    {
        public MudStripper(Product product) : base(product)
        {
            
        }
    }
}
