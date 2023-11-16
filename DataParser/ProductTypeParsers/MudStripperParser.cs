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
    internal class MudStripperParser : IProductTypeParser
    {
        private readonly List<MudStripper> _mudStrippers;
        public string LastError { get; }

        public MudStripperParser(List<MudStripper> mudStrippers)
        {
            _mudStrippers = mudStrippers;
        }
        public bool Parse(Product data)
        {
            MudStripper mudStripper = new MudStripper(data);
            _mudStrippers.Add(mudStripper);

            return true;
        }
    }
}
