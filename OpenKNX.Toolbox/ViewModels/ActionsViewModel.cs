using CommunityToolkit.Mvvm.Input;
using OpenKNX.Toolbox.Classes.Actions;
using OpenKNX.Toolbox.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKNX.Toolbox.ViewModels
{
    public partial class ActionsViewModel : INotifyPropertyChanged
    {
        public static ActionsViewModel Instanz { get; } = new ActionsViewModel();

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<IAction> Actions { get; set; } = new();
        public ObservableCollection<ActionHistory> History { get; set; } = new();

        private IAction? _currentAction = null;
        public IAction? CurrentAction
        {
            get { return _currentAction; }
            set
            {
                _currentAction = value;
                Changed("CurrentAction");
            }
        }

        private CancellationTokenSource? _token;
        private CancellationTokenSource? _currentActionToken;

        public void Init()
        {
            StartWorker();
        }

        public void AddAction(IAction action)
        {
            Actions.Add(action);
        }

        private void Changed(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async void StartWorker()
        {
            _token = new CancellationTokenSource();
            while(_token != null && !_token.Token.IsCancellationRequested)
            {
                if(Actions.Count == 0)
                {
                    await Task.Delay(500);
                    continue;
                }

                CurrentAction = Actions.First();
                Actions.Remove(CurrentAction);

                _currentActionToken = new();
                try
                {
                    await CurrentAction.Begin(_currentActionToken.Token);
                    History.Insert(0, new ActionHistory(CurrentAction));
                } catch(Exception ex)
                {
                    MainViewModel.Instanz.ShowError("Action Error", ex.Message);
                    History.Insert(0, new ActionHistory(CurrentAction, ex.Message));
                }
                CurrentAction = null;
            }
        }

        [RelayCommand]
        public void CancelAction()
        {
            _currentActionToken?.Cancel();
        }
    }
}
