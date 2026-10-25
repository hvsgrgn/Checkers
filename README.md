# Checkers: Piglets vs. Bearcubs 🐷🐻

A two-player checkers (draughts) game with a playful theme: the **Piglet Army** takes on the **Bearcub Army**. It's built with **C# / .NET 8** and **WPF**, using the MVVM pattern.

## How to play

- The game is played on a standard **8×8 board**. Each army starts with 12 pieces on the light squares of the three rows nearest to them.
- Players take turns. Click a piece, then click the square you want to move it to.
- Ordinary pieces move **one square diagonally forward**.
- **Capturing:** jump diagonally over an enemy piece onto the empty square behind it, and the enemy piece is removed.
- **Capturing is mandatory.** If you can capture, only capturing moves are offered.
- **Kings:** a piece that reaches the far side of the board is crowned, and its picture changes to a king. Kings can move **backwards** as well.
- **Winning:** you win when your opponent has no pieces left. If neither player can move, the game is a **draw**.

## Architecture

The app follows the **MVVM** pattern:

- **Model** (`Checkers/Model`): `GameModel` holds the board, the pieces and all the rules: move validation, forced captures, crowning, and detecting a win or draw. It reports changes through events (`MovesCalculated`, `FigureChanged`, `FigureDeleted`, `TransformedKing`, `IncorrectMove`, `GameOver`).
- **ViewModel** (`Checkers/ViewModel`): `GameViewModel` and `CellViewModel` handle selection, piece images and status messages, using CommunityToolkit.Mvvm.
- **View** (`Checkers/View`): a WPF window with an 8×8 grid board and a status bar.

## Running it

You need **Windows** and the [.NET 8 SDK](https://dotnet.microsoft.com/download). WPF only runs on Windows.

```
dotnet run --project Checkers
```

You can also open `Checkers.sln` in Visual Studio and press F5.

## About

Made as a university assignment at ELTE.
