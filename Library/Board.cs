using Library.Pieces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Library
{
    public class Board : PropertyChangedBase, ICloneable
    {
        private Color topColor;
        public static int BoardSize = 8;
        public List<Move> PossibleMoves { get; set; }

        public ObservableCollection<Square> Squares { get; set; }

        public Board()
        {
            MakeBoard();

            PossibleMoves = new List<Move>();
        }

        public static Board Get960Board()
        {
            string textLine = Get960Order().ToTxtLine();
            string[] content = File.ReadAllLines(DirectoryInfos.GetPath("Start_Black.txt"));
            content[3] = textLine;
            content[10] = textLine;

            string outputPath = Path.GetTempFileName();
            File.WriteAllLines(outputPath, content);

            return Serializer.ImportFromTxt(outputPath);
        }

        private static int[] Get960Order()
        {
            Random rand = new Random();
            int[] order = new int[8];
            List<int> openIndexes = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };

            //Get index for king NOT at an edge
            int kingPosition = rand.Next(1, 7);
            openIndexes.Remove(kingPosition);

            //Get indexes for rooks that surround king
            int rookPosition1 = rand.Next(0, kingPosition);
            openIndexes.Remove(rookPosition1);
            int rookPosition2 = rand.Next(kingPosition + 1, 8);
            openIndexes.Remove(rookPosition2);

            //Get indexes for bishops on alternate colored squares
            List<int> openEven = openIndexes.Where(x => (x + 1) % 2 == 0).ToList();
            List<int> openOdd = openIndexes.Where(x => (x + 1) % 2 == 1).ToList();
            int bishopPosition1 = openEven[rand.Next(0, openEven.Count())];
            openIndexes.Remove(bishopPosition1);
            int bishopPosition2 = openOdd[rand.Next(0, openOdd.Count())];
            openIndexes.Remove(bishopPosition2);

            //Get random position from remaining indexes for both knights and the queen
            int knightPosition1 = openIndexes[rand.Next(0, openIndexes.Count)];
            openIndexes.Remove(knightPosition1);
            int knightPosition2 = openIndexes[rand.Next(0, openIndexes.Count)];
            openIndexes.Remove(knightPosition2);
            int queenPosition = openIndexes[0];
            openIndexes.Remove(queenPosition);

            if (openIndexes.Count > 0) throw new InvalidOperationException("Error getting valid 960 order.");

            order[kingPosition] = 5;
            order[queenPosition] = 4;
            order[rookPosition1] = 1;
            order[rookPosition2] = 1;
            order[knightPosition1] = 2;
            order[knightPosition2] = 2;
            order[bishopPosition1] = 3;
            order[bishopPosition2] = 3;

            return order;
        }

        private void MakeBoard()
        {
            Squares = new ObservableCollection<Square>();
            Color toggle = Color.White;
            for (int y = 0; y < BoardSize; y++)
            {
                for (int x = 0; x < BoardSize; x++)
                {
                    Squares.Add(new Square() { Point = new Point(x, y) });
                    Squares.Last().Color = toggle;
                    if (x != BoardSize - 1)
                        toggle = toggle == Color.White ? Color.Black : Color.White;
                }
            }
        }

        public List<Square> CenterSquares => Squares.Where(x =>
            (x.Point.PosY == 3 && x.Point.PosX == 3) ||
            (x.Point.PosY == 3 && x.Point.PosX == 4) ||
            (x.Point.PosY == 4 && x.Point.PosX == 4) ||
            (x.Point.PosY == 4 && x.Point.PosX == 3)
        ).ToList();

        public Color TopColor
        {
            get { return topColor; }
            set { RaisePropertyChanged(ref topColor, value); }
        }

        public Piece IsKingChecked(Color turn)
        {
            var enemyPieces = GetAllPiecesByColor(GetOtherColor(turn));
            var playerKingSquare = Squares.FirstOrDefault(x => x.Point.Equals(GetKingByColor(turn).Point));

            foreach (var enemyPiece in enemyPieces)
            {
                if (enemyPiece.CanBeMovedToSquare(playerKingSquare))
                {
                    var dir = enemyPiece.ChooseRightDirection(playerKingSquare.Point);
                    if (!IsPieceBlocking(enemyPiece, playerKingSquare, dir))
                        return enemyPiece;
                }
            }

            return null;
        }

        public bool IsPieceBlocking(Piece piece, Square end, Point dir)
        {
            var allPieces = GetAllPieces().Where(x => x != null && !x.Point.Equals(piece.Point));
            var allDirs = piece.Point.AllMovesWithinDirection(end.Point, dir);
            allDirs.Remove(allDirs.Last());

            foreach (var p in allPieces)
                if (allDirs.FirstOrDefault(x => x.Equals(p.Point)) != null)
                    return true;

            return false;
        }

        public Color GetOtherColor(Color color)
        {
            return (color == Color.White) ? Color.Black : Color.White;
        }

        public Piece GetKingByColor(Color turn)
        {
            var allPieces = GetAllPiecesByColor(turn);

            return allPieces.FirstOrDefault(x => x is King);
        }

        public List<Piece> GetAllPiecesByColor(Color color)
        {
            return GetAllPieces().Where(x => x?.Color == color).ToList();
        }

        public List<Piece> GetRookNearKing(Piece king)
        {
            var rooks = GetAllPiecesByColor(king.Color).Where(x => x is Rook);
            return rooks.ToList();
        }

        public List<Piece> GetAllPieces()
        {
            var pieces = new List<Piece>();

            foreach (var s in Squares)
                pieces.Add(s.Piece);

            return pieces;
        }

        public Board PredictBoard(Piece piece, Square end)
        {
            var clone = Clone() as Board;
            clone.ShiftPiece(piece, end);
            return clone;
        }

        public Piece ShiftPiece(Piece piece, Square end)
        {
            var startPiece = piece.Clone() as Piece;
            var endPiece = Squares.FirstOrDefault(x => x.Point.Equals(end.Point))?.Piece;
            var saveEndPiece = endPiece?.Clone() as Piece;

            Squares.FirstOrDefault(x => x.Point.Equals(end.Point)).Piece = Squares.FirstOrDefault(x => x.Point.Equals(piece.Point)).Piece;
            Squares.FirstOrDefault(x => x.Point.Equals(startPiece.Point)).Piece = null;

            return saveEndPiece;
        }

        public void ShowPossibleMoves(Piece piece)
        {
            foreach (var move in CalcPossibleMoves(piece))
            {
                move.End.IsPossibileSquare = true;
                PossibleMoves.Add(move);
            }

        }

        public List<Move> CalcPossibleMoves(Piece piece)
        {
            var res = new List<Move>();
            foreach (var dir in piece.Directions)
            {
                var allMoves = piece.Point.AllMovesWithinDirection(dir);
                foreach (var end in allMoves)
                {
                    var square = Squares.FirstOrDefault(x => x.Point.Equals(end));
                    if (piece.CanMoveWithoutColliding(square, this))
                    {
                        var clonedBoard = PredictBoard(piece, square);
                        if (clonedBoard.IsKingChecked(piece.Color) == null)
                        {
                            var add = CanCastle(piece);
                            foreach (var castle in add)
                            {
                                if (!piece.CheckCollision(castle[0], this)) {
                                    var cMove = new Castle(this, piece, castle[0], castle[1]);
                                    cMove.OnInitiatePawnPromotion += Move_OnInitiatePawnPromotion;
                                    cMove.OnPieceCaptured += Move_OnPieceCaptured;
                                    res.Add(cMove);
                                }                                
                            }

                            var move = new Move(this, piece, square);
                            move.OnInitiatePawnPromotion += Move_OnInitiatePawnPromotion;
                            move.OnPieceCaptured += Move_OnPieceCaptured;
                            res.Add(move);
                        }
                    }
                }
            }

            return res;
        }

        public List<Square[]> CanCastle(Piece start)
        {
            List<Square[]> square = new List<Square[]>();

            if (!(start is King))
                return square;

            var rooks = GetRookNearKing(start);            
            foreach (var rook in rooks)
            {
                if (start.IsFirstMove && rook.IsFirstMove)
                {
                    var clone = Clone() as Board;
                    var dir = start.ChooseRightDirection(rook.Point);
                    var allMoves = start.Point.AllMovesWithinDirection(rook.Point, dir).Take(2);
                    if (allMoves.Count() < 2)
                        return square;

                    Square s = null;
                    foreach (var move in allMoves)
                    {
                        var end = clone.Squares.FirstOrDefault(x => x.Point.Equals(move));
                        clone.ShiftPiece(clone.GetKingByColor(start.Color), end);
                        if (clone.IsKingChecked(start.Color) != null)
                            return square;
                    }
                   
                    square.Add(new Square[] { Squares.FirstOrDefault(x => x.Point.Equals(allMoves.ElementAt(1))), Squares.FirstOrDefault(x => x.Point.Equals(rook.Point)) });                    
                }
            }            

            return square;
        }

        #region Events
        public event EventHandler OnInitiatePawnPromotion;
        public event EventHandler OnPieceCaptured;

        private void Move_OnPieceCaptured(object sender, EventArgs e)
        {
            OnPieceCaptured?.Invoke(sender, e);
        }

        private void Move_OnInitiatePawnPromotion(object sender, EventArgs e)
        {
            OnInitiatePawnPromotion?.Invoke(sender, e);
        }

        #endregion

        public object Clone()
        {
            Board board = new Board();
            board.Squares = new ObservableCollection<Square>();
            foreach (var s in Squares)
                board.Squares.Add(s.Clone() as Square);

            return board;
        }
    }
}
