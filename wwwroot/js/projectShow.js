function submitLeaveBoardForm(){
    document.getElementById("leaveBoardForm").submit();
}

function updateTaskColIdOnForm(id){
    document.getElementById("taskColumnId").value = id;
}

function updateTaskStatus(taskId) {
    var form = document.getElementById('taskStatus-' + taskId);
    var formData = new FormData(form);

    fetch(form.action, {
        method: 'POST',
        body: formData,
        headers: {
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    })
    .then(response => {
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        return response.json();
    })
    .then(data => {
        if (data.success) {
            console.log('Task status updated successfully');

            var status = formData.get("Status");
            var statusSquare = document.getElementById("taskStatusColor-" + taskId);
            statusSquare.classList.remove("bg-danger", "bg-warning", "bg-success");

            var taskCard = document.getElementById("taskCard-" + taskId);
            taskCard.classList.remove("border-danger", "border-warning", "border-danger");

            switch (status) {
                case "NotStarted":
                    statusSquare.classList.add("bg-danger");
                    taskCard.classList.add("border-danger");
                    break;
                case "InProgress":
                    statusSquare.classList.add("bg-warning");
                    taskCard.classList.add("border-warning");
                    break;
                case "Completed":
                    statusSquare.classList.add("bg-success");
                    taskCard.classList.add("border-success");
                    break;
                default:
                    statusSquare.classList.add("bg-dark");
                    taskCard.classList.add("border-dark");
                    break;
            }
        } else {
            alert('Failed to update task status: ' + data.message);
        }
    })
    .catch(error => {
        console.error('There was a problem with the fetch operation:', error);
    });
}

function editComment(commentId) {
    document.getElementById(`comment-content-${commentId}`).classList.add('d-none');
    document.getElementById(`edit-comment-form-${commentId}`).classList.remove('d-none');
}

function cancelEdit(commentId) {
    document.getElementById(`comment-content-${commentId}`).classList.remove('d-none');
    document.getElementById(`edit-comment-form-${commentId}`).classList.add('d-none');
}

function updateTaskOrder(taskId, newColumnId, newIndex, oldColumnId, oldIndex) {
    fetch('/Project/UpdateTaskDisplayInfo', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify({
            taskId: taskId,
            newColumnId: newColumnId,
            newIndex: newIndex,
            oldColumnId: oldColumnId,
            oldIndex: oldIndex
        })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            console.log('Task order updated successfully');
        } else {
            alert('Failed to update task order: ' + data.message);
        }
    })
    .catch(error => {
        console.error('There was a problem with the fetch operation:', error);
    });
}

function updateTaskColumnOrder(columnId, newIndex, oldIndex){
        fetch('/Project/UpdateTaskColumnDisplayInfo', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify({
            columnId: columnId,
            newIndex: newIndex,
            oldIndex: oldIndex
        })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            console.log('Task order updated successfully');
        } else {
            alert('Failed to update task order: ' + data.message);
        }
    })
    .catch(error => {
        console.error('There was a problem with the fetch operation:', error);
    });
}

function goBack() {
    let currentUrl = window.location.href;
    let previousUrl = document.referrer;

    if (window.history.length > 1) {
        if (previousUrl && previousUrl !== currentUrl) {
            window.history.back();
        } else {
            let steps = -1;
            while (steps > -window.history.length && document.referrer === currentUrl) {
                window.history.go(steps);
                steps--;
            }
            if (document.referrer === currentUrl) {
                window.location.href = '/';
            }
        }
    } else {
        window.location.href = '/';
    }
}

function updateDueDateMin() {
    var startDate = document.getElementById("StartDate").value;
    var dueDateInput = document.getElementById("DueDate");
    dueDateInput.min = startDate;
}