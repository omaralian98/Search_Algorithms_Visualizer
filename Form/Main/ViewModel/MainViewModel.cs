using Linear_Algebra_Calculator.Core;

namespace Form.Main.ViewModel;

public class MainViewModel : ObservableObjects
{
    public RelayCommand GridRelayCommand { get; set; }
    public RelayCommand EmptyRelayCommand { get; set; }
    public RelayCommand GraphRelayCommand { get; set; }

    public GridViewModel GridViewModel { get; set; }
    public EmptyViewModel EmptyViewModel { get; set; }
    public GraphViewModel GraphViewModel { get; set; }



    private object _currentView;

    public object CurrentView
    {
        get { return _currentView; }
        set
        {
            _currentView = value;
            OnPropertychanged();
        }
    }

    public MainViewModel()
    {
        GridViewModel = new GridViewModel();
        GraphViewModel = new GraphViewModel();
        CurrentView = new EmptyViewModel();
        GridRelayCommand = new RelayCommand(o =>
        {
            CurrentView = GridViewModel;
        });
        GraphRelayCommand = new RelayCommand(o =>
        {
            CurrentView = GraphViewModel;
        });
    }
}