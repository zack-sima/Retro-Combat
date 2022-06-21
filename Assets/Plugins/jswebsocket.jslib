mergeInto(LibraryManager.library, {
	WebSocketInit: function(uri) {
		uri = Pointer_stringify(uri);
		console.log(uri);
		this.socket = new WebSocket(uri);

		this.socket.onmessage = function(event) {
			//console.log(event.data);
			//NOTE: WebGLMaster is name of GAMEOBJECT INSIDE EDITOR... must match to send data
			unityGame.SendMessage("WebGLMaster", "ReceivedWebsocket", event.data);
		}
		this.socket.onopen = function(event) {
			unityGame.SendMessage("WebGLMaster", "OnConnected", event.data);
		}
	},
	WebSocketClose: function(message) {
		this.socket.close();
	},
	WebSocketSend: function(message) {
		//console.log(Pointer_stringify(message));
		this.socket.send(Pointer_stringify(message));
	},
});