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
            LogIfDebug( $"{pawn.NameStringShort} is checking potential things, of which there are { StackablesListForReading( pawn.Map ).Count}..."  );
            return StackablesListForReading( pawn.Map );
        }

        public override bool ShouldSkip( Pawn pawn )
        {
            LogIfDebug( $"{pawn.NameStringShort} is trying to skip merging, ShouldSkip is {!PotentialWorkThingsGlobal(pawn).Any()}..." );
            return !PotentialWorkThingsGlobal( pawn ).Any();
        }

        public override Job JobOnThing( Pawn pawn, Thing thing )
        {
            LogIfDebug( $"{pawn.NameStringShort} is trying to merge {thing.Label}..."  );

            // CheckRemove will gradually whittle down invalid stackables by removing them from the lister.
            // todo; figure out why they're still in the lister in the first place...
            if ( !CheckRemove( thing, pawn ) )
                return null;
            
            // standard hauling checks
            if ( !HaulAIUtility.PawnCanAutomaticallyHaulFast( pawn, thing ) )
                return null;

            // can reserve and reach target location
            LogIfDebug( $"{thing.LabelCap} can be hauled..." );

            // find better place, and haul there
            IntVec3 target;
            if ( pawn.Map.listerStackables().TryGetTargetCell( pawn, thing, out target ) )
                if ( pawn.Map.reservationManager.CanReserve( pawn, target, 1 ) )
                    return HaulAIUtility.HaulMaxNumToCellJob( pawn, thing, target , true );
            return null;
        }

        public override int LocalRegionsToScanFirst => 4;
    }
}
