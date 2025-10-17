function saveReport(isAdmin) {
    var defaultBtnValue = $('#submit_btn').html();
    $('#submit_btn').html("Importing...");
    $('#submit_btn').attr("disabled", true);

	var formData = new FormData();
	formData.append("isAdmin", isAdmin);
    var files = $('#files')[0].files;

    if (files.length === 0) {
        infoAlert('Please select a file');
        return;
    }

    for (var i = 0; i < files.length; i++) {
        formData.append('files', files[i]);
    }

    $.ajax({
		url: '/Report/UploadFiles',
        method: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (result) {
            if (!result.isError) {
                var url = location.href;
                newSuccessAlert(result.msg, url);
                $('#submit_btn').html(defaultBtnValue);
            } else {
                $('#submit_btn').html(defaultBtnValue);
                $('#submit_btn').attr("disabled", false);
                infoAlert(result.msg);
            }
        },
        error: function (error) {
            $('#submit_btn').html(defaultBtnValue);
            $('#submit_btn').attr("disabled", false);
            errorAlert("An error has occurred, try again. Please contact support if the error persists");
        }
    });
}
function login() {
	var defaultBtnValue = $('#submit_btn').html();
	$('#submit_btn').html("Please wait...");
	$('#submit_btn').attr("disabled", true);
	var email = $('#email').val();
	var password = $('#password').val();
	var userData = {};
	$.ajax({
		type: 'Post',
		url: '/Account/Login',
		dataType: 'json',
		data:
		{
			emailorphone: email,
			password: password
		},
		success: function (result) {
			if (!result.isError) {
				debugger
				location.href = result.dashboard;
			}
			else {
				if (result.data != null) {
					$('#submit_btn').html(defaultBtnValue);
					$('#submit_btn').attr("disabled", false);
					newSuccessAlert(result.msg, result.url);
				} else {
					$('#submit_btn').html(defaultBtnValue);
					$('#submit_btn').attr("disabled", false);
					errorAlert(result.msg);
				}

			}
		},
		error: function (ex) {
			errorAlert("An error has occurred, try again. Please contact support if the error persists");
		}
	});
}
function registerUser() {
	var data = {};
	var defaultBtnValue = $('#submit_btn').html();
	$('#submit_btn').html("Please wait...");
	$('#submit_btn').attr("disabled", true);
	var duration = $('#duration').val();
	if (duration == "Two Weeks") {
		data.PassWordType = "TwoWeeks"
	}
	if (duration == "One Week") {
		data.PassWordType = "OneWeek"
	}
	if (duration == "Never") {
		data.PassWordType = "DoNotExpire"
	}
	data.Email = $('#email').val();
	data.PhoneNumber = $('#phone').val();
	data.FullName = $('#fullName').val();
	$.ajax({
		type: 'Post',
		url: '/Admin/RegisterUser',
		dataType: 'json',
		data:
		{
			userData: JSON.stringify(data)
		},
		success: function (result) {
			if (!result.isError) {
				var url = location.href;
				newSuccessAlert(result.msg, url);
				$('#submit_btn').html(defaultBtnValue);
			}
			else {
				$('#submit_btn').html(defaultBtnValue);
				$('#submit_btn').attr("disabled", false);
				errorAlert(result.msg);
			}
		},
		error: function (ex) {
			errorAlert("An error has occurred, try again. Please contact support if the error persists");
		}
	});
}
function loadReportId(reportId) {
	$("#reportId, #reportIdForDelete").val(reportId);
}

