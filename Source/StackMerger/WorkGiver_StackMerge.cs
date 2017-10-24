using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using static StackMerger.Logger;

#if DEBUG
using static StackMerger.ListerStackables;
#endif

namespace StackMerger
{
    public class WorkGiver_StackMerge : WorkGiver_Haul
    {
        public override IEnumerable<Thing> PotentialWorkThingsGlobal( Pawn pawn )
        {
            Logger.Debug( $"{pawn.NameStringShort} is checking potential things, of which there are { pawn.Map.listerStackables().StackablesListForReading.Count}..."  );
            return pawn.Map.listerStackables().StackablesListForReading;
        }

        public override bool ShouldSkip( Pawn pawn )
        {
            Logger.Debug( $"{pawn.NameStringShort} is trying to skip merging, ShouldSkip is {!PotentialWorkThingsGlobal(pawn).Any()}..." );
            return !PotentialWorkThingsGlobal( pawn ).Any();
        }

        public override Job JobOnThing( Pawn pawn, Thing thing, bool forced = false )
        {
            Logger.Debug( $"{pawn.NameStringShort} is trying to merge {thing.Label}..."  );

            // standard hauling checks
            if ( !HaulAIUtility.PawnCanAutomaticallyHaulFast( pawn, thing, forced ) )
                return null;
            Logger.Debug( $"{thing.LabelCap} can be hauled..." );

            // find better place, and haul there
            IntVec3 target;
            if ( pawn.Map.listerStackables().TryGetTargetCell( pawn, thing, out target ) )
            {
                if ( pawn.Map.reservationManager.CanReserve( pawn, target, 1 ) )
                {
                    Logger.Debug( $"Hauling {thing.Label} to {target}..." );
                    return HaulAIUtility.HaulMaxNumToCellJob( pawn, thing, target, true );
                }

                Logger.Debug($"Couldn't reserve {target}...");
            }
            else
            {
                Logger.Debug($"Couldn't get target cell for {thing.Label}, removing from cache...");
                pawn.Map.listerStackables().TryRemove( thing );
            }
                 
            return null;
        }

        public override int LocalRegionsToScanFirst => 4;
    }
}
