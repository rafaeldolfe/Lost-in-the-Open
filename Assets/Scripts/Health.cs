using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int startingHealth;
    private int health;

    // Start is called before the first frame update
    void Start()
    {
        this.health = startingHealth;
    }
}
