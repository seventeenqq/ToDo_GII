
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
        private readonly TodoDatabase database;

        public TodoDatabaseTests()
        {
            database = new TodoDatabase(":memory:");
        }

        [Fact]
        public void AddTaskStoresTitleAndCategory()
        {
            TodoItem added = database.AddTask(
                "  Купить хлеб  ", "Покупки");
            
            List<TodoItem> items = database.GetTasks();
            TodoItem saved = Assert.Single(items);

            Assert.True(added.Id > 0);
            Assert.Equal(added.Id, saved.Id);
            Assert.Equal("Купить хлеб", saved.Title);
            Assert.Equal("Покупки", saved.Category);
            Assert.False(saved.IsCompleted);
            Assert.Null(saved.ParentId);
        }

        [Fact]
        public void DeleteTaskKeepsOtherTasks()
        {
            TodoItem first = database.AddTask(
                "Хлеб", "Покупки");

            TodoItem second = database.AddTask(
                "Молоко", "Покупки");

            database.DeleteTask(first.Id);
            database.DeleteTask(0);

            TodoItem remaining =
                Assert.Single(database.GetTasks());

            Assert.Equal(second.Id, remaining.Id);
        }

        [Fact]
        public void CompletionCanBeSetAndCancelled()
        {
            TodoItem item = database.AddTask(
                "Сделать лабораторную", "Учёба");

            database.SetCompleted(item.Id, true);

            Assert.True(
                Assert.Single(database.GetTasks()).IsCompleted);

            database.SetCompleted(item.Id, false);

            Assert.False(
                Assert.Single(database.GetTasks()).IsCompleted);
        }

        [Fact]
        public void DeletingParentAlsoDeletesItsSubtasks()
        {
            TodoItem parent = database.AddTask(
                "Уборка", "Дом");

            TodoItem child = database.AddTask(
                "Помыть пол", "Дом", parent.Id);

            database.AddTask(
                "Вытереть пыль", "Дом", parent.Id);

            TodoItem other = database.AddTask(
                "Прочитать книгу", "Учёба");

            Assert.Null(parent.ParentId);
            Assert.Equal(parent.Id, child.ParentId);
            Assert.Equal(4, database.GetTasks().Count);

            database.DeleteTask(parent.Id);

            TodoItem remaining =
                Assert.Single(database.GetTasks());

            Assert.Equal(other.Id, remaining.Id);
        }

        [Fact]
        public void DeletingSubtaskKeepsParentAndSibling()
        {
            TodoItem parent = database.AddTask(
                "Уборка", "Дом");

            TodoItem first = database.AddTask(
                "Пол", "Дом", parent.Id);

            TodoItem second = database.AddTask(
                "Окна", "Дом", parent.Id);

            database.DeleteTask(first.Id);

            List<TodoItem> items = database.GetTasks();

            Assert.Equal(2, items.Count);
            Assert.Contains(items, item => item.Id == parent.Id);
            Assert.Contains(items, item => item.Id == second.Id);
            Assert.DoesNotContain(items, item => item.Id == first.Id);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\n")]
        public void BlankTitleIsNotStored(string title)
        {
            Assert.Throws<ArgumentException>(() =>
                database.AddTask(title, "Дом"));

            Assert.Empty(database.GetTasks());
        }

        public void Dispose()
        {
            database.Dispose();
        }
    }
}
