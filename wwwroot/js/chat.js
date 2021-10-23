"use strict";

const connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();


//Disable send button until connection is established

document.getElementById("topicNameBtn").disabled = true;
document.getElementById("join").disabled = true;
document.getElementById("create").disabled = true; 
document.getElementById("showVotes").disabled = true;
document.getElementById("newRound").disabled = true;

connection.on("setTopic", function (message) {
    document.getElementById("topic").textContent = message;
});

connection.on("newRound", function () { 
    var elems = document.querySelectorAll(".score");

    [].forEach.call(elems, function (el) {
        el.textContent = "?";
        el.parentElement.classList.remove("voted");
    });
    document.querySelector('input[name="vote"]:checked').checked = false

});
 
connection.on("Joined", function (roomUsers) {

    for (let i = 0; i < roomUsers.length; i++) {
        var elementExists = document.getElementById(roomUsers[i].id)
        if (elementExists === null || elementExists === undefined) {
            let placing = document.createElement("div")
            placing.setAttribute("class", "col-6 col-sm-2");
            let div = document.createElement("div")
            div.setAttribute("class", "scrumplayer")
            let score = document.createElement("div")
            score.setAttribute("id", roomUsers[i].id)
            score.setAttribute("class", "score")
            score.textContent = "?";
            let playa = document.createElement("div")
            playa.setAttribute("class", "playername")
            playa.textContent = roomUsers[i].name;
            div.appendChild(score)
            div.appendChild(playa)
            placing.appendChild(div);
            document.getElementById("Users").appendChild(placing);
        }
    }
});

connection.on("showVotes", function (votes) {

    for (let i = 0; i < votes.length; i++) {
        document.getElementById(votes[i].userId).textContent = votes[i].score;
    }
    document.getElementById("messagesList").innerHTML = "";
    document.querySelector('input[name="vote"]:checked').checked = false

});

connection.on("voted", function (user) {
    document.getElementById(user).parentElement.classList.add("voted");
});
connection.on("voteMessage", function (user, message) {
    const msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    const encodedMsg = user + "  " + msg;
    const li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").insertBefore(li, document.getElementById("messagesList").firstChild);
});

connection.start().then(function () {

    document.getElementById("join").disabled = false;
    document.getElementById("create").disabled = false;
 
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("topicNameBtn").addEventListener("click", function (event) {
    const message = document.getElementById("topicName").value; 
    connection.invoke("Settopic", message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

document.getElementById("showVotes").addEventListener("click", function (event) { 
    connection.invoke("showVotes").catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});
document.getElementById("newRound").addEventListener("click", function (event) { 
    connection.invoke("newRound").catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
    document.querySelector('input[name="vote"]:checked').checked = false;
});
const checkboxElems = document.querySelectorAll("input[name = 'vote']");

for (let i = 0; i < checkboxElems.length; i++) {
    checkboxElems[i].addEventListener("click", function (event) {  
        const vote = document.querySelector('input[name="vote"]:checked').value;
        connection.invoke("Vote", vote).catch(function (err) {
            return console.error(err.toString());
        });
    });
}

 
document.getElementById("join").addEventListener("click", function (event) {
    var userName = document.getElementById("first_name").value;
    if (userName.length < 3 || userName.length > 10) {
        alert("user name needs to be between 3 and 10 characters long");
        return;
    }
    var room = document.getElementById("chatroom1").value;
    connection.invoke("RoomExists", room).catch(function (err) {
        return console.error(err.toString());
    });  
});
connection.on("RoomExists", function (room) {
    if (room.length > 0) {
        var userName = document.getElementById("first_name").value;
        connection.invoke("Join", userName, room).catch(function (err) {
            return console.error(err.toString());
        });
    }
    else {
        alert("Room does not exist")
    }
});

connection.on("enter", function (room) {
    document.getElementById("index").classList.add("d-none");
    document.getElementById("chat").classList.remove("d-none");
    document.getElementById("currentroom").textContent = room;
});
connection.on("admin", function () {
    var elems = document.querySelectorAll(".admin");

    [].forEach.call(elems, function (el) {
        el.classList.remove("d-none");
    });
    document.getElementById("topicNameBtn").disabled = false;
    document.getElementById("showVotes").disabled = false;
    document.getElementById("newRound").disabled = false;
    

});

document.getElementById("create").addEventListener("click", function (event) {
    event.preventDefault();
    var userName = document.getElementById("first_name1").value;

    if (userName.length < 3 || userName.length > 10) {
        alert("user name needs to be between 3 and 10 characters long");
        return;
    }
    connection.invoke("Join", userName, "").catch(function (err) {
        return console.error(err.toString());
    }); 
});

 