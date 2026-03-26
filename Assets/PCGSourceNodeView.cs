using UnityEngine;

namespace PCG
{
    public class PCGSourceNodeView : PCGNodeView
    {
        protected override void AddInputPort()
        {
            // Source нода не имеет входного порта
        }

        protected override void AddOutputPort()
        {
            base.AddOutputPort();
        }
    }
}
