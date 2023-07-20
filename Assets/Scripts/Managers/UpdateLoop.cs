using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateLoop : MonoBehaviour
{
    // https://forum.unity.com/threads/writing-update-manager-what-should-i-know.402571/

    private static HashSet<IEarlyUpdate> earlyUpdateSet = new HashSet<IEarlyUpdate>();
    private static HashSet<IUpdate> updateSet = new HashSet<IUpdate>();
    private static HashSet<IPostUpdate> postUpdateSet = new HashSet<IPostUpdate>();
    private static HashSet<ILateUpdate> lateUpdateSet = new HashSet<ILateUpdate>();
    private static HashSet<IFinalUpdate> finalUpdateSet = new HashSet<IFinalUpdate>();
    private static HashSet<IFixedUpdate> fixedUpdateSet = new HashSet<IFixedUpdate>();

    public static void RegisterEarly(IEarlyUpdate obj)
    {
        if (obj == null) throw new System.ArgumentNullException();

        earlyUpdateSet.Add(obj);
    }
    public static void UnregisterEarly(IEarlyUpdate obj)
    {
        if (obj == null) throw new System.ArgumentNullException();

        earlyUpdateSet.Remove(obj);
    }

    public static void Register(IUpdate obj)
    {
        if (obj == null) throw new System.ArgumentNullException();

        updateSet.Add(obj);
    }
    public static void Unregister(IUpdate obj)
    {
        if (obj == null) throw new System.ArgumentNullException();

        updateSet.Remove(obj);
    }

    public static void RegisterPost(IPostUpdate obj)
    {
        if (obj == null) throw new System.ArgumentNullException();

        postUpdateSet.Add(obj);
    }
    public static void UnregisterPost(IPostUpdate obj)
    {
        if (obj == null) throw new System.ArgumentNullException();

        postUpdateSet.Remove(obj);
    }

    public static void RegisterLate(ILateUpdate obj)
    {
        if (obj == null) throw new System.ArgumentNullException();

        lateUpdateSet.Add(obj);
    }
    public static void UnregisterLate(ILateUpdate obj)
    {
        if (obj == null) throw new System.ArgumentNullException();

        lateUpdateSet.Remove(obj);
    }

    public static void RegisterFinal(IFinalUpdate obj)
    {
        if (obj == null) throw new System.ArgumentNullException();

        finalUpdateSet.Add(obj);
    }
    public static void UnregisterFinal(IFinalUpdate obj)
    {
        if (obj == null) throw new System.ArgumentNullException();

        finalUpdateSet.Remove(obj);
    }

    public static void RegisterFixed(IFixedUpdate obj)
    {
        if (obj == null) throw new System.ArgumentNullException();

        fixedUpdateSet.Add(obj);
    }
    public static void UnregisterFixed(IFixedUpdate obj)
    {
        if (obj == null) throw new System.ArgumentNullException();

        fixedUpdateSet.Remove(obj);
    }

    void Update()
    {
        // Avoids GC from foreach loops

        var earlyUpdate = earlyUpdateSet.GetEnumerator(); 
        while (earlyUpdate.MoveNext())
            earlyUpdate.Current.OnEarlyUpdate();

        var update = updateSet.GetEnumerator();
        while (update.MoveNext())
            update.Current.OnUpdate();

        var postUpdate = postUpdateSet.GetEnumerator();
        while (postUpdate.MoveNext())
            postUpdate.Current.OnPostUpdate();
    }

    private void LateUpdate()
    {
        var lateUpdate = lateUpdateSet.GetEnumerator();
        while (lateUpdate.MoveNext())
            lateUpdate.Current.OnLateUpdate();

        var finalUpdate = finalUpdateSet.GetEnumerator();
        while (finalUpdate.MoveNext())
            finalUpdate.Current.OnFinalUpdate();
    }

    private void FixedUpdate()
    {
        var fixedUpdate = fixedUpdateSet.GetEnumerator();
        while (fixedUpdate.MoveNext())
            fixedUpdate.Current.OnFixedUpdate();
    }
}

public interface IEarlyUpdate
{
    void OnEarlyUpdate();
}

public interface IUpdate
{
    void OnUpdate();
}

public interface IPostUpdate
{
    void OnPostUpdate();
}

public interface ILateUpdate
{
    void OnLateUpdate();
}

public interface IFinalUpdate
{
    void OnFinalUpdate();
}

public interface IFixedUpdate
{
    void OnFixedUpdate();
}
