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
    internal class OuterHolderParser : IProductTypeParser
    {
        private readonly List<OuterHolder> _outerHolder;
        public string LastError { get; }

        public OuterHolderParser(List<OuterHolder> outerHolder)
        {
            _outerHolder = outerHolder;
        }

        public bool Parse(Product data)
        {
            OuterHolder holder = new OuterHolder(data);
            holder.Name.Replace("Внешняя обойма", "");

            _outerHolder.Add(holder);

            return true;
        }
    }
}
