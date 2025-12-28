using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SalvageCore : Currency
{
    protected override void OnPickUp()
    {
        player.GetComponent<Car>().salvageCores += 1;
        Destroy(gameObject);
    }
}
