// Karel Kroeze
// Map_Extensions.cs
// 2017-02-03

using Verse;

namespace StackMerger
{
    public static class Map_Extensions
    {
        public static ListerStackables listerStackables( this Map map )
        {
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