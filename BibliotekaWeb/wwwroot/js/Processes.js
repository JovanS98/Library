
"use strict";

$(document).ready(() => {

    let $theWarning = $("#theWarning");
    let $logBody = $("#logBody");
    let processes = [];

    /* $theWarning.hide();
    $logBody.on("click", ".delete-button", function () {
        deleteProcess(this);
    }); */

    const client = new signalR.HubConnectionBuilder()
        .withUrl("/Processes")
        .build();

    client.on("NewProcessReceived", newProcess => {
        addProcess(newProcess);
    });


    function addProcesses() {
        $logBody.empty();
        $.each(processes, (i, c) => addProcess(c));
    }

    function addProcess(process) {
        let template = `<tr>
  <td>${process.user.firstName}</td>
  <td>${process.user.lastName}</td>
  <td>${process.book.name}</td>
  <td>${process.time}</td>
  <td><a asp-controller="Reservation" asp-action="AcceptReservation" asp-route-userId=${process.userId} asp-route-bookId=${process.bookId} asp-route-time=${process.time} class="btn btn-primary" style=" width:100%">
           Accept reservation
      </a>
   </td>
   </tr>`;

        $logBody.append($(template));
    } 

  /*  function deleteProcess(button) {
        let id = $(button).attr("data-id");
        $.ajax({
            url: `/api/processes/${id}`,
            method: "delete"
        })
            .then(res => {
                $(button).closest("tr").remove();
            });
    } */

    function getProcesses() {
        $.getJSON("/api/processes")
            .then(res => {
                processes = res;
                addProcesses();
                client.start();
            })
            .catch(() => {
                $theWarning.text("Failed to get calls...");
                $theWarning.show();
            });
    }

    getProcesses();
});