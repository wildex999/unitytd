var net = require('net');


var server = net.createServer(function(c) {
	
	console.log('Client connected: ' + c.remoteAddress);
	
	c.on('data', function(data) {
	
		//Check if we have anough data
		
		
	
		if('<policy-file-request/>\0' == data.toString())
		{
			c.end('<?xml version="1.0"?>\
				<!DOCTYPE cross-domain-policy SYSTEM "http://www.adobe.com/xml/dtds/cross-domain-policy.dtd">\
				<cross-domain-policy>\
				<site-control permitted-cross-domain-policies="master-only"/>\
				<allow-access-from domain="*" to-ports="*"/>\
				</cross-domain-policy>\0');
			console.log('Wrote policy file');
		}
	});
		
});

server.listen(843, function() {
	console.log('Policy Server listening');
});