using UnityEngine;

namespace ApartPlatformer
{
    public interface IDistractionItem
    {
        void DistractEnemies();
        Vector2 GetPosition();
    }
} 