using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using RimWorld;
using Verse;

namespace StackMerger
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = HarmonyInstance.Create( "rimworld.fluffy.stackmerger" );
            harmony.PatchAll( Assembly.GetExecutingAssembly() );
        }
    }

    [HarmonyPatch( typeof( ListerHaulables ) )]
    [HarmonyPatch( "Notify_DeSpawned" )]
    [HarmonyPatch( new Type[] { typeof( Thing ) } )]
    class Notify_DeSpawned
    {
        static void Postfix( Thing t ) { ListerStackables.CheckRemove( t ); }
    }

    [HarmonyPatch( typeof( ListerHaulables ) )]
    [HarmonyPatch( "Notify_Forbidden" )]
    [HarmonyPatch( new Type[] { typeof( Thing ) } )]
    class Notify_Forbidden
    {
        static void Postfix( Thing t ) { ListerStackables.CheckRemove( t ); }
    }

    [HarmonyPatch( typeof( ListerHaulables ) )]
    [HarmonyPatch( "Notify_SlotGroupChanged" )]
    [HarmonyPatch( new Type[] { typeof( SlotGroup) } )]
    class Notify_SlotGroupChanged
    {
        static void Postfix( SlotGroup sg ) { ListerStackables.Update( sg ); }
    }

    [HarmonyPatch( typeof( ListerHaulables ) )]
    [HarmonyPatch( "Notify_Spawned" )]
    [HarmonyPatch( new Type[] { typeof( Thing ) } )]
    class Notify_Spawned
    {
        static void Postfix( Thing t ) { ListerStackables.CheckAdd( t ); }
    }

    [HarmonyPatch( typeof( ListerHaulables ) )]
    [HarmonyPatch( "Notify_Unforbidden" )]
    [HarmonyPatch( new Type[] { typeof( Thing ) } )]
    class Notify_Unforbidden
    {
        static void Postfix( Thing t ) { ListerStackables.CheckAdd( t ); }
    }
}
