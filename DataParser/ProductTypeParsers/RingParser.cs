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
    internal class RingParser : IProductTypeParser
    {
        private readonly List<Ring> _rings;
        public string LastError { get; }

        public RingParser(List<Ring> rings)
        {
            _rings = rings;
        }

        public bool Parse(Product data)
        {
            Ring ring = new Ring(data);
            _rings.Add(ring);

            return true;
        }
    }
}
