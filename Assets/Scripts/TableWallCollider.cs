using UnityEngine;
using ThreeDPool.Controllers;
using ThreeDPool.Managers;

namespace ThreeDPool
{
    public class TableWallCollider : MonoBehaviour
    {
        private void OnTriggerStay(Collider collider)
        {
            CueBallController cueBallController = collider.gameObject.GetComponent<CueBallController>();
            if (cueBallController != null && cueBallController.GetComponent<Rigidbody>().IsSleeping())
            {
                GameManager.Instance.AddToBallHitOutList(cueBallController);
            }
        }
    }
}
