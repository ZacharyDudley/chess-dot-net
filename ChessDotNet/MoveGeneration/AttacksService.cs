using System;
using ChessDotNet.Data;

namespace ChessDotNet.MoveGeneration
{
    public class AttacksService
    {
        public HyperbolaQuintessence HyperbolaQuintessence { get; set; }

        public AttacksService(HyperbolaQuintessence hyperbolaQuintessence)
        {
            HyperbolaQuintessence = hyperbolaQuintessence;
        }

        public ulong GetAllAttacked(BitBoards bitBoards, bool? byWhite = null)
        {
            byWhite = byWhite ?? bitBoards.WhiteToMove;
            var pawnsAttack = GetAttackedByPawns(bitBoards, byWhite);
            var knightsAttack = GetAttackedByKnights(bitBoards, byWhite);

            //var bishopsAttack = GetAttackedByBishops(bitBoards, forWhite);
            //var rooksAttack = GetAttackedByRooks(bitBoards, forWhite);
            //var queensAttack = GetAttackedByQueens(bitBoards, forWhite);

            var bq = byWhite.Value ? bitBoards.WhiteBishops | bitBoards.WhiteQueens : bitBoards.BlackBishops | bitBoards.BlackQueens;
            var bqAttack = GetAttackedBySlidingPieces(bitBoards, bq, HyperbolaQuintessence.DiagonalAntidiagonalSlide);

            var rq = byWhite.Value ? bitBoards.WhiteRooks | bitBoards.WhiteQueens : bitBoards.BlackRooks | bitBoards.BlackQueens;
            var rqAttack = GetAttackedBySlidingPieces(bitBoards, rq, HyperbolaQuintessence.HorizontalVerticalSlide);

            var kingsAttack = GetAttackedByKings(bitBoards, byWhite);

            var allAttacked = pawnsAttack | knightsAttack | bqAttack | rqAttack | kingsAttack;
            return allAttacked;
        }

        public ulong GetAttackedByBishops(BitBoards bitBoards, bool? byWhite = null)
        {
            var bishops = (byWhite ?? bitBoards.WhiteToMove) ? bitBoards.WhiteBishops : bitBoards.BlackBishops;
            return GetAttackedBySlidingPieces(bitBoards, bishops, HyperbolaQuintessence.DiagonalAntidiagonalSlide);
        }

        public ulong GetAttackedByRooks(BitBoards bitBoards, bool? byWhite = null)
        {
            var rooks = (byWhite ?? bitBoards.WhiteToMove) ? bitBoards.WhiteRooks : bitBoards.BlackRooks;
            return GetAttackedBySlidingPieces(bitBoards, rooks, HyperbolaQuintessence.HorizontalVerticalSlide);
        }

        public ulong GetAttackedByQueens(BitBoards bitBoards, bool? byWhite = null)
        {
            var queens = (byWhite ?? bitBoards.WhiteToMove) ? bitBoards.WhiteQueens : bitBoards.BlackQueens;
            return GetAttackedBySlidingPieces(bitBoards, queens, HyperbolaQuintessence.AllSlide);
        }

        public ulong GetAttackedByKings(BitBoards bitBoards, bool? byWhite = null)
        {
            var kings = (byWhite ?? bitBoards.WhiteToMove) ? bitBoards.WhiteKings : bitBoards.BlackKings;
            return GetAttackedByJumpingPieces(bitBoards, kings, BitBoards.KingSpan, BitBoards.KingSpanPosition);
        }

        public ulong GetAttackedByKnights(BitBoards bitBoards, bool? byWhite = null)
        {
            var knights = (byWhite ?? bitBoards.WhiteToMove) ? bitBoards.WhiteNights : bitBoards.BlackNights;
            return GetAttackedByJumpingPieces(bitBoards, knights, BitBoards.KnightSpan, BitBoards.KnightSpanPosition);
        }

        public ulong GetAttackedByPawns(BitBoards bitBoards, bool? byWhite = null)
        {
            ulong pawnsLeft;
            ulong pawnsRight;
            if (byWhite ?? bitBoards.WhiteToMove)
            {
                pawnsLeft = (bitBoards.WhitePawns << 7) & ~BitBoards.Files[7];
                pawnsRight = (bitBoards.WhitePawns << 9) & ~BitBoards.Files[0];
            }
            else
            {
                pawnsLeft = (bitBoards.BlackPawns >> 7) & ~BitBoards.Files[0];
                pawnsRight = (bitBoards.BlackPawns >> 9) & ~BitBoards.Files[7];
            }
            return pawnsLeft | pawnsRight;
        }

        private ulong GetAttackedBySlidingPieces(BitBoards bitBoards, ulong slidingPieces, Func<BitBoards, int, ulong> slideResolutionFunc)
        {
            var allSlide = 0UL;
            while (slidingPieces != 0)
            {
                var i = slidingPieces.BitScanForward();
                var slide = slideResolutionFunc.Invoke(bitBoards, i);
                allSlide |= slide;
                slidingPieces &= ~(1UL << i);
            }
            return allSlide;
        }

        private ulong GetAttackedByJumpingPieces(BitBoards bitBoards, ulong jumpingPieces, ulong jumpMask, int jumpMaskCenter)
        {
            ulong allJumps = 0;
            while (jumpingPieces != 0)
            {
                var i = jumpingPieces.BitScanForward();
                ulong jumps;
                if (i > jumpMaskCenter)
                {
                    jumps = jumpMask << (i - jumpMaskCenter);
                }
                else
                {
                    jumps = jumpMask >> (jumpMaskCenter - i);
                }

                jumps &= ~(i % 8 < 4 ? BitBoards.Files[6] | BitBoards.Files[7] : BitBoards.Files[0] | BitBoards.Files[1]);
                allJumps |= jumps;
                jumpingPieces &= ~(1UL << i);
            }
            return allJumps;
        }
    }
}