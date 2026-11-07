
using System;
using Microsoft.Maui.Controls;
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
                viewModel.AddTaskCommand.CanExecute(viewModel.NewTitle));

            viewModel.SelectedCategory = "Работа";

            Assert.False(
                viewModel.AddTaskCommand.CanExecute(viewModel.NewTitle));

            viewModel.NewTitle = "   ";

            Assert.False(
                viewModel.AddTaskCommand.CanExecute(viewModel.NewTitle));

            viewModel.AddTaskCommand.Execute(viewModel.NewTitle);

            Assert.Empty(database.GetTasks());

            viewModel.NewTitle = "Написать отчёт";

            Assert.True(
                viewModel.AddTaskCommand.CanExecute(viewModel.NewTitle));

            viewModel.AddTaskCommand.Execute(viewModel.NewTitle);

            Assert.Single(viewModel.Tasks);
            Assert.Equal(
                "Написать отчёт",
                viewModel.Tasks[0].Title);

            Assert.Null(viewModel.Tasks[0].ParentId);
            Assert.Equal("", viewModel.NewTitle);

            Assert.False(
                viewModel.AddTaskCommand.CanExecute(viewModel.NewTitle));
        }

        [Fact]
        public void ButtonRechecksCommandWhenParameterChanges()
        {
            TodoViewModel viewModel = new TodoViewModel(database);
            Button button = new Button
            {
                Command = viewModel.AddTaskCommand,
                CommandParameter = viewModel.NewTitle
            };

            Assert.False(button.IsEnabled);
            viewModel.NewTitle = "Купить хлеб";
            // В приложении новое значение параметра передаёт XAML-привязка.
            button.CommandParameter = viewModel.NewTitle;
            Assert.True(button.IsEnabled);
            Assert.Equal(viewModel.NewTitle, button.CommandParameter);

            viewModel.NewTitle = "   ";
            button.CommandParameter = viewModel.NewTitle;
            Assert.False(button.IsEnabled);

            viewModel.NewTitle = "Купить хлеб";
            button.CommandParameter = viewModel.NewTitle;
            button.Command.Execute(button.CommandParameter);
            Assert.Single(database.GetTasks());
            button.CommandParameter = viewModel.NewTitle;
            Assert.False(button.IsEnabled);
            Assert.Equal("", button.CommandParameter);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void AddCommandRejectsEmptyParameterEvenWhenTitleIsFilled(string? title)
        {
            TodoViewModel viewModel = new TodoViewModel(database);
            viewModel.NewTitle = "Купить хлеб";

            Assert.False(viewModel.AddTaskCommand.CanExecute(title));
            viewModel.AddTaskCommand.Execute(title);
            Assert.Empty(database.GetTasks());
        }

        [Fact]
        public void SubtaskAppearsBelowParentAndCanBeToggledAndDeleted()
        {
            TodoViewModel viewModel = new TodoViewModel(database);

            viewModel.NewTitle = "Уборка";
            viewModel.AddTaskCommand.Execute(viewModel.NewTitle);

            int parentId = viewModel.Tasks[0].Id;

            Assert.Null(viewModel.Tasks[0].ParentId);

            viewModel.SelectedParent = viewModel.Tasks[0];
            viewModel.NewTitle = "Помыть пол";
            viewModel.AddTaskCommand.Execute(viewModel.NewTitle);

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
            viewModel.AddTaskCommand.Execute(viewModel.NewTitle);
            int parentId = viewModel.Tasks[0].Id;

            viewModel.SelectedParent = viewModel.ParentOptions[1];
            viewModel.NewTitle = "Помыть пол";
            viewModel.AddTaskCommand.Execute(viewModel.NewTitle);
            Assert.Equal(parentId, viewModel.SelectedParent!.Id);
            Assert.Same(viewModel.ParentOptions[1], viewModel.SelectedParent);

            viewModel.ToggleTaskCommand.Execute(viewModel.Tasks[1]);
            Assert.Same(viewModel.ParentOptions[1], viewModel.SelectedParent);

            viewModel.NewTitle = "Помыть окна";
            viewModel.AddTaskCommand.Execute(viewModel.NewTitle);
            Assert.Equal(parentId, viewModel.Tasks[1].ParentId);
            Assert.Equal(parentId, viewModel.Tasks[2].ParentId);
            Assert.Equal(2, viewModel.ParentOptions.Count);

            viewModel.DeleteTaskCommand.Execute(viewModel.Tasks[1]);
            Assert.Same(viewModel.ParentOptions[1], viewModel.SelectedParent);

            viewModel.SelectedParent = viewModel.ParentOptions[0];
            viewModel.NewTitle = "Купить хлеб";
            viewModel.AddTaskCommand.Execute(viewModel.NewTitle);
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
            viewModel.AddTaskCommand.Execute(viewModel.NewTitle);
            viewModel.SelectedParent = viewModel.ParentOptions[1];
            viewModel.DeleteTaskCommand.Execute(viewModel.Tasks[0]);

            Assert.Same(Assert.Single(viewModel.ParentOptions), viewModel.SelectedParent);
            viewModel.NewTitle = "Купить хлеб";
            viewModel.AddTaskCommand.Execute(viewModel.NewTitle);

            Assert.Null(Assert.Single(database.GetTasks()).ParentId);
        }

        public void Dispose()
        {
            database.Dispose();
        }
    }
}
