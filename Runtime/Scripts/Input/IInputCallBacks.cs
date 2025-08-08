using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Concept.Input
{
    public interface IInputCallBacks
    {

        void OnTouch(float x, float y);
        void OnRelease(float x, float y);
        void OnDraggingHorizontal(float factor);
        void OnDraggingVertical(float factor);

        void OnDragEnded(float x, float y);
        void OnThreeFingersDragging(float x, float y);

        void OnPinchingStart(float factor);
        void OnPinching(float factor);

        void OnSelect(object sender);



    }
}
