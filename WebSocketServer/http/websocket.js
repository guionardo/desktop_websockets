var inp = document.getElementById("send");
var sel = document.getElementById('sel');
var retorno = document.getElementById('retorno');
var ws;
var conectado = false;

inp.setAttribute("disabled", true);

conectar();

function conectar() {
    if (conectado)
        return;
    try {
        addRetorno("Conectando...");
        var ws = new WebSocket("ws://localhost:80/chat");        
        ws.onopen = function (event) {
            addRetorno("Conectado...");
            conectado = true;
            inp.removeAttribute("disabled");
            ws.send("Here's some text that the server is urgently awaiting!");
            inp.onclick = function (t, ev) {
                let cmd = document.getElementById("sel").value;
                addRetorno("Enviando: " + cmd);
                ws.send(cmd);
            }
        };

        ws.onmessage = function (event) {
            addRetorno(event.data);
        }

        ws.onclose = function (event) {
            inp.setAttribute("disabled", true);
            addRetorno("Desconectado");
            conectado = false;
            setInterval(conectar, 5000);
        }

        ws.onerror = function (event) {
            addRetorno("ERRO: " + event);
        }

    } catch (err) {
        addRetorno("ERRO:" + err.message);
    }
}








function addRetorno(texto) {
    retorno.textContent = retorno.textContent + "\n" + texto;
}