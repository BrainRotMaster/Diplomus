using UnityEngine;

namespace PCG
{
    public class PCGSpawnerNodeView : PCGNodeView
    {
        protected override void AddInputPort()
        {
            base.AddInputPort();
        }

        protected override void AddOutputPort()
        {
            // Spawner нода обычно не имеет выходного порта
            // или имеет, если нужно передать точки дальше
        }
    }
}
