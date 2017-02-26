using UnityEngine;
using KsubakaPool.Controllers;
using KsubakaPool.Managers;

namespace KsubakaPool
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
