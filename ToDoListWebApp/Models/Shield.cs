namespace ToDoListWebApp.Models
{
    public class Shield
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? LastModified { get; set; }
        public string Title { get; set; }
        public string TargetUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public Guid UserId { get; set; }
        public virtual User User { get; set; }
    }
}
