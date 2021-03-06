﻿using System;
using System.Collections.Generic;
using ChessDotNet.Hashing;

namespace ChessDotNet.Data
{
    public class BoardFactory
    {
        public Board ParseFEN(string fen)
        {
            fen = fen.Trim();
            var board = new Board();
            board.ArrayBoard = new int[64];
            board.BitBoard = new ulong[13];
            board.CastlingPermissions = new bool[4];
            board.History = new HistoryEntry[0];

            var boardPosition = 0;
            var fenPosition = 0;
            for (; fenPosition < fen.Length; fenPosition++)
            {
                var fixedBoardPosition = (7 - boardPosition/8)*8 + boardPosition%8;
                var ch = fen[fenPosition];
                var pieceBitBoard = 1UL << fixedBoardPosition;
                switch (ch)
                {
                    case 'p':
                        board.BitBoard[ChessPiece.BlackPawn] |= pieceBitBoard;
                        boardPosition++;
                        continue;
                    case 'P':
                        board.BitBoard[ChessPiece.WhitePawn] |= pieceBitBoard;
                        boardPosition++;
                        continue;

                    case 'n':
                        board.BitBoard[ChessPiece.BlackKnight] |= pieceBitBoard;
                        boardPosition++;
                        continue;
                    case 'N':
                        board.BitBoard[ChessPiece.WhiteKnight] |= pieceBitBoard;
                        boardPosition++;
                        continue;

                    case 'b':
                        board.BitBoard[ChessPiece.BlackBishop] |= pieceBitBoard;
                        boardPosition++;
                        continue;
                    case 'B':
                        board.BitBoard[ChessPiece.WhiteBishop] |= pieceBitBoard;
                        boardPosition++;
                        continue;

                    case 'r':
                        board.BitBoard[ChessPiece.BlackRook] |= pieceBitBoard;
                        boardPosition++;
                        continue;
                    case 'R':
                        board.BitBoard[ChessPiece.WhiteRook] |= pieceBitBoard;
                        boardPosition++;
                        continue;

                    case 'q':
                        board.BitBoard[ChessPiece.BlackQueen] |= pieceBitBoard;
                        boardPosition++;
                        continue;
                    case 'Q':
                        board.BitBoard[ChessPiece.WhiteQueen] |= pieceBitBoard;
                        boardPosition++;
                        continue;

                    case 'k':
                        board.BitBoard[ChessPiece.BlackKing] |= pieceBitBoard;
                        boardPosition++;
                        continue;
                    case 'K':
                        board.BitBoard[ChessPiece.WhiteKing] |= pieceBitBoard;
                        boardPosition++;
                        continue;
                }

                byte emptySpaces;
                if (byte.TryParse(ch.ToString(), out emptySpaces))
                {
                    boardPosition += emptySpaces;
                    continue;
                }

                if (ch == ' ')
                {
                    break;
                }

            }

            fenPosition++;
            if (fen[fenPosition] == 'w')
            {
                board.WhiteToMove = true;
            }

            fenPosition += 2;

            for (var i = 0; i < 4; i++)
            {
                if (fenPosition >= fen.Length)
                    break;
                switch (fen[fenPosition])
                {
                    case 'K':
                        board.CastlingPermissions[CastlePermission.WhiteKingSide] = true;
                        break;
                    case 'Q':
                        board.CastlingPermissions[CastlePermission.WhiteQueenSide] = true;
                        break;
                    case 'k':
                        board.CastlingPermissions[CastlePermission.BlackKingSide] = true;
                        break;
                    case 'q':
                        board.CastlingPermissions[CastlePermission.BlackQueenSide] = true;
                        break;
                }
                fenPosition++;
            }
            board.SyncExtraBitBoards();
            board.SyncBitBoardsToArrayBoard();
            board.SyncPiecesCount();
            board.SyncMaterial();
            board.Key = ZobristKeys.CalculateKey(board);
            return board;
        }

        public ulong PiecesToBitBoard(IEnumerable<int> pieces)
        {
            var board = 0UL;
            foreach (var piece in pieces)
            {
                board |= 1UL << piece;
            }
            return board;
        }


        
    }
}
