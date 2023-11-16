using DataParser.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataParser.Models;
using DataParser.Models.Products;

namespace DataParser.ProductTypeParsers
{
    internal class CuffParser : IProductTypeParser
    {
        private readonly List<Cuff> _cuffs;
        public string LastError { get; }

        public CuffParser(List<Cuff> cuffs)
        {
            _cuffs = cuffs;
        }
        public bool Parse(Product data)
        {
            Cuff cuff = new Cuff(data);
            _cuffs.Add(cuff);

            return true;
        }
    }
}
