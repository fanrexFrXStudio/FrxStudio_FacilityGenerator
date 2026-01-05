using System.Collections.Generic;
using UnityEngine;

namespace FrxStudio.Generator
{
    [RequireComponent(typeof(FacilityGenerator))]
    public class FacilityGeneratorGizmo : MonoBehaviour
    {
        [SerializeField]
        private bool drawGizmo;

        private readonly List<IGizmoDrawable> drawables = new();

        public void AddDrawable(IGizmoDrawable target) => drawables.Add(target);
        public void ClearAllDrawables() => drawables.Clear();

        private void OnDrawGizmos()
        {
            if (!drawGizmo)
                return;
            
            foreach (var drawable in drawables)
                drawable.DrawGizmo();
        }
    }
}
