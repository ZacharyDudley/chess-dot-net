﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChessDotNet.Data;
using ChessDotNet.Evaluation;
using ChessDotNet.Hashing;
using ChessDotNet.MoveGeneration;

namespace ChessDotNet.Searching
{
    public class SearchService
    {
        public PossibleMovesService PossibleMovesService { get; set; }
        public EvaluationService EvaluationService { get; set; }

        private const int MaxDepth = 64;
        private PVSResult[] PVTable { get; set; }

        public const int MateScore = 50000;
        public const int MateThereshold = 49000;

        public SearchService(PossibleMovesService possibleMovesService, EvaluationService evaluationService)
        {
            PossibleMovesService = possibleMovesService;
            EvaluationService = evaluationService;
            Clear();
        }

        public void Clear()
        {
            PVTable = new PVSResult[MaxDepth];
        }

        public PVSResult Search(BitBoards bitBoards, int maxDepth)
        {
            for (var i = 1; i <= maxDepth; i++)
            {
                var score = PrincipalVariationSearch(int.MinValue + 1, int.MaxValue, bitBoards, i, 0);
            }
            return PVTable[0];
        }

        public class PVSResult
        {
            public PVSResult(int score, Move move)
            {
                Score = score;
                Move = move;
            }

            public int Score { get; }
            public Move Move { get; }
        }

        public int PrincipalVariationSearch(int alpha, int beta, BitBoards bitBoards, int depth, int currentDepth)
        {
            int score;
            if (currentDepth == depth)
            {
                score = EvaluationService.Evaluate(bitBoards);
                return score;
            }

            // sort moves
            var oldAlpha = alpha;
            var validMoves = 0;
            var bestMove = 0;
            var doPV = true;

            var potentialMoves = PossibleMovesService.GetAllPotentialMoves(bitBoards);
            for (var i = 0; i < potentialMoves.Count; i++)
            {
                var potentialMove = potentialMoves[i];
                var bbAfter = PossibleMovesService.DoMoveIfKingSafe(bitBoards, potentialMove);
                if (bbAfter == null)
                {
                    continue;
                }
                validMoves++;
                if (doPV)
                {
                    score = -PrincipalVariationSearch(-beta, -alpha, bbAfter, depth, currentDepth + 1);
                }
                else
                {
                    score = -ZeroWindowSearch(-alpha, bbAfter, depth, currentDepth + 1);
                    if (score > alpha)
                    {
                        score = -PrincipalVariationSearch(-beta, -alpha, bbAfter, depth, currentDepth + 1);
                    }
                }
                /*if (score > MateThereshold)
                {
                    break;
                }*/
                if (score > alpha)
                {
                    if (score >= beta)
                    {
                        Console.WriteLine("Beta cutoff, score " + score + ", beta: " + beta);
                        return beta;
                    }
                    alpha = score;
                    bestMove = i;
                    doPV = false;
                }
            }

            if (validMoves == 0)
            {
                var enemyAttacks = PossibleMovesService.AttacksService.GetAllAttacked(bitBoards, !bitBoards.WhiteToMove);
                var myKing = bitBoards.WhiteToMove ? bitBoards.WhiteKings : bitBoards.BlackKings;
                if ((enemyAttacks & myKing) != 0)
                {
                    return -MateScore + currentDepth;
                }
                else
                {
                    return 0;
                }
            }

            if (alpha != oldAlpha)
            {
                Console.WriteLine("Alpha changed from " + oldAlpha + " to " + alpha + " at depth " + currentDepth);
                PVTable[currentDepth] = new PVSResult(alpha, potentialMoves[bestMove]);
            }

            return alpha;
        }

        public int ZeroWindowSearch(int beta, BitBoards bitBoards, int depth, int currentDepth)
        {
            var alpha = beta - 1;
            var score = int.MinValue;
            if (currentDepth == depth)
            {
                score = EvaluationService.Evaluate(bitBoards);
                return score;
            }

            var potentialMoves = PossibleMovesService.GetAllPotentialMoves(bitBoards);
            foreach (var potentialMove in potentialMoves)
            {
                var bbAfter = PossibleMovesService.DoMoveIfKingSafe(bitBoards, potentialMove);
                if (bbAfter != null)
                {
                    score = -ZeroWindowSearch(-alpha, bbAfter, depth, currentDepth + 1);
                }

                if (score >= beta)
                {
                    return score;
                }
            }
            return alpha;
        }
    }
}
