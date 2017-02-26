using UnityEngine;
using KsubakaPool.Controllers;

namespace KsubakaPool
{
    class PocketsCollider : MonoBehaviour
    {
        private void OnTriggerEnter(Collider collider)
        {
            CueBallController cueBall = collider.gameObject.GetComponent<CueBallController>();
            if (cueBall != null)
                cueBall.BallPocketed();
        }
    }
}
