(function () {
    'use strict';

    angular
        .module('app')
        .factory('empresasMessageSender', empresasMessageSender);

    empresasMessageSender.$inject = ['$http'];

    function empresasMessageSender($http) {

        var urlPrefix = "http://localhost:50588";

        var service = {
            nuevaEmpresa: nuevaEmpresa
        };

        return service;

        function nuevaEmpresa(empresa) {
            return $http.post('/empresas/nueva-empresa', empresa);
        }
    }
})();