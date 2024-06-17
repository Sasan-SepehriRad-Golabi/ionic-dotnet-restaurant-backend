"use strict";

var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJkZXZlbG9wZXJAZ21haWwuY29tIiwianRpIjoiMjA1MDdlZjEtYjNmZi00NjcwLTgzZDMtOWNkNjlkMThhOTY4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiJlOTIyNjc4ZS0yNjU2LTQ1YjQtYmRhZi0yMGU5N2UxMTVkNmYiLCJFbWFpbCI6ImRldmVsb3BlckBnbWFpbC5jb20iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJPd25lciIsInVuaXF1ZV9uYW1lIjoiZGV2ZWxvcGVyQGdtYWlsLmNvbSIsImV4cCI6MTYxODU3OTY0MCwiaXNzIjoiUnVkZHkiLCJhdWQiOiJSdWRkeSJ9.C0e2DXKM0FTYed9EPlIpT92t9ulDPCURZw4p6I030Ww";
var connection = new signalR.HubConnectionBuilder().withUrl("https://ruddywebapi.azurewebsites.net/orderhub", { skipNegotiation: true, transport: signalR.HttpTransportType.WebSockets, accessTokenFactory: () => token }).configureLogging(signalR.LogLevel.Information).build();
//Disable send button until connection is established
document.getElementById("sendButton").disabled = true;

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

connection.on("ReceiveOrder", function () {
    /*var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var encodedMsg = user + " says " + msg;
    var li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);*/
});


connection.on("CancelOrder", function () {
    /*var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var encodedMsg = user + " says " + msg;
    var li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);*/
});

document.getElementById("connectButton").addEventListener("click", function (event) {
    connection.invoke("ConnectToGroups").catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});