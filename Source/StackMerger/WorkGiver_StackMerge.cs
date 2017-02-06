using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using static StackMerger.ListerStackables;

namespace StackMerger
{
    public class WorkGiver_StackMerge : WorkGiver_Haul
    {
        public override IEnumerable<Thing> PotentialWorkThingsGlobal( Pawn pawn )
        {
            LogIfDebug( $"{pawn.NameStringShort} is checking potential things, of which there are { pawn.Map.listerStackables().StackablesListForReading.Count}..."  );
            return pawn.Map.listerStackables().StackablesListForReading;
        }

        public override bool ShouldSkip( Pawn pawn )
        {
            LogIfDebug( $"{pawn.NameStringShort} is trying to skip merging, ShouldSkip is {!PotentialWorkThingsGlobal(pawn).Any()}..." );
            return !PotentialWorkThingsGlobal( pawn ).Any();
        }

        public override Job JobOnThing( Pawn pawn, Thing thing )
        {
            LogIfDebug( $"{pawn.NameStringShort} is trying to merge {thing.Label}..."  );
            
            // standard hauling checks
            if ( !HaulAIUtility.PawnCanAutomaticallyHaulFast( pawn, thing ) )
                return null;
            
            LogIfDebug( $"{thing.LabelCap} can be hauled..." );

            // find better place, and haul there
            IntVec3 target;
            if ( pawn.Map.listerStackables().TryGetTargetCell( pawn, thing, out target ) )
            {
                if ( pawn.Map.reservationManager.CanReserve( pawn, target, 1 ) )
                {
                    LogIfDebug( $"Hauling {thing.Label} to {target}..." );
                    return HaulAIUtility.HaulMaxNumToCellJob( pawn, thing, target, true );
                }
                LogIfDebug($"Couldn't reserve {target}...");
            }
            else
            {
                LogIfDebug($"Couldn't get target cell for {thing.Label}, removing from cache...");
                pawn.Map.listerStackables().TryRemove( thing );
            }
                 
            return null;
        }

        public override int LocalRegionsToScanFirst => 4;
    }
}
