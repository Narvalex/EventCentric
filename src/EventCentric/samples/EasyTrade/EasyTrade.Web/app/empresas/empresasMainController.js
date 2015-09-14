(function () {
    'use strict';

    angular
        .module('app')
        .controller('empresasMainController', empresasMainController);

    empresasMainController.$inject = ['empresasDao', '$state', 'utils'];

    function empresasMainController(empresasDao, $state, utils) {
        var vm = this;
        var dao = empresasDao;

        // View models
        vm.empresas = [];
        vm.mostrarTodo = false;

        // Commands

        activate();

        function activate() {
            //...
            obtenerTodasLasEmpresas();
        }

        function obtenerTodasLasEmpresas() {
            dao.obtenerTodasLasEmpresas()
                .then(function (data) {
                    vm.empresas = data;
                },
                function (message) {
                    toastr.error(message.data.exceptionMessage);
                });
        }
    }
})();
