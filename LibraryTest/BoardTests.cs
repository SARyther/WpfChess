using Library;
using Library.Pieces;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace LibraryTest
{
    public class BoardTests
    {
        private Board board;

        [SetUp]
        public void Setup()
        {
            board = new Board();
        }

        [Test]
        public void IsPieceBlockingYes()
        {
            board = Serializer.FromXml<Board>(DirectoryInfos.GetPath("PieceBlocking.xml"));

            var piece = board.Squares.FirstOrDefault(x => x.Piece is Bishop)?.Piece;
            var square = board.Squares.FirstOrDefault(x => x.Piece is Knight);

            var dir = piece.ChooseRightDirection(square.Point);

            Assert.IsTrue(board.IsPieceBlocking(piece, square, dir));
        }

        [Test]
        public void MakeBoardTest()
        {
            Board board = new Board();
            Assert.IsNotNull(board);
        }

        [Test]
        public void Make960BoardTest()
        {
            Board board = Board.Get960Board();
            Assert.IsNotNull(board);
        }

        [Test]
        public void Is960Mirrored()
        {
            Board board = Board.Get960Board();

            List<Piece> backRow1 = board.Squares.Where(square => square.Point.PosY == 0).Select(square => square.Piece).ToList();
            List<Piece> backRow2 = board.Squares.Where(square => square.Point.PosY == 7).Select(square => square.Piece).ToList();
            Assert.AreEqual(backRow1.Count, backRow2.Count);

            for (int i = 0; i < backRow1.Count; i++)
            {
                Piece a = backRow1[i];
                Piece b = backRow2[i];

                Assert.IsTrue(a.GetType().Equals(b.GetType()));
            }
        }

        [Test]
        public void Do960RooksSurroundKing()
        {
            Board board = Board.Get960Board();
            King king = board.GetKingByColor(Color.White) as King;
            List<Rook> rooks = board.GetRookNearKing(king).OrderBy(piece => piece.Point.PosX).Select(piece => piece as Rook).ToList();
            Rook rook1 = rooks.First();
            Rook rook2 = rooks.Last();

            int kingIndex = king.Point.PosX;
            int rookIndex1 = rook1.Point.PosX;
            int rookIndex2 = rook2.Point.PosX;

            Assert.IsTrue(kingIndex > rookIndex1 && kingIndex < rookIndex2);
        }

        [Test]
        public void Are960BishopsDifferentColors()
        {
            Board board = Board.Get960Board();
            List<Square> bishopSquares = board.Squares.Where(square => square.Point.PosY == 0 && square.Piece is Bishop).ToList();
            Assert.AreEqual(bishopSquares.Count, 2);

            Assert.AreNotEqual(bishopSquares[0].Color, bishopSquares[1].Color);
        }

        [Test]
        public void AreAll960PiecesPresent()
        {
            Board board = Board.Get960Board();
            List<Piece> backRow = board.Squares.Where(square => square.Point.PosY == 0).Select(square => square.Piece).ToList();
            List<Piece> frontRow = board.Squares.Where(square => square.Point.PosY == 1).Select(square => square.Piece).ToList();

            List<King> kings = backRow.Where(piece => piece is King).Select(piece => piece as King).ToList();
            Assert.AreEqual(kings.Count, 1);

            List<Queen> queens = backRow.Where(piece => piece is Queen).Select(piece => piece as Queen).ToList();
            Assert.AreEqual(queens.Count, 1);

            List<Bishop> bishops = backRow.Where(piece => piece is Bishop).Select(piece => piece as Bishop).ToList();
            Assert.AreEqual(bishops.Count, 2);

            List<Knight> knights = backRow.Where(piece => piece is Knight).Select(piece => piece as Knight).ToList();
            Assert.AreEqual(knights.Count, 2);

            List<Rook> rooks = backRow.Where(piece => piece is Rook).Select(piece => piece as Rook).ToList();
            Assert.AreEqual(rooks.Count, 2);

            Assert.IsTrue(frontRow.All(piece => piece is Pawn));
        }
    }
}
