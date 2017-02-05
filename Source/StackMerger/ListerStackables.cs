// Karel Kroeze
// ListerStackables.cs
// 2017-02-03

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace StackMerger
{
    public class ListerStackables : MapComponent
    {
        private List<Thing> stackables = new List<Thing>();
        private int slotGroupIndex;
        private int stackableIndex;

        public ListerStackables( Map map ) : base( map ) {}

        private void LogDebug( Thing thing )
        {
            LogIfDebug( $"{thing.Label}\n\tSpawned:{thing.Spawned}\n\tPos:{thing.Position}\n\tForbidden:{thing.IsForbidden( Faction.OfPlayer )}" );
        }

        public static void LogIfDebug( string message )
        {
#if DEBUG
            Log.Message( message );
#endif
        }

        public override void MapComponentTick()
        {
            if ( Current.Game.tickManager.TicksGame % 600 == 0 )
            {
                stackables.ForEach( LogDebug );
            }
            
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
            Update( current );
        }

        private void Update( IEnumerable<Thing> currentStackables )
        {
            // add things in current not in the list
            foreach ( Thing stackable in currentStackables )
                if ( !stackables.Contains( stackable ) )
                    stackables.Add( stackable );
        }

        public List<Thing> StackablesListForReading => new List<Thing>( stackables );
        
        internal static IEnumerable<Thing> GetStackables( SlotGroup storage )
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
                                                     // only move stuff to larger stacks
                                                     // should stop situation with stacksize mods 
                                                     // where pawns keep going back and forth between stacks
                                                     && other.stackCount >= thing.stackCount
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
        
        internal bool CheckRemove( Thing thing, Pawn pawn = null )
        {
            // reject garbage
            if ( thing == null )
                return false;
            
            bool stackable = Stackable( thing, pawn );

            // this was found not to be stackable, remove from the list
            if ( !stackable && stackables.Contains( thing ) )
                stackables.Remove( thing );
            
            return stackable;
        }

        internal void Update( SlotGroup slotgroup )
        {
            // get list of current stackables
            var currentStackables = GetStackables( slotgroup );

            // update list
            Update( currentStackables );
        }

        internal void CheckAdd( Thing thing )
        {
            if ( Stackable(thing) && !stackables.Contains( thing ))
                 stackables.Add( thing );
        }

        private static bool Stackable( Thing thing, Pawn pawn = null )
        {
            // stack still exists, is not full yet, and doesn't need to be hauled to a different storage
            bool stackable = thing.GetSlotGroup() != null // includes thing.Spawned
                             && thing.def.alwaysHaulable
                             // if pawn is not given, assume player faction
                             && !thing.IsForbidden( pawn?.Faction ?? Faction.OfPlayer )
                             && thing.stackCount < thing.def.stackLimit
                             && thing.IsInValidBestStorage();

            // cop out if this isn't stackable
            if ( !stackable )
                return false;

            // there is another target stack available
            var potentialTargets = thing.GetSlotGroup().HeldThings;

            stackable = stackable && potentialTargets.Any( other => thing != other
                                                                    && other.CanStackWith( thing )
                                                                    // only more stuff to stacks that are larger or equal size than you
                                                                    // (should make things more efficient, as well as stop pawns going back and 
                                                                    // forth with stackSize mods installed)
                                                                    && other.stackCount >= thing.stackCount
                                                                    // && other.IsInValidBestStorage() // true by definition if in same slotgroup
                                                                    && other.stackCount < other.def.stackLimit );
            return stackable;
        }
    }
}