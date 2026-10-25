using Checkers.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Checkers.ViewModel;

/// <summary>
/// 
/// FUNCTIONALITIES TO IMPLEMENT:
/// HIGHLIGHT SYSTEM FOR POSSIBLE MOVES: FIRST HIGHLIGHTING WHICH FIGURES CAN START A MOVE, AND TAKING OFF ALL THE OTHER HIGHLIGHTS AND HIGHLIGHTHING TO WHERE WE CAN MOVE FROM THERE
/// WHEN MOVING ONTO THE NEXT ROUND OR CANCELING FIGURE SELECTION, THE HIGHLIGHT SHOULD RESET TO THE CURRENT STATE OF POTENTIAL BEGINNING FIGURES
/// GAME FLOW: COMMUNICATION WITH MODEL TO GUIDE THRU THE WHOLE GAME ROUND-BY-ROUND
/// GAME OVER : POP-UP WINDOW WITH THE WINNERS NAME, SHUTTING DOWN CLICKS ON THE BOARD, OPPORTUNITY FOR RESTART
/// </summary>
public partial class GameViewModel : ObservableObject
{
    private GameModel _gameModel;
    public ObservableCollection<CellViewModel> Board { get; set; }
    private bool _roundStarted = false;
    [ObservableProperty]
    private string _prompt = "Welcome to the game!";

    private CellViewModel? _selectedCell = null;
    private bool _gameOver = false;

    private List<MoveData> _availableMoves = new List<MoveData>();
    public GameViewModel() {
        _gameModel = new GameModel();
        Board = new ObservableCollection<CellViewModel>();

        for (int i = 0; i < 8; i++) {
            for (int j = 0; j < 8; j++) {
                if (_gameModel.GameBoard[i, j] == Colour.White)
                {
                    CellViewModel cell = new CellViewModel { Position = new Position { Row = i, Col = j }, BackgroundColor = Brushes.White };
                    cell.ClickCommand = new RelayCommand(() => OnCellClicked(cell));
                    Board.Add(cell);
                }
                else {
                    CellViewModel cell = new CellViewModel { Position = new Position { Row = i, Col = j }, BackgroundColor = Brushes.Black };
                    cell.ClickCommand = new RelayCommand(() => OnCellClicked(cell));
                    Board.Add(cell);
                }
                    
            }
                
        }

        foreach (Figure figure in _gameModel.Figures) {
            Colour owner = figure.Owner;
            Position position = figure.Position;
            CellViewModel? cell = Board.FirstOrDefault(f => f.Position.Row == position.Row && f.Position.Col == position.Col);
            if (cell != null) {
                if (owner == Colour.White)
                    cell.FigureImage = LoadImage("pig.png");
                else
                    cell.FigureImage = LoadImage("bear.png");
            }
        }

        _gameModel.FigureDeleted += OnFigureDeleted;
        _gameModel.FigureChanged += OnFigureChanged;
        _gameModel.GameOver += OnGameOver;
        _gameModel.MovesCalculated += OnMovesCalculated;
        _gameModel.IncorrectMove += OnIncorrectMove;
        _gameModel.TransformedKing += OnTransformedKing;

        System.Diagnostics.Debug.WriteLine($"GameViewModel created! Board has {Board.Count} cells");

    }

    #region EVENTHANDLERS

    private void OnFigureDeleted(object? sender, Position e) {

        Board.FirstOrDefault(f => f.Position.Row == e.Row && f.Position.Col == e.Col)!.FigureImage = null;
    }

    private void OnFigureChanged(object? sender, MoveData e) {
        CellViewModel? oldCell = Board.FirstOrDefault(f => f.Position.Row == e.OldPosition.Row && f.Position.Col == e.OldPosition.Col);
        if (oldCell != null) {
            Board.FirstOrDefault(f => f.Position.Row == e.NewPosition.Row && f.Position.Col == e.NewPosition.Col)!.FigureImage = oldCell.FigureImage;
            oldCell.FigureImage = null;
        }
        if (_gameModel.CurrentPlayer == Colour.Black)
            Prompt = $"It's the Piglet Army's turn!";
        else
            Prompt = $"It's the Bearcub Army's turn!";
    }

    private void OnTransformedKing(object? sender, Position e)
    {
        CellViewModel? cell = Board.FirstOrDefault(f =>
        f.Position.Row == e.Row && f.Position.Col == e.Col);

        if (cell != null)
        {
            Figure? figure = _gameModel.Figures.FirstOrDefault(f =>
                f.Position.Row == e.Row && f.Position.Col == e.Col);

            if (figure != null)
            {
                if (figure.Owner == Colour.White)
                    cell.FigureImage = LoadImage("pig_king.png");
                else
                    cell.FigureImage = LoadImage("bear_king.png");
            }
        }
    }

