## Time synchronization

NetCode uses a server authoritative model, which means that the server executes a fixed time step based on how much time has passed since the last update. As such, the client needs to match the server time at all times for the model to work.

[NetworkTimeSystem](https://docs.unity3d.com/Packages/com.unity.netcode@latest/index.html?subfolder=/api/Unity.NetCode.NetworkTimeSystem.html) calculates which server time to present on the client. The network time system calculates an initial estimate of the server time based on the round trip time and latest received snapshot from the server. When the client receives an initial estimate, it makes small changes to the time progress rather than doing large changes to the current time. To make accurate adjustments, the server tracks how long it keeps commands in a buffer before it uses them. This is sent back to the client and the client adjusts its time so it receives commands just before it needs them.

The client sends commands to the server. The commands will arrive at some point in the future. When the server receives these commands, it uses them to run the game simulation. The client needs to estimate which tick this is going to happen on the server and present that, otherwise the client and server apply the inputs at different ticks. The tick the client estimates the server will apply the commands on is called the **prediction tick**. You should only use prediction time for a predicted object like the local player. 

For interpolated objects, the client should present them in a state it has received data for. This time is called **interpolation**, and Unity calculates it as a time offset from the prediction time. The offset is called **prediction delay** and Unity slowly adjusts it up and down in small increments to keep the interpolation time advancing at a smooth rate. Unity calculates the interpolation delay from round trip time and jitter so that the data is generally available. The delay adds additional time based on the network tick rate to make sure it can handle a packet being lost. You can visualize the time offsets and scales in this section in the graphs in the snapshot visualization tool, [NetDbg](ghost-snapshots#Snapshot-visualization-tool).

### Configuring clients interpolation
A __ClientTickRate__ singleton entity in the client World can be used to configure the interpolation times used by the client:
*__InterpolationTimeMS__ - if different than 0, override the interpolation time tick used to interpolate the ghosts. 
*__MaxExtrapolationTimeSimTicks__ - the maximum time in simulation ticks which the client can extrapolate ahead when data is missing.
It is possible to futher customize the client times calculation. Please read the [ClientTickRate](https://docs.unity3d.com/Packages/com.unity.netcode@latest/index.html?subfolder=/api/Unity.NetCode.ClientTickRate.html) documentation for more in depth information
