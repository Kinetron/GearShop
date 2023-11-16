using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParser.Models.Products
{
    /// <summary>
    /// Подшипник
    /// </summary>
    public class Bearing : Product
    {
        public Bearing(Product product) : base(product)
        {
        }

        /// <summary>
        /// Вес, кг.
        /// </summary>
        public decimal Weight { get; set; }
    }
}
