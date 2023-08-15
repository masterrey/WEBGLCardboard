using System;
using UnityEngine;

namespace TiltShift.Cardboard.Controls
{
    /// <summary>
    /// This interface need for implement anything controls for cardboard.
    /// </summary>
    public abstract class CardboardControlBase: MonoBehaviour
    {
        [NonSerialized]
        public bool IgnoreClick = false;

        public virtual void OnCursorHover(Vector3 position) { }

        public virtual void OnCursorLeave() { }

        public virtual void OnClick(Vector3 position) { }
    }
}