namespace GearShop.Services.Repository.Entities
{
    public class Users
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string HashSalt { get; set; }
    }
}
