using SQLite;

namespace Todo.Models
{
    public class TodoItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Title { get; set; } = "";
        public string Category { get; set; } = "Без категории";
        public bool IsCompleted { get; set; }
        public int? ParentId { get; set; }
    }
}