(function () {
    'use strict';

    angular
        .module('app')
        .factory('empresasDao', empresasDao);

    empresasDao.$inject = ['$http'];

    function empresasDao($http) {

        var urlPrefix = "http://172.16.251.125:83";

        var service = {
            awaitResult: awaitResult,
            obtenerTodasLasEmpresas: obtenerTodasLasEmpresas,
            obtenerEmpresa: obtenerEmpresa
        };

        return service;

        function awaitResult(transactionId) {
            return $http.get(urlPrefix + '/dao/await-result/' + transactionId)
                    .then(function (response) {
                        return response.data;
                    });
        }

        function obtenerTodasLasEmpresas() {
            return $http.get(urlPrefix + '/dao/obtener-todas-las-empresas')
                    .then(function (response) {
                        return response.data;
                    });
        }

        function obtenerEmpresa(idEmpresa) {
            return $http.get(urlPrefix + '/dao/empresa/' + idEmpresa)
                    .then(function (response) {
                        return response.data;
                    });
        }
    }
})();