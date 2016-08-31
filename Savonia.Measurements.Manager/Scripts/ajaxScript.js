
function AppendHtml(data, id) {
    //alert("append");
    $(id).html("");
    $(id).append(data);
}

function AjaxErrorRedirection(type, url, data, datatype, funcSuccess) {
    $.ajax({
        type: type,
        url: url,
        data: data,
        dataType: datatype,
        success: function (data) {
            funcSuccess(data);
        }
        ,
        error: function (xhr, ajaxOptions, error) {
            if (error != "Canceled") {
                location.href = error;
            }
            else {
                AppendHtml("Canceled", successSection);
            }
        }

    });
}

function AjaxFuncDelegate(type, url, data, datatype, func)
{
    $.ajax({
        type: type,
        url: url,
        data: data,
        dataType: datatype,

        success: function (data) {
            func(data);
        }
        ,
        error: function (xhr, ajaxOptions, thrownError) {
        }
    });
}

function AjaxNormal(type, url, data, datatype, SuccessElement) {
    $.ajax({
        type: type,
        url: url,
        data: data ,
        dataType: datatype,

        success: function (data) {
            AppendHtml(data, SuccessElement);
        }
        ,
        error: function (xhr, ajaxOptions, thrownError) {
        }

    });
}

function AjaxString(type, url, data, datatype, SuccessID) {
    $.ajax({
        type: type,
        url: url,
        data: { data: data.toString() },
        dataType: datatype,

        success: function (data) {
            if (data.redirect != null) {
                Redirect(data.redirect);
            }
            AppendHtml(data, "#" + SuccessID);
        }

        ,
        error: function (xhr, ajaxOptions, thrownError) {
        }

    });
}



function Redirect(url)
{
    window.location.href = url;
}