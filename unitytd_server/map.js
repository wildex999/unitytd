
Map.customMapId = 0; //The id of a map which does not exist in the system(Unpublished)

function Map(name, id, version)
{
	this.name = name;
	this.id = id;
	this.version = version;
}

module.exports = Map;