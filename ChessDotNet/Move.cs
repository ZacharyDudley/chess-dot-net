﻿namespace ChessDotNet
{
    public struct Move
    {
        public Move(int from, int to, ChessPiece piece)
        {
            From = from;
            To = to;
            Piece = piece;
        }

        public int From { get; }
        public int To { get; }
        public ChessPiece Piece { get; }

        private string PositionToText(int position)
        {
            var rank = position / 8;
            var file = position % 8;

            var str = (char)(65 + file) + (rank + 1).ToString();
            return str;
        }

        public override string ToString()
        {
            var text = PositionToText(From) + PositionToText(To);
            return $"{text}; From: {From}, To: {To}, Piece: {Piece}";
        }
    }
}