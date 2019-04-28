using UnityEngine;

namespace MapleGlaze.CardsOfLife.Utils
{
    public static class GizmoUtil
    {
        public static void DrawRectGizmoZ(Rect rect, float z)
        {
            Gizmos.DrawLine(new Vector3(rect.x, rect.y, z),
                            new Vector3(rect.x, rect.y + rect.height, z));

            Gizmos.DrawLine(new Vector3(rect.x, rect.y + rect.height, z),
                            new Vector3(rect.x + rect.width, rect.y + rect.height, z));
            
            Gizmos.DrawLine(new Vector3(rect.x + rect.width, rect.y + rect.height, z),
                            new Vector3(rect.x + rect.width, rect.y, z));

            Gizmos.DrawLine(new Vector3(rect.x + rect.width, rect.y, z),
                            new Vector3(rect.x, rect.y, z));
        }

        public static void DrawRectGizmoY(Rect rect, float y)
        {
            Gizmos.DrawLine(new Vector3(rect.x, y, rect.y),
                            new Vector3(rect.x, y, rect.y + rect.height));

            Gizmos.DrawLine(new Vector3(rect.x, y, rect.y + rect.height),
                            new Vector3(rect.x + rect.width, y, rect.y + rect.height));
            
            Gizmos.DrawLine(new Vector3(rect.x + rect.width, y, rect.y + rect.height),
                            new Vector3(rect.x + rect.width, y, rect.y));

            Gizmos.DrawLine(new Vector3(rect.x + rect.width, y, rect.y),
                            new Vector3(rect.x, y, rect.y));
        }
    }
}