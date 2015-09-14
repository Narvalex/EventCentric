(function () {
    'use strict';

    angular
        .module('app')
        .controller('empresasMainController', empresasMainController);

    empresasMainController.$inject = ['empresasMessageSender', 'empresasDao', '$state', 'utils'];

    function empresasMainController(empresasMessageSender, empresasDao, $state, utils) {
        var vm = this;
        var dao = empresasDao;
        var sender = empresasMessageSender;

        // View models
        vm.empresas = [];
        vm.mostrarTodo = false;

        // Commands
        vm.desactivarEmpresa = desactivarEmpresa;

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

        function desactivarEmpresa(idEmpresa) {
            sender.desactivarEmpresa(idEmpresa)
                .then(function (data) {
                    // await result
                    dao.awaitResult(data.data)
                        .then(function (data) {
                            // empresa desactivada
                            toastr.info(data.message);
                            
                            // volvemos a obtener todas las empresas
                            obtenerTodasLasEmpresas();
                        },
                        function (message) {
                            toast.empresas(message.data.exceptionMessage);
                        })
                },
                function (message) {
                    toastr.error(message.data.exceptionMessage);
                })
        }
    }
})();
