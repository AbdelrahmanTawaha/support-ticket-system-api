using DataAccessLayer.ConfigurationsSetting.Enums;
namespace BusinessLayer.Models
{


    public class UserSimpleModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public bool IsActive { get; set; }
        public UserType UserType { get; set; }
    }


}
