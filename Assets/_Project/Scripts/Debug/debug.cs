using UnityEngine;

public class debug : MonoBehaviour
{
    public bool isDebug;
    public PlayerHP hp;
    public int healAmount =1;
    public int damageAmount=1;
    public PlayerManager pmanager;
    public int getMoneyAmount =111;
    public int payAmount = 111;

   void Update()
    {
        if(!isDebug)return;

        if(Input.GetKeyDown("h"))
        {
            hp.Heal(healAmount);
        }
        
        if (Input.GetKeyDown("d"))
        {
            hp.TakeDamage(damageAmount);
        }

        if (Input.GetKeyDown("g"))
        {
            pmanager.AddMoney(getMoneyAmount);
        }

        if (Input.GetKeyDown("b"))
        {
            pmanager.TrySpendMoney(payAmount);
        }
    }
}
