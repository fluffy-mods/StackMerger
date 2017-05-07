using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

#if DEBUG
using static StackMerger.ListerStackables;
#endif

namespace StackMerger
{
    public class WorkGiver_StackMerge : WorkGiver_Haul
    {
        public override IEnumerable<Thing> PotentialWorkThingsGlobal( Pawn pawn )
        {
#if DEBUG
            LogIfDebug( $"{pawn.NameStringShort} is checking potential things, of which there are { pawn.Map.listerStackables().StackablesListForReading.Count}..."  );
#endif
            return pawn.Map.listerStackables().StackablesListForReading;
        }

        public override bool ShouldSkip( Pawn pawn )
        {
#if DEBUG
            LogIfDebug( $"{pawn.NameStringShort} is trying to skip merging, ShouldSkip is {!PotentialWorkThingsGlobal(pawn).Any()}..." );
#endif
            return !PotentialWorkThingsGlobal( pawn ).Any();
        }

        public override Job JobOnThing( Pawn pawn, Thing thing, bool forced = false )
        {
#if DEBUG
            LogIfDebug( $"{pawn.NameStringShort} is trying to merge {thing.Label}..."  );
#endif

            // standard hauling checks
            if ( !HaulAIUtility.PawnCanAutomaticallyHaulFast( pawn, thing, forced ) )
                return null;

#if DEBUG
            LogIfDebug( $"{thing.LabelCap} can be hauled..." );
#endif

            // find better place, and haul there
            IntVec3 target;
            if ( pawn.Map.listerStackables().TryGetTargetCell( pawn, thing, out target ) )
            {
                if ( pawn.Map.reservationManager.CanReserve( pawn, target, 1 ) )
                {
#if DEBUG
                    LogIfDebug( $"Hauling {thing.Label} to {target}..." );
#endif
                    return HaulAIUtility.HaulMaxNumToCellJob( pawn, thing, target, true );
                }
#if DEBUG
                LogIfDebug($"Couldn't reserve {target}...");
#endif
            }
            else
            {
#if DEBUG
                LogIfDebug($"Couldn't get target cell for {thing.Label}, removing from cache...");
#endif
                pawn.Map.listerStackables().TryRemove( thing );
            }
                 
            return null;
        }

        public override int LocalRegionsToScanFirst => 4;
    }
}
