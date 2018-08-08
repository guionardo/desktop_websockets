var inp = document.getElementById("send");
inp.setAttribute("disabled", true);

class GSWebSocket{
	constructor (){
		this.ws = null;
	}

	public function Open(){
		try{
			this.ws=new WebSocket("ws://localhost:65500")
		}
	}
}
var ws = new WebSocket("ws://localhost:80/chat");



ws.onopen = function (event) {
    inp.removeAttribute("disabled");
    ws.send("Here's some text that the server is urgently awaiting!");
    inp.onclick = function (t, ev) {
        let cmd = document.getElementById("sel").value;
        console.log(cmd);
    }
};

ws.onmessage = function (event) {
    console.log(event.data);
}

ws.onclose=function(event){
	console.log(event);
}


