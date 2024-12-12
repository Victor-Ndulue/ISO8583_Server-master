namespace ISO_Server.Models
{
    public class Account
    {
        public required string PAN { get; set; }
        public string FullName { get; set; }
        public decimal AccountBalance { get; set; }
    }
}
