namespace TaskProject.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }

        public string? Email { get; set; }
        public string? PersonalNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Role { get; set; }

        public Patient Patient { get; set; }
        public Doctor Doctor { get; set; }
    }
}
