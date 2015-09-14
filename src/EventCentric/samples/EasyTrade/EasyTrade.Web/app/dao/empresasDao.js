(function () {
    'use strict';

    angular
        .module('app')
        .factory('empresasDao', empresasDao);

    empresasDao.$inject = ['$http'];

    function empresasDao($http) {

        var urlPrefix = "http://localhost:60867";

        var service = {
            awaitResult: awaitResult,
            obtenerTodasLasEmpresas: obtenerTodasLasEmpresas
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
    }
})();