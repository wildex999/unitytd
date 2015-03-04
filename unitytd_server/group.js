var GroupData = require('./groupdata');

//Groups
/*
A group has an name that is unique within the parent group
A group is added to a HashTable with it's name
A group can have one parent, and multiple children
A member of a sub group is also a member of the parent group
A group has a number of members, each with a list of permissions for this and sub-groups(A sub group can add or subtract a permission for itself and subsequent child groups)
A group has data, identified by an name. This name must be unique for the group.(I.e a sub group can have different data with the same name)
*/

//name = string
//parent = Group or null
function GroupREMOVED(name, parent)
{
	this.name = name;
	this.members = {}; //Hashtable of members(Key = userId)
	this.subGroups = {}; //Hashtable of sub groups(Key = groupId)
	this.data = {}; //Hashtable, data for group
	
	//Add to parent
	if(parent != null)
		parent.addSubGroup(this);
}

Group.prototype.broadcast = function()
{
	//Send message to all members of group
	
}

//Add a member to this group
//user = User
//return: none
Group.prototype.addMember = function(user)
{
	if(user.id in this.members)
		return; //User already a member

	this.members[user.id] = user;
	user.memberGroups[this.name] = this;
	
	//Propagate the membership upwards
	//TODO: Mark the memberships as not direct somehow? When User leaves this group, should they also leave parent groups?
	if(this.parent != null)
		this.parent.addMember(user);
		
	//Broadcast joined message if enabled
}

//Remove a member from group
//user = User
//return: true if user war removed, false if not
Group.prototype.removeMember = function(user)
{
	//TODO: Delete is slow, try to just set undefined? Then have to use a different exists test in addMember('in' returns true if undefined)
	delete this.members[user.id];
	delete user.memberGroups[this.name];
	
	//Broadcast leave message if enabled
	
	return true;
}

//Returns the subgroup with the given id if found, or undefined if not.
//groupName = string
//return: Group or undefined
Group.prototype.getSubGroup = function(groupName)
{
	console.log("From " + this.name + " get sub group \"" + groupName + "\"");
	return this.subGroups[groupName];
}

//Adds a group as a sub group, returns false if failed.
//group = Group
//return: none
Group.prototype.addSubGroup = function(group)
{
	console.log("To " + this.name + " add sub group \"" + group.name + "\"");
	//TODO: If group already has a parent, remove from parent first
	if(group.name in this.subGroups)
		return false;
	
	this.subGroups[group.name] = group;
	group.parent = this;
	console.log("Added");
	
	return true;
}

//Return the data for a given key
//key = string
//return: data value or undefined
Group.prototype.getData = function(key)
{
	//TODO: Permission check
	return this.data[key].data;
}

//key = string
//return: GroupData or undefined
//Note: Changes done to the returned object is not written before setData is called with it.
Group.prototype.getDataInfo = function(key)
{
	//TODO: permission check
	return this.data[key];
}

//Set group data for given key
//data = GroupData
//return: none
Group.prototype.setData = function(data)
{
	//TODO: Permission check
	data.group = this;
	this.data[data.key] = data;
}

Group.prototype.createData = function(key, data)
{
	//TODO: Permissions
	var newData = new GroupData(key, this, data);
	this.data[key] = newData;
}

//Called whenever the group is serialized to storage
Group.prototype.onSave = function()
{
}

//Called whenever the group is loaded from storage
Group.prototype.onLoad = function()
{
}

module.exports = Group;
