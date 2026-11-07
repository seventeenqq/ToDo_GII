
using System;
using Todo.Services;
using Todo.ViewModels;
using Xunit;

namespace Todo.Tests
{
    public class TodoViewModelTests : IDisposable
    {
        private readonly TodoDatabase database;

        public TodoViewModelTests()
        {
            database = new TodoDatabase(":memory:");
        }

        [Fact]
        public void AddCommandDependsOnTitleAndDisablesAfterAdding()
        {
            TodoViewModel viewModel = new TodoViewModel(database);

            Assert.False(
                viewModel.AddTaskCommand.CanExecute(null));

            viewModel.SelectedCategory = "Работа";

            Assert.False(
                viewModel.AddTaskCommand.CanExecute(null));

            viewModel.NewTitle = "   ";

            Assert.False(
                viewModel.AddTaskCommand.CanExecute(null));

            viewModel.AddTaskCommand.Execute(null);

            Assert.Empty(database.GetTasks());

            viewModel.NewTitle = "Написать отчёт";

            Assert.True(
                viewModel.AddTaskCommand.CanExecute(null));

            viewModel.AddTaskCommand.Execute(null);

            Assert.Single(viewModel.Tasks);
            Assert.Equal(
                "Написать отчёт",
                viewModel.Tasks[0].Title);

            Assert.Null(viewModel.Tasks[0].ParentId);
            Assert.Equal("", viewModel.NewTitle);

            Assert.False(
                viewModel.AddTaskCommand.CanExecute(null));
        }

        [Fact]
        public void SubtaskAppearsBelowParentAndCanBeToggledAndDeleted()
        {
            TodoViewModel viewModel = new TodoViewModel(database);

            viewModel.NewTitle = "Уборка";
            viewModel.AddTaskCommand.Execute(null);

            int parentId = viewModel.Tasks[0].Id;

            Assert.Null(viewModel.Tasks[0].ParentId);

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
            Assert.Empty(database.GetTasks());
            Assert.Same(Assert.Single(viewModel.ParentOptions), viewModel.SelectedParent);
            Assert.Equal(0, viewModel.SelectedParent!.Id);
        }

        [Fact]
        public void SameAddCommandCreatesSubtasksAndReturnsToRootThroughPicker()
        {
            TodoViewModel viewModel = new TodoViewModel(database);
            Assert.Same(Assert.Single(viewModel.ParentOptions), viewModel.SelectedParent);

            viewModel.NewTitle = "Уборка";
            viewModel.AddTaskCommand.Execute(null);
            int parentId = viewModel.Tasks[0].Id;

            viewModel.SelectedParent = viewModel.ParentOptions[1];
            viewModel.NewTitle = "Помыть пол";
            viewModel.AddTaskCommand.Execute(null);
            Assert.Equal(parentId, viewModel.SelectedParent!.Id);
            Assert.Same(viewModel.ParentOptions[1], viewModel.SelectedParent);

            viewModel.ToggleTaskCommand.Execute(viewModel.Tasks[1]);
            Assert.Same(viewModel.ParentOptions[1], viewModel.SelectedParent);

            viewModel.NewTitle = "Помыть окна";
            viewModel.AddTaskCommand.Execute(null);
            Assert.Equal(parentId, viewModel.Tasks[1].ParentId);
            Assert.Equal(parentId, viewModel.Tasks[2].ParentId);
            Assert.Equal(2, viewModel.ParentOptions.Count);

            viewModel.DeleteTaskCommand.Execute(viewModel.Tasks[1]);
            Assert.Same(viewModel.ParentOptions[1], viewModel.SelectedParent);

            viewModel.SelectedParent = viewModel.ParentOptions[0];
            viewModel.NewTitle = "Купить хлеб";
            viewModel.AddTaskCommand.Execute(null);
            Assert.Null(viewModel.Tasks[2].ParentId);
            Assert.Equal("Купить хлеб", viewModel.Tasks[2].Title);
            Assert.Same(viewModel.ParentOptions[0], viewModel.SelectedParent);
            Assert.Equal(3, database.GetTasks().Count);
        }

        [Fact]
        public void DeletingSelectedParentAllowsAddingRootTask()
        {
            TodoViewModel viewModel = new TodoViewModel(database);
            viewModel.NewTitle = "Уборка";
            viewModel.AddTaskCommand.Execute(null);
            viewModel.SelectedParent = viewModel.ParentOptions[1];
            viewModel.DeleteTaskCommand.Execute(viewModel.Tasks[0]);

            Assert.Same(Assert.Single(viewModel.ParentOptions), viewModel.SelectedParent);
            viewModel.NewTitle = "Купить хлеб";
            viewModel.AddTaskCommand.Execute(null);

            Assert.Null(Assert.Single(database.GetTasks()).ParentId);
        }

        public void Dispose()
        {
            database.Dispose();
        }
    }
}
