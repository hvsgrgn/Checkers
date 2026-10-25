using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Buffers;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Checkers.Model;

public class GameModel
{
    #region PRIVATE DATA MEMBERS

    private Colour _currentPlayer;
    private List<Figure> _figures;
    private bool _hasPassed;

    #endregion
    public readonly Colour[,] GameBoard;
    public List<Figure> Figures => _figures;
    public Colour CurrentPlayer => _currentPlayer;

    
    //public Color[,] GameBoard => _gameBoard;

    public GameModel() {
        GameBoard = new Colour[8, 8];
        _figures = new List<Figure>();

        for (int i = 0; i < 8; i++) {
            for (int j = 0; j < 8; j++) {
                //if the indexes are simultaniously even or uneven, its a white cell
                if ((i % 2 == 0 && j % 2 == 0) || (i % 2 == 1 && j % 2 == 1))
                {
                    GameBoard[i, j] = Colour.White;
                    if (i < 3) // the white cells in rows 0-2 contain the black figures' starting position
                        _figures.Add(new Figure
                        {
                            Owner = Colour.Black, IsKing = false,
                            Position = new Position { Row = i, Col = j }
                        });
                    else if (i > 4) // the white cells in rows 5-7 contain the white figures's starting pos
                        _figures.Add(new Figure
                        {
                            Owner = Colour.White, IsKing = false,
                            Position = new Position { Row = i, Col = j }
                        });
                }
                //otherwise if one index is even and the other is uneven, its a black cell
                else GameBoard[i, j] = Colour.Black;
            }
        }

        // according to rules, the white player opens the match
        _currentPlayer = Colour.White;
        _hasPassed = false;


    }

    #region PRIVATE HELPER FUNCTIONS

    private Figure? FindFigureByPosition(Position pos) {
        foreach (Figure figure in _figures) {
            if (pos.Row == figure.Position.Row && pos.Col == figure.Position.Col)
                return figure;
        }
        return null;
    }

    //return whether the move is valid and if so, whether it involves taking out another figure
    private (bool,bool, Position?) IsValidMove(Figure figure, int newRow, int newCol) {

        Figure? target = FindFigureByPosition(new Position { Row = newRow, Col = newCol });
        int oldRow = figure.Position.Row;
        int oldCol = figure.Position.Col;

        if (!figure.IsKing && figure.Owner == Colour.Black && oldRow - newRow >= 0) 
            return(false, false, null);


        else if (!figure.IsKing && figure.Owner == Colour.White && oldRow - newRow <= 0)
            return (false, false, null);
            

        Figure? enemy = FindFigureByPosition(new Position { Row = oldRow + (newRow - oldRow) / 2, Col = oldCol + (newCol - oldCol) / 2 });
        //move without knockout
        if (target == null &&
            Math.Abs(newRow - oldRow) == 1 &&
            Math.Abs(newCol - oldCol) == 1)
            return (true, false, null);
        //move with knockout 
        else if (target == null &&
            //the target is diagonal to the figure and 2 cells away
            Math.Abs(newRow - oldRow) == 2 &&
            Math.Abs(newCol - oldCol) == 2 &&
            //and there is an figure in the way of the opposing color
            enemy != null &&
            enemy.Owner != figure.Owner)
            return (true, true, enemy?.Position);

        return (false, false, null);

    }

    //checks only for being out of figures (game over condition n1)
    private bool IsGameOver() {
        bool hasWhite = false;
        bool hasBlack = false;
        foreach (Figure figure in _figures) { 
            if(figure.Owner == Colour.White)
                hasWhite = true;
            else
                hasBlack = true;
            if (hasWhite && hasBlack)
                break;
        }
        if (!hasWhite || !hasBlack)
            return true;
        return false;
    }

    private (List<MoveData>, List<MoveData>) GetAvailableMoves() {
        List<MoveData> knockouts = new List<MoveData>();
        List<MoveData> moves = new List<MoveData>();
        foreach (Figure figure in _figures) {
            if (figure.Owner == _currentPlayer) {
                for (int i = 0; i < 8; i++) {
                    for (int j = 0; j < 8; j++) {
                        (bool validity, bool knockout, Position? enemy) = IsValidMove(figure, i, j);
                        if (validity && !knockout)
                            moves.Add(new MoveData { OldPosition = figure.Position, NewPosition = new Position { Row = i, Col = j } });
                        else if (validity && knockout)
                            knockouts.Add(new MoveData{OldPosition = figure.Position, NewPosition = new Position { Row = i, Col = j }, Target = enemy});
                    }
                }
            }
        }
        return (knockouts, moves);
    }

    private bool IsEdge(Position position, Colour owner) {
        if (position.Row == 0 && owner == Colour.White)
            return true;
        if (position.Row == 7 && owner == Colour.Black)
            return true;
        return false;
    }

    #endregion

    #region EVENTS

    public event EventHandler<Colour>? GameOver;
    public event EventHandler<MoveData>? FigureChanged;
    public event EventHandler<List<MoveData>>? MovesCalculated;
    public event EventHandler<String>? IncorrectMove;
    public event EventHandler<Position>? TransformedKing;
    public event EventHandler<Position>? FigureDeleted;

    #endregion

    public void StartRound() {
        (List<MoveData> knockouts, List<MoveData> moves) = GetAvailableMoves();

        if (knockouts.Count > 0) //forcing out an aggressive move if possible
            MovesCalculated?.Invoke(this, knockouts);
        else if (moves.Count > 0)
            MovesCalculated?.Invoke(this, moves);
        else if (_hasPassed) //if none of the players had a valid move, the game ends
            GameOver?.Invoke(this, Colour.Green);
        else
            _hasPassed = true; // and we should wait for validation here for the next round
    }

    public void Move(MoveData data) {

        Position oldPosition = data.OldPosition;
        Position newPosition = data.NewPosition;
        Position? enemy = data.Target;

        Figure? active = FindFigureByPosition(oldPosition);
        Figure? target = null;
        if(enemy != null)
            target = FindFigureByPosition((Position)enemy);

        if (active == null) {
            IncorrectMove?.Invoke(this, "There was no figure of the current player on this cell");
            return;
        }
            
        (bool validity, bool knockout, _) = IsValidMove(active, newPosition.Row, newPosition.Col);

        if (!validity) {
            IncorrectMove?.Invoke(this, "The target cell selected is not a valid move for this figure");
            return;
        }

        active.Position = newPosition;
        FigureChanged?.Invoke(this, data);

        if (enemy != null && target != null) {
            _figures.Remove(target);
            FigureDeleted?.Invoke(this, (Position)enemy);
        }
            

        if (IsEdge(newPosition, active.Owner)) {
            active.IsKing = true;
            TransformedKing?.Invoke(this, newPosition);
        }
        

        //if after the move, the enemy player has no more figures left, the current player is the winner
        if (IsGameOver())
            GameOver?.Invoke(this, _currentPlayer);

        //if the game over condition doesnt stand, we switch to the next player in turn
        if(_currentPlayer == Colour.White)
            _currentPlayer = Colour.Black;
        else
            _currentPlayer = Colour.White;

        //because a valid move was completed, the passing flag should be false for the next round to check
        _hasPassed = false;
    }
}
