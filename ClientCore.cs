using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

//Attach to game object in scene. 
public class ClientCore : GlobalCoreBase
{

    /*
     * Override Methods
     */

    public override void init() {
        addManager(connectionManager);
		
		//EXAMPLE. Starts repeating function to send packet 1 to server.
		//In practice, put code in a manager and not in this base class.
        StartCoroutine(Test());
    }

	//EXAMPLE. Starts repeating function to send packet 1 to server.
	//In practice, put code in a manager and not in this base class.
    private IEnumerator Test()
    {
		//Wait 3 seconds before sending
        yield return new WaitForSeconds(3);

		//Send 100 messages 2 seconds apart.
        for (int i = 0; i < 100; i++)
        {
            yield return new WaitForSeconds(2);

			//Create packet instance
            ExamplePacket_1 packet = new ExamplePacket_1();
			//Assign packet data
            packet.index = 243756;
            packet.index2 = 1002;

			//Send packet
            connectionManager.sendPacketToServer(packet);
			
			//Server will receive the packet by subscribing to event in ExampleManager. There it will output the packet data into console

        }
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

    public ConnectionManager connectionManager = new ConnectionManager(false);

    /*
     * Internal Functions
     */

}
