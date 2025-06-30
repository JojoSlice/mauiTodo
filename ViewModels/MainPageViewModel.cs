using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CouchbaseTodo.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly Services.DatabaseService _databaseService;

    public ICommand DeleteCommand => new RelayCommand<Models.ToDoTask>(async task => await DeleteTask(task));
   // public ICommand ToggleCommand => new RelayCommand<Models.ToDoTask>(async task => await ToggleTask(task));

    public ObservableCollection<Models.ToDoTask> TaskItems { get; set; } = new();

    [ObservableProperty]
    private string taskItem;

    [ObservableProperty]
    private bool snackbarVisible;

    [ObservableProperty]
    private Models.ToDoTask lastDeletedTask;
    public MainPageViewModel(Services.DatabaseService databaseService)
    {
        _databaseService = databaseService;
        LoadTasks();
    }

    private async void LoadTasks()
    {
        var tasks = await _databaseService.GetAllTasksAsync();
        MainThread.BeginInvokeOnMainThread(() =>
        {
            TaskItems.Clear();
            foreach (var task in tasks)
            {
                TaskItems.Add(task);
            }
        });
    }

    [RelayCommand]
    public async Task AddTask()
    {
        if (!string.IsNullOrWhiteSpace(TaskItem))
        {
            var newTask = new Models.ToDoTask { Text = TaskItem.Trim(), Completed = false };
            await _databaseService.AddTaskAsync(newTask);
            TaskItems.Add(newTask);
            TaskItem = string.Empty;
        }
    }


    //public async Task ToggleTask(Models.ToDoTask task)
    //{
    //    task.Completed = !task.Completed;
    //    await _databaseService.UpdateTaskAsync(task);
    //}

    public async Task DeleteTask(Models.ToDoTask task)
    {
        LastDeletedTask = task;
        await _databaseService.DeleteTaskAsync(task.Id);
        TaskItems.Remove(task);

        await Snackbar.Make(
        "Task deleted",
        async () => await UndoDelete(),
        "Undo",
        TimeSpan.FromSeconds(4)
        ).Show();
    }

    [RelayCommand]
    public async Task UndoDelete()
    {
        if (LastDeletedTask != null)
        {
            await _databaseService.AddTaskAsync(LastDeletedTask);
            TaskItems.Add(LastDeletedTask);
            LastDeletedTask = null;
            SnackbarVisible = false;
        }
    }

}