using SQLite;

namespace Todo.Models
{
    // Одна строка таблицы. ParentId = 0 означает основную задачу.
    public class TodoItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Title { get; set; } = "";
        public string Category { get; set; } = "Без категории";
        public bool IsCompleted { get; set; }
        public int ParentId { get; set; }
    }
}
