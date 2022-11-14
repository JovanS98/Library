
"use strict";

$(document).ready(() => {

    let $processBody = $("#processBody");
    let processes = [];

    const client = new signalR.HubConnectionBuilder()
        .withUrl("/Processes")
        .build();

    client.on("NewProcessReceived", (newProcess) => {
        console.log("Uspesno primljena poruka", newProcess);
        addProcess(newProcess);
    });

    function addProcesses() {
        $processBody.empty();
        $.each(processes, (i, c) => addProcess(c));
    }

    function addProcess(process) {

        let btn = document.createElement("button");
        btn.className = "btn btn-primary";

        if (process.status == 0) {
            btn.innerHTML = "Accept reservation";
            btn.onclick = function () {
                window.location = "https://localhost:7097/Reservation/AcceptReservation?userId=" + process.userId + "&bookId=" + process.bookId;
            }
        }
        else {
            btn.innerHTML = "Mark as returned";
            btn.onclick = function () {
                window.location = "https://localhost:7097/Reservation/MarkReturnedBook?userId=" + process.userId + "&bookId=" + process.bookId;
            }
        }

        let time = getDate(process.time);
        
        let template = `<tr>
                          <td>${process.user.firstName}</td>
                          <td>${process.user.lastName}</td>
                          <td>${process.book.name}</td>
                          <td>${time}</td>
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
        console.log("[PendingReservations] Connection to Hub Successful");
    }

    function rejected() {
        console.log("[PendingReservations] Connection to Hub was not Successful");
    }

    function getProcesses() {
        $.getJSON("/api/processes/pendingReservations")
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