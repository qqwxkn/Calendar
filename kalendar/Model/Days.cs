namespace kalendar.Model
{
    public class Days
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } 
        public string Category { get; set; } = "";
        public string Event { get; set; } = ""; 
    }
}
