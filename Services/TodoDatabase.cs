using System;
using System.Collections.Generic;
using SQLite;
using Todo.Models;

namespace Todo.Services
{
    // Сервис ничего не знает о странице и кнопках. Поэтому его можно тестировать отдельно.
    public class TodoDatabase : IDisposable
    {
        private SQLiteConnection connection;

        public TodoDatabase(string path)
        {
            connection = new SQLiteConnection(path);
            connection.CreateTable<TodoItem>();
        }

        public List<TodoItem> GetTasks()
        {
            return connection.Table<TodoItem>().ToList();
        }

        public TodoItem AddTask(string title, string category, int parentId)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Заголовок не должен быть пустым.");
            }

            if (parentId != 0)
            {
                TodoItem? parent = connection.Find<TodoItem>(parentId);
                if (parent == null || parent.ParentId != 0)
                {
                    throw new ArgumentException("Подзадачу можно добавить только к основной задаче.");
                }
            }

            TodoItem item = new TodoItem();
            item.Title = title.Trim();
            item.Category = category;
            item.ParentId = parentId;
            connection.Insert(item);
            return item;
        }

        public void SetCompleted(int id, bool isCompleted)
        {
            TodoItem? item = connection.Find<TodoItem>(id);
            if (item == null)
            {
                throw new ArgumentException("Задача не найдена.");
            }

            item.IsCompleted = isCompleted;
            connection.Update(item);
        }

        public void DeleteTask(int id)
        {
            if (id <= 0)
            {
                return;
            }

            List<TodoItem> items = GetTasks();
            connection.BeginTransaction();
            try
            {
                foreach (TodoItem item in items)
                {
                    if (item.ParentId == id)
                    {
                        connection.Delete<TodoItem>(item.Id);
                    }
                }

                connection.Delete<TodoItem>(id);
                connection.Commit();
            }
            catch
            {
                connection.Rollback();
                throw;
            }
        }

        public void Dispose()
        {
            connection.Dispose();
        }
    }
}
