using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class PortraitManager : MonoBehaviour
{
    private GlobalEventManager gem;
    private CameraManager cm;

    public SpriteRenderer portaitImage;
    public TextMeshPro portaitName;
    public GameObject healthBarFrame;
    public GameObject healthBar;
    public TextMeshPro healthText;
    public GameObject resourceBarFrame;
    public GameObject resourceBar;
    public TextMeshPro resourceText;

    private Portrait portraitData;

    void Awake()
    {
        gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager;
        cm = FindObjectOfType(typeof(CameraManager)) as CameraManager;
        if (gem == null || cm == null)
        {
            List<MonoBehaviour> deps = new List<MonoBehaviour> { gem, cm };
            List<Type> depTypes = new List<Type> { typeof(GlobalEventManager), typeof(CameraManager) };
            throw ProgramUtils.DependencyException(deps, depTypes);
        }
    }

    void Start()
    {
        cm.AttachToCamera(gameObject); 
        gem.StartListening("SetPortrait", SetPortrait);
        gem.StartListening("ResetPortrait", ResetPortrait);
        gem.StartListening("TakeDamage", UpdateHealth);
    }
    void OnDestroy()
    {
        gem.StopListening("SetPortrait", SetPortrait);
        gem.StopListening("ResetPortrait", ResetPortrait);
        gem.StopListening("TakeDamage", UpdateHealth);
    }

    private void UpdateHealth(GameObject invoker, List<object> parameters, int x, int z, int tx, int tz)
    {
        if (portraitData == null || portraitData.go.GetComponent<Actor>() == null)
        {
            return;
        }
        Position pos = portraitData.go.GetComponent<Position>();
        if (pos.x != tx || pos.z != tz)
        {
            return;
        }
        Health hp = portraitData.go.GetComponent<Health>();
        portraitData.health = hp.health;
        portraitData.maxHealth = hp.maxHealth;
        UpdatePortrait();
    }
    private void SetPortrait(GameObject invoker, List<object> parameters, int x, int z, int tx, int tz)
    {
        if (parameters.Count < 2)
        {
            throw new System.Exception("Expected list with name, image, etc., found list with " + parameters.Count + " elements");
        }
        if (parameters.Count > 7)
        {
            throw new System.Exception("Expected list with less than 7 parameters");
        }
        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
        portraitData = new Portrait(invoker);
        portraitData.LoadData(parameters);
        UpdatePortrait();
    }
    private void ResetPortrait(GameObject invoker, List<object> parameters, int x, int z, int tx, int tz)
    {
        if (parameters.Count != 0)
        {
            throw new System.Exception("Expected empty list, found " + parameters.Count + "elements");
        }
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        portraitData = new Portrait();
        UpdatePortrait();
    }
    private void UpdatePortrait()
    {
        if (portraitData == null)
        { 
            return;
        }
        portaitImage.sprite = portraitData.image;
        portaitName.text = portraitData.name;
        healthText.text = "??/??";
        resourceText.text = "";
        healthBarFrame.SetActive(false);
        resourceBarFrame.SetActive(false);
        healthBar.transform.localScale = new Vector3(0, healthBar.transform.localScale.y, healthBar.transform.localScale.z);
        resourceBar.transform.localScale = new Vector3(0, resourceBar.transform.localScale.y, resourceBar.transform.localScale.z);

        if (portraitData.maxHealth != null && portraitData.health != null)
        {
            healthBarFrame.SetActive(true);
            healthText.text = string.Format("{0}/{1}", portraitData.health, portraitData.maxHealth);
            healthBar.transform.localScale = new Vector3((float) portraitData.health / (float) portraitData.maxHealth, healthBar.transform.localScale.y, healthBar.transform.localScale.z);
        }
        if (portraitData.maxResource != null && portraitData.resource != null)
        {
            resourceBarFrame.SetActive(true);
            resourceText.text = string.Format("{0}/{1}", portraitData.resource, portraitData.maxResource);
            resourceBar.transform.localScale = new Vector3((float) portraitData.resource / (float) portraitData.maxResource, resourceBar.transform.localScale.y, resourceBar.transform.localScale.z);
        }
    }
    public GameObject GetAttachable()
    {
        return gameObject;
    }

    public class Portrait
    {
        public string name;
        public Sprite image;

        public Nullable<int> maxHealth;
        public Nullable<int> health;

        public Color? color;
        public Nullable<int> maxResource;
        public Nullable<int> resource;
        
        public GameObject go;

        public Portrait(GameObject go)
        {
            this.go = go;
        }
        public Portrait() { }
        public void LoadData(List<object> data)
        {
            if (data.Count >= 2)
            {
                SetNameAndImage(data[0] as string, data[1] as Sprite);
            }
            if (data.Count >= 4)
            {
                SetHealth(data[2] as int?, data[3] as int?);
            }
            if (data.Count >= 7)
            {
                SetPrimaryResource(data[4] as Color?, data[5] as int?, data[6] as int?);
            }
        }
        void SetNameAndImage(string name, Sprite image)
        {
            this.name = name;
            this.image = image;
        }
        void SetHealth(int? maxHealth, int? health)
        {
            this.health = health;
            this.maxHealth = maxHealth;
        }
        void SetPrimaryResource(Color? color, int? maxResource, int? resource)
        {
            this.color = color;
            this.maxResource = maxResource;
            this.resource = resource;
        }
    }
}
