(function () {
    'use strict';

    var app = angular.module('app', [
            // TODO: add modules
    ]);

    app.run(function () {

        //configToastr();
    });

    //#region Angular Exception Handler Decorator

    // (to do stuff when an error occurs)
    app.config(function ($provide) {
        $provide.decorator("$exceptionHandler",
            ["$delegate",
                function ($delegate) {
                    return function (exception, cause) {
                        exception.message = "Fatal error: " + exception.message;
                        $delegate(exception, cause);

                        //alert(exception.message);
                        console.log(exception.message);
                    };
                }]);
    });

    //#endregion

    //function configToastr() {
    //    // More info: http://codeseven.github.io/toastr/demo.html
    //    toastr.options = {
    //        "closeButton": true,
    //        "debug": false,
    //        "progressBar": false,
    //        "positionClass": "toast-bottom-right",
    //        "onclick": null,
    //        "showDuration": "300",
    //        "hideDuration": "1000",
    //        "timeOut": "10000",
    //        "extendedTimeOut": "1000",
    //        "showEasing": "swing",
    //        "hideEasing": "linear",
    //        "showMethod": "fadeIn",
    //        "hideMethod": "fadeOut"
    //    }
    //}

})();