function serverCmdSetNickName(%client,%a0,%a1,%a2,%a3,%a4,%a5,%a6,%a7,%a8,%a9,%a10,%a11,%a12,%a13,%a14,%a15)
{
	%name = "";
	for(%i = 0; %i < 16; %i++)
	{
		%name = %name SPC %a[%i];
	}
	%name = trim(%name);

	%client.NickNames_Set(%name);
}

function GameConnection::NickNames_Set(%client,%name)
{
	%client.Nicknames_name = %name;
	if(%name !$= "")
	{
		%client.chatMessage("\c6Your new nickname: \c3" @ %name);
	}
	else
	{
		%client.chatMessage("\c6Your nickname has been removed");
	}
	
	%client.NickNames_UpdateName();
}

function GameConnection::NickNames_UpdateName(%client)
{
	%name = %client.getPlayerName();

	//update shape name
	%player = %client.player;
	if(isObject(%player))
	{
		%player.setShapeName (StripMLcontrolChars(%name), 8564862);
	}

	//update player list
	%group = ClientGroup;
	%count = %group.getCount ();
	for(%i = 0; %i < %count; %i++)
	{
		%currClient = %group.getObject(%i);
		%currClient.sendPlayerListUpdate();
	}
}

//overwrite to allow this function to still work
function FindClientByName(%partialName)
{
	%pnLen = strlen (%partialName);
	%clientIndex = 0;
	%bestCL = -1;
	%bestPos = 9999;
	%clientIndex = 0;
	while (%clientIndex < ClientGroup.getCount ())
	{
		%cl = ClientGroup.getObject (%clientIndex);
		%pos = -1;
		%name = strlwr (%cl.name);
		%pos = strstr (%name, strlwr (%partialName));
		if (%pos != -1)
		{
			%bestCL = %cl;
			if (%pos == 0)
			{
				return %cl;
			}
			if (%pos < %bestPos)
			{
				%bestPos = %pos;
				%bestCL = %cl;
			}
		}
		%clientIndex += 1;
	}
	if (%bestCL != -1)
	{
		return %bestCL;
	}
	else 
	{
		return 0;
	}
}

package NicknamePackage
{
	function GameConnection::getPlayerName(%client)
	{
		%name = %client.Nicknames_name;
		if(%name !$= "")
		{
			return %name;
		}

		return Parent::getPlayerName(%client);
	}

	function GameConnection::SpawnPlayer(%client)
	{
		%r = parent::SpawnPlayer(%client);
		%client.NickNames_UpdateName();
		return %r;
	}

	function GameConnection::onClientEnterGame(%this)
	{
		//update names for all players so the newly connected player recieves it
		%group = ClientGroup;
		%count = %group.getCount ();
		for(%i = 0; %i < %count; %i++)
		{
			%currClient = %group.getObject(%i);
			%currClient.NickNames_UpdateName();
		}

		parent::onClientEnterGame(%this);
	}
};
activatepackage(NicknamePackage);