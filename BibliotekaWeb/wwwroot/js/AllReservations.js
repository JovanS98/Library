
"use strict";

$(document).ready(() => {

    let $reservationsBody = $("#reservationsBody");
    let processes = [];

    const client = new signalR.HubConnectionBuilder()
        .withUrl("/Processes")
        .build();

    client.on("AddedReservation", (process) => {
        console.log("Rezervisana knjiga ", process);
        addProcess(process);
    });

    client.on("DeletedReservation", (process) => {
        console.log("Vracena knjiga ", process);
        deleteProcess(process);
    });

    function deleteProcess(process) {

        let bookName = process.book.name;
        let userId = process.userId;

        let row = document.getElementById("row " + bookName + " " + userId);

        row.remove();
    }

    function addProcesses() {
        $reservationsBody.empty();
        $.each(processes, (i, c) => addProcess(c));
    }

    function addProcess(process) {

        let reservationTime = getDate(process.time);
        let returnDeadline = getDate(process.returnDeadline);

        let template = `<tr id="row ${process.book.name} ${process.userId}">
                          <td>${process.user.firstName}</td>
                          <td>${process.user.lastName}</td>
                          <td>${process.book.name}</td>
                          <td>${process.book.author}</td>
                          <td>${reservationTime}</td>
                          <td>${returnDeadline}</td>
                        </tr>`;

        $reservationsBody.append($(template));
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
        console.log("[All Reservations] Connection to Hub Successful");
    }

    function rejected() {
        console.log("[All Reservations] Connection to Hub was not Successful");
    }

    function getProcesses() {
        $.getJSON("/api/processes/allUsersReservations")
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