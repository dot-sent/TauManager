function configureControlPostbackObject($controlObject, action, parameterConfig, eventName, success, error){
    const feedbackHtml = '\
        <div class="spinner-border spinner-border-sm text-success d-none" role="status">\
            <span class="sr-only">Loading...</span>\
        </div>\
        <span class="text-success update-success d-none">&#10003;</span>\
        <span class="text-error update-error d-none">&#10006;</span>\
    ';
    if (eventName == undefined) {
        eventName = 'change';
    }
    $controlObject.after(feedbackHtml);
    $controlObject.on(eventName, function(evt){
        $(evt.target).siblings('.spinner-border').removeClass('d-none');
        $(evt.target).siblings('.update-success').addClass('d-none');
        $(evt.target).siblings('.update-error').addClass('d-none');
        var data = {};
        Object.keys(parameterConfig).forEach(function(key){
            if (parameterConfig[key].substring(0,1) == '#') {
                data[key] = $(evt.target).prop(parameterConfig[key].substring(1));
            } else if (parameterConfig[key] == 'val') {
                data[key] = $(evt.target).val();
            } else {
                data[key] = $(evt.target).data(parameterConfig[key]);
            }
        });

        $.ajax({
            url: action,
            data: data,
            cache: false,
            type: "POST",
            success: function(data, textStatus, XMLHttpRequest){
                $(evt.target).siblings('.spinner-border').addClass('d-none');
                $(evt.target).siblings('.update-success').removeClass('d-none');
                if (success != undefined) success(data);
            },
            error: function(jqXHR, textStatus, errorThrown){
                $(evt.target).siblings('.spinner-border').addClass('d-none');
                $(evt.target).siblings('.update-error').removeClass('d-none');
                if (error != undefined) error();
            }
        })
    });
}

function configureControlPostback(controlSelector, action, parameterConfig, eventName, success, error){
    configureControlPostbackObject($(controlSelector), action, parameterConfig, eventName, success, error);
}