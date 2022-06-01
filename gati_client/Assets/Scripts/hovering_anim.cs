using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class hovering_anim : MonoBehaviour
{
    [SerializeField] private string anim="enlarge";
    [SerializeField] private float delay = 0.03f;
    [SerializeField] private float rate = 0.05f;
    private float scl=1;
    private float ascl = 1;
    private float pos;
    private float bpos;
    private float apos;
    private bool mouseon = false;
    private float elt;
    private Button btn;
    private int pr = 0;
    private float wt = 1;
    RectTransform rt;

    // Start is called before the first frame update
    void Start()
    {
        elt = Time.realtimeSinceStartup;

        bpos = transform.position.y;
        pos = transform.position.y;
        ascl = 1;
        apos = bpos;

        btn = GetComponent<Button>();

        if (btn!=null)
        {
            btn.onClick.AddListener(BtnClick);
        }

        rt = (RectTransform)GetComponent<Transform>();
    }

    public void BtnClick()
    {
        UIManager.Singleton.clickaudio.Play();
    }

    public void MouseEntered()
    {
        if (btn==null || btn.interactable)
        {
            UIManager.Singleton.hoveraudio.Play();

            mouseon = true;

            if (anim == "enlarge")
            {
                ascl = 1 + rate;
                wt = delay;
                //transform.localScale = new Vector3(1.1F, 1.1f, 1);
            }
            else if (anim == "reduce")
            {
                ascl = 1 - rate;
                wt = delay;
                //transform.localScale = new Vector3(0.9F, 0.9f, 1);
            }
            else if (anim == "down")
            {
                float dtm = (rate / 100) * rt.rect.height * (Display.main.systemHeight / 1080);
                pr = (int) Mathf.Round(Display.main.systemHeight / 1080);
                wt = delay / dtm;
                apos = bpos - dtm;
            }
            else if (anim == "up")
            {
                float dtm = (rate / 100) * rt.rect.height * (Display.main.systemHeight / 1080);
                pr = (int)Mathf.Round(Display.main.systemHeight / 1080);
                wt = delay / dtm;
                apos = bpos + dtm;
            }
        }
    }

    public void MouseExited()
    {
        mouseon = false;

        ascl = 1;
        apos = bpos;
    }

    // Update is called once per frame
    void Update()
    {
        if ((Time.realtimeSinceStartup-elt)>wt)
        {
            elt = Time.realtimeSinceStartup;

            if (Mathf.Abs(scl-ascl)<0.01)
            {
                scl = ascl;
            } else
            {
                if (scl < ascl)
                {
                    scl += 0.01f;
                }
                else
                {
                    scl -= 0.01f;
                }

                transform.localScale = new Vector3(scl, scl, scl);
            }

            if (Mathf.Abs(pos - apos) < 1)
            {
                transform.position = new Vector3(transform.position.x, apos, transform.position.z);
                pos = apos;
            }
            else
            {
                if (pos < apos)
                {
                    pos += pr;
                }
                else
                {
                    pos -= pr;
                }

                transform.position = new Vector3(transform.position.x, pos, transform.position.z);
            }
        }
    }
}
