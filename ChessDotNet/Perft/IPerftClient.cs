﻿using System;
using System.Collections.Generic;

namespace ChessDotNet.Perft
{
    public interface IPerftClient : IDisposable
    {
        int GetMoveCount(int depth);
        IList<MoveAndNodes> GetMovesAndNodes(int depth, IEnumerable<string> moves);
    }
}
