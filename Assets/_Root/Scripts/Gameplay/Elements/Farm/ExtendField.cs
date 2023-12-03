using System;
using System.Collections.Generic;
using System.Linq;
using Pancake;
using UnityEditor;
using UnityEngine;

public class ExtendField : GameComponent
{
    [SerializeField, UniqueID] private string uniqueId;
    [SerializeField] private ScriptableListInt fieldStateList;
    [SerializeField] private List<Field> fieldList = new List<Field>();
    [SerializeField] private List<ResourceConfig> resourceConfigList = new List<ResourceConfig>();

    private int canSeedCount;
    private int canWaterCount;
    private int canHarvestCount;
    private ResourceConfig resourceConfig;

    public Action OnStateChange;
    public Action<bool> OnHarvest;

    public ResourceConfig ResourceConfig => resourceConfig;

    public EnumPack.ResourceType ResourceType
    {
        get => Data.Load(uniqueId, EnumPack.ResourceType.None);

        set => Data.Save(uniqueId, value);
    }

    public EnumPack.FieldState TransferFarmState
    {
        get
        {
            if (canHarvestCount > 0) return EnumPack.FieldState.Harvestable;
            if (canSeedCount > 0) return EnumPack.FieldState.Seedale;
            if (canWaterCount > 0) return EnumPack.FieldState.Waterable;
            return EnumPack.FieldState.Seedale;            
        }
    }

    private void Start()
    {
        if (ResourceType != EnumPack.ResourceType.None)
        {
            Initialize();
            InitCount();
        }
    }

    public void Initialize()
    {
        foreach (var resource in resourceConfigList)
        {
            if (ResourceType == resource.resourceType)
            {
                resourceConfig = resource;
                break;
            }
        }

        foreach (var field in fieldList)
        {
            field.Initialize(this, resourceConfig);
        }
    }

    private void InitCount()
    {
        foreach (var field in fieldList)
        {
            switch (field.FieldState)
            {
                case EnumPack.FieldState.Seedale:
                    canSeedCount++;
                    break;
                case EnumPack.FieldState.Waterable:
                    canWaterCount++;
                    break;
                case EnumPack.FieldState.Harvestable:
                    canHarvestCount++;
                    break;
            }
        }
    }

    public void GenerateRandomResource()
    {
        ResourceType = resourceConfigList[UnityEngine.Random.Range(0, resourceConfigList.Count)].resourceType;
        Initialize();
    }

    private void CalculateExtendFieldState()
    {
        fieldStateList.Reset();

        if (canSeedCount > 0) fieldStateList.Add((int)EnumPack.FieldState.Seedale);
        if (canWaterCount > 0) fieldStateList.Add((int)EnumPack.FieldState.Waterable);
        if (canHarvestCount > 0) fieldStateList.Add((int)EnumPack.FieldState.Harvestable);
    }

    public List<Field> SortingFieldListByDis(Vector3 pos, EnumPack.FieldState fieldState)
    {
        List<Field> newList = new List<Field>();
        foreach (var field in fieldList)
        {
            if (field.FieldState == fieldState)
            {
                newList.Add(field);
            }
        }

        newList.Sort((a, b) => SimpleMath.SqrDist(pos, a.transform.position).CompareTo(SimpleMath.SqrDist(pos, b.transform.position)));
        return newList;
    }

    public Field GetNearestFieldWithState(Vector3 pos, EnumPack.FieldState fieldState)
    {
        Field targetField = null;
        var minDistance = 10000f;

        foreach (var field in fieldList)
        {
            if (field.FieldState == fieldState)
            {
                var distance = SimpleMath.SqrDist(pos, field.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    targetField = field;
                }
            }
        }

        return targetField;
    }

    public void DoSeed()
    {
        CheckChangeState(ref canSeedCount, ref canWaterCount);
        if (canSeedCount == 0) OnStateChange?.Invoke();
    }

    public void DoWater()
    {
        CheckChangeState(ref canWaterCount, ref canHarvestCount);
        if (canWaterCount == 0) OnStateChange?.Invoke();
    }

    public void DoHarvest()
    {
        CheckChangeState(ref canHarvestCount, ref canSeedCount);
        OnHarvest?.Invoke(canHarvestCount == 0);
        // if (canHarvestCount == 0) OnStateChange?.Invoke();
    }

    private void CheckChangeState(ref int previousStateCount, ref int newStateCount)
    {
        previousStateCount--;
        newStateCount++;
    }

    public bool IsSeeded()
    {
        foreach (var field in fieldList)
        {
            if (field.FieldState != EnumPack.FieldState.Seedale)
            {
                return true;
            }
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        CalculateExtendFieldState();
        if (other.TryGetComponent<IFarmer>(out var farmer)) farmer.TriggerActionFarm(gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IFarmer>(out var farmer)) farmer.ExitTriggerActionFarm();
    }

#if UNITY_EDITOR
    [ContextMenu("Reset Unique ID")]
    public void ResetUniqueID()
    {
        Guid guid = Guid.NewGuid();
        uniqueId = guid.ToString();
    }

    [ContextMenu("Get Fields")]
    public void GetFields()
    {
        fieldList = GetComponentsInChildren<Field>().ToList();
        EditorUtility.SetDirty(this);
    }

    [ContextMenu("Get Farm Resources")]
    public void GetFarmResources()
    {
        const string resourcesFolderPath = "Assets/_Root/ScriptableData/Resources/FarmResources";

        var resourcePaths = AssetDatabase.FindAssets("t:ResourceConfig", new string[] { resourcesFolderPath });

        var resourceConfigs = new ResourceConfig[resourcePaths.Length];

        for (var i = 0; i < resourcePaths.Length; i++)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(resourcePaths[i]);
            resourceConfigs[i] = AssetDatabase.LoadAssetAtPath<ResourceConfig>(assetPath);
        }

        resourceConfigList = resourceConfigs.ToList();
        EditorUtility.SetDirty(this);
    }

    [ContextMenu("Setup Extend Field")]
    public void SetupExtendField()
    {
        GetFields();
        GetFarmResources();
        ResetUniqueID();
        EditorUtility.SetDirty(this);
    }
#endif
}