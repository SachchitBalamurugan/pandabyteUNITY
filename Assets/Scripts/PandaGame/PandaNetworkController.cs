using UnityEngine;
using Photon.Pun;
using TurnBasedCore.Core.Players;
using TurnBasedCore.Core.TurnSystem;

namespace PandaGame
{
    public class PandaNetworkController : MonoBehaviourPun, IPunObservable
    {
        public DirectionalSpriteController[] directionalSpriteControllers;


        public float moveSpeed = 5f;
        public float circleRadius = 10;
        public Animator animator;

        
        private Vector3 targetPosition;
        private SpriteRenderer spriteRenderer;


        private bool IsStopped = true;


        public bool CanMove { get; set; }

        public PlayerInfo Info { get; set; }

        private void Start()
        {
            targetPosition = transform.position;

            CanMove = true;

            if (photonView.IsMine)
                GetComponent<Collider2D>().enabled = false;


            Info = new PlayerInfo(photonView.ViewID, photonView.Owner.NickName, false, photonView.IsMine);
        }

        private void Update()
        {
            if (!CanMove) return;

            if (photonView.IsMine)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    mousePos.z = 0;
                    targetPosition = mousePos;

                    IsStopped = false;
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
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

                if (Vector3.Distance(transform.position, targetPosition) < circleRadius)
                {
                    if (!IsStopped)
                    {
                        var hit = Physics2D.CircleCast(transform.position, circleRadius, Vector2.zero);

                        if (hit.collider != null && hit.collider.TryGetComponent(out Entity entity))
                        {
                            // connect;
                            BattleConnector.Instance.InitializeBattle(GetComponent<Entity>(), entity);
                        }

                        IsStopped = true;
                    }

                    IsStopped = true;
                }
            }
            if (!IsStopped)
            {
                foreach (var item in directionalSpriteControllers)
                {
                    item.SetSpriteBasedOnDirection(direction);
                }
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


       

        public void GameComplete()
        {
            CanMove = true;
        }
    }


    [System.Serializable]
    public class DirectionalSpriteController
    {
        public SpriteRenderer spriteRenderer;
        [Header("Sprites for each direction")]
        public Sprite upSprite;
        public Vector3 posUp;
        public Sprite downSprite;
        public Vector3 posDown;
        public Sprite leftSprite;
        public Vector3 posLeft;
        public Sprite rightSprite;
        public Vector3 posRight;


        public void SetSpriteBasedOnDirection(Vector2 dir)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // Normalize angle to 0-360
            if (angle < 0) angle += 360;

            // Check direction ranges
            if (angle >= 45 && angle < 135) // UP
            {
                spriteRenderer.sprite = upSprite;
                spriteRenderer.transform.localPosition = posUp;
            }
            else if (angle >= 135 && angle < 225) // LEFT
            {
                spriteRenderer.sprite = leftSprite;
                spriteRenderer.transform.localPosition = posLeft;
            }
            else if (angle >= 225 && angle < 315) // DOWN
            {
                spriteRenderer.sprite = downSprite;
                spriteRenderer.transform.localPosition = posDown;
            }
            else // RIGHT
            {
                spriteRenderer.sprite = rightSprite;
                spriteRenderer.transform.localPosition = posRight;
            }
        }
    }

}
