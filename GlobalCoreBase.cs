using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class GlobalCoreBase : MonoBehaviour
{

    /*
     * Overrides
     */
    public abstract void init();
    public abstract void update();
    public abstract void shutdown();

    /*
     * Mono Behavior Inherits
     */

    private void Start()
    {
        //Assign static variable
        managerObject = this;

        //Call abstract start method
        init();
    }

    private void Update()
    {
        //Call abstract method for derived class
        update();
    }

    private void OnApplicationQuit()
    {
        //Call abstract method for derived class
        shutdown();
    }

    /*
    * Internal Variables
    */

    protected List<Manager> managers = new List<Manager>();

    /*
     * Internal Functions
     */

    //Instantiate manager of type
    public void addManager(Manager m, params System.Object[] parameters)
    {        
        //Init manager
        m.init(parameters);

        //Add to list for updating
        managers.Add(m);
        
    }

    //Iterate managers and update them
    public void updateManagers()
    {
        foreach (Manager m in managers)
        {
            m.update();
        }
    }

    //Iterate managers and inform of shutdown
    public void shutdownManagers()
    {
        foreach (Manager m in managers)
        {
            m.shutdown();
        }
    }

    /*
     * Static Access
     */

    private static GlobalCoreBase managerObject;
    
    //Get manager in client form
    public static ClientCore GetClientManager()
    {
        return (ClientCore) managerObject;
    }


}
