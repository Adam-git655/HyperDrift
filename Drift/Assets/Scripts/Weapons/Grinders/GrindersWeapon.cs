using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrindersWeapon : Weapon
{
    public GameObject grinderPrefab;

    private Transform car;
    private GrindersWeaponData grindersWeaponData;

    private float speed;
    private int numGrinders;

    private float angle;

    private readonly List<Transform> grinders = new();

    protected override void Awake()
    {
        base.Awake();
        car = transform.parent;
        grindersWeaponData = (GrindersWeaponData)data;
        transform.SetParent(null);

        Vector3 dir = transform.position - car.position;
        angle = Mathf.Atan2(dir.y, dir.x);

        transform.localScale = new Vector3(grindersWeaponData.size, grindersWeaponData.size);
        speed = grindersWeaponData.speed;
        numGrinders = grindersWeaponData.numGrinders;

        SpawnGrinders();
    }

    private void SpawnGrinders()
    {
        foreach (Transform grinder in grinders)
            Destroy(grinder.gameObject);

        grinders.Clear();

        float spacing = Mathf.PI * 2f / numGrinders;

        for (int i = 0; i < numGrinders; i++)
        {
            GameObject grinder = Instantiate(grinderPrefab, transform.position, Quaternion.identity, transform);

            grinder.transform.localScale = Vector3.one * grindersWeaponData.size;
            grinder.GetComponent<Grinder>().damage = damage;
            grinder.GetComponent<Grinder>().rotationSpeed = 1200;
            grinders.Add(grinder.transform);

            //angle
            float a = angle + spacing * i;
            grinder.transform.position = car.position + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * grindersWeaponData.radius;
        }
    }

    protected override void Update()
    {
        base.Update();

        angle += speed * Time.deltaTime;

        float spacing = Mathf.PI * 2f / grinders.Count;

        for (int i = 0; i < grinders.Count; i++)
        {
            float a = angle + spacing * i;

            float x = Mathf.Cos(a) * grindersWeaponData.radius;
            float y = Mathf.Sin(a) * grindersWeaponData.radius;

            grinders[i].position = car.position + new Vector3(x, y, 0f);
        }

    }

    protected override void Fire()
    {
        //do nothing
    }

    protected override void ApplyLevelUp(int level)
    {
        damage *= 1.2f;
        speed *= 1.2f;

        foreach (var grinder in grinders)
            grinder.GetComponent<Grinder>().damage = damage;

        if (level % 2 == 0)
        {
            numGrinders++;
            SpawnGrinders();
        }
    }
}
