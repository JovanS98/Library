"use strict";

$(document).ready(() => {

    const client = new signalR.HubConnectionBuilder()
        .withUrl("/Processes")
        .build();

    client.on("BookIsAccepted", (userId, processView) => {
        document.getElementById(`${processView.process.book.name}`).innerHTML = "The book is successfully reserved";
    });

    client.on("BookIsReturned", (userId, processView) => {
        document.getElementById(`${processView.process.book.name}`).innerHTML = "The book is successfully returned";
    });

    function fulfilled() {
        console.log("[Library] Connection to Hub Successful");
    }

    function rejected() {
        console.log("[Library] Connection to Hub was not Successful");
    }

    client.start().then(fulfilled, rejected);

});