
"use strict";

$(document).ready(() => {

    let $processBody = $("#processBody");
    let processes = [];

    const client = new signalR.HubConnectionBuilder()
        .withUrl("/Processes")
        .build();

    client.on("BookIsAccepted", (userId, newProcessView) => {
        addProcess(newProcessView);
    });

    client.on("BookIsReturned", (userId, newProcess) => {
        console.log("Primljena poruka ", newProcess);
        deleteProcess(newProcess);
    });

    function deleteProcess(process) {

        let name = process.book.name;

        let row = document.getElementById("row" + name);
        let btn = document.getElementById("btn" + name);

        row.remove();
        btn.remove();
    }

    function addProcesses() {
        $processBody.empty();
        $.each(processes, (i, c) => addProcess(c));
    }

    function addProcess(processView) {

        let btn = document.createElement("button");
        btn.className = `btn btn-primary`;
        btn.id = `btn${processView.process.book.name}`;

        if (processView.pendingForReturn == false) {
            btn.innerHTML = "Return the book";
            btn.onclick = function () {
                window.location = "https://localhost:7097/Reservation/ReturnBook?bookId=" + processView.process.bookId;
            }
        }
        else {
            btn.innerHTML = "Pending for return";
            btn.style.pointerEvents = "none";
            btn.style.backgroundColor = "red";
        }

        let reservationTime = getDate(processView.process.time);
        let returnDeadline = getDate(processView.process.returnDeadline);

        let template = `<tr id="row${processView.process.book.name}">
                          <td>${processView.process.book.name}</td>
                          <td>${processView.process.book.author}</td>
                          <td>${reservationTime}</td>
                          <td>${returnDeadline}</td>
                        </tr>`;

        $processBody.append($(template)).append($(btn));
    }

    function getDate(processTime) {

        let date = new Date(processTime);
        let day = date.getDate();
        let month = date.getMonth() + 1;
        let year = date.getFullYear();
        let hour = date.getHours();
        let minute = date.getMinutes();
        let second = date.getSeconds();

        let time = day + "-" + month + "-" + year + " " + hour + ':' + minute + ':' + second;

        return time;
    }

    function fulfilled() {
        console.log("[User Reservations] Connection to Hub Successful");
    }

    function rejected() {
        console.log("[User Reservations] Connection to Hub was not Successful");
    }

    function getProcesses() {
        $.getJSON("/api/processes/userReservations")
            .then(res => {
                processes = res;
                addProcesses();
                client.start().then(fulfilled, rejected);
            })
            .catch(() => {
                $theWarning.text("Failed to get calls...");
                $theWarning.show();
            });
    }

    getProcesses();
});