function deleteReport() {
	var defaultBtnValue = $('#dlt_btn').html();
	$('#dlt_btn').html("Please wait...");
	$('#dlt_btn').attr("disabled", true);
	var reportId = $('#reportIdForDelete').val();
	$.ajax({
		type: 'Post',
		url: '/Report/Delete',
		dataType: 'json',
		data:
		{
			reportId: reportId
		},
		success: function (result) {
			if (!result.isError) {
				var url = location.href;
				newSuccessAlert(result.msg, url);
				$('#dlt_btn').html(defaultBtnValue);
			} else {
				$('#dlt_btn').html(defaultBtnValue);
				$('#dlt_btn').attr("disabled", false);
				errorAlert(result.msg);
			}
		},
		error: function (ex) {
			errorAlert("An error has occurred, try again. Please contact support if the error persists");
		}
	});
}
function rejectAcceptReport(isAccept) {
	var btnId = isAccept ? '#aprrove_btn' : '#reject_btn';
	var defaultBtnValue = $(btnId).html();
	$(btnId).html("Please wait...");
	$(btnId).attr("disabled", true);
	var reportId = $('#reportId').val();
	$.ajax({
		type: 'Post',
		url: '/Report/DecideResultStatus',
		dataType: 'json',
		data:
		{
			reportId: reportId,
            isAccept: isAccept
		},
		success: function (result) {
			if (!result.isError) {
				var url = location.href;
				newSuccessAlert(result.msg, url);
				$(btnId).html(defaultBtnValue);
			} else {
				$(btnId).html(defaultBtnValue);
				$(btnId).attr("disabled", false);
				errorAlert(result.msg);
			}
		},
		error: function (ex) {
			errorAlert("An error has occurred, try again. Please contact support if the error persists");
		}
	});
}
function openChangeTypeModal(userId) {
	$('#userIdForType').val(userId);
	$('#change_password_type_modal').modal('show');
}

function openChangePasswordModal(userId) {
	$('#userIdForPassword').val(userId);
	$('#change_password_modal').modal('show');
}

function changePasswordType() {
	var userId = $('#userIdForType').val();
	var newType = $('#newPassType').val();
	if (!newType) {
		infoAlert('Please select a new password type.');
		return;
	}
	var defaultBtnValue = $('#change_type_btn').html();
	$('#change_type_btn').html("Please wait...").attr("disabled", true);

	$.ajax({
		type: 'POST',
		url: '/Admin/ChangePasswordType',
		dataType: 'json',
		data: { userId: userId, newType: newType },
		success: function (result) {
			if (!result.isError) {
				newSuccessAlert(result.msg, location.href);
			} else {
				errorAlert(result.msg);
			}
			$('#change_type_btn').html(defaultBtnValue).attr("disabled", false);
		},
		error: function () {
			errorAlert("An error has occurred, please try again.");
			$('#change_type_btn').html(defaultBtnValue).attr("disabled", false);
		}
	});
}

function changePassword() {
	var userId = $('#userIdForPassword').val();
	var newPassword = $('#newPassword').val();
	if (!newPassword) {
		infoAlert('Please enter a new password.');
		return;
	}
	var defaultBtnValue = $('#change_pass_btn').html();
	$('#change_pass_btn').html("Please wait...").attr("disabled", true);

	$.ajax({
		type: 'POST',
		url: '/Admin/ChangePassword',
		dataType: 'json',
		data: { userId: userId, newPassword: newPassword },
		success: function (result) {
			if (!result.isError) {
				newSuccessAlert(result.msg, location.href);
			} else {
				errorAlert(result.msg);
			}
			$('#change_pass_btn').html(defaultBtnValue).attr("disabled", false);
		},
		error: function () {
			errorAlert("An error has occurred, please try again.");
			$('#change_pass_btn').html(defaultBtnValue).attr("disabled", false);
		}
	});
}
function updateReportStatus(ids, approve) {
	$.ajax({
		url: '/Reports/BulkUpdateStatus',
		type: 'POST',
		data: { ids: ids, approve: approve },
		traditional: true,
		success: function (result) {
			if (!result.isError) {
				var url = location.href;
				newSuccessAlert(result.msg, url);
			}
			else {

				errorAlert(result.msg);
			}
		},
		error: function () {
			errorAlert('Something went wrong while extending the duration.');
		}
	});
}

function deleteReports(ids) {
	$.ajax({
		url: '/Reports/BulkDelete',
		type: 'POST',
		data: { ids: ids },
		traditional: true,
		success: function (result) {
			if (!result.isError) {
				var url = location.href;
				newSuccessAlert(result.msg, url);
			}
			else {

				errorAlert(result.msg);
			}
		},
		error: function () {
			errorAlert('Something went wrong while extending the duration.');
		}
	});
}

