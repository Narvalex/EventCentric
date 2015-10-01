
angular.module('app').config(function ($provide) {
    $provide.decorator('$exceptionHandler', ['$delegate', function ($delegate) {
        return function (exception, cause) {
            $delegate(exception, cause);
            console.log(exception.message);
        };
    }]);
});