    private void OnGameOver(object? sender, Colour e) {
        _gameOver = true;
        if (e == Colour.Green)
            Prompt = $"The Game Ended In A Draw!";
        else if (e == Colour.White) {
            Prompt = $"The Piglet Army Won The Match!";
        }
        else
            Prompt = $"The Bearcub Army Won The Match!";
        ClearAllHighlights();

    }

    private void OnMovesCalculated(object? sender, List<MoveData> e) {

        _availableMoves = e;

        ClearAllHighlights();

        foreach (MoveData k in e) {
            CellViewModel? possibility = Board.FirstOrDefault(f => f.Position.Row == k.OldPosition.Row && f.Position.Col == k.OldPosition.Col);

            if (possibility != null)
                possibility.BorderOpacity = false;
        }
    }

    private void OnIncorrectMove(object? sender, string e) {
        Prompt = e;
    }


    #endregion

    #region CLICKHANDLING
    private void OnCellClicked(CellViewModel clickedCell)
    {   

        if (_gameOver)
            return;

        if (!_roundStarted)
        {
            _roundStarted = true;
            Prompt = $"Player {_gameModel.CurrentPlayer.ToString()}, choose a figure to move with!";
            _gameModel.StartRound();
            return;
        }

        // CASE 1: Clicking the same cell again (deselect)
        if (_selectedCell == clickedCell)
        {
            DeselectCell();
            return;
        }

        // CASE 2: Clicking a cell that can start a move (selecting a figure)
        if (clickedCell.BorderOpacity == false && _selectedCell == null)
        {
            SelectCell(clickedCell);
            return;
        }

        // CASE 3: Clicking a different figure that can also move (switching selection)
        if (clickedCell.BorderOpacity == false && _selectedCell != null && clickedCell.FigureImage != null)
        {
            DeselectCell();
            SelectCell(clickedCell);
            return;
        }

        // CASE 4: Clicking a destination cell (executing a move)
        if (_selectedCell != null && clickedCell.BorderOpacity == false)
        {
            // Find the move that matches selected -> clicked
            MoveData? moveToExecute = _availableMoves.FirstOrDefault(m =>
                m.OldPosition.Row == _selectedCell.Position.Row &&
                m.OldPosition.Col == _selectedCell.Position.Col &&
                m.NewPosition.Row == clickedCell.Position.Row &&
                m.NewPosition.Col == clickedCell.Position.Col);

            if (moveToExecute != null)
            {
                ExecuteMove(moveToExecute.Value);
            }
            return;
        }

        // CASE 5: Clicking anywhere else (cancel selection)
        if (_selectedCell != null)
        {
            DeselectCell();
        }
    }
   
    private void SelectCell(CellViewModel cell)
    {
        _selectedCell = cell;

        // Clear all highlights
        ClearAllHighlights();

        // Highlight only the destinations this figure can move to
        foreach (MoveData move in _availableMoves)
        {
            if (move.OldPosition.Row == cell.Position.Row &&
                move.OldPosition.Col == cell.Position.Col)
            {

                CellViewModel? destinationCell = Board.FirstOrDefault(f =>
                    f.Position.Row == move.NewPosition.Row &&
                    f.Position.Col == move.NewPosition.Col);

                if (destinationCell != null)
                    destinationCell.BorderOpacity = false;
            }
        }

        // Also keep the selected cell highlighted
        cell.BorderOpacity = false;
    }

    private void DeselectCell()
    {
        _selectedCell = null;

        // Reset highlights to show all figures that can start a move
        ClearAllHighlights();

        foreach (MoveData move in _availableMoves)
        {
            CellViewModel? cell = Board.FirstOrDefault(f =>
                f.Position.Row == move.OldPosition.Row &&
                f.Position.Col == move.OldPosition.Col);

            if (cell != null)
                cell.BorderOpacity = false;
        }
    }

    private void ExecuteMove(MoveData move)
    {
        // Execute the move in the model
        _gameModel.Move(move);

        // Reset selection
        _selectedCell = null;

        // Start the next round
        _gameModel.StartRound();
    }

    private void ClearAllHighlights()
    {
        foreach (var cell in Board)
        {
            cell.BorderOpacity = true;
        }
    }


    #endregion

    private BitmapImage LoadImage(string fileName)
    {
        return new BitmapImage(new Uri($"pack://application:,,,/Assets/{fileName}"));
    }
}