$('#filterBtn').click(function () {
	const minAge = parseInt($('#minAge').val()) || 0;
	const maxAge = parseInt($('#maxAge').val()) || 200;
	const dateFrom = $('#studyDateFrom').val() ? new Date($('#studyDateFrom').val()) : null;
	const dateTo = $('#studyDateTo').val() ? new Date($('#studyDateTo').val()) : null;

	$('table tbody tr').each(function () {
		const age = parseInt($(this).find('td:nth-child(3) span:contains("Age")').text().replace(/\D/g, '')) || 0;
		const studyDateStr = $(this).find('td:nth-child(4)').text().trim();
		const studyDate = studyDateStr ? new Date(studyDateStr) : null;

		let show = true;
		if (age < minAge || age > maxAge) show = false;
		if (dateFrom && studyDate && studyDate < dateFrom) show = false;
		if (dateTo && studyDate && studyDate > dateTo) show = false;

		$(this).toggle(show);
	});
});

// Reset filters
$('#resetBtn').click(function () {
	$('#minAge, #maxAge, #studyDateFrom, #studyDateTo').val('');
	$('table tbody tr').show();
});

function openExtendDurationModal(userId) {
	$('#userIdForExtension').val(userId);
	$('#extend_duration_modal').modal('show');
}

function extendPasswordDuration() {
	var userId = $('#userIdForExtension').val();
	var days = parseInt($('#extraDays').val());

	if (!days) {
		errorAlert("Please enter a valid number of days.");
		return;
	}

	$.ajax({
		url: '/Admin/ExtendPasswordDuration', 
		type: 'POST',
		data: { userId: userId, extraDays: days },

		success: function (result) {
			if (!result.isError) {
				var url = location.href;
				newSuccessAlert(result.msg, url);
			}
			else {
				
				errorAlert(result.msg);
			}
		},
		error: function () {
			errorAlert('Something went wrong while extending the duration.');
		}
	});
}

function printReport(reportId) {
	const link = event.currentTarget;
	const originalHtml = link.innerHTML;

	link.innerHTML = `<i class="fa fa-spinner fa-spin" style="color:#F37438;"></i>`;
	link.style.pointerEvents = 'none';

	window.location.href = '/Report/DownloadReports?ids=' + reportId;

	setTimeout(() => {
		link.innerHTML = originalHtml;
		link.style.pointerEvents = 'auto';
	}, 4000);
}

$('#select-all').change(function () {
	$('.row-checkbox').prop('checked', this.checked);
});

function getSelectedIds() {
	return $('.row-checkbox:checked').map(function () {
		return $(this).val();
	}).get();
}

$('#bulkApprove').click(function () {
	const ids = getSelectedIds();
	if (ids.length === 0) return infoAlert("Select at least one record to approve.");
	updateReportStatus(ids, true);
	//hideBulkActions();
});

$('#bulkReject').click(function () {
	const ids = getSelectedIds();
	if (ids.length === 0) return infoAlert("Select at least one record to reject.");
	updateReportStatus(ids, false);
	//hideBulkActions();
});

$('#bulkDelete').click(function () {
	const ids = getSelectedIds();
	if (ids.length === 0) return infoAlert("Select at least one record to delete.");
	deleteReports(ids);
	//hideBulkActions();
});

$('#bulkDownload').click(function () {
	const btn = $(this);
	const originalHtml = btn.html();
	const ids = getSelectedIds();

	if (ids.length === 0) return infoAlert("Select at least one record to download.");

	btn.html(`<i class="fa fa-spinner fa-spin me-1" style="color:white;"></i> Downloading...`);
	btn.prop('disabled', true);

	window.location.href = '/Report/DownloadReports?ids=' + ids.join(',');

	setTimeout(() => {
		btn.html(originalHtml);
		btn.prop('disabled', false);
		//hideBulkActions();
	}, 5000);
});

function hideBulkActions() {
	$('#bulkActions').addClass('hide-important');
	$('#select-all').prop('checked', false);
	$('.row-checkbox').prop('checked', false);
}