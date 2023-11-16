using DataParser.Models.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataParser.Models
{
    /// <summary>
    /// Все продукты загружаемые из прайса.
    /// </summary>
    public class AllProducts
    {
        /// <summary>
        /// Подшипники
        /// </summary>
        public List<Bearing> Bearings { get; set; } = new List<Bearing>();

        /// <summary>
        /// Внешняя обойма.
        /// </summary>
        public List<OuterHolder> OuterHolders { get; set; } = new List<OuterHolder>();

        /// <summary>
        /// Втулки.
        /// </summary>
        public List<Sleeve> Sleeves { get; set; } = new List<Sleeve>();

        /// <summary>
        /// Грязесъемники
        /// </summary>
        public List<MudStripper> MudStrippers { get; set; } = new List<MudStripper>();

        /// <summary>
        /// Звенья
        /// </summary>
        public List<СhainLink> СhainLinks { get; set; } = new List<СhainLink>();

        /// <summary>
        /// Кольца.
        /// </summary>
        public List<Ring> Rings { get; set; } = new List<Ring>();

        /// <summary>
        /// Корпуса.
        /// </summary>
        public List<Box> Boxes { get; set; } = new List<Box>();

        /// <summary>
        /// Манжеты.
        /// </summary>
        public List<Cuff> Cuffs { get; set; } = new List<Cuff>();
    }
}
