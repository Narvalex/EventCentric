(function () {
    'use strict';

    angular
        .module('app')
        .factory('empresasDao', empresasDao);

    empresasMessageSender.$inject = ['$http'];

    function empresasMessageSender($http) {

        var urlPrefix = "http://localhost:60867";

        var service = {
            nuevaEmpresa: nuevaEmpresa
        };

        return service;

        function esperarNuevaEmpresa(transactionId) {
            return $http.post(urlPrefix + '/empresas/nueva-empresa', empresa);
        }
    }
})();