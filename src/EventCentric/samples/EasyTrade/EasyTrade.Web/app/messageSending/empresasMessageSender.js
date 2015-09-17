(function () {
    'use strict';

    angular
        .module('app')
        .factory('empresasMessageSender', empresasMessageSender);

    empresasMessageSender.$inject = ['$http'];

    function empresasMessageSender($http) {

        var empresasQueueUrl = "http://172.16.251.125:82";
        //var empresasQueueUrl = "http://192.168.1.4:82";

        var service = {
            nuevaEmpresa: nuevaEmpresa,
            desactivarEmpresa: desactivarEmpresa,
            reactivarEmpresa: reactivarEmpresa,
            actualizarEmpresa: actualizarEmpresa
        };

        return service;

        function nuevaEmpresa(empresa) {
            return $http.post(empresasQueueUrl + '/empresas/nueva-empresa', empresa);
        }

        function desactivarEmpresa(idEmpresa) {
            return $http.post(empresasQueueUrl + '/empresas/desactivar-empresa/' + idEmpresa);
        }

        function reactivarEmpresa(idEmpresa) {
            return $http.post(empresasQueueUrl + '/empresas/reactivar-empresa/' + idEmpresa);
        }

        function actualizarEmpresa(empresa) {
            return $http.post(empresasQueueUrl + '/empresas/actualizar-empresa', empresa);
        }
    }
})();