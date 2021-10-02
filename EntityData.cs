using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using Newtonsoft.Json;

public class EntityData : MonoBehaviour
{
    public int entityID = 0;
    public string entityName = "";
    public int health = 100;
    public int maxHealth = 100;
    public int armour = 0;
    public int grenadeAmmo = 0;

    public GameObject damageEffect;

    public enum FACTIONS
    {
        Player,
        Enemy
    }

    public string faction = "enemy";

    private EntityProperties properties = new EntityProperties();
    private EnemyDestroy entity_destroy;
    private IEntityAudio IAudio;

    // Start is called before the first frame update
    void Start()
    {
        entity_destroy = GetComponent<EnemyDestroy>();
        IAudio = GetComponent<IEntityAudio>();
    }

    // Update is called once per frame

    public void TakeDamage(int _damage)
    {
        health -= _damage;
        if(damageEffect != null)
            Instantiate(damageEffect, transform.position, Quaternion.identity);
        if(health <= 0) {
            entity_destroy.InputDestroyByDamage();
        }
        else {
            IAudio.PlaySound("Hurt");
        }
    }
    public void AddHealth(int _h)
    {
        health += _h;
        if (health > maxHealth)
            health = maxHealth;
    }

    public int getEntityID()
    {
        return entityID;
    }

    public void SetEntityID(int ID)
    {
        entityID = ID;
    }

    public void SetFaction(string f)
    {
        faction = f;
    }

    public string GetFaction()
    {
        return faction;
    }

    public EntityProperties GetProperties()
    {
        properties.prefab = gameObject.name;
        properties.isActive = this.enabled;
        properties.health = health;
        properties.armour = armour;
        properties.grenadeAmmo = grenadeAmmo;
        properties.localPosition = transform.position;
        return properties;
    }

    public void SetProperties()
    {
        gameObject.name = properties.prefab;
        this.enabled = properties.isActive;
        health = properties.health;
        armour = properties.armour;
        grenadeAmmo = properties.grenadeAmmo;
        transform.position = properties.localPosition;
    }

    public void WriteJsonFile(string jsonFile)
    {
        string properties = JsonUtility.ToJson(GetProperties());
        File.WriteAllText(jsonFile, properties);
    }

    public void ReadJsonFile(string jsonFile)
    {
        string p = File.ReadAllText(jsonFile);
        properties = JsonUtility.FromJson<EntityProperties>(p);
        SetProperties();
    }

    public string WriteJson()
    {
        return JsonUtility.ToJson(GetProperties());
    }

    public void ReadJson(string json)
    {
        properties = JsonUtility.FromJson<EntityProperties>(json);
        SetProperties();
    }
}
