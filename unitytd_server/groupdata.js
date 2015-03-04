//Group Data

//key = string
//group = Group
//data = anything
function GroupData(key, group, data)
{
	this.key = key;
	this.group = group;
	this.permissions = ""; //TODO: Permissions
	this.data = data;
}

module.exports = GroupData;