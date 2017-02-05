using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using RimWorld;
using Verse;
using static StackMerger.HarmonyPatches;

namespace StackMerger
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            // do harmony patches
            var harmony = HarmonyInstance.Create( "rimworld.fluffy.stackmerger" );
            harmony.PatchAll( Assembly.GetExecutingAssembly() );

            // get field info
            MapFieldInfo = typeof( ListerHaulables ).GetField( "map", (BindingFlags)60 );
            if (MapFieldInfo == null)
                throw new Exception( "Could not get ListerHaulables.map FieldInfo!" );
        }

        public static FieldInfo MapFieldInfo;
    }

    [HarmonyPatch( typeof( ListerHaulables ) )]
    [HarmonyPatch( "Notify_DeSpawned" )]
    [HarmonyPatch( new Type[] { typeof( Thing ) } )]
    class Notify_DeSpawned
    {
        static void Postfix( Thing t, ListerHaulables __instance )
        {
            Map map = MapFieldInfo.GetValue( __instance ) as Map;
            if (map == null)
                throw new Exception( $"Could not get map to despawn {t.Label} from...");

            map.listerStackables().CheckRemove( t );
        }
    }

    [HarmonyPatch( typeof( ListerHaulables ) )]
    [HarmonyPatch( "Notify_Forbidden" )]
    [HarmonyPatch( new Type[] { typeof( Thing ) } )]
    class Notify_Forbidden
    {
        static void Postfix( Thing t ) { t.MapHeld.listerStackables().CheckRemove( t ); }
    }

    [HarmonyPatch( typeof( ListerHaulables ) )]
    [HarmonyPatch( "Notify_SlotGroupChanged" )]
    [HarmonyPatch( new Type[] { typeof( SlotGroup) } )]
    class Notify_SlotGroupChanged
    {
        static void Postfix( SlotGroup sg, ListerHaulables __instance )
        {
            Map map = MapFieldInfo.GetValue( __instance ) as Map;
            if ( map == null )
                throw new Exception( $"Could not get map to change {sg.parent.SlotYielderLabel()} on..." );
            
            map.listerStackables().Update( sg );
        }
    }

    [HarmonyPatch( typeof( ListerHaulables ) )]
    [HarmonyPatch( "Notify_Spawned" )]
    [HarmonyPatch( new Type[] { typeof( Thing ) } )]
    class Notify_Spawned
    {
        static void Postfix( Thing t ) { t.MapHeld.listerStackables().CheckAdd( t ); }
    }

    [HarmonyPatch( typeof( ListerHaulables ) )]
    [HarmonyPatch( "Notify_Unforbidden" )]
    [HarmonyPatch( new Type[] { typeof( Thing ) } )]
    class Notify_Unforbidden
    {
        static void Postfix( Thing t ) { t.MapHeld.listerStackables().CheckAdd( t ); }
    }
}
