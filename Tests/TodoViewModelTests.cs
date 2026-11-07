using System;
using System.IO;
using Todo.Services;
using Todo.ViewModels;
using Xunit;

namespace Todo.Tests
{
    public class TodoViewModelTests : IDisposable
    {
        private string path;
        private TodoDatabase database;

        public TodoViewModelTests()
        {
            path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db3");
            database = new TodoDatabase(path);
        }

        [Fact]
        public void AddCommandDependsOnTitleAndDisablesAfterAdding()
        {
            TodoViewModel viewModel = new TodoViewModel(database);
            Assert.False(viewModel.AddTaskCommand.CanExecute(null));

            viewModel.SelectedCategory = "Работа";
            Assert.False(viewModel.AddTaskCommand.CanExecute(null));

            viewModel.NewTitle = "   ";
            Assert.False(viewModel.AddTaskCommand.CanExecute(null));
            viewModel.AddTaskCommand.Execute(null);
            Assert.Empty(database.GetTasks());

            viewModel.NewTitle = "Написать отчёт";
            Assert.True(viewModel.AddTaskCommand.CanExecute(null));
            viewModel.AddTaskCommand.Execute(null);

            Assert.Single(viewModel.Tasks);
            Assert.Equal("Написать отчёт", viewModel.Tasks[0].Title);
            Assert.Equal("", viewModel.NewTitle);
            Assert.False(viewModel.AddTaskCommand.CanExecute(null));
        }

        [Fact]
        public void SubtaskAppearsBelowParentAndCanBeToggledAndDeleted()
        {
            TodoViewModel viewModel = new TodoViewModel(database);
            viewModel.NewTitle = "Уборка";
            viewModel.AddTaskCommand.Execute(null);
            int parentId = viewModel.Tasks[0].Id;

            viewModel.SelectedParent = viewModel.Tasks[0];
            viewModel.NewTitle = "Помыть пол";
            viewModel.AddTaskCommand.Execute(null);

            Assert.Equal(2, viewModel.Tasks.Count);
            Assert.Equal(parentId, viewModel.Tasks[1].ParentId);
            viewModel.ToggleTaskCommand.Execute(viewModel.Tasks[1]);
            Assert.True(viewModel.Tasks[1].IsCompleted);
            viewModel.ToggleTaskCommand.Execute(viewModel.Tasks[1]);
            Assert.False(viewModel.Tasks[1].IsCompleted);

            viewModel.DeleteTaskCommand.Execute(viewModel.Tasks[0]);
            Assert.Empty(viewModel.Tasks);
            Assert.Single(viewModel.ParentOptions);
            Assert.NotNull(viewModel.SelectedParent);
            Assert.Equal(0, viewModel.SelectedParent.Id);
        }

        public void Dispose()
        {
            database.Dispose();
            File.Delete(path);
        }
    }
}
