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
    internal class SleeveParser : IProductTypeParser
    {
        private readonly List<Sleeve> _sleeves;
        public string LastError { get; }

        public SleeveParser(List<Sleeve> sleeves)
        {
            _sleeves = sleeves;
        }
        public bool Parse(Product data)
        {
            Sleeve sleeve = new Sleeve(data);
            _sleeves.Add(sleeve);

            return true;
        }
    }
}
