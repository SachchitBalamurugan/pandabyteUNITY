using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class NameTagUI : MonoBehaviourPun
{
    public TextMeshProUGUI nameText;
    private Transform target;

    private void Start()
    {
        target = transform.parent;
        nameText.text = photonView.Owner.NickName;
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            transform.position = Camera.main.WorldToScreenPoint(target.position + new Vector3(0, 1.5f, 0));
        }
    }
}
