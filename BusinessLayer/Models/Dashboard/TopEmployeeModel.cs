namespace BusinessLayer.Models.Dashboard
{
    public class TopEmployeeModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int AssignedCount { get; set; }
        public int ClosedCount { get; set; }
        public int ResolvedCount { get; set; }
        public string ImageUrl { get; set; }

    }
}
