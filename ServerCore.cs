using System;
using System.Collections;
using System.Collections.Generic;

//Attach to game object in scene. 
public class ServerCore : GlobalCoreBase
{

    /*
     * Override Methods
     */

    public override void init()
    {
        addManager(connectionManager);
        addManager(exampleManager, connectionManager);
    }

    public override void update()
    {
        updateManagers();
    }

    public override void shutdown()
    {
        shutdownManagers();
    }

    /*
     * Internal Variables
     */

    public ConnectionManager connectionManager = new ConnectionManager(true);
    public ExampleManager exampleManager = new ExampleManager();

    /*
     * Internal Functions
     */

}
