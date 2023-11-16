using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataParser;
using DataParser.Models;

namespace Tests
{
    public class DataParserTests
    {
        [Test]
        public void ParserTest()
        {
           Excel1сShopParser2022Format parser = new Excel1сShopParser2022Format();
           List<ProductInfo> products = parser.ParseFile("g:\\LocalRepository\\GearShop\\прайс с картинками.xlsx", 200);
           Assert.IsNotNull(products, parser.LastError);
           parser.SaveToСsvFile(products, "parse.txt", "|");
        }
    }
}
