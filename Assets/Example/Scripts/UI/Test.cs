using System.Reflection;
using Astraia;
using Runtime;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

[UIMask(1)]
public class Test : MonoBehaviour
{
    [Button]
    void Start()
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        var type = GetType();
        UIMaskAttribute attr = null;
        for (int i = 0; i < 100000; i++)
        {
            attr = Attribute<UIMaskAttribute>.GetAttribute(type, false);
            //   CustomAttributeExtensions.GetCustomAttribute<UIMaskAttribute>(type);
        }

        Debug.Log(attr!.layerMask);
        watch.Stop();
        Debug.Log(watch.ElapsedMilliseconds);

        watch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < 100000; i++)
        {
            attr = TypeExtensions.GetAttribute<UIMaskAttribute>(type, false);
        }

        Debug.Log(attr!.layerMask);
        watch.Stop();
        Debug.Log(watch.ElapsedMilliseconds);
    }

    // Update is called once per frame
    void Update()
    {
    }
}