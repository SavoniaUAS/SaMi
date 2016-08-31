OPCUAPlugin

OpcUAPluginSettings is parsed using DataContract serializer which requires that setting items are in certain order so DO NOT change the order of the items in SampleSettingsFile.xml!

Note that OPC UA code saves a self created certificate so the first time this plugin is ran the executing user 
must have enough rights!

OPC UA sources:

Programming an OPC UA .NET Client with C# for the SIMATIC NET OPC UA Server
https://support.industry.siemens.com/cs/document/42014088/programming-an-opc-ua-.net-client-with-c%23-for-the-simatic-net-opc-ua-server?dti=0&lc=en-WW


This project uses dlls:
Opc.Ua.Client
Opc.Ua.Core
Siemens.OpcUA

Dlls are from 
Code Executable OPC UA Clients, commented source code (simple & advanced), complete associated STEP 7 V13 project
https://support.industry.siemens.com/cs/attachments/42014088/42014088_OPC_UAClient_CODE_V1_1.zip


Simple OPC UA Client app is used as a reference for this plugin. Following help from 42014088_opc_uaclient_doku_v1_1_e.pdf page 11.

1.	Server URL. For the SIMATIC NET OPC	Server this is composed of opc.tcp://<computername>:4845.
	In the Namespace URI: the namespace used is indicated. 
		This is 
		- S7: for direct addressing, 
		- S7COM: for direct addressing via the OPC DA compatible Syntax and
		- SYM: for symbolic addressing.
2.	In the text boxes for the Tag Identifier the identification code of the NodeID is indicated. For
	namespace S7: this is composed of <S7connection>.<dataarea>.<offset>,<datatype>
	The NodeID for reading and writing is made up of identification and namespace.
3.	Via the Connect and Disconnect buttons, the connection to the OPC server can be established
	or disconnected. The connection is only established without security. Secure connection
	establishment is explained in the next example.
4.	A subscription is created via the Monitor button and two Monitored Items are created in the
	Subscription with both NodeIDs. The data changes are displayed in the text boxes next to the
	button. Errors are displayed instead of the values.
5.	The Read button reads the values (Attribute value) of both tags with the specified NodeIDs and
	displays them in the text boxes next to the button.
6.	The Write button writes the value from the text box next to the button onto the tag identified by
	the NodeID.
	In order to write, “read” has to be called first since the text from the text box has to be converted
	in the data type suitable for the tag. The conversion is on the basis of the data type which is
	supplied at “read”.
7.	In the “Block Read” group, data can be received which is actively sent by the S7 with the
	BSEND block service. This can be, for example, used for the sending of result data from the S7
	to a PC application.
8.	In the “Block Write” group, data blocks can be sent to the S7 which are there received by the
	BRECV block service. Two blocks with different contents can be sent. This can be used, for
	example, for the download of recipe data for the S7.