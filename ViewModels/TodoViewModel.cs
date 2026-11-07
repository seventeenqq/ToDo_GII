using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Todo.Models;
using Todo.Services;

namespace Todo.ViewModels
{
    public class TodoViewModel : INotifyPropertyChanged
    {
        private TodoDatabase database;
        private Command<string> addTaskCommand;
        private string newTitle = "";
        private string selectedCategory = "Без категории";
        private TodoItem? selectedParent;

        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<TodoItem> Tasks { get; private set; }
        public ObservableCollection<TodoItem> ParentOptions { get; private set; }
        public List<string> Categories { get; private set; }
        public ICommand AddTaskCommand
        {
            get { return addTaskCommand; }
        }

        public ICommand DeleteTaskCommand { get; private set; }
        public ICommand ToggleTaskCommand { get; private set; }
        public string NewTitle
        {
            get { return newTitle; }
            set
            {
                newTitle = value;
                OnPropertyChanged(nameof(NewTitle));
            }
        }

        public string SelectedCategory
        {
            get { return selectedCategory; }
            set
            {
                selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));
            }
        }

        public TodoItem? SelectedParent
        {
            get { return selectedParent; }
            set
            {
                selectedParent = value;
                OnPropertyChanged(nameof(SelectedParent));
            }
        }

        public TodoViewModel(TodoDatabase database)
        {
            this.database = database;
            Tasks = new ObservableCollection<TodoItem>();
            ParentOptions = new ObservableCollection<TodoItem>();

            Categories = new List<string>();
            Categories.Add("Без категории");
            Categories.Add("Покупки");
            Categories.Add("Учёба");
            Categories.Add("Дом");
            Categories.Add("Работа");

            addTaskCommand = new Command<string>(AddTask, CanAddTask);
            DeleteTaskCommand = new Command<TodoItem>(DeleteTask);
            ToggleTaskCommand = new Command<TodoItem>(ToggleTask);
            LoadTasks();
        }

        private bool CanAddTask(string? title)
        {
            return !string.IsNullOrWhiteSpace(title);
        }

        private void AddTask(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return;
            }

            int? parentId = null;
            if (SelectedParent != null && SelectedParent.Id != 0)
            {
                parentId = SelectedParent.Id;
            }

            database.AddTask(
                title,
                SelectedCategory,
                parentId);

            NewTitle = "";
            LoadTasks();
        }

        private void DeleteTask(TodoItem item)
        {
            database.DeleteTask(item.Id);
            LoadTasks();
        }

        private void ToggleTask(TodoItem item)
        {
            database.SetCompleted(item.Id, !item.IsCompleted);
            LoadTasks();
        }

        private void LoadTasks()
        {
            int? selectedParentId = SelectedParent?.Id;

            List<TodoItem> items = database.GetTasks();

            Tasks.Clear();
            ParentOptions.Clear();

            TodoItem noParent = new TodoItem();
            noParent.Title = "Основная задача (без родителя)";
            ParentOptions.Add(noParent);
            TodoItem parentToSelect = noParent;

            foreach (TodoItem parent in items)
            {
                if (parent.ParentId != null)
                {
                    continue;
                }

                Tasks.Add(parent);
                ParentOptions.Add(parent);

                if (parent.Id == selectedParentId)
                {
                    parentToSelect = parent;
                }

                foreach (TodoItem child in items)
                {
                    if (child.ParentId == parent.Id)
                    {
                        Tasks.Add(child);
                    }
                }
            }

            SelectedParent = parentToSelect;
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
