// Karel Kroeze
// Map_Extensions.cs
// 2017-02-03

using System;
using Verse;

namespace StackMerger
{
    public static class Map_Extensions
    {
        public static ListerStackables listerStackables( this Map map )
        {
            if ( map == null )
                throw new ArgumentNullException( nameof( map ) );

            // fetch component
            var lister = map.GetComponent<ListerStackables>();

            // if not exists, create and inject component
            if ( lister == null )
            {
                lister = new ListerStackables( map );
                map.components.Add( lister );
            }

            // done.
            return lister;
        }
    }
}