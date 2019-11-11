# TcpChat
Simple, TCP based, two clients -> server, chat application written for the project at the Poznań University of Technology.  

Server application can serve multiple clients, but maximum number of participators in a single communication session is two.  

## Server application operation's tags:
| Tag | Operation |
| --- | --- |
| -h | open help menu | 
| -s | start listening  |
| -q | try to quit | 

## Client application operation's tags:  
| Tag | Operation |
| --- | --- |
|-h|														open help menu|  
|-cn [ip_address:port_number]| 	try to connect to server with given IPv4 address and listening on given port | 
|-id| 													get your session ID and other client's IDs | 
|-i [id]|												invite client with given ID to your session | 
|-a [id]|												accept invite to other session from client with given ID | 
|-d [id]|												decline invite to other session from client with given ID | 
|-dn|														disconnect from server | 
|-c|														close current session and move to new one (possible only if other client is in your session) | 
|-q|														quit (disconnect if connected) | 
