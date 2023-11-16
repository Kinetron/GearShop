using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParser.Models.Products
{
    /// <summary>
    /// Звено переходное, Звено соединительное
    /// </summary>
    public class СhainLink : Product
    {
        public СhainLink(Product product) : base(product)
        {
            
        }
    }
}
