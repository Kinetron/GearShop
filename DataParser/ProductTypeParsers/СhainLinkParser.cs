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
    internal class СhainLinkParser : IProductTypeParser
    {
        private readonly List<СhainLink> _сhainLinks;
        public string LastError { get; }

        public СhainLinkParser(List<СhainLink> сhainLinks)
        {
            _сhainLinks = сhainLinks;
        }
        public bool Parse(Product data)
        {
            СhainLink сhainLink = new СhainLink(data);
            _сhainLinks.Add(сhainLink);

            return true;
        }
    }
}
