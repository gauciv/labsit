namespace LaboratorySitInSystem.Models
{
    public class Student
    {
        public string StudentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Course { get; set; }
        public int YearLevel { get; set; }
        public string FullName => $"{FirstName} {LastName}";
    }
}
