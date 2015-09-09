(function () {
    'use strict';

    angular
        .module('app')
        .factory('empresasMessageSender', empresasMessageSender);

    empresasMessageSender.$inject = ['$http'];

    function empresasMessageSender($http) {

        var service = {
            nuevaEmpresa: nuevaEmpresa
        };

        return service;

        function nuevaEmpresa(empresa) {
            return $http.post(empresasQueueUrl + '/empresas/nueva-empresa', empresa);
        }
    }
})();