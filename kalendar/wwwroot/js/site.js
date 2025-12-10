$(document).ready(function () {
    let currentDay = null;

    $(document).on('click', '.day-cell', function () {
        currentDay = $(this).data('day');
        $('#modal-day').text(currentDay);
        loadEventsForDay(currentDay);
        const modal = new bootstrap.Modal(document.getElementById('dayModal'));
        modal.show();
    });

    function loadEventsForDay(day) {
        $('#form-day-input').val(day);
        $.get('/Index?handler=GetEventsByDay', { day: day }, function (events) {
            let html = '';
            if (events.length > 0) {
                html = '<h6>Существующие события:</h6><ul class="list-group list-group-flush">';
                events.forEach(ev => {
                    html += `
                        <li class="list-group-item d-flex justify-content-between align-items-start">
                            <div>
                                <strong>${ev.category}</strong><br>
                                <small class="text-muted">${ev.event}</small>
                            </div>
                            <div>
                                <button class="btn btn-sm btn-outline-warning ms-1" onclick="editEvent(${ev.id}, \`${ev.category}\`, \`${ev.event}\`)">✏</button>
                                <button class="btn btn-sm btn-outline-danger ms-1" onclick="deleteEvent(${ev.id})">🗑</button>
                            </div>
                        </li>`;
                });
                html += '</ul>';
            } else {
                html = '<p class="text-muted">Нет событий.</p>';
            }
            $('#events-list').html(html);
        });
    }

    window.editEvent = function (id, cat, evt) {
        const newCat = prompt('Категория:', cat);
        const newEvt = prompt('Описание:', evt);
        if (newCat === null || newEvt === null) return;

        fetch('/Index?handler=EditEvent', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8',
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            body: $.param({
                Id: id,
                Day: currentDay,
                Category: newCat,
                Event: newEvt
            })
        }).then(() => {
            window.location.reload();
        });
    };

    window.deleteEvent = function (id) {
        if (!confirm('Удалить событие?')) return;

        fetch('/Index?handler=DeleteEvent', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8',
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            body: $.param({ id: id })
        }).then(() => {
            window.location.reload();
        });
    };
});