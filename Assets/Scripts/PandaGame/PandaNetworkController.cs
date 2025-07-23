using UnityEngine;
using Photon.Pun;

namespace PandaGame
{
    public class PandaNetworkController : MonoBehaviourPun, IPunObservable
    {
        public float moveSpeed = 5f;
        private Vector3 targetPosition;
        public Animator animator;
        private SpriteRenderer spriteRenderer;

        private void Start()
        {
            targetPosition = transform.position;
           // animator = GetComponent<Animator>();
           // spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (photonView.IsMine)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    mousePos.z = 0;
                    targetPosition = mousePos;
                }

                MoveAndAnimate();
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }
        }

        private void MoveAndAnimate()
        {
            Vector3 direction = targetPosition - transform.position;
            animator.SetFloat("Speed", direction.magnitude);

            if (direction.magnitude > 0.01f)
            {
               //ad spriteRenderer.flipX = direction.x < 0;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(targetPosition);
            }
            else
            {
                targetPosition = (Vector3)stream.ReceiveNext();
            }
        }
    }
}
