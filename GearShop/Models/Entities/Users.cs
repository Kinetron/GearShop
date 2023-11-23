namespace GearShop.Models.Entities
{
    public class Users
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string HashSalt { get; set; }

        public int Deleted { get; set; }
    }
}
