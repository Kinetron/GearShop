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
    internal class BoxParser : IProductTypeParser
    {
        private readonly List<Box> _boxes;
        public string LastError { get; }

        public BoxParser(List<Box> boxes)
        {
            _boxes = boxes;
        }

        public bool Parse(Product data)
        {
            Box box = new Box(data);
            _boxes.Add(box);

            return true;
        }
    }
}
