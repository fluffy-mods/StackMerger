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

        public ListerStackables( Map map ) : base( map ) {}

#if DEBUG
        private void LogDebug( Thing thing )
        {
            LogIfDebug( $"{thing.Label}\n\tSpawned:{thing.Spawned}\n\tPos:{thing.Position}\n\tForbidden:{thing.IsForbidden( Faction.OfPlayer )}" );
        }

        public static void LogIfDebug( string message )
        {
            Log.Message( message );
        }
#endif

        public override void MapComponentTick()
        {
#if DEBUG
            if ( Current.Game.tickManager.TicksGame % 600 == 0 )
            {
                stackables.ForEach( LogDebug );
            }
#endif
            
            // check one slotgroup every 30 ticks (2Hz)
            if ( Current.Game.tickManager.TicksGame % 30 != 0 )
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
        
        public List<Thing> StackablesListForReading => new List<Thing>( stackables );
        
        internal static IEnumerable<Thing> GetStackables( SlotGroup storage )
        {
            // garbage in, slightly better garbage out
            if ( storage?.HeldThings == null )
                return new List<Thing>();

            return storage.HeldThings.Where( thing => TheoreticallyStackable( thing )
                                                      && storage.HeldThings.Any( other => CanBeStackTarget( other, thing ) ) );
        }

        public bool TryGetTargetCell( Pawn pawn, Thing thing, out IntVec3 target )
        {
            if ( TheoreticallyStackable( thing, pawn ) )
            {
                // get valid cells 
                var targetThings = thing.GetSlotGroup()?
                                        .HeldThings?
                                        .Where( other => CanBeStackTarget( other, thing, pawn ) );

                // select valid cell with the current highest count, if any
                if (targetThings != null && targetThings.Any())
                {
                    target = targetThings.MaxBy(t => t.stackCount).Position;
                    return true;
                }
            }
            
            // no targets :(
            target = IntVec3.Invalid;
            return false;
        }
        
        internal void TryRemove( Thing thing )
        {
            if ( stackables.Contains( thing ) )
                stackables.Remove( thing );
        }

        internal void Update( SlotGroup slotgroup )
        {
            // get list of current stackables
            var currentStackables = GetStackables( slotgroup );
            
            // add things in current not in the list
            foreach (Thing stackable in currentStackables)
                if (!stackables.Contains(stackable))
                    stackables.Add(stackable);
        }

        internal void CheckAdd( Thing thing )
        {
            if ( Stackable(thing) && !stackables.Contains( thing ))
                 stackables.Add( thing );
        }

        private static bool CanBeStackTarget( Thing target, Thing thing, Pawn pawn = null )
        {
            return thing != target
                   && target != null && thing != null
                   && target.CanStackWith( thing )
                   // only move stuff to larger stacks
                   // should stop situation with stacksize mods 
                   // where pawns keep going back and forth between stacks
                   && target.stackCount >= thing.stackCount
                   && target.stackCount < target.def.stackLimit
                   // is a good storage cell (no blockers, can be reserved, reachable, no fires, etc)
                   && StoreUtility.IsGoodStoreCell( target.Position, target.Map, thing, pawn,
                                                    pawn?.Faction ?? Faction.OfPlayer )
                   // is going to stay around for a while
                   && target.IsInValidBestStorage();
        }

        private static bool TheoreticallyStackable( Thing thing, Pawn pawn = null )
        {
            // stack still exists, is not full yet, and doesn't need to be hauled to a different storage
            return thing?.GetSlotGroup() != null // includes thing.Spawned
                   && thing.def.alwaysHaulable
                   // if pawn is not given, assume player faction
                   && !thing.IsForbidden( pawn?.Faction ?? Faction.OfPlayer )
                   && thing.stackCount < thing.def.stackLimit
                   && thing.IsInValidBestStorage();
        }

        private static bool Stackable( Thing thing, Pawn pawn = null )
        {
            // is this stack a viable target at all, and is there another target stack available
            return TheoreticallyStackable(thing, pawn) 
                && thing.GetSlotGroup().HeldThings.Any( other => CanBeStackTarget( other, thing, pawn ) );
            
        }
    }
}