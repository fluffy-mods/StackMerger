// Karel Kroeze
// ListerStackables.cs
// 2017-02-03

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace StackMerger
{
    public class ListerStackables : MapComponent
    {
        private HashSet<Thing> stackables;
        private int slotGroupIndex;

        public ListerStackables( Map map ) : base( map ) {
            stackables = new HashSet<Thing>();
        }

        public override void MapComponentTick()
        {
            // check one slotgroup every 10 ticks (6Hz)
            if ( !( Current.Game.tickManager.TicksGame % 10 == 0 ) )
                return;

            // do we have any slotgroups?
            if ( map.slotGroupManager.AllGroupsListForReading.NullOrEmpty() )
                return;
            
            // iterate slotgroups
            slotGroupIndex++;
            if ( slotGroupIndex == int.MaxValue )
                slotGroupIndex = 0;

            // get current slotgroup
            List<SlotGroup> groups = map.slotGroupManager.AllGroupsListForReading;
            SlotGroup current = groups[slotGroupIndex % groups.Count];
            
            Log.Message( $"Checking {current.parent.SlotYielderLabel()}..."  );

            // get list of current stackables
            var currentStackables = GetStackables( current );

            Log.Message( $"Found {currentStackables.Count()} stackables..." );

            // update list
            UpdateStackables( current, currentStackables );

            Log.Message( $"There are now {stackables.Count} stackables in total..." );
        }

        private void UpdateStackables( SlotGroup slotgroup, IEnumerable<Thing> currentStackables )
        {
            // add things in current not in the list
            foreach ( Thing stackable in currentStackables )
                if ( !stackables.Contains( stackable ) )
                    stackables.Add( stackable );
        }

        public List<Thing> StackablesListForReading => new List<Thing>( stackables );

        internal IEnumerable<Thing> GetStackables( SlotGroup storage )
        {
            // garbage in, slightly better garbage out
            if ( storage?.HeldThings == null )
                return new List<Thing>();

            // get list of potential stacks in the room(s) of this slotgroup
            var potentialTargets = storage.HeldThings
                                          .Where( t => t.stackCount < t.def.stackLimit &&
                                                       t.IsInValidBestStorage() );

            return storage.HeldThings.Where( thing => thing.stackCount < thing.def.stackLimit &&
                                                      thing.IsInValidBestStorage() &&
                                                      potentialTargets.Any( other => thing != other &&
                                                                                     thing.CanStackWith( other ) &&
                                                                                     thing.stackCount <= other.stackCount ) );

        }

        public bool TryGetTargetCell( Pawn pawn, Thing thing, out IntVec3 target )
        {
            // get valid cells 
            var targetThings = thing.GetSlotGroup()?
                                    .HeldThings?
                                    .Where( other => thing != other
                                                     && other.CanStackWith( thing )
                                                     && map.reservationManager.CanReserve( pawn, other, 1 )
                                                     && other.IsInValidBestStorage()
                                                     && other.stackCount < other.def.stackLimit );

            // select valid cell with the current highest count, if any
            if ( targetThings != null && targetThings.Any() )
            {
                target = targetThings.MaxBy( t => t.stackCount ).Position;
                return true;
            }
            
            // no targets :(
            target = IntVec3.Invalid;
            return false;
        }
        
        internal bool Check( Thing thing )
        {
            // reject garbage
            if ( thing == null )
                return false;
            
            // stack is not full yet, and doesn't need to be hauled to a different storage
            if ( thing.stackCount >= thing.def.stackLimit || !thing.IsInValidBestStorage() )
                return false;

            // there is another target stack available
            var potentialTargets = thing.GetSlotGroup().HeldThings;
            var stackable = potentialTargets.Any( other => thing != other
                                                           && other.CanStackWith( thing )
                                                           // && other.IsInValidBestStorage() // true by definition if in same slotgroup
                                                           && other.stackCount < other.def.stackLimit );

            // this was found not to be stackable, remove from the list
            if ( !stackable && stackables.Contains( thing ) )
                stackables.Remove( thing );

            return stackable;
        }
    }
}