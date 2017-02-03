using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace StackMerger
{
    public class WorkGiver_StackMerge : WorkGiver_Haul
    {
        public override IEnumerable<Thing> PotentialWorkThingsGlobal( Pawn pawn )
        {
            Log.Message( $"{pawn.NameStringShort} is checking potential things, of which there are {pawn.Map.listerStackables().StackablesListForReading.Count}..."  );
            return pawn.Map.listerStackables().StackablesListForReading;
        }

        public override bool ShouldSkip( Pawn pawn )
        {
            Log.Message( $"{pawn.NameStringShort} is trying to skip merging, ShouldSkip is {!PotentialWorkThingsGlobal(pawn).Any()}..." );
            return !PotentialWorkThingsGlobal( pawn ).Any();
        }

        public override Job JobOnThing( Pawn pawn, Thing thing )
        {
            Log.Message( $"{pawn.NameStringShort} is trying to merge {thing.Label}..."  );

            if ( !thing.MapHeld.listerStackables().Check( thing ) )
                return null;

            Log.Message( $"{thing.LabelCap} checks out..." );

            // standard hauling checks
            if ( !HaulAIUtility.PawnCanAutomaticallyHaulFast( pawn, thing ) )
                return null;

            // can reserve and reach target location

            Log.Message( $"{thing.LabelCap} can be hauled..." );

            // find better place
            IntVec3 target;
            if ( pawn.Map.listerStackables().TryGetTargetCell( pawn, thing, out target ) )
                if ( pawn.Map.reservationManager.CanReserve( pawn, target, 1 ) )
                    return HaulAIUtility.HaulMaxNumToCellJob( pawn, thing, target , true );
            return null;
        }

        public override int LocalRegionsToScanFirst => 4;
    }
}
