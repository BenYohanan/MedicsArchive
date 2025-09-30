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
		url: '/Report/RegisterUser',
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