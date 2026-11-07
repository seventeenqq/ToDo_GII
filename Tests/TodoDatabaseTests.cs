using System;
using System.Collections.Generic;
using System.IO;
using Todo.Models;
using Todo.Services;
using Xunit;

namespace Todo.Tests
{
    public class TodoDatabaseTests : IDisposable
    {
        private string path;
        private TodoDatabase database;

        public TodoDatabaseTests()
        {
            
            path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db3");
            database = new TodoDatabase(path);
        }

        [Fact]
        public void AddTaskStoresTitleAndCategory()
        {
            TodoItem added = database.AddTask("  Купить хлеб  ", "Покупки", 0);

            List<TodoItem> items = database.GetTasks();
            Assert.Single(items);
            Assert.True(added.Id > 0);
            Assert.Equal(added.Id, items[0].Id);
            Assert.Equal("Купить хлеб", items[0].Title);
            Assert.Equal("Покупки", items[0].Category);
            Assert.False(items[0].IsCompleted);
        }

        [Fact]
        public void DeleteTaskKeepsOtherTasks()
        {
            TodoItem first = database.AddTask("Хлеб", "Покупки", 0);
            TodoItem second = database.AddTask("Молоко", "Покупки", 0);

            database.DeleteTask(first.Id);
            database.DeleteTask(0);

            List<TodoItem> items = database.GetTasks();
            Assert.Single(items);
            Assert.Equal(second.Id, items[0].Id);
        }

        [Fact]
        public void CompletionCanBeSetAndCancelled()
        {
            TodoItem item = database.AddTask("Сделать лабораторную", "Учёба", 0);

            database.SetCompleted(item.Id, true);
            Assert.True(database.GetTasks()[0].IsCompleted);

            database.SetCompleted(item.Id, false);
            Assert.False(database.GetTasks()[0].IsCompleted);
        }

        [Fact]
        public void DeletingParentAlsoDeletesItsSubtasks()
        {
            TodoItem parent = database.AddTask("Уборка", "Дом", 0);
            TodoItem child = database.AddTask("Помыть пол", "Дом", parent.Id);
            database.AddTask("Вытереть пыль", "Дом", parent.Id);
            TodoItem other = database.AddTask("Прочитать книгу", "Учёба", 0);

            Assert.Equal(parent.Id, child.ParentId);
            Assert.Equal(4, database.GetTasks().Count);
            database.DeleteTask(parent.Id);

            List<TodoItem> items = database.GetTasks();
            Assert.Single(items);
            Assert.Equal(other.Id, items[0].Id);
        }

        [Fact]
        public void DeletingSubtaskKeepsParentAndSibling()
        {
            TodoItem parent = database.AddTask("Уборка", "Дом", 0);
            TodoItem first = database.AddTask("Пол", "Дом", parent.Id);
            TodoItem second = database.AddTask("Окна", "Дом", parent.Id);

            database.DeleteTask(first.Id);

            List<TodoItem> items = database.GetTasks();
            Assert.Equal(2, items.Count);
            Assert.Equal(parent.Id, items[0].Id);
            Assert.Equal(second.Id, items[1].Id);
        }

        [Fact]
        public void DataSurvivesClosingAndReopeningDatabase()
        {
            TodoItem parent = database.AddTask("Магазин", "Покупки", 0);
            TodoItem child = database.AddTask("Чай", "Покупки", parent.Id);
            database.SetCompleted(child.Id, true);

            database.Dispose();
            database = new TodoDatabase(path);

            List<TodoItem> items = database.GetTasks();
            Assert.Equal(2, items.Count);
            Assert.Equal("Чай", items[1].Title);
            Assert.Equal("Покупки", items[1].Category);
            Assert.Equal(parent.Id, items[1].ParentId);
            Assert.True(items[1].IsCompleted);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\n")]
        public void BlankTitleIsNotStored(string title)
        {
            bool rejected = false;
            try
            {
                database.AddTask(title, "Дом", 0);
            }
            catch (ArgumentException)
            {
                rejected = true;
            }

            Assert.True(rejected);
            Assert.Empty(database.GetTasks());
        }

        public void Dispose()
        {
            database.Dispose();
            File.Delete(path);
        }
    }
}
