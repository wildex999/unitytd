//Cache the results from Resources.Load

using System;
using System.Collections.Generic;
public class ResourceCache<T>
{
    private Dictionary<string, T> cache = new Dictionary<string, T>();

    //Add resource to cache, returns false if unable to add(name already exists)
    public bool setResource(string name, T resource)
    {
        try
        {
            cache.Add(name, resource);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public T getResource(string name)
    {
        T resource;
        cache.TryGetValue(name, out resource);
        return resource;
    }


}