using UnityEngine;

namespace Assets.Models
{
    public interface IGrabbable
    {
        void Grabbed(Transform grabber);
        void UnGrabbed();
    }
} 