using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class AddressablesExample : MonoBehaviour
{

    public AssetReference assetReference;
    // Start is called before the first frame update
    void Start()
    {
        assetReference.InstantiateAsync();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
