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
        public override IEnumerable<Thing> PotentialWorkThingsGlobal( Pawn Pawn )
        {
            return Pawn.Map.listerStackables().StackablesListForReading;
        }

        public override bool ShouldSkip( Pawn pawn )
        {
            return PotentialWorkThingsGlobal( pawn ).Any();
        }

        public override Job JobOnThing( Pawn pawn, Thing thing )
        {
            if ( !thing.MapHeld.listerStackables().Check( thing ) )
                return null;

            // standard hauling checks
            if ( !HaulAIUtility.PawnCanAutomaticallyHaulFast( pawn, thing ) )
                return null;

            // find better place
            IntVec3 target;
            if ( pawn.Map.listerStackables().TryGetTargetCell( pawn, thing, out target ) )
                return HaulAIUtility.HaulMaxNumToCellJob( pawn, thing, target , true );
            return null;
        }

        public override int LocalRegionsToScanFirst => 4;
    }
}
